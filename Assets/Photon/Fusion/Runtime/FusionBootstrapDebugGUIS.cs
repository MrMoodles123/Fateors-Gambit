
namespace Fusion {
  using System;
  using UnityEngine;
  using System.Collections.Generic;
  using UnityEngine.Audio;

  /// <summary>
  /// Companion component for <see cref="FusionBootstrap"/>. Automatically added as needed for rendering in-game networking IMGUI.
  /// </summary>
  [RequireComponent(typeof(FusionBootstrap))]
  [AddComponentMenu("Fusion/Fusion Boostrap Debug GUI")]
  [ScriptHelp(BackColor = ScriptHeaderBackColor.Steel)]
  public class FusionBootstrapDebugGUIS : Fusion.Behaviour {

    // UI Color and font 
    [SerializeField] private Color back;
    [SerializeField] private Color button;
    [SerializeField] private Font font; 

    // slider hitndrnolgn
    // float vol = PlayerPrefs.GetFloat("Volume");
    public float vol;
    public AudioMixer am;
    float ms;

    /// <summary>
    /// When enabled, the in-game user interface buttons can be activated with the keys H (Host), S (Server) and C (Client).
    /// </summary>
    [InlineHelp]
    public bool EnableHotkeys;

    /// <summary>
    /// The GUISkin to use as the base for the scalable in-game UI.
    /// </summary>
    [InlineHelp]
    public GUISkin BaseSkin;

    /// <summary>
    /// The image that displays the game logo on the left
    /// <summary>
    [InlineHelp]
    public Texture2D LeftImage;

    FusionBootstrap _networkDebugStart;
    string _clientCount;
    bool _isMultiplePeerMode;

    Dictionary<FusionBootstrap.Stage, string> _nicifiedStageNames;

#if UNITY_EDITOR

    protected virtual void Reset() {
      _networkDebugStart = EnsureNetworkDebugStartExists();
      _clientCount = _networkDebugStart.AutoClients.ToString();
      BaseSkin = GetAsset<GUISkin>("e59b35dfeb4b6f54e9b2791b2a40a510");
    }

#endif

    protected virtual void OnValidate() {
      ValidateClientCount();
    }

    protected void ValidateClientCount() {
      if (_clientCount == null) {
        _clientCount = "1";
      } else {
        _clientCount = System.Text.RegularExpressions.Regex.Replace(_clientCount, "[^0-9]", "");
      }
    }
    protected int GetClientCount() {
      try {
        return Convert.ToInt32(_clientCount);
      } catch {
        return 0;
      }
    }

    protected virtual void Awake() {

      _nicifiedStageNames = ConvertEnumToNicifiedNameLookup<FusionBootstrap.Stage>("Fusion Status: ");
      _networkDebugStart = EnsureNetworkDebugStartExists();
      _clientCount = _networkDebugStart.AutoClients.ToString();
      ValidateClientCount();
    }
    protected virtual void Start() {
        vol = PlayerPrefs.GetFloat("Volume");
        ms = PlayerPrefs.GetFloat("Mouse Sensitivity");
        
      _isMultiplePeerMode = NetworkProjectConfig.Global.PeerMode == NetworkProjectConfig.PeerModes.Multiple;
    }

    protected FusionBootstrap EnsureNetworkDebugStartExists() {
      if (_networkDebugStart) {
        if (_networkDebugStart.gameObject == gameObject)
          return _networkDebugStart;
      }

      if (TryGetBehaviour<FusionBootstrap>(out var found)) {
        _networkDebugStart = found;
        return found;
      }

      _networkDebugStart = AddBehaviour<FusionBootstrap>();
      return _networkDebugStart;
    }

    private void Update() {

      var nds = EnsureNetworkDebugStartExists();
      if (!nds.ShouldShowGUI) {
        return;
      }

      var currentstage = nds.CurrentStage;
      if (currentstage != FusionBootstrap.Stage.Disconnected) {
        return;
      }

      if (EnableHotkeys) {
        if (Input.GetKeyDown(KeyCode.I)) {
          _networkDebugStart.StartSinglePlayer();
        }

        if (Input.GetKeyDown(KeyCode.H)) {
          if (_isMultiplePeerMode) {
            StartHostWithClients(_networkDebugStart);
          } else {
            _networkDebugStart.StartHost();
          }
        }

        if (Input.GetKeyDown(KeyCode.S)) {
          if (_isMultiplePeerMode) {
            StartServerWithClients(_networkDebugStart);
          } else {
            _networkDebugStart.StartServer();
          }
        }

        if (Input.GetKeyDown(KeyCode.C)) {
          if (_isMultiplePeerMode) {
            StartMultipleClients(nds);
          } else {
            nds.StartClient();
          }
        }

        if (Input.GetKeyDown(KeyCode.A)) {
          if (_isMultiplePeerMode) {
            StartMultipleAutoClients(nds);
          } else {
            nds.StartAutoClient();
          }
        }

        if (Input.GetKeyDown(KeyCode.P)) {
          if (_isMultiplePeerMode) {
            StartMultipleSharedClients(nds);
          } else {
            nds.StartSharedClient();
          }
        }
      }
    }

    protected virtual void OnGUI() {

      GUI.backgroundColor = back;

      var nds = EnsureNetworkDebugStartExists();
      if (!nds.ShouldShowGUI) {
        return;
      }

      var currentstage = nds.CurrentStage;
      if (nds.AutoHideGUI && currentstage == FusionBootstrap.Stage.AllConnected) {
        return;
      }

      var holdskin = GUI.skin;

      GUI.skin = FusionScalableIMGUI.GetScaledSkin(BaseSkin, out var height, out var width, out var padding, out var margin, out var leftBoxMargin);

      // Draw image
      // GUI.DrawTexture(new Rect(Screen.width/4-120, Screen.height/2-125, 300, 250), LeftImage);
      GUI.DrawTexture(new Rect(margin*2, padding*3, 2*Screen.width/3-10*margin, Screen.height - 5*padding), LeftImage);

      GUI.backgroundColor = back;
      GUI.skin.label.font = font;
      GUI.skin.button.font = font;
      GUI.skin.window.font = font;

      GUILayout.BeginArea(new Rect(Screen.width/2 + 30*margin, padding*13, width - 15*margin, Screen.height));
      {
        GUILayout.BeginVertical(GUI.skin.window);
        {
          GUILayout.BeginHorizontal(GUILayout.Height(height));
          {
            var stagename = _nicifiedStageNames.TryGetValue(nds.CurrentStage, out var stage) ? stage : "Unrecognized Stage";
            GUILayout.Label(stagename, new GUIStyle(GUI.skin.label) { fontSize = (int)(GUI.skin.label.fontSize * .8f), alignment = TextAnchor.UpperLeft });

            // Add button to hide Shutdown option after all connect, which just enables AutoHide - so that interface will reappear after a disconnect.
            if (nds.AutoHideGUI == false && nds.CurrentStage == FusionBootstrap.Stage.AllConnected) {
              if (GUILayout.Button("X", GUILayout.ExpandHeight(true), GUILayout.Width(height))) {
                nds.AutoHideGUI = true;
              }
            }
          }
          GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();

        GUILayout.BeginVertical(GUI.skin.window);
        {

          GUI.backgroundColor = button;

          if (currentstage == FusionBootstrap.Stage.Disconnected) {
              
            GUILayout.Label("<size=70>Volume:</size>");
            GUILayout.BeginHorizontal();
            {
            //   nds.DefaultRoomName = GUILayout.TextField(nds.DefaultRoomName, 25, GUILayout.Height(height));
                vol = GUILayout.HorizontalSlider(vol, -80.0f, 10.0f);
                am.SetFloat("master", vol);
                PlayerPrefs.SetFloat("Volume", vol);
                PlayerPrefs.Save();
                // GUILayout.Label(vol);

            }
            GUILayout.EndHorizontal();

            GUILayout.Label("<size=70>Mouse Sensitivity:</size>");
            GUILayout.BeginHorizontal();
            {
            //   nds.DefaultRoomName = GUILayout.TextField(nds.DefaultRoomName, 25, GUILayout.Height(height));
                ms = GUILayout.HorizontalSlider(ms, 0.1f, 10.0f);

                PlayerPrefs.SetFloat("Mouse Sensitivity", ms);
                PlayerPrefs.Save();
                // GUILayout.Label(vol);

            }
            GUILayout.EndHorizontal();

            if (GUILayout.Button(EnableHotkeys ? "<size=70>Back to Main Menu</size>" : "<size=70>Back to Main Menu</size>", GUILayout.Height(height))) {
              Debug.Log("Main Menu");
              _networkDebugStart.ShowUserInterface();
            }

            if (_isMultiplePeerMode) {

              GUILayout.BeginHorizontal(/*GUI.skin.button*/);
              {
                GUILayout.Label("Client Count:", GUILayout.Height(height));
                GUILayout.Label("", GUILayout.Width(4));
                string newcount = GUILayout.TextField(_clientCount, 10, GUILayout.Width(width * .25f), GUILayout.Height(height));
                if (_clientCount != newcount) {
                  // Remove everything but numbers from our client count string.
                  _clientCount = newcount;
                  ValidateClientCount();
                }
              }
              GUILayout.EndHorizontal();
            }
          } else {

            if (GUILayout.Button("Shutdown", GUILayout.Height(height))) {
              _networkDebugStart.ShutdownAll();
            }
          }

          GUILayout.EndVertical();
        }
      }
      GUILayout.EndArea();

      GUI.skin = holdskin;
    }

    private void StartHostWithClients(FusionBootstrap nds) {
      int count;
      try {
        count = Convert.ToInt32(_clientCount);
      } catch {
        count = 0;
      }
      nds.StartHostPlusClients(count);
    }

    private void StartServerWithClients(FusionBootstrap nds) {
      int count;
      try {
        count = Convert.ToInt32(_clientCount);
      } catch {
        count = 0;
      }
      nds.StartServerPlusClients(count);
    }

    private void StartMultipleClients(FusionBootstrap nds) {
      int count;
      try {
        count = Convert.ToInt32(_clientCount);
      } catch {
        count = 0;
      }
      nds.StartMultipleClients(count);
    }

    private void StartMultipleAutoClients(FusionBootstrap nds) {
      int.TryParse(_clientCount, out int count);
      nds.StartMultipleAutoClients(count);
    }

    private void StartMultipleSharedClients(FusionBootstrap nds) {
      int count;
      try {
        count = Convert.ToInt32(_clientCount);
      } catch {
        count = 0;
      }
      nds.StartMultipleSharedClients(count);
    }

    // TODO Move to a utility
    public static Dictionary<T, string> ConvertEnumToNicifiedNameLookup<T>(string prefix = null, Dictionary<T, string> nonalloc = null) where T : System.Enum {

      System.Text.StringBuilder sb = new System.Text.StringBuilder();

      if (nonalloc == null) {
        nonalloc = new Dictionary<T, string>();
      } else {
        nonalloc.Clear();
      }

      var names = Enum.GetNames(typeof(T));
      var values = Enum.GetValues(typeof(T));
      for (int i = 0, cnt = names.Length; i < cnt; ++i) {
        sb.Clear();
        if (prefix != null) {
          sb.Append(prefix);
        }
        var name = names[i];
        for (int n = 0; n < name.Length; n++) {
          // If this character is a capital and it is not the first character add a space.
          // This is because we don't want a space before the word has even begun.
          if (char.IsUpper(name[n]) == true && n != 0) {
            sb.Append(" ");
          }

          // Add the character to our new string
          sb.Append(name[n]);
        }
        nonalloc.Add((T)values.GetValue(i), sb.ToString());
      }
      return nonalloc;
    }
#if UNITY_EDITOR

    public static T GetAsset<T>(string Guid) where T : UnityEngine.Object {
      var path = UnityEditor.AssetDatabase.GUIDToAssetPath(Guid);
      if (string.IsNullOrEmpty(path)) {
        return null;
      } else {
        return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
      }
    }
#endif
  }
}