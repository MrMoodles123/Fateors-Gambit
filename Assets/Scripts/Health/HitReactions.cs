using UnityEngine;

namespace Projectiles
{
	/// <summary>
	/// A simple component for playing hit feedback on dummy targets (moving spheres).
	/// </summary>
	[RequireComponent(typeof(Health))]
	public class HitReactions : MonoBehaviour
	{
		// PRIVATE MEMBERS

		[Header("Animation")]
		[SerializeField]
		private Animation _animation;
		[SerializeField]
		private AnimationClip _hitClip;
		[SerializeField]
		private AnimationClip _fatalHitClip;

		// MONOBEHAVIOUR

		// get health component of game object and subscripe to hittaken event
		protected void OnEnable()
		{
			var health = GetComponent<Health>();
			health.HitTaken += OnHitTaken;
		}

		// get health component of game object and unsubscripe to hittaken event
		protected void OnDisable()
		{
			var health = GetComponent<Health>();
			health.HitTaken -= OnHitTaken;
		}

		// PRIVATE MEMBERS
		// if animation exists, lpay animation based on hit type
		private void OnHitTaken(HitData hitData)
		{
			if (_animation != null)
			{
				var clip = hitData.IsFatal == true ? _fatalHitClip : _hitClip;
				_animation.Play(clip.name);
			}
		}
	}
}
