﻿using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace Projectiles
{
	// enum for hit action types
	public enum EHitAction : byte
	{
		None,
		Damage,
		Heal,
	}

	// data relevant to hit event
	public struct HitData
	{
		public EHitAction     Action;
		public float          Amount;
		public bool           IsFatal;
		public Vector3        Position;
		public Vector3        Direction;
		public Vector3        Normal;
		public Vector3        RootPosition;
		public PlayerRef      InstigatorRef;
		public IHitInstigator Instigator;
		public IHitTarget     Target;
		public EHitType       HitType;
	}

	//type of hit
	public enum EHitType
	{
		None,
		Projectile,
		Explosion,
		Suicide,
	}

    // interface that defines the behavior of a hit target
	public interface IHitTarget
	{
		bool      IsActive    { get; }
		Transform HeadPivot   { get; }
		Transform BodyPivot   { get; }
		Transform GroundPivot { get; }
		Hitbox    BodyHitbox  { get; }

		void ProcessHit(ref HitData hit);
	}

    // interface that defines the behavior of a hit instigator
	public interface IHitInstigator
	{
		void HitPerformed(HitData hit);
	}

	/// <summary>
	/// A utility that encapsulates common approach of handling hits.
	/// </summary>
	public static class HitUtility
	{
		// PUBLIC METHODS

        // retrieves all targets that can be hit, filtering by whether they are active or not
		public static void GetAllTargets(NetworkRunner runner, List<IHitTarget> targets, bool onlyActive = true)
		{
			targets.Clear();

			var healths = ListPool.Get<Health>(targets.Count);

			// TODO: Best would be to get IHitTargets directly, but that is not possible for now
			runner.GetAllBehaviours(healths);

			if (onlyActive == true)
			{
				for (int i = 0; i < healths.Count; i++)
				{
					var target = healths[i] as IHitTarget;

					if (target.IsActive == true)
						targets.Add(target);
				}
			}
			else
			{
				targets.AddRange(healths);
			}

			ListPool.Return(healths);
		}

        // processes a hit and returns the corresponding HitData
		public static HitData ProcessHit(PlayerRef instigatorRef, Vector3 direction, LagCompensatedHit hit, float baseDamage, EHitType hitType)
		{
			var target = GetHitTarget(hit.Hitbox, hit.Collider);
			if (target == null)
				return default;

			HitData hitData = default;

			hitData.Action        = EHitAction.Damage;
			hitData.Amount        = baseDamage;
			hitData.Position      = hit.Point;
			hitData.Normal        = hit.Normal;
			hitData.Direction     = direction;
			hitData.Target        = target;
			hitData.InstigatorRef = instigatorRef;
			hitData.HitType       = hitType;

			return ProcessHit(ref hitData);
		}

        // Networked process hit
		public static HitData ProcessHit(NetworkBehaviour instigator, Vector3 direction, LagCompensatedHit hit, float baseDamage, EHitType hitType)
		{
			var target = GetHitTarget(hit.Hitbox, hit.Collider);
			if (target == null)
				return default;

			HitData hitData = default;

			hitData.Action        = EHitAction.Damage;
			hitData.Amount        = baseDamage;
			hitData.Position      = hit.Point;
			hitData.Normal        = hit.Normal;
			hitData.Direction     = direction;
			hitData.Target        = target;
			hitData.InstigatorRef = instigator != null ? instigator.Object.InputAuthority : default;
			hitData.Instigator    = instigator != null ? instigator.GetComponent<IHitInstigator>() : null;
			hitData.HitType       = hitType;

			return ProcessHit(ref hitData);
		}

        // processes a hit when the target is hit via collider (e.g., explosion)
		public static HitData ProcessHit(NetworkBehaviour instigator, Collider collider, float damage, EHitType hitType)
		{
			var target = GetHitTarget(null, collider);
			if (target == null)
				return default;

			HitData hitData = default;

			hitData.Action        = EHitAction.Damage;
			hitData.Amount        = damage;
			hitData.InstigatorRef = instigator.Object.InputAuthority;
			hitData.Instigator    = instigator.GetComponent<IHitInstigator>();
			hitData.Position      = collider.transform.position;
			hitData.Normal        = (instigator.transform.position - collider.transform.position).normalized;
			hitData.Direction     = -hitData.Normal;
			hitData.Target        = target;
			hitData.HitType       = hitType;

			return ProcessHit(ref hitData);
		}

        //processes the hit data and applie it to target
		public static HitData ProcessHit(ref HitData hitData)
		{
			hitData.Target.ProcessHit(ref hitData);

			// For local debug targets we show hit feedback immediately
			// if (hitData.Instigator != null && hitData.Target is Health == false)
			// {
			// 	hitData.Instigator.HitPerformed(hitData);
			// }

			return hitData;
		}

        // gets  hit target based on provided hitbox/collider
		public static IHitTarget GetHitTarget(Hitbox hitbox, Collider collider)
		{
			if (hitbox != null)
				return hitbox.Root.GetComponent<IHitTarget>();

			if (collider == null)
				return null;

			if (ObjectLayerMask.HitTargets.value.IsBitSet(collider.gameObject.layer) == false)
				return null;

			return collider.GetComponentInParent<IHitTarget>();
		}
	}
}
