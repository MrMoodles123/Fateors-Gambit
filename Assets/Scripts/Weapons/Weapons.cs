using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Fusion.Sockets.NetBitBuffer;

namespace Projectiles
{
	public class WeaponContext
	{
		// We do not want to link AgentInput directly in case the weapon is used
		// by other entities (e.g. turret, NPC)
		public NetworkButtons Buttons;
		public NetworkButtons PressedButtons;
		public Vector3 MoveVelocity;

		public Transform FireTransform;
		public HitscanProjectileBuffer HitscanProjectiles;
		public KinematicProjectileBuffer KinematicProjectiles;
	}

	/// <summary>
	/// Handles player weapons.
	/// </summary>
	[DefaultExecutionOrder(5)]
	public class Weapons : ContextBehaviour
	{
        // PUBLIC MEMBERS
        private bool weaponAssignedInPlayground = false;
        [SerializeField] private string triggerTag = "WeaponPickup";
		public bool IsSwitchingWeapon => _switchCooldown.ExpiredOrNotRunning(Runner) == false;
		public float ElapsedSwitchTime => _weaponSwitchDuration - _switchCooldown.RemainingTime(Runner).GetValueOrDefault();

		public Weapon CurrentWeapon => _weapons[CurrentWeaponSlot];
		public Weapon PendingWeapon => _weapons[PendingWeaponSlot];

		[Networked, HideInInspector]
		public int CurrentWeaponSlot { get; private set; }
		[Networked, HideInInspector]
		public int PendingWeaponSlot { get; private set; }

		public int Version => _version;

		// PRIVATE MEMBERS

		[Networked, Capacity(12)]
		public NetworkArray<Weapon> _weapons { get; }
		[Networked]
		private TickTimer _switchCooldown { get; set; }


		[SerializeField]
		private Weapon[] _initialWeapons;
		[SerializeField]
		private Transform _weaponsRoot;
		[SerializeField]
		private Transform _fireTransform;
		[SerializeField]
		private Vector3 _firstPersonWeaponOffset = new(-0.15f, 0f, 0f);
		[SerializeField]
		private Vector3 swordOffset = new Vector3(-0.029f, -0.247f, 0.232f);

		[Header("Weapon Switch")]
		[SerializeField]
		private float _weaponSwitchDuration = 1f;
		[SerializeField, Tooltip("When the actual weapon swap happens during weapon switch")]
		private float _weaponSwapTime = 0.5f;

		private int _version;

		private PlayerAgent _agent;
		private WeaponContext _weaponContext = new();

		private Dictionary<int, (Vector3 position, Quaternion rotation)> _weaponTransformations = new Dictionary<int, (Vector3, Quaternion)>();

		// PUBLIC METHODS

		public bool SwitchWeapon(int weaponSlot, bool immediate)
		{
			//RefreshWeapons();
			if (weaponSlot < 0 || weaponSlot >= _weapons.Length)
				return false;

			var weapon = _weapons[weaponSlot];
			if (weapon == null)
				return false;

			if (immediate == true || _weaponSwitchDuration <= 0f)
			{
				PendingWeaponSlot = weaponSlot;
				RefreshWeapons();
				CurrentWeaponSlot = weaponSlot;
				_switchCooldown = default;
			}
			else
			{
				StartWeaponSwitch(weaponSlot);
			}

			RefreshWeapons();
			return true;
		}

		public int GetNextWeaponSlot(int fromSlot, bool ignoreZeroWeapon = false)
		{
			int weaponsLength = _weapons.Length;

			for (int i = 0; i < weaponsLength; i++)
			{
				int slot = (fromSlot + i + 1) % weaponsLength;

				if (slot == 0 && ignoreZeroWeapon == true)
					continue;

				if (_weapons[slot] != null)
					return slot;
			}

			return 0;
		}

		public int GetPreviousWeaponSlot(int fromSlot, bool ignoreZeroWeapon = false)
		{
			int weaponsLength = _weapons.Length;

			for (int i = 0; i < weaponsLength; i++)
			{
				int slot = (weaponsLength + fromSlot - i - 1) % weaponsLength;

				if (slot == 0 && ignoreZeroWeapon == true)
					continue;

				if (_weapons[slot] != null)
					return slot;
			}

			return 0;
		}

		public void GetAllWeapons(List<Weapon> weapons)
		{
			for (int i = 0; i < _weapons.Length; i++)
			{
				if (_weapons[i] != null)
				{
					weapons.Add(_weapons[i]);
				}
			}
		}

		// NetworkBehaviour INTERFACE

		public override void Spawned()
		{
			if (HasStateAuthority == false)
			{
				RefreshWeapons();
				return;
			}

			int minWeaponSlot = int.MaxValue;

			// Spawn initial weapons
			for (int i = 0; i < _initialWeapons.Length; i++)
			{
				var weaponPrefab = _initialWeapons[i];
				if (weaponPrefab == null)
					continue;

				var weapon = Runner.Spawn(weaponPrefab, inputAuthority: Object.InputAuthority);
				AddWeapon(weapon);

				if (weapon.WeaponSlot < minWeaponSlot)
				{
					minWeaponSlot = weapon.WeaponSlot;
				}
				_weaponTransformations[weapon.GetInstanceID()] = (weapon.transform.localPosition, weapon.transform.localRotation);
			}


            if (SceneManager.GetActiveScene().name == "Playground")
            {
                Debug.Log("Playground scene detected, spawning default weapon");
                SwitchWeapon(0, true); // Spawn default weapon
            }
            else
            {
                int randomIndex = Random.Range(0, _initialWeapons.Length);
                Debug.Log("Randomly selecting weapon: " + randomIndex);
                SwitchWeapon(randomIndex, true);
            }

            SetWeaponView(ObjectLayer.FirstPerson, _firstPersonWeaponOffset);
			RefreshWeapons();
		}

		public override void Despawned(NetworkRunner runner, bool hasState)
		{
			// Cleanup weapons
			for (int i = 0; i < _weapons.Length; i++)
			{
				var weapon = _weapons[i];
				if (weapon != null)
				{
					RemoveWeapon(weapon.WeaponSlot, true);
				}
			}
		}

		public override void FixedUpdateNetwork()
		{
			ProcessInput();
			UpdateWeaponSwitch();
		}

        public override void Render()
        {
            if (CurrentWeapon == null)
                return;

            int layer = HasInputAuthority ? ObjectLayer.FirstPerson : ObjectLayer.ThirdPerson;
            var offset = HasInputAuthority ? _firstPersonWeaponOffset : Vector3.zero;

            RefreshWeapons();
            SetWeaponView(layer, offset);
        }


        // MONOBEHAVIOUR

        protected void Awake()
		{
			_agent = GetComponent<PlayerAgent>();

			_weaponContext.KinematicProjectiles = GetComponent<KinematicProjectileBuffer>();
			_weaponContext.HitscanProjectiles = GetComponent<HitscanProjectileBuffer>();
			_weaponContext.FireTransform = _fireTransform;
		}

		// PRIVATE METHODS

		private void ProcessInput()
		{
			if (IsProxy == true)
				return;

			//if (CurrentWeapon == null)
			//	return;

			if (GetInput(out GameplayInput input) == false)
				return;

			SwitchWeapon(input.WeaponSlot, false);

			if (IsSwitchingWeapon == true)
				return;

			_weaponContext.Buttons = input.Buttons;
			_weaponContext.PressedButtons = input.Buttons.GetPressed(_agent.Input.PreviousButtons);
			_weaponContext.MoveVelocity = _agent.KCC.RealVelocity;

			if (CurrentWeapon.ProcessFireInput() == true)
			{
				_agent.Health.StopImmortality();
			}
		}

		private void StartWeaponSwitch(int weaponSlot)
		{
			//if (weaponSlot == PendingWeaponSlot)
			//	return;

			PendingWeaponSlot = weaponSlot;

			if (ElapsedSwitchTime < _weaponSwapTime)
				return; // We haven't swap weapon yet, just continue with new pending weapon

			_switchCooldown = TickTimer.CreateFromSeconds(Runner, _weaponSwitchDuration);
		}

		private void UpdateWeaponSwitch()
		{
			if (IsProxy == true)
				return;

			if (CurrentWeaponSlot == PendingWeaponSlot)
				return;

			if (ElapsedSwitchTime < _weaponSwapTime)
				return;

			CurrentWeaponSlot = PendingWeaponSlot;
		}

		private void RefreshWeapons()
		{
			var currentWeapon = CurrentWeapon;
			if (currentWeapon == null)
				return;

			if (currentWeapon.IsArmed == true)
				return; // Proper weapon is ready

			for (int i = 0; i < _weapons.Length; i++)
			{
				var weapon = _weapons[i];

				if (weapon == null)
					continue;

				weapon.SetWeaponContext(_weaponContext);

				if (weapon != currentWeapon)
				{
					weapon.DisarmWeapon();
					weapon.SetActive(false);
				}
			}

			currentWeapon.transform.SetParent(_weaponsRoot, false);
			currentWeapon.SetActive(true);

			currentWeapon.ArmWeapon();

			_version++;

			if (_weaponContext.HitscanProjectiles != null)
			{
				_weaponContext.HitscanProjectiles.UpdateBarrelTransforms(currentWeapon.BarrelTransforms);
			}

			if (_weaponContext.KinematicProjectiles != null)
			{
				_weaponContext.KinematicProjectiles.UpdateBarrelTransforms(currentWeapon.BarrelTransforms);
			}
		}

		private void SetWeaponView(int layer, Vector3 offset)
		{
			var currentWeapon = CurrentWeapon;

			//if (currentWeapon == null)
			//	return;

			if (currentWeapon.gameObject.layer != layer)
			{
				// First person weapon is rendered differently (see ForwardRenderer asset)
				currentWeapon.gameObject.SetLayer(layer, true);
			}

			// Weapon is in different position for first person vs third person to align nicely in camera view
			currentWeapon.transform.localPosition = offset;
		}

		private void AddWeapon(Weapon weapon)
		{
			if (weapon == null)
				return;

			RemoveWeapon(weapon.WeaponSlot);

			weapon.Object.AssignInputAuthority(Object.InputAuthority);

			_weapons.Set(weapon.WeaponSlot, weapon);
		}

		private void RemoveWeapon(int slot, bool despawn = true)
		{
			var weapon = _weapons[slot];
			if (weapon == null)
				return;

			if (despawn == true)
			{
				Runner.Despawn(weapon.Object);
			}
			else
			{
				weapon.Object.RemoveInputAuthority();
			}

			_weapons.Set(slot, null);
		}


		private void OnTriggerEnter(Collider other)
		{
			// Always allow switching weapons in the Playground scene
			if (SceneManager.GetActiveScene().name == "Playground")
			{
				// Check if the collided object has the specific tag or identifier
				if (other.CompareTag(triggerTag))
				{
					// Assuming the collider object can give us the weapon slot to switch to
					int weaponSlot = GetWeaponSlotFromCollider(other);

					// Trigger the weapon switch
					Debug.Log($"Switching to weapon slot {weaponSlot} in Playground");
					SwitchWeapon(weaponSlot, true);
				}
			}
			else
			{
				// Only process triggers for other scenes
				if (other.CompareTag(triggerTag))
				{
					// Assuming the collider object can give us the weapon slot to switch to
					int weaponSlot = GetWeaponSlotFromCollider(other);

					// Trigger the weapon switch for non-Playground scenes
					SwitchWeapon(weaponSlot, true);
				}
			}
		}

		private int GetWeaponSlotFromCollider(Collider collider)
		{

            if (collider.gameObject.name == "RemoveWeapon")
            {
                return 0;
            }
            if (collider.gameObject.name == "Jump (1)")
			{
				return 1;
			}
			else if (collider.gameObject.name == "Melee")
			{
				return 2;
			}
			return 0;
		}

	}
}
