using System.Collections.Generic;
using System.Linq;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.Stats;
using Endless.Shared;
using Newtonsoft.Json;
using UnityEngine;

namespace Endless.Gameplay.Scripting;

public class Game
{
	private static Game instance;

	public LuaInterfaceEvent OnPlayerCountChanged = new LuaInterfaceEvent();

	public LuaInterfaceEvent OnPlayerLeft = new LuaInterfaceEvent();

	public LuaInterfaceEvent OnPlayerJoined = new LuaInterfaceEvent();

	[JsonProperty]
	internal Dictionary<string, BasicStat> BasicStatMap = new Dictionary<string, BasicStat>();

	[JsonProperty]
	internal Dictionary<string, ComparativeStat> ComparativeStatMap = new Dictionary<string, ComparativeStat>();

	[JsonProperty]
	internal Dictionary<string, PerPlayerStat> PerPlayerStatMap = new Dictionary<string, PerPlayerStat>();

	internal static Game Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new Game();
			}
			return instance;
		}
	}

	public Game()
	{
		MonoBehaviourSingleton<PlayerManager>.Instance.OnNewPlayerRegistered.AddListener(HandlePlayerRegistered);
		MonoBehaviourSingleton<PlayerManager>.Instance.PlayerUnregistered.AddListener(HandlePlayerUnregistered);
	}

	private void HandlePlayerUnregistered(ulong _, PlayerReferenceManager playerReferenceManager)
	{
		if (playerReferenceManager.WorldObject.BaseType != null)
		{
			OnPlayerLeft.InvokeEvent(playerReferenceManager.WorldObject.Context);
			HandlePlayerCountChanged();
		}
	}

	private void HandlePlayerRegistered(ulong _, PlayerReferenceManager playerReferenceManager)
	{
		if (playerReferenceManager.WorldObject.BaseType != null)
		{
			OnPlayerJoined.InvokeEvent(playerReferenceManager.WorldObject.Context);
			HandlePlayerCountChanged();
		}
	}

	private void HandlePlayerCountChanged()
	{
		OnPlayerCountChanged.InvokeEvent(Context.StaticGameContext, GetPlayerCount());
	}

	public Context GetGameContext()
	{
		return Context.StaticGameContext;
	}

	public Context GetCurrentLevelContext()
	{
		return Context.StaticLevelContext;
	}

	public int GetPlayerCount()
	{
		return MonoBehaviourSingleton<PlayerManager>.Instance.CurrentPlayerCount;
	}

	public Context GetPlayerByIndex(int playerIndex)
	{
		PlayerReferenceManager[] playerObjects = MonoBehaviourSingleton<PlayerManager>.Instance.GetPlayerObjects();
		if (playerIndex < 0 || playerIndex > playerObjects.Length - 1)
		{
			return null;
		}
		return playerObjects[playerIndex].WorldObject.Context;
	}

	public Context GetPlayerBySlot(int playerSlot)
	{
		PlayerReferenceManager playerReferenceManager = MonoBehaviourSingleton<PlayerManager>.Instance.GetPlayerObjects().FirstOrDefault((PlayerReferenceManager player) => player.UserSlot == playerSlot);
		if (!(playerReferenceManager == null))
		{
			return playerReferenceManager.WorldObject.Context;
		}
		return null;
	}

	public Context[] GetPlayers()
	{
		return (from player in MonoBehaviourSingleton<PlayerManager>.Instance.GetPlayerObjects()
			orderby player.UserSlot
			select player.WorldObject.Context).ToArray();
	}

	public string GetGameTitle()
	{
		return MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame.Name;
	}

	public string GetGameDescription()
	{
		return MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame.Description;
	}

	public string GetLevelName()
	{
		return MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.Name;
	}

	public string GetLevelDescription()
	{
		return MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.Description;
	}

	public int GetMinPlayerCount()
	{
		return MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame.MininumNumberOfPlayers;
	}

	public int GetMaxPlayerCount()
	{
		return MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame.MaximumNumberOfPlayers;
	}

	public bool Teleport(Context instigator, Context teleportTarget, UnityEngine.Vector3 position, float rotation, int teleportType)
	{
		return Teleport(instigator, teleportTarget, position, rotation, teleportType, snapPlayerCamera: true);
	}

	public bool Teleport(Context instigator, Context teleportTarget, UnityEngine.Vector3 position, float rotation, int teleportType, bool snapPlayerCamera)
	{
		TeleportInfo teleportInfo = RuntimeDatabase.GetTeleportInfo((TeleportType)teleportType);
		UnityEngine.Vector3? closestOpenPosition = PlacementManager.GetClosestOpenPosition(position.RoundToVector3Int(), claimPosition: true, teleportInfo.FramesToTeleport);
		if (!closestOpenPosition.HasValue)
		{
			return false;
		}
		position = closestOpenPosition.Value;
		DraggablePhysicsCube component2;
		if (teleportTarget.WorldObject.TryGetUserComponent<PlayerLuaComponent>(out var component))
		{
			component.references.PlayerController.TriggerTeleport(position - new UnityEngine.Vector3(0f, 0.5f, 0f), rotation, (TeleportType)teleportType, snapPlayerCamera, instigator);
		}
		else if (teleportTarget.IsNpc())
		{
			teleportTarget.WorldObject.GetUserComponent<NpcEntity>().Teleport((TeleportType)teleportType, position, rotation);
		}
		else if (teleportTarget.WorldObject.TryGetUserComponent<DraggablePhysicsCube>(out component2))
		{
			component2.PhysicsCubeController.TriggerTeleport(position, (TeleportType)teleportType);
		}
		else
		{
			Item item = teleportTarget.WorldObject.transform.GetChild(0)?.GetComponent<Item>();
			if (!item)
			{
				return false;
			}
			item.TriggerTeleport(position, (TeleportType)teleportType);
		}
		return true;
	}

	public bool Teleport(Context instigator, Context teleportTarget, UnityEngine.Vector3 position, int teleportType)
	{
		return Teleport(instigator, teleportTarget, position, 0f, teleportType, snapPlayerCamera: true);
	}

	public bool Teleport(Context instigator, Context teleportTarget, CellReference cellReference, int teleportType)
	{
		return Teleport(instigator, teleportTarget, cellReference.GetCellPosition(), cellReference.Rotation.GetValueOrDefault(), teleportType, snapPlayerCamera: true);
	}

	public bool Teleport(Context instigator, Context teleportTarget, CellReference cellReference, int teleportType, bool snapPlayerCamera)
	{
		return Teleport(instigator, teleportTarget, cellReference.GetCellPosition(), cellReference.Rotation.GetValueOrDefault(), teleportType, snapPlayerCamera);
	}

	public void RecordStat(Context instigator, BasicStat basicStat)
	{
		BasicStatMap[basicStat.Identifier] = basicStat;
	}

	public void RecordStat(Context instigator, PerPlayerStat perPlayerStat)
	{
		PerPlayerStatMap[perPlayerStat.Identifier] = perPlayerStat;
	}

	public void RecordStat(Context instigator, ComparativeStat comparativeStat)
	{
		ComparativeStatMap[comparativeStat.Identifier] = comparativeStat;
	}

	public void LoadBasicStat(Context instigator, BasicStat stat)
	{
		if (BasicStatMap.TryGetValue(stat.Identifier, out var value))
		{
			stat.LoadFromString(value.ToString());
		}
	}

	public void LoadPerPlayerStat(Context instigator, PerPlayerStat stat)
	{
		if (PerPlayerStatMap.TryGetValue(stat.Identifier, out var value))
		{
			stat.LoadFromString(value.ToString());
		}
	}

	public void LoadComparativeStat(Context instigator, ComparativeStat stat)
	{
		if (ComparativeStatMap.TryGetValue(stat.Identifier, out var value))
		{
			stat.LoadFromString(value.ToString());
		}
	}

	internal string ToJson()
	{
		return JsonConvert.SerializeObject(this);
	}

	internal Game FromJson(string json)
	{
		return JsonConvert.DeserializeObject<Game>(json);
	}
}
