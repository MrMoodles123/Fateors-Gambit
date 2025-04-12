using Fusion;
using System.Collections.Generic;
using UnityEngine;

namespace Projectiles
{
	/// <summary>
	/// Component representing joined player. Each player can have a visual representation in the gameplay - player agent.
	/// </summary>
	public class Player : ContextBehaviour
	{
		// PUBLIC MEMBERS

		// networked variables storing player information
		[Networked]
		public PlayerAgent ActiveAgent { get; set; }
		[Networked, Capacity(150)] public string buffEffect { get; set; }
        [Networked, Capacity(150)] public string nerfEffect { get; set; }
        //public PlayerAgent AgentPrefab => _agentPrefab;
        public PlayerAgent AgentPrefab { get; set; }

		public PlayerAgent[] agents = new PlayerAgent[3];

        // PRIVATE MEMBERS

        [SerializeField]
		private PlayerAgent _agentPrefab;

		private PlayerAgent _assignedAgent;
		private int _lastWeaponSlot;

		// PUBLIC METHODS
        // Assigns agent to the player and switches to last used weapon if needed
		public void AssignAgent(PlayerAgent agent)
		{
			ActiveAgent = agent;
			ActiveAgent.Owner = this;

			if (HasStateAuthority == true && _lastWeaponSlot != 0)
			{
				agent.Weapons.SwitchWeapon(_lastWeaponSlot, true);
			}
		}

		// clear current agent
		public void ClearAgent()
		{
			if (ActiveAgent == null)
				return;

			ActiveAgent.Owner = null;
			ActiveAgent = null;
		}

		// randomly switches player's weapon
		public void switchWeapon()
		{
			int numWeapons = ActiveAgent.Weapons._weapons.Length;
			bool weaponSwitched = false;
			while (!weaponSwitched)
			{
				int newWeapon = Random.Range(0, numWeapons);
                weaponSwitched = ActiveAgent.Weapons.SwitchWeapon(newWeapon, true);
			}
		}

		// NetworkBehaviour INTERFACE
		// spawns player inro gamelay context
		public override void Spawned()
		{
			if (Context.Gameplay != null)
			{
				Context.Gameplay.Join(this);
			}
			
		}

		// reset player active effects
		public void resetEffects()
		{
			buffEffect = "";
			nerfEffect = "";
		}

        // update player's state on the network
		public override void FixedUpdateNetwork()
		{
			bool agentValid = ActiveAgent != null && ActiveAgent.Object != null;
			if (agentValid == true && HasStateAuthority == true)
			{
				_lastWeaponSlot = ActiveAgent.Weapons.CurrentWeaponSlot;
			}
		}

		// removes player from gameplay context when despawned
		public override void Despawned(NetworkRunner runner, bool hasState)
		{
			if (hasState == false)
				return;

			if (Context.Gameplay != null)
			{
				Context.Gameplay.Leave(this);
			}

			if (HasStateAuthority == true && ActiveAgent != null)
			{
				Runner.Despawn(ActiveAgent.Object);
			}

			ActiveAgent = null;
		}

		// return current health of player
		public  float getCurrentHealth()
		{
			return ActiveAgent.Health.CurrentHealth;
		}
	}
}
