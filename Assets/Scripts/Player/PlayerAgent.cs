using Fusion;
using UnityEngine;
using Fusion.Addons.SimpleKCC;

namespace Projectiles
{
	/// <summary>
	/// Main script handling player agent. It provides access to common components and handles movement input processing and camera.
	/// </summary>
	[DefaultExecutionOrder(-5)]
	[RequireComponent(typeof(Weapons), typeof(Health), typeof(SimpleKCC))]
	public class PlayerAgent : ContextBehaviour
	{
		// PUBLIC MEMBERS

		[Networked]
		public Player      Owner         { get; set; }
		public Weapons     Weapons       { get; private set; }
		public Health      Health        { get; private set; }
		public SimpleKCC   KCC           { get; private set; }
		public PlayerInput Input         { get; private set; }

        public bool isTutorialActive { get; set; } = false;


        public bool        InputBlocked  => Health.IsAlive == false;
        public bool canMove = false;
        // PRIVATE MEMBERS


        [SerializeField]
		private Transform _cameraPivot;
		[SerializeField]
		private Transform _cameraHandle;
		[SerializeField]
		private GameObject hitbox;

		[Header("Movement")]
		//[SerializeField]
		private float _moveSpeed = 6f;
		[SerializeField]
		public float _upGravity = 15f;
		[SerializeField]
		public float _downGravity = 25f;
		[SerializeField]
		private float _maxCameraAngle = 75f;
		//[SerializeField]
		private float _jumpImpulse = 6f; 
		[SerializeField]
		public float _groundAcceleration = 55f;
		[SerializeField]
		public float _groundDeceleration = 25f;
		[SerializeField]
		public float _airAcceleration = 25f;
		[SerializeField]
		public float _airDeceleration = 1.3f;

		// new stats
		[Networked] public Vector3 _scale { get; set; }

		[Networked] public bool justSpawned { get; set; }
		//public bool justSpawned = false;

        [Networked]
		private Vector3 _moveVelocity { get; set; }

		private Vector2 _lastFUNLookRotation;

        private bool _inputBlocked = false;


		// set player stats as blcoked
        public void SetInputBlocked(bool blocked)
        {
            _inputBlocked = blocked;
        }
		//  increase movement speeed by a percentage
        public void incMoveSpeed(int percent)
		{
			_moveSpeed = _moveSpeed + (_moveSpeed * (percent / 100f));
			Debug.LogError($"Speed:{_moveSpeed}");
        }
		//  decrease movement speeed by a percentage
        public void decMoveSpeed(int percent)
        {
            _moveSpeed = _moveSpeed - (_moveSpeed * (percent / 100f));
            Debug.LogError($"Speed:{_moveSpeed}");
        }
		//  increase jump height by a percentage
        public void incJumpHeight(int percent)
        {
            _jumpImpulse = _jumpImpulse + (_jumpImpulse * (percent / 100f));
            Debug.LogError($"Jump:{_jumpImpulse}"); 
        }
		//  decrease jump height by a percentage
        public void decJumpHeight(int percent)
        {
            _jumpImpulse = _jumpImpulse - (_jumpImpulse * (percent / 100f));
            Debug.LogError($"Jump:{_jumpImpulse}");
        }
		//  increase max heatlh by a percentage
        public void incMaxHealth(int percent)
		{
			float amount = Health.MaxHealth * (percent / 100f);
            Health.incMaxHealth(Mathf.RoundToInt(amount));

        }
		//  decrease max heatlh by a percentage
		public float getMaxHealth()
		{
			return Health.MaxHealth;
		}

		// getters
		public float getJump()
		{
			return _jumpImpulse;
		}
		public float getSpeed()
		{
			return _moveSpeed;
		}
		public int getRegen()
		{
			return Health.regen;
		}

		// set stat values
		public void setStats(float hp, float jump, float speed, int regen)
		{
			Health.setMaxHealth(hp);
			_jumpImpulse = jump;
			_moveSpeed = speed;
			Health.regen = regen;
		}

		// decrease values by percentage
        public void decScale(int percent)
		{
            if (_scale.x <= 30f) // cap scale at 30%
            {
                return;
            }
            float percDec = 1f - (percent / 100f);
			_scale *= percDec;
		}
		// increase values by percentage
        public void incScale(int percent)
        {
            float percDec = 1f + (percent / 100f);
            _scale *= percDec;
        }

		// increase all stats by given percent
        public void boostAll(int percent)
		{
			decScale(percent);
			incJumpHeight(percent);
			incMoveSpeed(percent);
			incMaxHealth(percent);
		}

		// set false so player is not jsut spawned
		public void setPlayerSpawned()
		{
			justSpawned = false;
		}

        // NetworkBehaviour INTERFACE
		// called when player is spawned
		// sets music, jsut spawned true
        public override void Spawned()
		{
			name = Object.InputAuthority.ToString();
			_scale = transform.localScale;

			if (HasInputAuthority)
			{
				Context.musicPlaying = false;
			}
            justSpawned = true;
			// Only local player needs networked properties (move velocity).
			// This saves network traffic by not synchronizing networked properties to other clients except local player.
			ReplicateToAll(false);
			ReplicateTo(Object.InputAuthority, true);
		}

		// despawn player and set owner to null
		public override void Despawned(NetworkRunner runner, bool hasState)
		{
			Owner = null;
		}

		public override void FixedUpdateNetwork()
		{
			if (Owner != null && Health.IsAlive == true && isTutorialActive == false)
			{
				ProcessMovementInput();
			}

			//transform.localScale = _scale;
			//hitbox.transform.localScale = _scale;

			// Setting camera pivot rotation
			var pitchRotation = KCC.GetLookRotation(true, false);
			_cameraPivot.localRotation = Quaternion.Euler(pitchRotation);

			_lastFUNLookRotation = KCC.GetLookRotation();
		}

		// set tutorial true 
        public void StartTutorial()
        {
            isTutorialActive = true;
        }

		// set tutorial active false
        public void EndTutorial()
        {
            isTutorialActive = false;
        }
        // MONOBEHAVIOUR

		// initialises references to import components
        protected void Awake()
		{
			//justSpawned = false;
			KCC = GetComponent<SimpleKCC>();
			Weapons = GetComponent<Weapons>();
			Health = GetComponent<Health>();
			Input = GetComponent<PlayerInput>();
            gameObject.tag = "Player"; // You can replace "Player" with whatever tag you prefer
        }

		//updates camera rotation and position based on player movement
		protected void LateUpdate()
		{
			if (HasInputAuthority == true && Owner != null && Health.IsAlive == true)
			{
				// For responsive look experience we use last FUN look + accumulated look rotation delta
				KCC.SetLookRotation(_lastFUNLookRotation + Input.AccumulatedLook, -_maxCameraAngle, _maxCameraAngle);
			}

			// Update camera pitch
			// Camera pivot influences also weapon rotation so it needs to be set on proxies as well
			var pitchRotation = KCC.GetLookRotation(true, false);
			_cameraPivot.localRotation = Quaternion.Euler(pitchRotation);

			if (HasInputAuthority == true)
			{
				var cameraTransform = Context.Camera.transform;

				// Setting base camera transform based on handle
				cameraTransform.position = _cameraHandle.position;
				cameraTransform.rotation = _cameraHandle.rotation;
			}
		}

		// PRIVATE METHODS

		// adjust movement mechanics based on input
		private void ProcessMovementInput()
		{
			if (GetInput(out GameplayInput input) == false)
				return;

			KCC.AddLookRotation(input.LookRotationDelta, -_maxCameraAngle, _maxCameraAngle);

			// It feels better when player falls quicker
			KCC.SetGravity(KCC.RealVelocity.y >= 0f ? _upGravity : _downGravity);

			// Calculate input direction based on recently updated look rotation (the change propagates internally also to KCC.TransformRotation)
			var inputDirection = KCC.TransformRotation * new Vector3(input.MoveDirection.x, 0f, input.MoveDirection.y);

			var desiredMoveVelocity = inputDirection * _moveSpeed;
			float acceleration = 1f;

			if (desiredMoveVelocity == Vector3.zero)
			{
				// No desired move velocity - we are stopping.
				acceleration = KCC.IsGrounded == true ? _groundDeceleration : _airDeceleration;
			}
			else
			{
				acceleration = KCC.IsGrounded == true ? _groundAcceleration : _airAcceleration;
			}

			_moveVelocity = Vector3.Lerp(_moveVelocity, desiredMoveVelocity, acceleration * Runner.DeltaTime);

			float jumpImpulse = input.Buttons.WasPressed(Input.PreviousButtons, EInputButton.Jump) && KCC.IsGrounded ? _jumpImpulse : 0f;
			KCC.Move(_moveVelocity, jumpImpulse);
		}
	}
}
