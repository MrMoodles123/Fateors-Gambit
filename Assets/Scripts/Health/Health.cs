using System;
using Fusion;
using UnityEngine;

namespace Projectiles
{
	/// <summary>
	/// Handles synchronization of health state and hits (hit reactions) between clients.
	/// </summary>
	public class Health : ContextBehaviour, IHitTarget, IHitInstigator
	{
		// PUBLIC MEMBERS

		// events triggered when hit is taken, performed, or fatal
		public event Action<HitData> HitTaken;
		public event Action<HitData> HitPerformed;
		public event Action<HitData> FatalHitTaken;

		// variables to check state of health
		public bool    IsAlive       => CurrentHealth > 0f;
		public bool    IsImmortal    => _immortalCooldown.ExpiredOrNotRunning(Runner) == false;
		public float   MaxHealth     => _maxHealth;

		// network health to sync across clients
		[Networked]
		public float   CurrentHealth { get; private set; }

		// PRIVATE MEMBERS

		[SerializeField]
		private float _maxHealth = 100f;
		[SerializeField]
		private Transform _headPivot;
		[SerializeField]
		private Transform _bodyPivot;
		[SerializeField]
		private Transform _groundPivot;
		[SerializeField]
		private Hitbox _bodyHitbox;

		[Networked]
		private int _hitCount { get; set; }
		[Networked, Capacity(8)]
		private NetworkArray<Hit> _hits { get; }

		[Networked]
		private TickTimer _immortalCooldown { get; set; }

		private int _visibleHitCount;

		[Networked] public int regen { get; set; }


		// PUBLIC METHODS
		// method to increase max health 
		public void incMaxHealth(float amount)
		{
			_maxHealth += amount;
			CurrentHealth = _maxHealth;
		}

		// method to set max health
		public void setMaxHealth(float hp)
		{
            _maxHealth = hp;
            CurrentHealth = _maxHealth;
        }

		// method to decrease max health
        public void decMaxHealth(float amount)
        {
            _maxHealth -= amount;
            CurrentHealth = _maxHealth;
        }

		// method to regenerate health based on regen value
		public void regenHealth()
		{
			if ((CurrentHealth + regen) <= 1) return; // don't want to die from negative regen
			CurrentHealth = Mathf.Min(CurrentHealth + regen, MaxHealth);
		}

		// method to increase health regeneration
		public void incRegen(int val)
		{
			regen += val;
		}

		// set immortality timer
        public void SetImmortality(float duration)
		{
			_immortalCooldown = TickTimer.CreateFromSeconds(Runner, duration);
		}

		// stop immortality
		public void StopImmortality()
		{
			_immortalCooldown = default;
		}

		// resets health back to max value
		public void ResetHealth()
		{
			CurrentHealth = _maxHealth;
		}

		// sets health low so one hit will end overtime
		public void setOvertimeHealth()
		{
			CurrentHealth = 1f;
		}

		// NetworkBehaviour INTERFACE
		// called when object spawned on network
		public override void Spawned()
		{
			_visibleHitCount = _hitCount;
			regen = 0;
		}
		// called when object despawned on network
		public override void Despawned(NetworkRunner runner, bool hasState)
		{
			HitTaken = null;
			HitPerformed = null;
			FatalHitTaken = null;
		}

		// render health over network
		public override void Render()
		{
			// Interpolated value is used to show visible hits.
			// This basically mean that we are waiting for confirmation
			// from the server to show hit effects and death. It adds small delay
			// but ensures correct feedback is presented to the player.
			var interpolator = new NetworkBehaviourBufferInterpolator(this);
			int hitCount = interpolator.Int(nameof(_hitCount));

			UpdateVisibleHits(hitCount);
		}

        // copies the object's state when synced with the network
		public override void CopyBackingFieldsToState(bool firstTime)
		{
			InvokeWeavedCode();
			base.CopyBackingFieldsToState(firstTime);

			CurrentHealth = _maxHealth;
		}

		// IHitTarget INTERFACE

		bool IHitTarget.IsActive => Object != null && IsAlive;

        // provides transforms for hit detection (head, body, ground)
		Transform IHitTarget.HeadPivot   => _headPivot != null ? _headPivot : transform;
		Transform IHitTarget.BodyPivot   => _bodyPivot != null ? _bodyPivot : transform;
		Transform IHitTarget.GroundPivot => _groundPivot != null ? _groundPivot : transform;
		Hitbox    IHitTarget.BodyHitbox  => _bodyHitbox;

        // process a hit (apply damage or healing)
		void IHitTarget.ProcessHit(ref HitData hitData)
		{
			ApplyHit(ref hitData);

			if (hitData.Amount == 0)
				return;

			if (IsAlive == false)
			{
				hitData.IsFatal = true;
			}

			if (HasStateAuthority == true)
			{
				// On state authority we fire events immediately
				OnHitTaken(ref hitData);
			}
		}

		// IHitInstigator INTERFACE
        // triggered when the instigator performs a hit (outside of this object)
		void IHitInstigator.HitPerformed(HitData hitData)
		{
			if (hitData.Amount > 0 && hitData.Target != (IHitTarget)this && Runner.IsResimulation == false)
			{
				HitPerformed?.Invoke(hitData);
			}
		}

		// PRIVATE METHODS
        // applies a hit (either damage or healing) and updates health state
		private void ApplyHit(ref HitData hitData)
		{
			if (IsAlive == false || IsImmortal == true)
			{
				hitData.Amount = 0f;
				return;
			}

			if (hitData.Action == EHitAction.Damage)
			{
				hitData.Amount = RemoveHealth(hitData.Amount);
			}
			else if (hitData.Action == EHitAction.Heal)
			{
				hitData.Amount = AddHealth(hitData.Amount);
			}

			if (hitData.Amount <= 0)
				return;

			var hit = new Hit
			{
				Action           = hitData.Action,
				Damage           = hitData.Amount,
				Direction        = hitData.Direction,
				RelativePosition = hitData.Position != Vector3.zero ? hitData.Position - transform.position : Vector3.zero,
				Instigator       = hitData.InstigatorRef,
				IsFatal          = IsAlive == false,
			};

			int hitIndex = _hitCount % _hits.Length;
			_hits.Set(hitIndex, hit);

			_hitCount++;
		}

		// adds health based on amount
		private float AddHealth(float amount)
		{
			float previousHealth = CurrentHealth;
			SetHealth(CurrentHealth + amount);
			return CurrentHealth - previousHealth;
		}

		// removes health based on amount
		private float RemoveHealth(float amount)
		{
			float previousHealth = CurrentHealth;
			SetHealth(CurrentHealth - amount);
			return previousHealth - CurrentHealth;
		}

		// sets health based on amount
		private void SetHealth(float health)
		{
			CurrentHealth = Mathf.Clamp(health, 0, _maxHealth);
		}

        // updates visible hits based on networked hit count
		private void UpdateVisibleHits(int hitCount)
		{
			if (HasStateAuthority == true)
				return; // On state authority hits are shown immediately from FUN

			if (_visibleHitCount == hitCount)
				return;

			int bufferLength = _hits.Length;
			int oldestValidHit = hitCount - bufferLength;

			for (int i = Mathf.Max(_visibleHitCount, oldestValidHit); i < hitCount; i++)
			{
				int hitIndex = i % bufferLength;
				var hit = _hits.Get(hitIndex);

				var hitData = new HitData
				{
					Action        = hit.Action,
					Amount        = hit.Damage,
					Position      = transform.position + hit.RelativePosition,
					Direction     = hit.Direction,
					Normal        = -(Vector3)hit.Direction,
					Target        = this,
					InstigatorRef = hit.Instigator,
					IsFatal       = hit.IsFatal,
				};

				OnHitTaken(ref hitData);
			}

			_visibleHitCount = hitCount;
		}

        // handles the reaction when a hit is taken
		private void OnHitTaken(ref HitData hitData)
		{
			// We use _hitData buffer to inform instigator about successful hit as this needs
			// to be synchronized over network as well (e.g. when spectating other players)
			if (hitData.InstigatorRef == Context.Runner.LocalPlayer)
			{
				var instigator = hitData.Instigator;

				if (instigator == null)
				{
					var playerObject = Runner.GetPlayerObject(hitData.InstigatorRef);
					var agent = playerObject != null ? playerObject.GetComponent<Player>().ActiveAgent : null;

					instigator = agent != null ? agent.Health : null;
				}

				if (instigator != null)
				{
					instigator.HitPerformed(hitData);
				}
			}

			HitTaken?.Invoke(hitData);

			if (hitData.IsFatal == true)
			{
				FatalHitTaken?.Invoke(hitData);
			}
		}

		// HELPERS

        // structure for hit event, including the action (damage/heal), damage amount, etc.
		public struct Hit : INetworkStruct
		{
			public EHitAction         Action;
			public float              Damage;
			public Vector3Compressed  RelativePosition;
			public Vector3Compressed  Direction;
			public PlayerRef          Instigator;
			public NetworkBool        IsFatal;
		}
	}
}
