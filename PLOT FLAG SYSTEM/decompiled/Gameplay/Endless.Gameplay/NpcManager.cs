using System;
using System.Collections.Generic;
using Endless.Data;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.Scripting;
using Endless.Matchmaking;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay;

public class NpcManager : EndlessBehaviourSingleton<NpcManager>, NetClock.ISimulateFrameLateSubscriber, IGameEndSubscriber
{
	private readonly SerializableGuid npcGuid = new SerializableGuid("fb664eff-834f-4df3-ac38-0457c919354f");

	[SerializeField]
	private LayerMask layerMask;

	private readonly HashSet<GameObject> dynamicSpawnNpcs = new HashSet<GameObject>();

	private const int MAX_TICK_OFFSETS = 10;

	private const int TARGET_NPCS_PER_TICK = 5;

	private readonly List<NpcEntity> npcs = new List<NpcEntity>();

	private readonly Dictionary<NpcGroup, List<NpcEntity>> npcsByGroup = new Dictionary<NpcGroup, List<NpcEntity>>();

	private int tickOffsetRange = 1;

	public Dictionary<uint, NpcEntity> NpcEntityMap { get; } = new Dictionary<uint, NpcEntity>();

	public IReadOnlyList<NpcEntity> Npcs => npcs.AsReadOnly();

	public int TickOffsetRange
	{
		get
		{
			return tickOffsetRange;
		}
		private set
		{
			if (tickOffsetRange != value)
			{
				tickOffsetRange = value;
				UpdateNpcTickOffset(tickOffsetRange);
			}
		}
	}

	protected override void Awake()
	{
		base.Awake();
		NetClock.Register(this);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		NetClock.Unregister(this);
	}

	private void UpdateNpcTickOffset(int offsetRange)
	{
		if (offsetRange == 1)
		{
			npcs.ForEach(delegate(NpcEntity entity)
			{
				entity.Components.GoapController.TickOffset = 0;
			});
			return;
		}
		int num = 0;
		for (int num2 = 0; num2 < npcs.Count; num2++)
		{
			npcs[num2].Components.GoapController.TickOffset = num++;
			if (num >= offsetRange)
			{
				num = 0;
			}
		}
	}

	public List<NpcEntity> GetNpcsInGroup(NpcGroup group)
	{
		if (!npcsByGroup.TryGetValue(group, out var value))
		{
			return new List<NpcEntity>();
		}
		return value;
	}

	public int GetNumNpcsInGroup(NpcGroup group)
	{
		int result = 0;
		if (npcsByGroup.TryGetValue(group, out var value))
		{
			result = value.Count;
		}
		return result;
	}

	public Context GetNpcInGroupByIndex(NpcGroup group, int index)
	{
		if (npcsByGroup.TryGetValue(group, out var value) && index < value.Count)
		{
			return value[index].Context;
		}
		return null;
	}

	public Context SpawnNpc(UnityEngine.Vector3 position, Quaternion rotation, NpcConfiguration config)
	{
		if (!EndlessCloudService.CanHaveRiflemen() && config.NpcClass.NpcClass == NpcClass.Rifleman)
		{
			ErrorHandler.HandleError(ErrorCodes.SpawnNpc_ContentRestricted_Rifleman, new Exception("Content Restricted: Account is not allowed to have riflemen."), displayModal: true, leaveMatch: true);
			return null;
		}
		if (!EndlessCloudService.CanHaveGrunt() && config.NpcClass.NpcClass == NpcClass.Grunt)
		{
			ErrorHandler.HandleError(ErrorCodes.SpawnNpc_ContentRestricted_Grunt, new Exception("Content Restricted: Account is not allowed to have grunts."), displayModal: true, leaveMatch: true);
			return null;
		}
		if (!EndlessCloudService.CanHaveZombies() && config.NpcClass.NpcClass == NpcClass.Zombie)
		{
			ErrorHandler.HandleError(ErrorCodes.SpawnNpc_ContentRestricted_Zombie, new Exception("Content Restricted: Account is not allowed to have zombies."), displayModal: true, leaveMatch: true);
			return null;
		}
		UnityEngine.Vector3? closestOpenPosition = PlacementManager.GetClosestOpenPosition(position);
		if (!closestOpenPosition.HasValue)
		{
			return null;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary[npcGuid].EndlessProp, closestOpenPosition.Value, rotation).gameObject;
		WorldObject component = gameObject.GetComponent<WorldObject>();
		gameObject.GetComponent<NetworkObject>().Spawn();
		component.GetUserComponent<NpcEntity>().ConfigureSpawnedNpc_ClientRpc(new NetworkableNpcConfig(config));
		dynamicSpawnNpcs.Add(gameObject);
		component.Context.InternalId = SerializableGuid.NewGuid();
		return component.Context;
	}

	public Context SpawnNpcAtPosition(UnityEngine.Vector3 position, Quaternion rotation, NpcConfiguration config)
	{
		if (!EndlessCloudService.CanHaveRiflemen() && config.NpcClass.NpcClass == NpcClass.Rifleman)
		{
			ErrorHandler.HandleError(ErrorCodes.SpawnNpcAtPosition_ContentRestricted_Rifleman, new Exception("Content Restricted: Account is not allowed to have riflemen."), displayModal: true, leaveMatch: true);
			return null;
		}
		if (!EndlessCloudService.CanHaveGrunt() && config.NpcClass.NpcClass == NpcClass.Grunt)
		{
			ErrorHandler.HandleError(ErrorCodes.SpawnNpcAtPosition_ContentRestricted_Grunt, new Exception("Content Restricted: Account is not allowed to have grunts."), displayModal: true, leaveMatch: true);
			return null;
		}
		if (!EndlessCloudService.CanHaveZombies() && config.NpcClass.NpcClass == NpcClass.Zombie)
		{
			ErrorHandler.HandleError(ErrorCodes.SpawnNpcAtPosition_ContentRestricted_Zombie, new Exception("Content Restricted: Account is not allowed to have zombies."), displayModal: true, leaveMatch: true);
			return null;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary[npcGuid].EndlessProp, position, rotation).gameObject;
		WorldObject component = gameObject.GetComponent<WorldObject>();
		gameObject.GetComponent<NetworkObject>().Spawn();
		component.GetUserComponent<NpcEntity>().ConfigureSpawnedNpc_ClientRpc(new NetworkableNpcConfig(config));
		dynamicSpawnNpcs.Add(gameObject);
		return component.Context;
	}

	public void RegisterNewNpc(NpcEntity npcEntity)
	{
		npcs.Add(npcEntity);
		AddNpcToGroupMap(npcEntity);
	}

	private void AddNpcToGroupMap(NpcEntity npcEntity)
	{
		if (npcsByGroup.TryGetValue(npcEntity.Group, out var value))
		{
			if (!value.Contains(npcEntity))
			{
				value.Add(npcEntity);
			}
		}
		else
		{
			npcsByGroup.Add(npcEntity.Group, new List<NpcEntity> { npcEntity });
		}
	}

	public void UpdateNpcGroup(NpcEntity entity, NpcGroup oldGroup)
	{
		if (npcsByGroup.TryGetValue(oldGroup, out var value))
		{
			value.Remove(entity);
		}
		AddNpcToGroupMap(entity);
	}

	public void RemoveNpc(NpcEntity npcEntity)
	{
		npcs.Remove(npcEntity);
		if (npcsByGroup.TryGetValue(npcEntity.Group, out var value))
		{
			value.Remove(npcEntity);
		}
	}

	public void SimulateFrameLate(uint frame)
	{
		TickOffsetRange = Mathf.Clamp(npcs.Count / 5, 1, 10);
	}

	public void EndlessGameEnd()
	{
		foreach (GameObject dynamicSpawnNpc in dynamicSpawnNpcs)
		{
			UnityEngine.Object.Destroy(dynamicSpawnNpc);
		}
		dynamicSpawnNpcs.Clear();
		npcs.Clear();
		npcsByGroup.Clear();
	}
}
