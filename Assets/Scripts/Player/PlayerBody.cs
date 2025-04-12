using Fusion;
using UnityEngine;
using UnityEngine.Rendering;

namespace Projectiles
{
	/// <summary>
	/// Component handling all visual/hierarchy related tasks and effects (immortality, death).
	/// </summary>
	public class PlayerBody : ContextBehaviour
	{
		// PRIVATE MEMBERS

		[SerializeField]
		private GameObject _root;
		[SerializeField]
		private GameObject _visual;
		[SerializeField]
		private GameObject _immortalityEffect;

		[SerializeField]
		private float _capImpulse = 10f;
		[SerializeField]
		private GameObject _deathEffectPrefab;

		private PlayerAgent _agent;
		private HitboxRoot _hitboxRoot;

		// ContextBehaviour INTERFACE
		// initialises player visuals and subscribes to health events
		public override void Spawned()
		{
			_root.SetActive(_agent.Health.IsAlive);
			_agent.Health.FatalHitTaken += OnFatalHit;

			// Disable visual for local player
			var renderers = _visual.GetComponentsInChildren<MeshRenderer>();
			for (int i = 0; i < renderers.Length; i++)
			{
				renderers[i].shadowCastingMode = HasInputAuthority ? ShadowCastingMode.ShadowsOnly : ShadowCastingMode.On;
			}
		}

		// disables hitbox when player is dead
		public override void FixedUpdateNetwork()
		{
			// Disable hitbox detection when agent is dead
			_hitboxRoot.HitboxRootActive = _agent.Health.IsAlive;
		}

		// called whne rendering frames to update visual effects
		public override void Render()
		{
			_immortalityEffect.SetActive(_agent.Health.IsImmortal);
		}

		// unsubscribes from health events to prevent memory leaks
		public override void Despawned(NetworkRunner runner, bool hasState)
		{
			_agent.Health.FatalHitTaken -= OnFatalHit;
		}

		// MONOBEHAVIOUR

		// get player and hitbox component
		protected void Awake()
		{
			_agent = GetComponent<PlayerAgent>();
			_hitboxRoot = GetComponent<HitboxRoot>();
		}

		// PRIVATE METHODS

		// disable player movement and spawns death effect 
		private void OnFatalHit(HitData hit)
		{
			_agent.KCC.SetActive(false);
			_root.SetActive(false);

			var deathEffect = Runner.InstantiateInRunnerScene(_deathEffectPrefab);
			deathEffect.transform.position = transform.position + Vector3.up;


			var direction = (hit.Direction + 2f * Vector3.up).normalized;

			if (Runner.Config.PeerMode == NetworkProjectConfig.PeerModes.Multiple)
			{
				Runner.AddVisibilityNodes(deathEffect.gameObject);
			}
		}
	}
}
