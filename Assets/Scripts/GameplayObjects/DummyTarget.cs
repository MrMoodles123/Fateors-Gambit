﻿using Fusion;
using UnityEngine;

namespace Projectiles
{
	/// <summary>
	/// Main script controlling dummy targets that can be played in the scene.
	/// Dummy target can be stationary or moving (when SimpleMove component is added).
	/// </summary>
	[RequireComponent(typeof(Health), typeof(HitboxRoot))]
	public class DummyTarget : NetworkBehaviour
	{
		// PRIVATE MEMBERS

		[SerializeField]
		private float _reviveTime = 3f;
		[SerializeField]
		private Animation _animation;
		[SerializeField]
		private AnimationClip _reviveClip;
		[SerializeField]
		private bool _useLagCompensation;

		[Networked]
		private TickTimer _reviveCooldown { get; set; }

		private Health _health;
		private HitboxRoot _hitboxRoot;
		private Collider _collider;

		private bool _isAlive;

		// MONOBEHAVIOUR

		// initialise health, hitbox, and collider on awake
		protected void Awake()
		{
			_health = GetComponent<Health>();
			_hitboxRoot = GetComponent<HitboxRoot>();
			_collider = GetComponentInChildren<Collider>();
		}

		//resets alive statuse when the object is enabled
		protected void OnEnable()
		{
			_isAlive = false;
		}

		// NetworkBehaviour INTERFACE

		// setup lag compensation and collider state when object spawns
		public override void Spawned()
		{
			_collider.enabled = _useLagCompensation == false;
			_hitboxRoot.HitboxRootActive = _useLagCompensation;
		}

		// halndles health and lag during game
		public override void FixedUpdateNetwork()
		{
			if (_useLagCompensation == true)
			{
				_hitboxRoot.HitboxRootActive = _health.IsAlive;
			}
			else
			{
				_collider.enabled = _health.IsAlive;
			}

			if (_health.IsAlive == false)
			{
				if (_reviveCooldown.Expired(Runner) == true)
				{
					_health.ResetHealth();
					_reviveCooldown = default;
				}
				else if (_reviveCooldown.IsRunning == false)
				{
					_reviveCooldown = TickTimer.CreateFromSeconds(Runner, _reviveTime);
				}
			}
		}

		// render the alive state of the target
		public override void Render()
		{
			SetIsAlive(_health.IsAlive);
		}

		// PRIVATE MEMBERS
		// set the alive state for player
		private void SetIsAlive(bool value, bool force = false)
		{
			if (value == _isAlive && force == false)
				return;

			_isAlive = value;

			if (value == true)
			{
				_animation.Play(_reviveClip.name);
			}
		}
	}
}
