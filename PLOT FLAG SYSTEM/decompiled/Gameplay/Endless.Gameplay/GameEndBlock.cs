using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Gameplay.LuaInterfaces;
using Endless.Gameplay.Scripting;
using Endless.Gameplay.Stats;
using Endless.Shared;
using UnityEngine;

namespace Endless.Gameplay;

public class GameEndBlock : AbstractBlock, IStartSubscriber, IGameEndSubscriber, IScriptInjector
{
	public enum StatType
	{
		Global,
		Most,
		Least
	}

	public class AutoStat
	{
		public Stats Stat;

		public int Order;

		public StatType StatType;
	}

	[Flags]
	public enum Stats
	{
		None = 0,
		Downs = 1,
		Kills = 2,
		Revives = 4,
		TotalResourcesCollected = 8,
		EachResourceCollected = 0x10,
		Jumps = 0x20
	}

	[Flags]
	public enum ComparativeStats
	{
		None = 0,
		MostDowns = 1,
		LeastDowns = 2,
		MostRevives = 4,
		LeastRevives = 8,
		MostResourcesCollected = 0x10,
		LeastResourcesCollected = 0x20
	}

	private const float PLAYERS_DOWNED_DELAY = 2.5f;

	private List<AutoStat> autoStats = new List<AutoStat>();

	private Stats perPlayerStats;

	private bool displayCustomGameStats;

	private bool endGameOnAllPlayersDowned;

	private LevelDestination levelDestination;

	private Dictionary<string, BasicStat> basicStatMap = new Dictionary<string, BasicStat>();

	private Dictionary<string, ComparativeStat> comparativeStatMap = new Dictionary<string, ComparativeStat>();

	private Dictionary<string, PerPlayerStat> perPlayerStatMap = new Dictionary<string, PerPlayerStat>();

	private Endless.Gameplay.LuaInterfaces.GameEndBlock luaInterface;

	internal EndlessScriptComponent scriptComponent;

	public string Title { get; private set; } = "Game Over";

	public string Description { get; private set; } = "The game is over, what's next?";

	public bool ShowReplayButton { get; private set; }

	public bool ShowEndMatchButton { get; private set; }

	public bool ShowNextLevelButton
	{
		get
		{
			if (levelDestination != null)
			{
				return levelDestination.IsValidLevel();
			}
			return false;
		}
	}

	public object LuaObject => luaInterface ?? (luaInterface = new Endless.Gameplay.LuaInterfaces.GameEndBlock(this));

	public Type LuaObjectType => typeof(Endless.Gameplay.LuaInterfaces.GameEndBlock);

	public void RecordBasicStat(Context instigator, BasicStat basicStat)
	{
		if (string.IsNullOrEmpty(basicStat.Identifier))
		{
			ArgumentException ex = new ArgumentException("A stat is required to have an identifier. It should be a unique string of data to avoid overwriting other stats.");
			scriptComponent.LogException(ex);
			throw ex;
		}
		basicStatMap[basicStat.Identifier] = basicStat;
	}

	public void RecordPerPlayerStat(Context instigator, PerPlayerStat perPlayerStat)
	{
		perPlayerStatMap[perPlayerStat.Identifier] = perPlayerStat;
	}

	public void RecordComparativeStat(Context instigator, ComparativeStat comparativeStat)
	{
		comparativeStatMap[comparativeStat.Identifier] = comparativeStat;
	}

	public void EndGame(Context instigator)
	{
		try
		{
			scriptComponent.TryExecuteFunction("OnGameEndTriggered", out var _, instigator);
			NetworkBehaviourSingleton<GameEndManager>.Instance.TriggerGameEndScreen(this);
		}
		catch (Exception exception)
		{
			scriptComponent.LogException(exception, "EndGame");
		}
	}

	public void TriggerNextLevel()
	{
		levelDestination.ChangeLevel(Game.Instance.GetGameContext());
	}

	internal PerPlayerStat[] GatherPerPlayerStats()
	{
		List<PerPlayerStat> list = new List<PerPlayerStat>();
		if (perPlayerStats.HasFlag(Stats.Downs))
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
		if (perPlayerStats.HasFlag(Stats.Kills))
		{
			PerPlayerStat item = new PerPlayerStat
			{
				Identifier = "PerPlayer_Kills",
				Message = new LocalizedString("Kills:"),
				Order = 100,
				DefaultValue = "0"
			};
			list.Add(item);
		}
		if (perPlayerStats.HasFlag(Stats.Revives))
		{
			PerPlayerStat perPlayerStat2 = new PerPlayerStat
			{
				Identifier = "PerPlayer_Revives",
				Message = new LocalizedString("Revives:"),
				Order = 100,
				DefaultValue = "0"
			};
			perPlayerStat2.SetStats(MonoBehaviourSingleton<StatTracker>.Instance.GetRevivesAsFloat());
			list.Add(perPlayerStat2);
		}
		if (perPlayerStats.HasFlag(Stats.TotalResourcesCollected) && perPlayerStats.HasFlag(Stats.Revives))
		{
			PerPlayerStat item2 = new PerPlayerStat
			{
				Identifier = "PerPlayer_Resources",
				Message = new LocalizedString("Resources:"),
				Order = 100,
				DefaultValue = "0"
			};
			list.Add(item2);
		}
		perPlayerStats.HasFlag(Stats.EachResourceCollected);
		if (displayCustomGameStats)
		{
			list.AddRange(Game.Instance.PerPlayerStatMap.Values);
		}
		list.AddRange(perPlayerStatMap.Values);
		return list.OrderByDescending((PerPlayerStat stat) => stat.Order).ToArray();
	}

	internal BasicStat[] GatherGlobalStats()
	{
		List<BasicStat> list = new List<BasicStat>();
		foreach (AutoStat autoStat in autoStats)
		{
			try
			{
				switch (autoStat.StatType)
				{
				case StatType.Global:
				{
					if (ProcessGlobalStat(autoStat, out var newStat))
					{
						list.Add(newStat);
					}
					break;
				}
				case StatType.Most:
				{
					if (ProcessComparativeStat(autoStat, most: true, out var basicStat2))
					{
						list.Add(basicStat2);
					}
					break;
				}
				case StatType.Least:
				{
					if (ProcessComparativeStat(autoStat, most: false, out var basicStat))
					{
						list.Add(basicStat);
					}
					break;
				}
				default:
					throw new ArgumentOutOfRangeException("Invalid stat type!");
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		try
		{
			if (displayCustomGameStats)
			{
				list.AddRange(Game.Instance.BasicStatMap.Values);
				list.AddRange(Game.Instance.ComparativeStatMap.Values.Select((ComparativeStat stat) => stat.ToBasicStat()));
			}
			list.AddRange(basicStatMap.Values);
			list.AddRange(comparativeStatMap.Values.Select((ComparativeStat comperative) => comperative.ToBasicStat()));
		}
		catch (Exception exception2)
		{
			Debug.LogException(exception2);
		}
		return list.OrderByDescending((BasicStat stat) => stat.Order).ToArray();
	}

	private static bool ProcessGlobalStat(AutoStat autoStat, out BasicStat newStat)
	{
		switch (autoStat.Stat)
		{
		case Stats.Downs:
			newStat = new BasicStat
			{
				Identifier = "Global_Downs",
				Message = new LocalizedString("Player Downs: "),
				Order = autoStat.Order,
				Value = MonoBehaviourSingleton<StatTracker>.Instance.GetGlobalDowns().ToString()
			};
			return true;
		case Stats.Revives:
			newStat = new BasicStat
			{
				Identifier = "Global_Revives",
				Message = new LocalizedString("Player Revives: "),
				Order = autoStat.Order,
				Value = MonoBehaviourSingleton<StatTracker>.Instance.GetGlobalRevives().ToString()
			};
			return true;
		default:
			newStat = null;
			return false;
		}
	}

	private static bool ProcessComparativeStat(AutoStat autoStat, bool most, out BasicStat basicStat)
	{
		basicStat = null;
		switch (autoStat.Stat)
		{
		case Stats.Downs:
		{
			ComparativeStat comparativeStat2 = new ComparativeStat
			{
				Identifier = GetPrefix() + "_Downs",
				Message = new LocalizedString(most ? "%PLAYER% went down a whopping %VALUE% times!" : "%PLAYER% managed to only go down %VALUE% times."),
				DisplayFormat = NumericDisplayFormat.Int,
				Order = autoStat.Order,
				Comparison = ((!most) ? ComparativeStat.ValueComparison.Least : ComparativeStat.ValueComparison.Most)
			};
			comparativeStat2.SetStats(MonoBehaviourSingleton<StatTracker>.Instance.GetDownsAsFloat());
			if (comparativeStat2.IsValid)
			{
				basicStat = comparativeStat2.ToBasicStat();
				return true;
			}
			return false;
		}
		case Stats.Revives:
		{
			ComparativeStat comparativeStat = new ComparativeStat
			{
				Identifier = GetPrefix() + "_Revives",
				Message = new LocalizedString(most ? "%PLAYER% revived others an impressive %VALUE% times!" : "%PLAYER% only revived others %VALUE% times."),
				DisplayFormat = NumericDisplayFormat.Int,
				Order = autoStat.Order,
				Comparison = ((!most) ? ComparativeStat.ValueComparison.Least : ComparativeStat.ValueComparison.Most)
			};
			comparativeStat.SetStats(MonoBehaviourSingleton<StatTracker>.Instance.GetRevivesAsFloat());
			if (comparativeStat.IsValid)
			{
				basicStat = comparativeStat.ToBasicStat();
				return true;
			}
			return false;
		}
		default:
			return false;
		}
		string GetPrefix()
		{
			if (!most)
			{
				return "Least_";
			}
			return "Most_";
		}
	}

	public void SetNextLevel(Context instigator, LevelDestination nextLevel)
	{
		levelDestination = nextLevel;
	}

	public void EndlessStart()
	{
		if (base.IsServer && endGameOnAllPlayersDowned)
		{
			MonoBehaviourSingleton<PlayerManager>.Instance.OnNewPlayerRegistered.AddListener(HandleNewPlayerJoined);
			PlayerReferenceManager[] playerObjects = MonoBehaviourSingleton<PlayerManager>.Instance.GetPlayerObjects();
			for (int i = 0; i < playerObjects.Length; i++)
			{
				playerObjects[i].PlayerDownedComponent.OnDowned.AddListener(HandlePlayerDowned);
			}
		}
	}

	private void HandlePlayerDowned()
	{
		Invoke("CheckIfAllPlayersDead", 2.5f);
	}

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
			EndGame(base.Context);
		}
	}

	private void HandleNewPlayerJoined(ulong clientId, PlayerReferenceManager playerObject)
	{
		playerObject.PlayerDownedComponent.OnDowned.AddListener(HandlePlayerDowned);
	}

	public void EndlessGameEnd()
	{
		basicStatMap.Clear();
		perPlayerStatMap.Clear();
		comparativeStatMap.Clear();
		if ((bool)MonoBehaviourSingleton<PlayerManager>.Instance)
		{
			MonoBehaviourSingleton<PlayerManager>.Instance.OnNewPlayerRegistered.RemoveListener(HandleNewPlayerJoined);
			PlayerReferenceManager[] playerObjects = MonoBehaviourSingleton<PlayerManager>.Instance.GetPlayerObjects();
			for (int i = 0; i < playerObjects.Length; i++)
			{
				playerObjects[i].PlayerDownedComponent.OnDowned.RemoveListener(HandlePlayerDowned);
			}
		}
	}

	public void ScriptInitialize(EndlessScriptComponent endlessScriptComponent)
	{
		scriptComponent = endlessScriptComponent;
	}
}
