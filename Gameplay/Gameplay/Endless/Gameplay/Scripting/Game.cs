using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.Stats;
using Endless.Shared;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay.Scripting
{
	// Token: 0x02000495 RID: 1173
	public class Game
	{
		// Token: 0x170005A1 RID: 1441
		// (get) Token: 0x06001CBD RID: 7357 RVA: 0x0007E3A9 File Offset: 0x0007C5A9
		internal static Game Instance
		{
			get
			{
				if (Game.instance == null)
				{
					Game.instance = new Game();
				}
				return Game.instance;
			}
		}

		// Token: 0x06001CBE RID: 7358 RVA: 0x0007E3C4 File Offset: 0x0007C5C4
		public Game()
		{
			MonoBehaviourSingleton<PlayerManager>.Instance.OnNewPlayerRegistered.AddListener(new UnityAction<ulong, PlayerReferenceManager>(this.HandlePlayerRegistered));
			MonoBehaviourSingleton<PlayerManager>.Instance.PlayerUnregistered.AddListener(new UnityAction<ulong, PlayerReferenceManager>(this.HandlePlayerUnregistered));
		}

		// Token: 0x06001CBF RID: 7359 RVA: 0x0007E44F File Offset: 0x0007C64F
		private void HandlePlayerUnregistered(ulong _, PlayerReferenceManager playerReferenceManager)
		{
			if (playerReferenceManager.WorldObject.BaseType != null)
			{
				this.OnPlayerLeft.InvokeEvent(new object[] { playerReferenceManager.WorldObject.Context });
				this.HandlePlayerCountChanged();
			}
		}

		// Token: 0x06001CC0 RID: 7360 RVA: 0x0007E483 File Offset: 0x0007C683
		private void HandlePlayerRegistered(ulong _, PlayerReferenceManager playerReferenceManager)
		{
			if (playerReferenceManager.WorldObject.BaseType != null)
			{
				this.OnPlayerJoined.InvokeEvent(new object[] { playerReferenceManager.WorldObject.Context });
				this.HandlePlayerCountChanged();
			}
		}

		// Token: 0x06001CC1 RID: 7361 RVA: 0x0007E4B7 File Offset: 0x0007C6B7
		private void HandlePlayerCountChanged()
		{
			this.OnPlayerCountChanged.InvokeEvent(new object[]
			{
				Context.StaticGameContext,
				this.GetPlayerCount()
			});
		}

		// Token: 0x06001CC2 RID: 7362 RVA: 0x0004EBBD File Offset: 0x0004CDBD
		public Context GetGameContext()
		{
			return Context.StaticGameContext;
		}

		// Token: 0x06001CC3 RID: 7363 RVA: 0x0007DE3A File Offset: 0x0007C03A
		public Context GetCurrentLevelContext()
		{
			return Context.StaticLevelContext;
		}

		// Token: 0x06001CC4 RID: 7364 RVA: 0x0007E4E0 File Offset: 0x0007C6E0
		public int GetPlayerCount()
		{
			return MonoBehaviourSingleton<PlayerManager>.Instance.CurrentPlayerCount;
		}

		// Token: 0x06001CC5 RID: 7365 RVA: 0x0007E4EC File Offset: 0x0007C6EC
		public Context GetPlayerByIndex(int playerIndex)
		{
			PlayerReferenceManager[] playerObjects = MonoBehaviourSingleton<PlayerManager>.Instance.GetPlayerObjects();
			if (playerIndex < 0 || playerIndex > playerObjects.Length - 1)
			{
				return null;
			}
			return playerObjects[playerIndex].WorldObject.Context;
		}

		// Token: 0x06001CC6 RID: 7366 RVA: 0x0007E520 File Offset: 0x0007C720
		public Context GetPlayerBySlot(int playerSlot)
		{
			PlayerReferenceManager playerReferenceManager = MonoBehaviourSingleton<PlayerManager>.Instance.GetPlayerObjects().FirstOrDefault((PlayerReferenceManager player) => player.UserSlot == playerSlot);
			if (!(playerReferenceManager == null))
			{
				return playerReferenceManager.WorldObject.Context;
			}
			return null;
		}

		// Token: 0x06001CC7 RID: 7367 RVA: 0x0007E56C File Offset: 0x0007C76C
		public Context[] GetPlayers()
		{
			return (from player in MonoBehaviourSingleton<PlayerManager>.Instance.GetPlayerObjects()
				orderby player.UserSlot
				select player.WorldObject.Context).ToArray<Context>();
		}

		// Token: 0x06001CC8 RID: 7368 RVA: 0x0007E5D0 File Offset: 0x0007C7D0
		public string GetGameTitle()
		{
			return MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame.Name;
		}

		// Token: 0x06001CC9 RID: 7369 RVA: 0x0007E5E1 File Offset: 0x0007C7E1
		public string GetGameDescription()
		{
			return MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame.Description;
		}

		// Token: 0x06001CCA RID: 7370 RVA: 0x00015F40 File Offset: 0x00014140
		public string GetLevelName()
		{
			return MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.Name;
		}

		// Token: 0x06001CCB RID: 7371 RVA: 0x0007E5F2 File Offset: 0x0007C7F2
		public string GetLevelDescription()
		{
			return MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.Description;
		}

		// Token: 0x06001CCC RID: 7372 RVA: 0x0007E608 File Offset: 0x0007C808
		public int GetMinPlayerCount()
		{
			return MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame.MininumNumberOfPlayers;
		}

		// Token: 0x06001CCD RID: 7373 RVA: 0x0007E619 File Offset: 0x0007C819
		public int GetMaxPlayerCount()
		{
			return MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame.MaximumNumberOfPlayers;
		}

		// Token: 0x06001CCE RID: 7374 RVA: 0x0007E62A File Offset: 0x0007C82A
		public bool Teleport(Context instigator, Context teleportTarget, Vector3 position, float rotation, int teleportType)
		{
			return this.Teleport(instigator, teleportTarget, position, rotation, teleportType, true);
		}

		// Token: 0x06001CCF RID: 7375 RVA: 0x0007E63C File Offset: 0x0007C83C
		public bool Teleport(Context instigator, Context teleportTarget, Vector3 position, float rotation, int teleportType, bool snapPlayerCamera)
		{
			TeleportInfo teleportInfo = RuntimeDatabase.GetTeleportInfo((TeleportType)teleportType);
			Vector3? closestOpenPosition = PlacementManager.GetClosestOpenPosition(position.RoundToVector3Int(), true, teleportInfo.FramesToTeleport);
			if (closestOpenPosition == null)
			{
				return false;
			}
			position = closestOpenPosition.Value;
			PlayerLuaComponent playerLuaComponent;
			DraggablePhysicsCube draggablePhysicsCube;
			if (teleportTarget.WorldObject.TryGetUserComponent<PlayerLuaComponent>(out playerLuaComponent))
			{
				playerLuaComponent.references.PlayerController.TriggerTeleport(position - new Vector3(0f, 0.5f, 0f), rotation, (TeleportType)teleportType, snapPlayerCamera, instigator);
			}
			else if (teleportTarget.IsNpc())
			{
				teleportTarget.WorldObject.GetUserComponent<NpcEntity>().Teleport((TeleportType)teleportType, position, rotation);
			}
			else if (teleportTarget.WorldObject.TryGetUserComponent<DraggablePhysicsCube>(out draggablePhysicsCube))
			{
				draggablePhysicsCube.PhysicsCubeController.TriggerTeleport(position, (TeleportType)teleportType);
			}
			else
			{
				Transform child = teleportTarget.WorldObject.transform.GetChild(0);
				Item item = ((child != null) ? child.GetComponent<Item>() : null);
				if (!item)
				{
					return false;
				}
				item.TriggerTeleport(position, (TeleportType)teleportType);
			}
			return true;
		}

		// Token: 0x06001CD0 RID: 7376 RVA: 0x0007E72F File Offset: 0x0007C92F
		public bool Teleport(Context instigator, Context teleportTarget, Vector3 position, int teleportType)
		{
			return this.Teleport(instigator, teleportTarget, position, 0f, teleportType, true);
		}

		// Token: 0x06001CD1 RID: 7377 RVA: 0x0007E742 File Offset: 0x0007C942
		public bool Teleport(Context instigator, Context teleportTarget, CellReference cellReference, int teleportType)
		{
			return this.Teleport(instigator, teleportTarget, cellReference.GetCellPosition(), cellReference.Rotation.GetValueOrDefault(), teleportType, true);
		}

		// Token: 0x06001CD2 RID: 7378 RVA: 0x0007E760 File Offset: 0x0007C960
		public bool Teleport(Context instigator, Context teleportTarget, CellReference cellReference, int teleportType, bool snapPlayerCamera)
		{
			return this.Teleport(instigator, teleportTarget, cellReference.GetCellPosition(), cellReference.Rotation.GetValueOrDefault(), teleportType, snapPlayerCamera);
		}

		// Token: 0x06001CD3 RID: 7379 RVA: 0x0007E77F File Offset: 0x0007C97F
		public void RecordStat(Context instigator, BasicStat basicStat)
		{
			this.BasicStatMap[basicStat.Identifier] = basicStat;
		}

		// Token: 0x06001CD4 RID: 7380 RVA: 0x0007E793 File Offset: 0x0007C993
		public void RecordStat(Context instigator, PerPlayerStat perPlayerStat)
		{
			this.PerPlayerStatMap[perPlayerStat.Identifier] = perPlayerStat;
		}

		// Token: 0x06001CD5 RID: 7381 RVA: 0x0007E7A7 File Offset: 0x0007C9A7
		public void RecordStat(Context instigator, ComparativeStat comparativeStat)
		{
			this.ComparativeStatMap[comparativeStat.Identifier] = comparativeStat;
		}

		// Token: 0x06001CD6 RID: 7382 RVA: 0x0007E7BC File Offset: 0x0007C9BC
		public void LoadBasicStat(Context instigator, BasicStat stat)
		{
			BasicStat basicStat;
			if (this.BasicStatMap.TryGetValue(stat.Identifier, out basicStat))
			{
				stat.LoadFromString(basicStat.ToString());
			}
		}

		// Token: 0x06001CD7 RID: 7383 RVA: 0x0007E7EC File Offset: 0x0007C9EC
		public void LoadPerPlayerStat(Context instigator, PerPlayerStat stat)
		{
			PerPlayerStat perPlayerStat;
			if (this.PerPlayerStatMap.TryGetValue(stat.Identifier, out perPlayerStat))
			{
				stat.LoadFromString(perPlayerStat.ToString());
			}
		}

		// Token: 0x06001CD8 RID: 7384 RVA: 0x0007E81C File Offset: 0x0007CA1C
		public void LoadComparativeStat(Context instigator, ComparativeStat stat)
		{
			ComparativeStat comparativeStat;
			if (this.ComparativeStatMap.TryGetValue(stat.Identifier, out comparativeStat))
			{
				stat.LoadFromString(comparativeStat.ToString());
			}
		}

		// Token: 0x06001CD9 RID: 7385 RVA: 0x0006B81F File Offset: 0x00069A1F
		internal string ToJson()
		{
			return JsonConvert.SerializeObject(this);
		}

		// Token: 0x06001CDA RID: 7386 RVA: 0x0007E84A File Offset: 0x0007CA4A
		internal Game FromJson(string json)
		{
			return JsonConvert.DeserializeObject<Game>(json);
		}

		// Token: 0x040016B4 RID: 5812
		private static Game instance;

		// Token: 0x040016B5 RID: 5813
		public LuaInterfaceEvent OnPlayerCountChanged = new LuaInterfaceEvent();

		// Token: 0x040016B6 RID: 5814
		public LuaInterfaceEvent OnPlayerLeft = new LuaInterfaceEvent();

		// Token: 0x040016B7 RID: 5815
		public LuaInterfaceEvent OnPlayerJoined = new LuaInterfaceEvent();

		// Token: 0x040016B8 RID: 5816
		[JsonProperty]
		internal Dictionary<string, BasicStat> BasicStatMap = new Dictionary<string, BasicStat>();

		// Token: 0x040016B9 RID: 5817
		[JsonProperty]
		internal Dictionary<string, ComparativeStat> ComparativeStatMap = new Dictionary<string, ComparativeStat>();

		// Token: 0x040016BA RID: 5818
		[JsonProperty]
		internal Dictionary<string, PerPlayerStat> PerPlayerStatMap = new Dictionary<string, PerPlayerStat>();
	}
}
