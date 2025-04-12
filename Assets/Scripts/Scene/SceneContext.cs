using Fusion;
using NUnit.Framework.Internal;
using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;
using static Unity.Collections.Unicode;

namespace Projectiles
{
	/// <summary>
	/// Holds scene specific references and common runtime data.
	/// </summary>
	public class SceneContext : MonoBehaviour
	{
		// General

		public ObjectCache      ObjectCache;
		public GeneralInput     GeneralInput;
		public SceneCamera      Camera;

		// Gameplay

		[HideInInspector]
		public Gameplay         Gameplay;
		[HideInInspector]
		public NetworkRunner    Runner;
		[HideInInspector]
		public PlayerAgent      LocalAgent;
		[HideInInspector]
		//public CardManager cardManager;

        [SerializeField]
        public TextMeshProUGUI _player1ScoreText; // p1 score
        [SerializeField]
        public TextMeshProUGUI _player2ScoreText; // p2 score
        [SerializeField]
        private TextMeshProUGUI _timerText;  // match timer

        [SerializeField] private GameObject cardSelectors;
        [SerializeField] private GameObject winnerScreen;
        [SerializeField] private GameObject loserScreen;
        [SerializeField] private GameObject infoScreen;
        [SerializeField] private GameObject healthScreen;
        [SerializeField] private GameObject waitScreen;
        [SerializeField] private GameObject deathScreen;
        [SerializeField] private GameObject OvertimeScreen;
        [SerializeField] private GameObject roundLost;
        [SerializeField] private GameObject roundWon;

        private int prevP1Score = -1;
        private int prevP2Score = -1;

        // card 1 data
        [SerializeField] TextMeshProUGUI card1Title;
        [SerializeField] TextMeshProUGUI card1Desc;
        [SerializeField] Button card1Select;
        [SerializeField] Image card1Image;

        // card 2 data
        [SerializeField] TextMeshProUGUI card2Title;
        [SerializeField] TextMeshProUGUI card2Desc;
        [SerializeField] Button card2Select;
        [SerializeField] Image card2Image;

        // card 3 data
        [SerializeField] TextMeshProUGUI card3Title;
        [SerializeField] TextMeshProUGUI card3Desc;
        [SerializeField] Button card3Select;
        [SerializeField] Image card3Image;

        StatCard c1;
        StatCard c2;
        StatCard c3;

        StatCard selectedCard;

        [SerializeField] TextMeshProUGUI healthVal;
        [SerializeField] Image healthImage;

        [SerializeField] GameObject cardInfoScreen;
        [SerializeField] TextMeshProUGUI countdown;
        [SerializeField] TextMeshProUGUI goodInfo;
        [SerializeField] TextMeshProUGUI badInfo;
        [SerializeField] TextMeshProUGUI waitText;

        [SerializeField] FusionBootstrap fusionBootstrap;

        [SerializeField] MusicManager musicManager;

        // custom colours
        private Color yellow = new Color(234, 255, 67);
        private Color red = new Color(253, 53, 53);

        public bool hasSelectedCard = false;
        public bool test = true;
        public bool doCountDown = false;
        public float cdTime = 5f;
        public bool cdJustFinished = false;
        public bool musicPlaying = true;
        public bool overtimeMusicPlaying = false;

        private string wait1 = "Waiting for player 2...";
        private string wait2 = "Waiting for other player...";

        // hide cards, update time and initalise onclick events
        private void Start()
        {
            //if (Runner.is)
			hideCards();
			UpdateTimer(60);
            addOnClickEvents();
        }

        // add onclick events to cards
        private void addOnClickEvents()
        {
            card1Select.onClick.AddListener(() =>
            {
                selectCard(c1);
            });

            card2Select.onClick.AddListener(() =>
            {
                selectCard(c2);
            });

            card3Select.onClick.AddListener(() =>
            {
                selectCard(c3);
            });
        }

        // set selected card and hide cards
        private void selectCard(StatCard c)
        {
            selectedCard = c;
            Gameplay.addCardEffect(LocalAgent.Owner, selectedCard.StatAffected, selectedCard.Value, selectedCard.IsGood);
            //if (c.IsGood)
            //{
            //    Gameplay.incStat(LocalAgent.Owner, selectedCard.StatAffected, selectedCard.Value);
            //}
            //else
            //{
            //    Gameplay.decStat(LocalAgent.Owner, selectedCard.StatAffected, selectedCard.Value);
            //}

            Gameplay.setSelectedCard(LocalAgent.Owner, true);
            hideCards();
        }

        // update information constantly
        private void Update()
        {
            if (fusionBootstrap == null && Gameplay == null)
            {
                GameObject[] objs = GameObject.FindGameObjectsWithTag("networkDebugStart");

                if (objs.Length == 1)
                {
                    fusionBootstrap = objs[0].GetComponent<FusionBootstrap>();
                    fusionBootstrap.ShowUserInterface();
                }
            }
			if (Gameplay == null) return;
            if (LocalAgent == null) return;
            if (doCountDown)
            {
                if (cdTime > 0)
                {
                    cdTime -= Time.deltaTime;
                    countdown.text = cdTime.ToString("F0");
                    updateInfo();
                }
                else
                {
                    doCountDown = false;
                    cdJustFinished = true;
                    cardInfoScreen.SetActive(false);
                    Gameplay.setPlayerReady(LocalAgent.Owner, true);
                }
                return;
            }
            if ((Gameplay.p1Score != prevP1Score) || (Gameplay.p2Score != prevP2Score)) {
				UpdateScore(Gameplay.p1Score, Gameplay.p2Score);
				prevP1Score = Gameplay.p1Score;
				prevP2Score = Gameplay.p2Score;
                cdJustFinished = false;
                
            }
            if (checkForWinner()) return;
            UpdateTimer(Gameplay.timeLeft);
            UpdateHealthVal();

            if ((LocalAgent.justSpawned == true) && (Gameplay.playersConnected == true))
            {
                OvertimeScreen.SetActive(false);
                if (!musicPlaying)
                {
                    musicManager.playMusic(Gameplay.map);
                    musicPlaying = true;
                }
                waitScreen.SetActive(false);
                showCards();
                Gameplay.setPlayerSpawned(LocalAgent.Owner);
            }
            else if (LocalAgent.justSpawned)
            {
                OvertimeScreen.SetActive(false);
                while (GeneralInput.IsLocked)
                {
                    GeneralInput.RequestCursorRelease();
                }
                if (!musicPlaying)
                {
                    musicManager.playMusic(Gameplay.map);
                    musicPlaying = true;
                }
                waitText.text = wait1.ToUpper();
                waitScreen.SetActive(true);
                //Gameplay.setPlayerSpawned(LocalAgent.Owner);
            }

            if (Gameplay.allPlayersSelectedCard() && !Gameplay.allPlayersReady() && !cdJustFinished)
            {
                //while (!GeneralInput.IsLocked)
                //{
                //    GeneralInput.RequestCursorLock();
                //}
                showCardInfoScreen();
            }

            if (Gameplay.allPlayersSelectedCard() && Gameplay.allPlayersReady())
            {
                while (!GeneralInput.IsLocked)
                {
                    GeneralInput.RequestCursorLock();
                }
            }
            if (Gameplay.overtimeActive)
            {
                OvertimeScreen.SetActive(true);
            }
            //if (Gameplay.overtimeActive && !overtimeMusicPlaying)
            //{
            //    musicManager.startOvertimeMusic();
            //    overtimeMusicPlaying = true;
            //}

            if (hasSelectedCard) return;
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.digit1Key.isPressed)
                {
                    hasSelectedCard = true;
                    card1Select.onClick.Invoke();
                }
                if (keyboard.digit2Key.isPressed)
                {
                    hasSelectedCard = true;
                    card2Select.onClick.Invoke();
                }
                if (keyboard.digit3Key.isPressed)
                {
                    hasSelectedCard = true;
                    card3Select.onClick.Invoke();
                }
            }
        }

        // update the cord stat effect text
        private void updateInfo()
        {
            //string display = "";
            //if (LocalAgent.Owner.buffEffect != "")
            //{
            //    display = display + LocalAgent.Owner.buffEffect + "\n";
            //}
            //if (LocalAgent.Owner.nerfEffect != "")
            //{
            //    display = display + LocalAgent.Owner.nerfEffect + "\n";
            //}

            goodInfo.text = LocalAgent.Owner.buffEffect;
            badInfo.text = LocalAgent.Owner.nerfEffect;
            if (goodInfo.text == "")
            {
                badInfo.rectTransform.anchoredPosition = new Vector2(0, -80);
            }
            else
            {
                badInfo.rectTransform.anchoredPosition = new Vector2(0, -358);
            }
        }

        // display the card screen for players
        private void showCardInfoScreen()
        {
            waitScreen.SetActive(false);
            delay(1f);
            if (Gameplay.allPlayersReady()) return;
            updateInfo();
            cardInfoScreen.SetActive(true);
            // start countdown
            cdTime = 5f;
            doCountDown = true;
            
        }

        // add a delay to timer
        private IEnumerator delay(float time)
        {

            yield return new WaitForSeconds(time);
        }

        // close game and go back to main menu
        public IEnumerator backToMainMenu(float time)
        {

            yield return new WaitForSeconds(time);
            RestartApplication();
            if (Runner != null)
            {
                yield return Runner.Shutdown();
            }
            if (fusionBootstrap != null) 
            {
                fusionBootstrap.ShutdownAll();
            }

            musicManager.playMenuMusic();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

        }

        private void RestartApplication()
        {
            UnityEngine.Debug.Log("Restarting Application...");

            // Get the path to the current executable
            string exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;

            // Start a new process to launch the application again
            ProcessStartInfo startInfo = new ProcessStartInfo(exePath);
            startInfo.UseShellExecute = true;

            try
            {
                Process.Start(startInfo);

                // Close the current application
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false; // In editor, just stop play mode
#else
          Application.Quit(); // In build, exit the application
#endif
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Failed to restart application: {ex.Message}");
            }
        }

        // check if there is a winner, if true show end screen
        private bool checkForWinner()
        {
            if (!Gameplay.playerHasWon) return false;
            if (Gameplay.winner == Gameplay.getRole(LocalAgent.Owner))
            {
                ShowEndScreen(true);
                return true;
            }
            else
            {
                ShowEndScreen(false);
                return true;
            }
        }

        // show end screen to players, wait, then go back to main menu
        private void ShowEndScreen(bool winner)
        {
            while (GeneralInput.IsLocked)
            {
                GeneralInput.RequestCursorRelease();
            }

            infoScreen.SetActive(false);
            healthScreen.SetActive(false);
            deathScreen.SetActive(false);

            winnerScreen.SetActive(winner);
            loserScreen.SetActive(!winner);

            StartCoroutine(backToMainMenu(6f));

        }

        // update the score play announcer voice of whether round was lost or not
		private void UpdateScore(int p1Score, int p2Score)
		{
            roundWon.SetActive(false);
            roundLost.SetActive(false);
            int role = Gameplay.getRole(LocalAgent.Owner);
            test = true;
            if (role == 0)
            {
                _player1ScoreText.text = p1Score.ToString();
                _player2ScoreText.text = p2Score.ToString();
                if (p1Score > prevP1Score)
                {
                    if (p1Score != 0 && p1Score != 5)
                    {
                        roundWon.SetActive(true);
                    }
                }
                else if (p2Score > prevP2Score)
                {
                    if (p2Score != 0 && p2Score != 5)
                    {
                        roundLost.SetActive(true);
                    }
                }
            }
            else 
            {
                _player1ScoreText.text = p2Score.ToString();
                _player2ScoreText.text = p1Score.ToString();
                if (p1Score > prevP1Score)
                {
                    if (p1Score != 0 && p1Score != 5)
                    {
                        roundLost.SetActive(true);
                    }
                }
                else if (p2Score > prevP2Score)
                {
                    if (p2Score != 0 && p2Score != 5)
                    {
                        roundWon.SetActive(true);
                    }
                }
            }

        }

        // update the timer
		private void UpdateTimer(float time)
		{
			if (time > 10)
			{
                _timerText.fontSize = 36;
                _timerText.color = Color.white;
                _timerText.text = time.ToString("F0");
            }
			else
			{
                _timerText.fontSize = 55;
                _timerText.color = Color.red;
                _timerText.text = time.ToString("F2", CultureInfo.InvariantCulture).Replace(",", ".");
            }
				

        }

        // upadte health values
        public void UpdateHealthVal()
        {
            int val = int.Parse(healthVal.text);
            if (val < 25)
            {
                healthVal.color = red;
                healthImage.color = red;
            }
            else if (val < 50)
            {
                healthVal.color = yellow;
                healthImage.color = yellow;
            }
            else
            {
                healthVal.color = Color.white;
                healthImage.color = Color.white;
            }
        }

        // remove cards from screen
        public void hideCards()
		{
            waitText.text = wait2.ToUpper();
            cardSelectors.SetActive(false);
            waitScreen.SetActive(true);
        }

        // show cards on screen
        public void showCards()
		{
            c1 = Gameplay.chooseGoodCard();
            c2 = Gameplay.chooseRandomStatCard();
            c3 = Gameplay.chooseBadCard();

            // card 1
            card1Title.text = c1.Title.ToUpper();
            card1Desc.text = c1.Desc;
            Sprite image1 = Resources.Load<Sprite>(c1.Title);
            card1Image.sprite = image1;

            // card 2
            card2Title.text = c2.Title;
            card2Desc.text = c2.Desc;
            Sprite image2 = Resources.Load<Sprite>(c2.Title);
            card2Image.sprite = image2;

            // card 3
            card3Title.text = c3.Title;
            card3Desc.text = c3.Desc;
            Sprite image3 = Resources.Load<Sprite>(c3.Title);
            card3Image.sprite = image3;

            cardSelectors.SetActive(true);
            //cardManager.showCards();
			hasSelectedCard = false;

            while (GeneralInput.IsLocked)
			{
                GeneralInput.RequestCursorRelease();
            }
            
        }
    }
}
