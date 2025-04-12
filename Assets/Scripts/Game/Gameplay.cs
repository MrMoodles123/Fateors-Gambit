using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using Projectiles.UI;
using TMPro;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace Projectiles
{
	/// <summary>
	/// Represents the actual gameplay loop. Handles PlayerAgent spawning and despawning for each Player that joins gameplay.
	/// </summary>
	public class Gameplay : ContextBehaviour
	{
		// PUBLIC MEMBERS

		// networked dictionaries used to keep track of game state
		[Networked, Capacity(200)]
		public NetworkDictionary<PlayerRef, Player> Players { get; }
        [Networked, Capacity(200)]
        public NetworkDictionary<Player, int> Deaths { get; }
        [Networked, Capacity(2)]
        public NetworkDictionary<int, Player> PlayerRoles { get; }
        [Networked, Capacity(200)]
        public NetworkDictionary<Player, bool> hasSelectedCard { get; }
        [Networked, Capacity(200)]
        public NetworkDictionary<Player, bool> ready { get; }
        //[Networked, Capacity(200)]
        //public NetworkDictionary<int, StatCard> statCards { get; }

        public List<StatCard> statCards = new List<StatCard>();

		// networked variables to keep track of game state
        [Networked] public bool playersSpawned { get => default; set { } }
        [Networked] public bool playersConnected { get => default; set { } }
        [Networked] public bool roundOngoing { get => default; set { } }
		[Networked] public bool playerHasWon { get => default; set { } }
        [Networked] public bool overtimeActive { get => default; set { } }
        [Networked] public float timeLeft { get; set; }
		[Networked] public int winner { get; set; }
		[Networked] public int map { get; set; }

        // PRIVATE METHODS

		// varibales to store spawn points, number of maps, timer for health regen, and storing card effects
        private SpawnPoint[] _spawnPoints;
		//private int _lastSpawnPoint = -1;
		private int numMaps = 3;
        private float regenTimer = 0f;

        private List<CardEffect> _effects = new();

		// networked variables to store player scores
        [Networked] 
		public int p1Score { get; set; }
        [Networked] 
		public int p2Score { get; set; }

		// stores player position of spaen, the match duration, and csv storing stat cards info
        public int spawn;
        private float matchTime = 30f;
        [SerializeField] private string triggerTag = "WeaponPickup";
        // csv files that store cards
        private string statCardsFile = "StatCards";

        // PUBLIC METHODS
		// sets the player ready staus
        public void setPlayerReady(Player player, bool val)
        {
            if (HasStateAuthority)
            {
                ready.Set(player, val);
            }
            else
            {
                RPC_SetPlayerReady(player, val);
            }

        }

		// network component of seetting player ready status
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_SetPlayerReady(Player player, bool val)
        {
            setPlayerReady(player, val);
        }

		// set if player has seleccted a card
        public void setSelectedCard(Player player, bool val)
		{
			if (HasStateAuthority)
			{
				hasSelectedCard.Set(player, val);
			}
			else
			{
				RPC_SetSelectedCard(player, val);
			}

        }

		// netwrok component of setting if player has seleccted a card
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_SetSelectedCard(Player player, bool val)
        {
            setSelectedCard(player, val);
        }

		// marks a player as spawned
		public void setPlayerSpawned(Player player)
		{
			if (HasStateAuthority)
			{
				player.ActiveAgent.setPlayerSpawned();
			}
			else
			{
				RPC_setPlayerSpawned(player);
			}

        }

		// netwrok component to mark a player as spawned
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_setPlayerSpawned(Player player)
        {
			setPlayerSpawned(player);
        }

		// increases a players stats
		public void incStat(Player player, StatCard.Stat stat, int val)
		{
			if (HasStateAuthority)
			{
				//string message = "";
				switch (stat)
				{
					case StatCard.Stat.HP:
						//message = "Your " + stat.ToString() + " has increased by " + val.ToString();
						player.ActiveAgent.Health.incMaxHealth(val);
						break;
					case StatCard.Stat.All:
                        //message = "Your HP, Speed and Jump Height have increased by" + val.ToString() + "%";
                        player.ActiveAgent.boostAll(val);
						break;
					case StatCard.Stat.Speed:
                        //message = "Your Speed has increased by " + val.ToString() + "%";
                        player.ActiveAgent.incMoveSpeed(val);
						break;
					case StatCard.Stat.JumpHeight:
                        //message = "Your Jump Height has increased by " + val.ToString() + "%";
                        player.ActiveAgent.incJumpHeight(val);
						break;
                    case StatCard.Stat.Regen:
                        //message = "Your Regeneration has increased by " + val.ToString() + " per second";
                        player.ActiveAgent.Health.incRegen(val);
                        break;
                }
				//player.buffEffect = message;
			}
			else
			{
				RPC_incStat(player, stat, val);
			}
        }

		// network component to increase a players stats
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_incStat(Player player, StatCard.Stat stat, int val)
		{
			incStat(player, stat, val);
		}

		// decreases a players stats
        public void decStat(Player player, StatCard.Stat stat, int val)
        {
            if (HasStateAuthority)
            {
				// get other player
				Player affectedPlayer;
				int role = getRole(player);
				if (role == 0)
				{
					affectedPlayer = PlayerRoles[1];
				}
				else
				{
					affectedPlayer = PlayerRoles[0];
				}
				//string message = "";
				switch (stat)
				{
					case StatCard.Stat.HP:
                        //message = "Your HP has decreased by " + val.ToString();
                        affectedPlayer.ActiveAgent.Health.decMaxHealth(val);
						break;
					case StatCard.Stat.All:
                        //affectedPlayer.ActiveAgent.boostAll(val);
						break;
					case StatCard.Stat.Speed:
                        //message = "Your Speed has decreased by " + val.ToString() + "%";
                        affectedPlayer.ActiveAgent.decMoveSpeed(val);
						break;
					case StatCard.Stat.JumpHeight:
                        //message = "Your Jump Height has decreased by " + val.ToString() + "%";
                        affectedPlayer.ActiveAgent.decJumpHeight(val);
						break;
					case StatCard.Stat.Regen:
                        //message = "Your Regeneration has decreased by " + val.ToString() + " per second";
                        affectedPlayer.ActiveAgent.Health.incRegen(-val);
                        break;
				}
				//affectedPlayer.nerfEffect = message;
            }
            else
            {
                RPC_decStat(player, stat, val);
            }
        }

		// network component to decrease a players stats
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
		private void RPC_decStat(Player player, StatCard.Stat stat, int val)
		{
			decStat(player, stat, val);
		}

		// uses the stats applied to send message to players about what stats of theirs have been modified
		private void buildMessage(Player player, StatCard.Stat stat, int val, bool good)
		{
            if (good)
            {
                string message = "";
                switch (stat)
                {
                    case StatCard.Stat.HP:
                        message = "Your " + stat.ToString() + " has increased by " + val.ToString();
                        break;
                    case StatCard.Stat.All:
                        message = "Your HP, Speed and Jump Height have increased by" + val.ToString() + "%";
                        break;
                    case StatCard.Stat.Speed:
                        message = "Your Speed has increased by " + val.ToString() + "%";
                        break;
                    case StatCard.Stat.JumpHeight:
                        message = "Your Jump Height has increased by " + val.ToString() + "%";
                        break;
                    case StatCard.Stat.Regen:
                        message = "Your Regeneration has increased by " + val.ToString() + " per second";
                        break;
                }
                player.buffEffect = message;
            }
            else
            {
                // get other player
                Player affectedPlayer;
                int role = getRole(player);
                if (role == 0)
                {
                    affectedPlayer = PlayerRoles[1];
                }
                else
                {
                    affectedPlayer = PlayerRoles[0];
                }
                string message = "";
                switch (stat)
                {
                    case StatCard.Stat.HP:
                        message = "Your HP has decreased by " + val.ToString();
                        break;
                    case StatCard.Stat.All:
                        break;
                    case StatCard.Stat.Speed:
                        message = "Your Speed has decreased by " + val.ToString() + "%";
                        break;
                    case StatCard.Stat.JumpHeight:
                        message = "Your Jump Height has decreased by " + val.ToString() + "%";
                        break;
                    case StatCard.Stat.Regen:
                        message = "Your Regeneration has decreased by " + val.ToString() + " per second";
                        break;
                }
                affectedPlayer.nerfEffect = message;
            }
        }

		// add the effects of cards to players and build their message
		public void addCardEffect(Player player, StatCard.Stat stat, int val, bool good)
		{
			if (HasStateAuthority)
			{
				CardEffect c = new();
				c.player = player;
				c.stat = stat;
				c.val = val;
				c.good = good;

				_effects.Add(c);
				buildMessage(player, stat, val, good);
			}
			else
			{
				RPC_addCardEffect(player, stat, val, good);
			}
		}

		// netwrok component to add the effects of cards to players and build their message
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_addCardEffect(Player player, StatCard.Stat stat, int val, bool good)
		{
			addCardEffect(player, stat, val, good);
		}
        //public string buildString(Player player, StatCard.Stat stat, int val, bool good)
        //{
        //	string
        //}

		// applies all stat card effects to player
        public void applyEffects()
		{
			while (_effects.Count > 0)
			{
				CardEffect c = _effects[0];
				if (c.good)
				{
					incStat(c.player, c.stat, c.val);
				}
				else
                {
                    decStat(c.player, c.stat, c.val);
                }
				_effects.RemoveAt(0);
            }
		}

		// regenerate health for all players
		private void regenHealth()
		{
			foreach (var kvp in PlayerRoles)
			{
				Player p = kvp.Value;
				p.ActiveAgent.Health.regenHealth();
			}
        }

		//loads stat cards from csv file and places them into array
        private void populateStatCards()
        {
			// read in all lines
			TextAsset StatCardCSV = Resources.Load<TextAsset>(statCardsFile);
            string[] lines = StatCardCSV.text.Split('\n');

            for (int i = 1; i < lines.Length; i++) // skip first line which is just header
            {
                string[] cardInfo = lines[i].Split(',');
                addStatCard(cardInfo, i);
            }
        }

		// add stat cards
		private void addStatCard(string[] cardInfo, int id)
		{
            Debug.Log($"{cardInfo[0]},{cardInfo[1]},{cardInfo[2]}, {cardInfo[3]}");
            int stat = int.Parse(cardInfo[2]);
			int isGood = int.Parse(cardInfo[4]);
			StatCard card = new StatCard(id, cardInfo[0], cardInfo[1], (StatCard.Stat)stat, int.Parse(cardInfo[3]), Convert.ToBoolean(isGood));
			statCards.Add(card);
        }

		// choose a random stat caard from list
        public StatCard chooseRandomStatCard()
		{
			int id = UnityEngine.Random.Range(0, statCards.Count);
			return statCards[id];
		}

		// choose a random, good, stat card from list
		public StatCard chooseGoodCard()
		{
			while (true)
			{
				int id = UnityEngine.Random.Range(0, statCards.Count);
				if (statCards[id].IsGood)
				{
					return statCards[id];

                }
            }
		}

		// choose a random, bad, stat card from list
        public StatCard chooseBadCard()
        {
            while (true)
            {
                int id = UnityEngine.Random.Range(0, statCards.Count);
                if (!statCards[id].IsGood)
                {
                    return statCards[id];

                }
            }
        }

		// update player scores
        public void UpdateScore()
        {
            if (!HasStateAuthority) return; // Only the host updates scores

            if (PlayerRoles.TryGet(0, out Player p1) && PlayerRoles.TryGet(1, out Player p2))
            {
                p1Score = Deaths.Get(p2); // P2 deaths = P1 score
                p2Score = Deaths.Get(p1); // P1 deaths = P2 score

				if (p1Score >= 5)
				{
					EndGame(p1);
				}
                else if (p2Score >= 5)
                {
                    EndGame(p2);
                }
            }
			else if (PlayerRoles.TryGet(0, out Player p)) {
				p1Score = 0;
                p2Score = Deaths.Get(p);
				if (p2Score >= 1)
				{
					EndGame(p);
				}
            }
        }

		// set a winner to the game and end game
		private void EndGame(Player pWinner)
		{
			playerHasWon = true;
			winner = getRole(pWinner);
		}

		// not sure how to explain this
        public void Join(Player player)
		{
			if (HasStateAuthority == false)
				return;

			var playerRef = player.Object.InputAuthority;

            if (Players.Count >= 2) // Enforcing 1v1 mode
            {
                Debug.LogError($"Max players reached! Cannot add player {playerRef}");
                return;
            }

            if (Players.ContainsKey(playerRef) == true)
			{
				Debug.LogError($"Player {playerRef} already joined");
				return;
			}

			int role = Players.Count; // 0 is p1, 1 is p2
			Players.Add(playerRef, player);
			PlayerRoles.Add(role, player);
			Deaths.Add(player, 0);
			hasSelectedCard.Add(player, false);
			ready.Add(player, false);

            timeLeft = matchTime;
            OnPlayerJoined(player);
			if (Players.Count == 2)
			{
				timeLeft = matchTime;
				//roundOngoing = true;
                playersConnected = true;
				overtimeActive = false;
            }
		}

		// boolean to declare when both players ahve selected a card
		public bool allPlayersSelectedCard()
		{
            foreach (var kvp in hasSelectedCard)
            {
				//Debug.LogError($"Player:{kvp.Key.Id}.Val:{kvp.Value}");
                if (kvp.Value == false)
                {
                    return false;
                }
            }

			return true;
        }

		// boolean to declare when all players are ready to play
        public bool allPlayersReady()
        {
            foreach (var kvp in ready)
            {
                //Debug.LogError($"Player:{kvp.Key.Id}.Val:{kvp.Value}");
                if (kvp.Value == false)
                {
                    return false;
                }
            }

            return true;
        }

		// update time and set overtime timer if applicable 
        private void UpdateTimer()
        {
			if (!allPlayersSelectedCard()) return;
			applyEffects();
			if (!allPlayersReady()) return;
			// only do regen and update timer if not in overtime / sudden death
			if (overtimeActive == false)
			{
				regenTimer += Runner.DeltaTime;
				if (regenTimer >= 1f)
				{
					regenTimer = 0f;
					regenHealth();
				}
                // update timer
                if (timeLeft > 0)
                {
                    timeLeft -= Runner.DeltaTime;
                }
                else
                {
                    //EndRound(true);
                    startOvertime();
                }
            }
            
        }

		// start the overtime game mode
		private void startOvertime()
		{
			overtimeActive = true;
            foreach (var kvp in PlayerRoles)
            {
				kvp.Value.ActiveAgent.Health.setOvertimeHealth();
            }
        }

		// end the round based on necessary conditions
		private void EndRound(bool timer)
		{
            foreach (var kvp in hasSelectedCard)
            {
				hasSelectedCard.Set(kvp.Key, false);
            }
            foreach (var kvp in ready)
            {
                ready.Set(kvp.Key, false);
            }
            // if timer ends and neither player is dead, take player with least health
            if (timer)
			{
				Player loser = null;
				float minHealth = 100000000f;
				foreach (var kvp in PlayerRoles)
				{
					Player p = kvp.Value;
					if (p.getCurrentHealth() < minHealth)
					{
						minHealth = p.getCurrentHealth();
						loser = p;

					}
					else if (p.getCurrentHealth() == minHealth) // if tie, choose random to win (can add better tie breaker system later)
					{
						int rand = UnityEngine.Random.Range(0, 1);
						if (rand == 0)
						{
							loser = p;
						}
					}
				}

				OnPlayerDeath(loser);
			}
			else
			{
				//foreach (var kvp in PlayerRoles)
				//{
				//	SpawnPlayerAgent(kvp.Value, kvp.Key);
				//}
				//restartTimer();
				StartCoroutine(RoundRestartDelay());
			}
		}

		// set a delay, choose a new map, spawn platyers, and restart timer
        private IEnumerator RoundRestartDelay()
        {
			chooseNewMap();
            yield return new WaitForSeconds(5f); // 5-second delay

            foreach (var kvp in PlayerRoles)
            {
				kvp.Value.resetEffects();
                SpawnPlayerAgent(kvp.Value, kvp.Key);
            }
            restartTimer();
        }

		// randomly select a new map (that is not the current map)
		private void chooseNewMap()
		{
			int oldMap = map;
			while (true)
			{
                int newMap = UnityEngine.Random.Range(0, numMaps);
				if (newMap != oldMap)
				{
					map = newMap;
					return;
				}
            }
			
		}

		// if no overtime, reset timer to matchtime
        private void restartTimer()
		{
			overtimeActive = false;
			timeLeft = matchTime;
		}

		// player leaves game
        public void Leave(Player player)
		{
			if (HasStateAuthority == false)
				return;

			if (Players.ContainsKey(player.Object.InputAuthority) == false)
				return;

			Players.Remove(player.Object.InputAuthority);

			OnPlayerLeft(player);
		}

		// NetworkBehaviour INTERFACE

		// set the gameplay contect, populate the stat cards and choose a map
		public override void Spawned()
		{
            Context.Gameplay = this;
			populateStatCards();
            map = UnityEngine.Random.Range(0, numMaps);
        }

		// update game timer if network instances correct
		public override void FixedUpdateNetwork()
		{
			if (HasStateAuthority == false)
				return;

			//int currentTick = Runner.Tick;

			if (playersConnected == true)
			{
				UpdateTimer();
			}

			
		}

		//clear the gameplay context when despawning
		public override void Despawned(NetworkRunner runner, bool hasState)
		{
			// Clear from context
			Context.Gameplay = null;
		}

		// PROTECTED METHODS

		//assign a role to player when they join
		protected virtual void OnPlayerJoined(Player player)
		{
			SpawnPlayerAgent(player, getRole(player));
			//AddSpawnRequest(player, 0f);
		}

		//find the player role in dictionary, return -1 if not found
		public int getRole(Player player)
		{
			foreach (var kvp in PlayerRoles)
			{
				if (kvp.Value == player)
				{
					return kvp.Key;
				}
			}
			return -1;
		}

		// despawn player when they leave
		protected virtual void OnPlayerLeft(Player player)
		{
			DespawnPlayerAgent(player);
		}

		// when a player dies, increment death cound, update score, and end round with loss condition
		protected virtual void OnPlayerDeath(Player player)
		{
			//AddSpawnRequest(player, 3f);
			Deaths.Set(player, Deaths.Get(player) + 1);
			UpdateScore();
			EndRound(false);
        }

		//mark agent as spawn and givve them temporary immunity
		protected virtual void OnPlayerAgentSpawned(PlayerAgent agent)
		{
			agent.justSpawned = true;
			agent.Health.SetImmortality(3f);
		}

		protected virtual void OnPlayerAgentDespawned(PlayerAgent agent)
		{
		}

		// store stats from previous player agent (if any)
		// despawn agent befroe spawning new one
		// when spawning new agent, choose appropriate prefab, then choose appropriate state and equip weapon
		protected void SpawnPlayerAgent(Player player, int role)
		{
			// get old stats for new player agent
			PlayerAgent oldAgent = player.ActiveAgent;
			float hp = 0f;
			float jump = 0f;
			float speed = 0f;
			int regen = 0;

            if (oldAgent != null) 
			{
                hp = oldAgent.getMaxHealth();
                jump = oldAgent.getJump();
                speed = oldAgent.getSpeed();
                regen = oldAgent.getRegen();
            }
			
            DespawnPlayerAgent(player);
			
			// Spawn custom agent in for tutorial.
			if (SceneManager.GetActiveScene().name == "Playground")
			{
				player.AgentPrefab = player.agents[3];
			}
			else
			{
                player.AgentPrefab = player.agents[map];
            }

            var agent = SpawnAgent(player.Object.InputAuthority, player.AgentPrefab, role) as PlayerAgent;
			player.AssignAgent(agent);

			if (oldAgent != null)
			{
                agent.setStats(hp, jump, speed, regen);
            }
			

			agent.Health.FatalHitTaken += OnFatalHitTaken;

			playersSpawned = true;

			player.switchWeapon();
			OnPlayerAgentSpawned(agent);
		}

		// remove player from game and clear references
		protected void DespawnPlayerAgent(Player player)
		{
			if (player.ActiveAgent == null)
				return;

			player.ActiveAgent.Health.FatalHitTaken -= OnFatalHitTaken;

			OnPlayerAgentDespawned(player.ActiveAgent);

			DespawnAgent(player.ActiveAgent);
			player.ClearAgent();
		}

		//protected void AddSpawnRequest(Player player, float spawnDelay)
		//{
		//	int delayTicks = Mathf.RoundToInt(Runner.TickRate * spawnDelay);

		//	_spawnRequests.Add(new SpawnRequest()
		//	{
		//		Player = player,
		//		Tick = Runner.Tick + delayTicks,
		//	});
		//}

		// PRIVATE METHODS

		// handle death event if hit taken kills player
		private void OnFatalHitTaken(HitData hitData)
		{
			var health = hitData.Target as Health;

			if (health == null)
				return;

			if (Players.TryGet(health.Object.InputAuthority, out Player player) == true)
			{
				OnPlayerDeath(player);
			}
		}

		//initialise and choose spawn points then spawn player
		private PlayerAgent SpawnAgent(PlayerRef inputAuthority, PlayerAgent agentPrefab, int role)
		{
			if (_spawnPoints == null)
			{
				_spawnPoints = Runner.SimulationUnityScene.FindObjectsOfTypeInOrder<SpawnPoint>(false);
			}

			//_lastSpawnPoint = (_lastSpawnPoint + 1) % _spawnPoints.Length;
			var spawnPoint = _spawnPoints[getSpawnPoint(role, map)].transform;

			var agent = Runner.Spawn(agentPrefab, spawnPoint.position, spawnPoint.rotation, inputAuthority);
			return agent;
		}

		//determing spawn point values based on map
		private int getSpawnPoint(int role, int map)
		{
			if (SceneManager.GetActiveScene().name != "Playground")
			{
				spawn = (map * 2) + role;
			}

			return spawn;
		}


		// ensure agen is valud, then despawn
        private void DespawnAgent(PlayerAgent agent)
		{
			if (agent == null)
				return;

			Runner.Despawn(agent.Object);
		}

        // HELPERS

		// player spawn information
        public struct SpawnRequest
		{
			public Player Player;
			public int Tick;
		}

		// card effect information
		public struct CardEffect
		{
			public Player player;
			public StatCard.Stat stat;
			public int val;
			public bool good;
		}
	}
}
