using UnityEngine;
using UnityEngine.InputSystem;
using Fusion;
using Fusion.Addons.SimpleKCC;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;  // Add this for scene management

namespace Projectiles
{
    // represents different input actions
    public enum EInputButton
    {
        Fire = 0,
        AltFire = 1,
        Jump = 2,
        Reload = 3,
    }

    // network structure for game input (move/look etc)
    public struct GameplayInput : INetworkInput
    {
        public int WeaponSlot => WeaponButton - 1;

        public Vector2 MoveDirection;
        public Vector2 LookRotationDelta;
        public byte WeaponButton;
        public NetworkButtons Buttons;
    }

    /// <summary>
    /// PlayerInput handles accumulating player input from Unity and passes the accumulated input to Fusion.
    /// </summary>
    public sealed class PlayerInput : ContextBehaviour, IBeforeUpdate, IAfterTick
    {
        // PUBLIC METHODS
        private Health _health;
        private bool _inputBlocked = false;

        // Public property to check if input is blocked
        public bool InputBlocked => _inputBlocked || _health == null || !_health.IsAlive;


        // Method to set input blocking
        public void SetInputBlocked(bool blocked)
        {
            _inputBlocked = blocked;
        }
        public NetworkButtons PreviousButtons => _previousButtons;
        public Vector2 AccumulatedLook => _lookRotationAccumulator.AccumulatedValue;

        // PRIVATE MEMBERS

        private float _lookSensitivity = 3;
        private NetworkButtons _previousButtons { get; set; }

        private GameplayInput _accumulatedInput;
        private Vector2Accumulator _lookRotationAccumulator = new(0.02f, true);

        private PlayerAgent _agent;

        // NetworkBehaviour INTERFACE

        // spawn player and relevant information
        public override void Spawned()
        {
            _lookSensitivity = PlayerPrefs.GetFloat("Mouse Sensitivity");

            // Only local player needs networked properties (previous input buttons).
            ReplicateToAll(false);
            ReplicateTo(Object.InputAuthority, true);

            if (HasInputAuthority == false)
                return;

            // Register to Fusion input poll callback
            var networkEvents = Runner.GetComponent<NetworkEvents>();
            networkEvents.OnInput.AddListener(OnInput);

            Context.GeneralInput.RequestCursorLock();

            // Disable movement initially if the current scene is "Playground"
            if (SceneManager.GetActiveScene().name == "Playground")
            {
                _agent.SetInputBlocked(true);  // Use SetInputBlocked to disable movement
            }
        }

        // despawn player
        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (runner == null)
                return;

            var networkEvents = runner.GetComponent<NetworkEvents>();
            if (networkEvents != null)
            {
                networkEvents.OnInput.RemoveListener(OnInput);
            }
        }

        // IBeforeUpdate INTERFACE

        // runs before each game update
        void IBeforeUpdate.BeforeUpdate()
        {
            if (HasInputAuthority == false)
                return;

            if (Runner.ProvideInput == false || Context.GeneralInput.IsLocked == false || _agent.InputBlocked == true)
            {
                _accumulatedInput = default;
                return;
            }

            var mouse = Mouse.current;
            if (mouse != null)
            {
                var mouseDelta = mouse.delta.ReadValue();
                var lookRotationDelta = new Vector2(-mouseDelta.y, mouseDelta.x);
                lookRotationDelta *= _lookSensitivity / 60f;
                _lookRotationAccumulator.Accumulate(lookRotationDelta);

                _accumulatedInput.Buttons.Set(EInputButton.Fire, mouse.leftButton.isPressed);
                _accumulatedInput.Buttons.Set(EInputButton.AltFire, mouse.rightButton.isPressed);
            }

            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                var moveDirection = Vector2.zero;

                if (keyboard.wKey.isPressed) { moveDirection += Vector2.up; }
                if (keyboard.sKey.isPressed) { moveDirection += Vector2.down; }
                if (keyboard.aKey.isPressed) { moveDirection += Vector2.left; }
                if (keyboard.dKey.isPressed) { moveDirection += Vector2.right; }

                _accumulatedInput.MoveDirection = moveDirection.normalized;

                _accumulatedInput.Buttons.Set(EInputButton.Jump, keyboard.spaceKey.isPressed);
                _accumulatedInput.Buttons.Set(EInputButton.Reload, keyboard.rKey.isPressed);
            }
        }

        // IAfterTick INTERFACE

        // runs after each network tick
        void IAfterTick.AfterTick()
        {
            _previousButtons = GetInput<GameplayInput>().GetValueOrDefault().Buttons;
        }

        // MONOBEHAVIOUR

        // get player and health refereneces
        private void Awake()
        {
            _agent = GetComponent<PlayerAgent>();
            _health = GetComponent<Health>();
        }

        // PRIVATE METHODS

        // retireve accumulated infromation for accurate movement updates using tick
        private void OnInput(NetworkRunner runner, NetworkInput networkInput)
        {
            _accumulatedInput.LookRotationDelta = _lookRotationAccumulator.ConsumeTickAligned(runner);

            if (_agent.InputBlocked == true)
                return;

            networkInput.Set(_accumulatedInput);
        }
    }
}