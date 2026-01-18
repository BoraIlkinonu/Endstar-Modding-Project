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

namespace Endless.Gameplay
{
	// Token: 0x02000234 RID: 564
	public class NpcManager : EndlessBehaviourSingleton<NpcManager>, NetClock.ISimulateFrameLateSubscriber, IGameEndSubscriber
	{
		// Token: 0x17000226 RID: 550
		// (get) Token: 0x06000B9D RID: 2973 RVA: 0x0003FF1C File Offset: 0x0003E11C
		public Dictionary<uint, NpcEntity> NpcEntityMap { get; } = new Dictionary<uint, NpcEntity>();

		// Token: 0x17000227 RID: 551
		// (get) Token: 0x06000B9E RID: 2974 RVA: 0x0003FF24 File Offset: 0x0003E124
		public IReadOnlyList<NpcEntity> Npcs
		{
			get
			{
				return this.npcs.AsReadOnly();
			}
		}

		// Token: 0x17000228 RID: 552
		// (get) Token: 0x06000B9F RID: 2975 RVA: 0x0003FF31 File Offset: 0x0003E131
		// (set) Token: 0x06000BA0 RID: 2976 RVA: 0x0003FF39 File Offset: 0x0003E139
		public int TickOffsetRange
		{
			get
			{
				return this.tickOffsetRange;
			}
			private set
			{
				if (this.tickOffsetRange == value)
				{
					return;
				}
				this.tickOffsetRange = value;
				this.UpdateNpcTickOffset(this.tickOffsetRange);
			}
		}

		// Token: 0x06000BA1 RID: 2977 RVA: 0x0003FF58 File Offset: 0x0003E158
		protected override void Awake()
		{
			base.Awake();
			NetClock.Register(this);
		}

		// Token: 0x06000BA2 RID: 2978 RVA: 0x0003FF66 File Offset: 0x0003E166
		protected override void OnDestroy()
		{
			base.OnDestroy();
			NetClock.Unregister(this);
		}

		// Token: 0x06000BA3 RID: 2979 RVA: 0x0003FF74 File Offset: 0x0003E174
		private void UpdateNpcTickOffset(int offsetRange)
		{
			if (offsetRange == 1)
			{
				this.npcs.ForEach(delegate(NpcEntity entity)
				{
					entity.Components.GoapController.TickOffset = 0;
				});
				return;
			}
			int num = 0;
			for (int i = 0; i < this.npcs.Count; i++)
			{
				this.npcs[i].Components.GoapController.TickOffset = num++;
				if (num >= offsetRange)
				{
					num = 0;
				}
			}
		}

		// Token: 0x06000BA4 RID: 2980 RVA: 0x0003FFF0 File Offset: 0x0003E1F0
		public List<NpcEntity> GetNpcsInGroup(NpcGroup group)
		{
			List<NpcEntity> list;
			if (!this.npcsByGroup.TryGetValue(group, out list))
			{
				return new List<NpcEntity>();
			}
			return list;
		}

		// Token: 0x06000BA5 RID: 2981 RVA: 0x00040014 File Offset: 0x0003E214
		public int GetNumNpcsInGroup(NpcGroup group)
		{
			int num = 0;
			List<NpcEntity> list;
			if (this.npcsByGroup.TryGetValue(group, out list))
			{
				num = list.Count;
			}
			return num;
		}

		// Token: 0x06000BA6 RID: 2982 RVA: 0x0004003C File Offset: 0x0003E23C
		public Context GetNpcInGroupByIndex(NpcGroup group, int index)
		{
			List<NpcEntity> list;
			if (this.npcsByGroup.TryGetValue(group, out list) && index < list.Count)
			{
				return list[index].Context;
			}
			return null;
		}

		// Token: 0x06000BA7 RID: 2983 RVA: 0x00040070 File Offset: 0x0003E270
		public Context SpawnNpc(global::UnityEngine.Vector3 position, Quaternion rotation, NpcConfiguration config)
		{
			if (!EndlessCloudService.CanHaveRiflemen() && config.NpcClass.NpcClass == NpcClass.Rifleman)
			{
				ErrorHandler.HandleError(ErrorCodes.SpawnNpc_ContentRestricted_Rifleman, new Exception("Content Restricted: Account is not allowed to have riflemen."), true, true);
				return null;
			}
			if (!EndlessCloudService.CanHaveGrunt() && config.NpcClass.NpcClass == NpcClass.Grunt)
			{
				ErrorHandler.HandleError(ErrorCodes.SpawnNpc_ContentRestricted_Grunt, new Exception("Content Restricted: Account is not allowed to have grunts."), true, true);
				return null;
			}
			if (!EndlessCloudService.CanHaveZombies() && config.NpcClass.NpcClass == NpcClass.Zombie)
			{
				ErrorHandler.HandleError(ErrorCodes.SpawnNpc_ContentRestricted_Zombie, new Exception("Content Restricted: Account is not allowed to have zombies."), true, true);
				return null;
			}
			global::UnityEngine.Vector3? closestOpenPosition = PlacementManager.GetClosestOpenPosition(position, false, 0U);
			if (closestOpenPosition == null)
			{
				return null;
			}
			GameObject gameObject = global::UnityEngine.Object.Instantiate<EndlessProp>(MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary[this.npcGuid].EndlessProp, closestOpenPosition.Value, rotation).gameObject;
			WorldObject component = gameObject.GetComponent<WorldObject>();
			gameObject.GetComponent<NetworkObject>().Spawn(false);
			component.GetUserComponent<NpcEntity>().ConfigureSpawnedNpc_ClientRpc(new NetworkableNpcConfig(config), default(ClientRpcParams));
			this.dynamicSpawnNpcs.Add(gameObject);
			component.Context.InternalId = SerializableGuid.NewGuid();
			return component.Context;
		}

		// Token: 0x06000BA8 RID: 2984 RVA: 0x00040198 File Offset: 0x0003E398
		public Context SpawnNpcAtPosition(global::UnityEngine.Vector3 position, Quaternion rotation, NpcConfiguration config)
		{
			if (!EndlessCloudService.CanHaveRiflemen() && config.NpcClass.NpcClass == NpcClass.Rifleman)
			{
				ErrorHandler.HandleError(ErrorCodes.SpawnNpcAtPosition_ContentRestricted_Rifleman, new Exception("Content Restricted: Account is not allowed to have riflemen."), true, true);
				return null;
			}
			if (!EndlessCloudService.CanHaveGrunt() && config.NpcClass.NpcClass == NpcClass.Grunt)
			{
				ErrorHandler.HandleError(ErrorCodes.SpawnNpcAtPosition_ContentRestricted_Grunt, new Exception("Content Restricted: Account is not allowed to have grunts."), true, true);
				return null;
			}
			if (!EndlessCloudService.CanHaveZombies() && config.NpcClass.NpcClass == NpcClass.Zombie)
			{
				ErrorHandler.HandleError(ErrorCodes.SpawnNpcAtPosition_ContentRestricted_Zombie, new Exception("Content Restricted: Account is not allowed to have zombies."), true, true);
				return null;
			}
			GameObject gameObject = global::UnityEngine.Object.Instantiate<EndlessProp>(MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary[this.npcGuid].EndlessProp, position, rotation).gameObject;
			WorldObject component = gameObject.GetComponent<WorldObject>();
			gameObject.GetComponent<NetworkObject>().Spawn(false);
			component.GetUserComponent<NpcEntity>().ConfigureSpawnedNpc_ClientRpc(new NetworkableNpcConfig(config), default(ClientRpcParams));
			this.dynamicSpawnNpcs.Add(gameObject);
			return component.Context;
		}

		// Token: 0x06000BA9 RID: 2985 RVA: 0x00040291 File Offset: 0x0003E491
		public void RegisterNewNpc(NpcEntity npcEntity)
		{
			this.npcs.Add(npcEntity);
			this.AddNpcToGroupMap(npcEntity);
		}

		// Token: 0x06000BAA RID: 2986 RVA: 0x000402A8 File Offset: 0x0003E4A8
		private void AddNpcToGroupMap(NpcEntity npcEntity)
		{
			List<NpcEntity> list;
			if (this.npcsByGroup.TryGetValue(npcEntity.Group, out list))
			{
				if (!list.Contains(npcEntity))
				{
					list.Add(npcEntity);
					return;
				}
			}
			else
			{
				this.npcsByGroup.Add(npcEntity.Group, new List<NpcEntity> { npcEntity });
			}
		}

		// Token: 0x06000BAB RID: 2987 RVA: 0x000402F8 File Offset: 0x0003E4F8
		public void UpdateNpcGroup(NpcEntity entity, NpcGroup oldGroup)
		{
			List<NpcEntity> list;
			if (this.npcsByGroup.TryGetValue(oldGroup, out list))
			{
				list.Remove(entity);
			}
			this.AddNpcToGroupMap(entity);
		}

		// Token: 0x06000BAC RID: 2988 RVA: 0x00040324 File Offset: 0x0003E524
		public void RemoveNpc(NpcEntity npcEntity)
		{
			this.npcs.Remove(npcEntity);
			List<NpcEntity> list;
			if (this.npcsByGroup.TryGetValue(npcEntity.Group, out list))
			{
				list.Remove(npcEntity);
			}
		}

		// Token: 0x06000BAD RID: 2989 RVA: 0x0004035B File Offset: 0x0003E55B
		public void SimulateFrameLate(uint frame)
		{
			this.TickOffsetRange = Mathf.Clamp(this.npcs.Count / 5, 1, 10);
		}

		// Token: 0x06000BAE RID: 2990 RVA: 0x00040378 File Offset: 0x0003E578
		public void EndlessGameEnd()
		{
			foreach (GameObject gameObject in this.dynamicSpawnNpcs)
			{
				global::UnityEngine.Object.Destroy(gameObject);
			}
			this.dynamicSpawnNpcs.Clear();
			this.npcs.Clear();
			this.npcsByGroup.Clear();
		}

		// Token: 0x04000AE8 RID: 2792
		private readonly SerializableGuid npcGuid = new SerializableGuid("fb664eff-834f-4df3-ac38-0457c919354f");

		// Token: 0x04000AE9 RID: 2793
		[SerializeField]
		private LayerMask layerMask;

		// Token: 0x04000AEA RID: 2794
		private readonly HashSet<GameObject> dynamicSpawnNpcs = new HashSet<GameObject>();

		// Token: 0x04000AEB RID: 2795
		private const int MAX_TICK_OFFSETS = 10;

		// Token: 0x04000AEC RID: 2796
		private const int TARGET_NPCS_PER_TICK = 5;

		// Token: 0x04000AEE RID: 2798
		private readonly List<NpcEntity> npcs = new List<NpcEntity>();

		// Token: 0x04000AEF RID: 2799
		private readonly Dictionary<NpcGroup, List<NpcEntity>> npcsByGroup = new Dictionary<NpcGroup, List<NpcEntity>>();

		// Token: 0x04000AF0 RID: 2800
		private int tickOffsetRange = 1;
	}
}
