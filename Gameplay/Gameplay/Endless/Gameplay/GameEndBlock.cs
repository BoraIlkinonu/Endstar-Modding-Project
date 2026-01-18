using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Endless.Gameplay.LuaInterfaces;
using Endless.Gameplay.Scripting;
using Endless.Gameplay.Stats;
using Endless.Shared;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay
{
	// Token: 0x02000318 RID: 792
	public class GameEndBlock : AbstractBlock, IStartSubscriber, IGameEndSubscriber, IScriptInjector
	{
		// Token: 0x170003A6 RID: 934
		// (get) Token: 0x06001260 RID: 4704 RVA: 0x0005B0B7 File Offset: 0x000592B7
		// (set) Token: 0x06001261 RID: 4705 RVA: 0x0005B0BF File Offset: 0x000592BF
		public string Title { get; private set; } = "Game Over";

		// Token: 0x170003A7 RID: 935
		// (get) Token: 0x06001262 RID: 4706 RVA: 0x0005B0C8 File Offset: 0x000592C8
		// (set) Token: 0x06001263 RID: 4707 RVA: 0x0005B0D0 File Offset: 0x000592D0
		public string Description { get; private set; } = "The game is over, what's next?";

		// Token: 0x170003A8 RID: 936
		// (get) Token: 0x06001264 RID: 4708 RVA: 0x0005B0D9 File Offset: 0x000592D9
		// (set) Token: 0x06001265 RID: 4709 RVA: 0x0005B0E1 File Offset: 0x000592E1
		public bool ShowReplayButton { get; private set; }

		// Token: 0x170003A9 RID: 937
		// (get) Token: 0x06001266 RID: 4710 RVA: 0x0005B0EA File Offset: 0x000592EA
		// (set) Token: 0x06001267 RID: 4711 RVA: 0x0005B0F2 File Offset: 0x000592F2
		public bool ShowEndMatchButton { get; private set; }

		// Token: 0x170003AA RID: 938
		// (get) Token: 0x06001268 RID: 4712 RVA: 0x0005B0FB File Offset: 0x000592FB
		public bool ShowNextLevelButton
		{
			get
			{
				return this.levelDestination != null && this.levelDestination.IsValidLevel();
			}
		}

		// Token: 0x06001269 RID: 4713 RVA: 0x0005B114 File Offset: 0x00059314
		public void RecordBasicStat(Context instigator, BasicStat basicStat)
		{
			if (string.IsNullOrEmpty(basicStat.Identifier))
			{
				ArgumentException ex = new ArgumentException("A stat is required to have an identifier. It should be a unique string of data to avoid overwriting other stats.");
				this.scriptComponent.LogException(ex, null);
				throw ex;
			}
			this.basicStatMap[basicStat.Identifier] = basicStat;
		}

		// Token: 0x0600126A RID: 4714 RVA: 0x0005B15A File Offset: 0x0005935A
		public void RecordPerPlayerStat(Context instigator, PerPlayerStat perPlayerStat)
		{
			this.perPlayerStatMap[perPlayerStat.Identifier] = perPlayerStat;
		}

		// Token: 0x0600126B RID: 4715 RVA: 0x0005B16E File Offset: 0x0005936E
		public void RecordComparativeStat(Context instigator, ComparativeStat comparativeStat)
		{
			this.comparativeStatMap[comparativeStat.Identifier] = comparativeStat;
		}

		// Token: 0x0600126C RID: 4716 RVA: 0x0005B184 File Offset: 0x00059384
		public void EndGame(Context instigator)
		{
			try
			{
				object[] array;
				this.scriptComponent.TryExecuteFunction("OnGameEndTriggered", out array, new object[] { instigator });
				NetworkBehaviourSingleton<GameEndManager>.Instance.TriggerGameEndScreen(this);
			}
			catch (Exception ex)
			{
				this.scriptComponent.LogException(ex, "EndGame");
			}
		}

		// Token: 0x0600126D RID: 4717 RVA: 0x0005B1E0 File Offset: 0x000593E0
		public void TriggerNextLevel()
		{
			this.levelDestination.ChangeLevel(Game.Instance.GetGameContext());
		}

		// Token: 0x0600126E RID: 4718 RVA: 0x0005B1F8 File Offset: 0x000593F8
		internal PerPlayerStat[] GatherPerPlayerStats()
		{
			List<PerPlayerStat> list = new List<PerPlayerStat>();
			if (this.perPlayerStats.HasFlag(GameEndBlock.Stats.Downs))
			{
				PerPlayerStat perPlayerStat = new PerPlayerStat
				{
					Identifier = "PerPlayer_Downs",
					Message = new LocalizedString("Downs:"),
					Order = 100,
					DefaultValue = "0"
				};
				perPlayerStat.SetStats(MonoBehaviourSingleton<StatTracker>.Instance.GetDownsAsFloat());
				list.Add(perPlayerStat);
			}
			if (this.perPlayerStats.HasFlag(GameEndBlock.Stats.Kills))
			{
				PerPlayerStat perPlayerStat2 = new PerPlayerStat
				{
					Identifier = "PerPlayer_Kills",
					Message = new LocalizedString("Kills:"),
					Order = 100,
					DefaultValue = "0"
				};
				list.Add(perPlayerStat2);
			}
			if (this.perPlayerStats.HasFlag(GameEndBlock.Stats.Revives))
			{
				PerPlayerStat perPlayerStat3 = new PerPlayerStat
				{
					Identifier = "PerPlayer_Revives",
					Message = new LocalizedString("Revives:"),
					Order = 100,
					DefaultValue = "0"
				};
				perPlayerStat3.SetStats(MonoBehaviourSingleton<StatTracker>.Instance.GetRevivesAsFloat());
				list.Add(perPlayerStat3);
			}
			if (this.perPlayerStats.HasFlag(GameEndBlock.Stats.TotalResourcesCollected) && this.perPlayerStats.HasFlag(GameEndBlock.Stats.Revives))
			{
				PerPlayerStat perPlayerStat4 = new PerPlayerStat
				{
					Identifier = "PerPlayer_Resources",
					Message = new LocalizedString("Resources:"),
					Order = 100,
					DefaultValue = "0"
				};
				list.Add(perPlayerStat4);
			}
			this.perPlayerStats.HasFlag(GameEndBlock.Stats.EachResourceCollected);
			if (this.displayCustomGameStats)
			{
				list.AddRange(Game.Instance.PerPlayerStatMap.Values);
			}
			list.AddRange(this.perPlayerStatMap.Values);
			return list.OrderByDescending((PerPlayerStat stat) => stat.Order).ToArray<PerPlayerStat>();
		}

		// Token: 0x0600126F RID: 4719 RVA: 0x0005B404 File Offset: 0x00059604
		internal BasicStat[] GatherGlobalStats()
		{
			List<BasicStat> list = new List<BasicStat>();
			foreach (GameEndBlock.AutoStat autoStat in this.autoStats)
			{
				try
				{
					switch (autoStat.StatType)
					{
					case GameEndBlock.StatType.Global:
					{
						BasicStat basicStat;
						if (GameEndBlock.ProcessGlobalStat(autoStat, out basicStat))
						{
							list.Add(basicStat);
						}
						break;
					}
					case GameEndBlock.StatType.Most:
					{
						BasicStat basicStat2;
						if (GameEndBlock.ProcessComparativeStat(autoStat, true, out basicStat2))
						{
							list.Add(basicStat2);
						}
						break;
					}
					case GameEndBlock.StatType.Least:
					{
						BasicStat basicStat3;
						if (GameEndBlock.ProcessComparativeStat(autoStat, false, out basicStat3))
						{
							list.Add(basicStat3);
						}
						break;
					}
					default:
						throw new ArgumentOutOfRangeException("Invalid stat type!");
					}
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
				}
			}
			try
			{
				if (this.displayCustomGameStats)
				{
					list.AddRange(Game.Instance.BasicStatMap.Values);
					list.AddRange(Game.Instance.ComparativeStatMap.Values.Select((ComparativeStat stat) => stat.ToBasicStat()));
				}
				list.AddRange(this.basicStatMap.Values);
				list.AddRange(this.comparativeStatMap.Values.Select((ComparativeStat comperative) => comperative.ToBasicStat()));
			}
			catch (Exception ex2)
			{
				Debug.LogException(ex2);
			}
			return list.OrderByDescending((BasicStat stat) => stat.Order).ToArray<BasicStat>();
		}

		// Token: 0x06001270 RID: 4720 RVA: 0x0005B5AC File Offset: 0x000597AC
		private static bool ProcessGlobalStat(GameEndBlock.AutoStat autoStat, out BasicStat newStat)
		{
			GameEndBlock.Stats stat = autoStat.Stat;
			switch (stat)
			{
			case GameEndBlock.Stats.None:
			case GameEndBlock.Stats.Kills:
			case GameEndBlock.Stats.Downs | GameEndBlock.Stats.Kills:
			case GameEndBlock.Stats.Downs | GameEndBlock.Stats.Revives:
			case GameEndBlock.Stats.Kills | GameEndBlock.Stats.Revives:
			case GameEndBlock.Stats.Downs | GameEndBlock.Stats.Kills | GameEndBlock.Stats.Revives:
			case GameEndBlock.Stats.TotalResourcesCollected:
				break;
			case GameEndBlock.Stats.Downs:
				newStat = new BasicStat
				{
					Identifier = "Global_Downs",
					Message = new LocalizedString("Player Downs: "),
					Order = autoStat.Order,
					Value = MonoBehaviourSingleton<StatTracker>.Instance.GetGlobalDowns().ToString()
				};
				return true;
			case GameEndBlock.Stats.Revives:
				newStat = new BasicStat
				{
					Identifier = "Global_Revives",
					Message = new LocalizedString("Player Revives: "),
					Order = autoStat.Order,
					Value = MonoBehaviourSingleton<StatTracker>.Instance.GetGlobalRevives().ToString()
				};
				return true;
			default:
				if (stat != GameEndBlock.Stats.EachResourceCollected && stat != GameEndBlock.Stats.Jumps)
				{
				}
				break;
			}
			newStat = null;
			return false;
		}

		// Token: 0x06001271 RID: 4721 RVA: 0x0005B694 File Offset: 0x00059894
		private static bool ProcessComparativeStat(GameEndBlock.AutoStat autoStat, bool most, out BasicStat basicStat)
		{
			GameEndBlock.<>c__DisplayClass39_0 CS$<>8__locals1;
			CS$<>8__locals1.most = most;
			basicStat = null;
			GameEndBlock.Stats stat = autoStat.Stat;
			switch (stat)
			{
			case GameEndBlock.Stats.None:
			case GameEndBlock.Stats.Kills:
			case GameEndBlock.Stats.Downs | GameEndBlock.Stats.Kills:
			case GameEndBlock.Stats.Downs | GameEndBlock.Stats.Revives:
			case GameEndBlock.Stats.Kills | GameEndBlock.Stats.Revives:
			case GameEndBlock.Stats.Downs | GameEndBlock.Stats.Kills | GameEndBlock.Stats.Revives:
			case GameEndBlock.Stats.TotalResourcesCollected:
				break;
			case GameEndBlock.Stats.Downs:
			{
				ComparativeStat comparativeStat = new ComparativeStat
				{
					Identifier = GameEndBlock.<ProcessComparativeStat>g__GetPrefix|39_0(ref CS$<>8__locals1) + "_Downs",
					Message = new LocalizedString(CS$<>8__locals1.most ? "%PLAYER% went down a whopping %VALUE% times!" : "%PLAYER% managed to only go down %VALUE% times."),
					DisplayFormat = NumericDisplayFormat.Int,
					Order = autoStat.Order,
					Comparison = (CS$<>8__locals1.most ? ComparativeStat.ValueComparison.Most : ComparativeStat.ValueComparison.Least)
				};
				comparativeStat.SetStats(MonoBehaviourSingleton<StatTracker>.Instance.GetDownsAsFloat());
				if (comparativeStat.IsValid)
				{
					basicStat = comparativeStat.ToBasicStat();
					return true;
				}
				return false;
			}
			case GameEndBlock.Stats.Revives:
			{
				ComparativeStat comparativeStat2 = new ComparativeStat
				{
					Identifier = GameEndBlock.<ProcessComparativeStat>g__GetPrefix|39_0(ref CS$<>8__locals1) + "_Revives",
					Message = new LocalizedString(CS$<>8__locals1.most ? "%PLAYER% revived others an impressive %VALUE% times!" : "%PLAYER% only revived others %VALUE% times."),
					DisplayFormat = NumericDisplayFormat.Int,
					Order = autoStat.Order,
					Comparison = (CS$<>8__locals1.most ? ComparativeStat.ValueComparison.Most : ComparativeStat.ValueComparison.Least)
				};
				comparativeStat2.SetStats(MonoBehaviourSingleton<StatTracker>.Instance.GetRevivesAsFloat());
				if (comparativeStat2.IsValid)
				{
					basicStat = comparativeStat2.ToBasicStat();
					return true;
				}
				return false;
			}
			default:
				if (stat != GameEndBlock.Stats.EachResourceCollected && stat != GameEndBlock.Stats.Jumps)
				{
				}
				break;
			}
			return false;
		}

		// Token: 0x06001272 RID: 4722 RVA: 0x0005B7FD File Offset: 0x000599FD
		public void SetNextLevel(Context instigator, LevelDestination nextLevel)
		{
			this.levelDestination = nextLevel;
		}

		// Token: 0x06001273 RID: 4723 RVA: 0x0005B808 File Offset: 0x00059A08
		public void EndlessStart()
		{
			if (base.IsServer && this.endGameOnAllPlayersDowned)
			{
				MonoBehaviourSingleton<PlayerManager>.Instance.OnNewPlayerRegistered.AddListener(new UnityAction<ulong, PlayerReferenceManager>(this.HandleNewPlayerJoined));
				PlayerReferenceManager[] playerObjects = MonoBehaviourSingleton<PlayerManager>.Instance.GetPlayerObjects();
				for (int i = 0; i < playerObjects.Length; i++)
				{
					playerObjects[i].PlayerDownedComponent.OnDowned.AddListener(new UnityAction(this.HandlePlayerDowned));
				}
			}
		}

		// Token: 0x06001274 RID: 4724 RVA: 0x0005B877 File Offset: 0x00059A77
		private void HandlePlayerDowned()
		{
			base.Invoke("CheckIfAllPlayersDead", 2.5f);
		}

		// Token: 0x06001275 RID: 4725 RVA: 0x0005B88C File Offset: 0x00059A8C
		private void CheckIfAllPlayersDead()
		{
			bool flag = true;
			PlayerReferenceManager[] playerObjects = MonoBehaviourSingleton<PlayerManager>.Instance.GetPlayerObjects();
			for (int i = 0; i < playerObjects.Length; i++)
			{
				if (playerObjects[i].HealthComponent.CurrentHealth > 0)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				this.EndGame(base.Context);
			}
		}

		// Token: 0x06001276 RID: 4726 RVA: 0x0005B8D7 File Offset: 0x00059AD7
		private void HandleNewPlayerJoined(ulong clientId, PlayerReferenceManager playerObject)
		{
			playerObject.PlayerDownedComponent.OnDowned.AddListener(new UnityAction(this.HandlePlayerDowned));
		}

		// Token: 0x06001277 RID: 4727 RVA: 0x0005B8F8 File Offset: 0x00059AF8
		public void EndlessGameEnd()
		{
			this.basicStatMap.Clear();
			this.perPlayerStatMap.Clear();
			this.comparativeStatMap.Clear();
			if (MonoBehaviourSingleton<PlayerManager>.Instance)
			{
				MonoBehaviourSingleton<PlayerManager>.Instance.OnNewPlayerRegistered.RemoveListener(new UnityAction<ulong, PlayerReferenceManager>(this.HandleNewPlayerJoined));
				PlayerReferenceManager[] playerObjects = MonoBehaviourSingleton<PlayerManager>.Instance.GetPlayerObjects();
				for (int i = 0; i < playerObjects.Length; i++)
				{
					playerObjects[i].PlayerDownedComponent.OnDowned.RemoveListener(new UnityAction(this.HandlePlayerDowned));
				}
			}
		}

		// Token: 0x170003AB RID: 939
		// (get) Token: 0x06001278 RID: 4728 RVA: 0x0005B984 File Offset: 0x00059B84
		public object LuaObject
		{
			get
			{
				GameEndBlock gameEndBlock;
				if ((gameEndBlock = this.luaInterface) == null)
				{
					gameEndBlock = (this.luaInterface = new GameEndBlock(this));
				}
				return gameEndBlock;
			}
		}

		// Token: 0x170003AC RID: 940
		// (get) Token: 0x06001279 RID: 4729 RVA: 0x0005B9AA File Offset: 0x00059BAA
		public Type LuaObjectType
		{
			get
			{
				return typeof(GameEndBlock);
			}
		}

		// Token: 0x0600127A RID: 4730 RVA: 0x0005B9B6 File Offset: 0x00059BB6
		public void ScriptInitialize(EndlessScriptComponent endlessScriptComponent)
		{
			this.scriptComponent = endlessScriptComponent;
		}

		// Token: 0x0600127C RID: 4732 RVA: 0x0005BA15 File Offset: 0x00059C15
		[CompilerGenerated]
		internal static string <ProcessComparativeStat>g__GetPrefix|39_0(ref GameEndBlock.<>c__DisplayClass39_0 A_0)
		{
			if (!A_0.most)
			{
				return "Least_";
			}
			return "Most_";
		}

		// Token: 0x04000FB9 RID: 4025
		private const float PLAYERS_DOWNED_DELAY = 2.5f;

		// Token: 0x04000FBC RID: 4028
		private List<GameEndBlock.AutoStat> autoStats = new List<GameEndBlock.AutoStat>();

		// Token: 0x04000FBD RID: 4029
		private GameEndBlock.Stats perPlayerStats;

		// Token: 0x04000FBE RID: 4030
		private bool displayCustomGameStats;

		// Token: 0x04000FBF RID: 4031
		private bool endGameOnAllPlayersDowned;

		// Token: 0x04000FC2 RID: 4034
		private LevelDestination levelDestination;

		// Token: 0x04000FC3 RID: 4035
		private Dictionary<string, BasicStat> basicStatMap = new Dictionary<string, BasicStat>();

		// Token: 0x04000FC4 RID: 4036
		private Dictionary<string, ComparativeStat> comparativeStatMap = new Dictionary<string, ComparativeStat>();

		// Token: 0x04000FC5 RID: 4037
		private Dictionary<string, PerPlayerStat> perPlayerStatMap = new Dictionary<string, PerPlayerStat>();

		// Token: 0x04000FC6 RID: 4038
		private GameEndBlock luaInterface;

		// Token: 0x04000FC7 RID: 4039
		internal EndlessScriptComponent scriptComponent;

		// Token: 0x02000319 RID: 793
		public enum StatType
		{
			// Token: 0x04000FC9 RID: 4041
			Global,
			// Token: 0x04000FCA RID: 4042
			Most,
			// Token: 0x04000FCB RID: 4043
			Least
		}

		// Token: 0x0200031A RID: 794
		public class AutoStat
		{
			// Token: 0x04000FCC RID: 4044
			public GameEndBlock.Stats Stat;

			// Token: 0x04000FCD RID: 4045
			public int Order;

			// Token: 0x04000FCE RID: 4046
			public GameEndBlock.StatType StatType;
		}

		// Token: 0x0200031B RID: 795
		[Flags]
		public enum Stats
		{
			// Token: 0x04000FD0 RID: 4048
			None = 0,
			// Token: 0x04000FD1 RID: 4049
			Downs = 1,
			// Token: 0x04000FD2 RID: 4050
			Kills = 2,
			// Token: 0x04000FD3 RID: 4051
			Revives = 4,
			// Token: 0x04000FD4 RID: 4052
			TotalResourcesCollected = 8,
			// Token: 0x04000FD5 RID: 4053
			EachResourceCollected = 16,
			// Token: 0x04000FD6 RID: 4054
			Jumps = 32
		}

		// Token: 0x0200031C RID: 796
		[Flags]
		public enum ComparativeStats
		{
			// Token: 0x04000FD8 RID: 4056
			None = 0,
			// Token: 0x04000FD9 RID: 4057
			MostDowns = 1,
			// Token: 0x04000FDA RID: 4058
			LeastDowns = 2,
			// Token: 0x04000FDB RID: 4059
			MostRevives = 4,
			// Token: 0x04000FDC RID: 4060
			LeastRevives = 8,
			// Token: 0x04000FDD RID: 4061
			MostResourcesCollected = 16,
			// Token: 0x04000FDE RID: 4062
			LeastResourcesCollected = 32
		}
	}
}
