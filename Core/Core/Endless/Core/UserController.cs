using System;
using Endless.Creator;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.Screenshotting;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;

namespace Endless.Core
{
	// Token: 0x02000030 RID: 48
	public class UserController : NetworkBehaviour
	{
		// Token: 0x060000D4 RID: 212 RVA: 0x000064A4 File Offset: 0x000046A4
		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();
			Debug.Log(string.Format("{0} OnNetworkSpawn. OwnerClientId: {1}", "UserController", base.OwnerClientId));
			if (base.IsServer)
			{
				this.HandleStateEntered(NetworkBehaviourSingleton<GameStateManager>.Instance.CurrentState);
				NetworkBehaviourSingleton<GameStateManager>.Instance.OnGameStateChanged.AddListener(new UnityAction<GameState, GameState>(this.HandleGameStateChanged));
				MonoBehaviourSingleton<EndlessLoop>.Instance.OnCharacterSpawnRequested.AddListener(new UnityAction(this.SpawnGameplayCharacter));
			}
			if (base.IsClient)
			{
				MonoBehaviourSingleton<ScreenshotAPI>.Instance.OnToggleCharacterVisibilty.AddListener(new UnityAction<bool>(this.HandleScreenshotCharacterVisibilityToggle));
				if (base.IsOwner)
				{
					NetworkBehaviourSingleton<UserIdManager>.Instance.RegisterClientUser();
				}
			}
		}

		// Token: 0x060000D5 RID: 213 RVA: 0x0000655C File Offset: 0x0000475C
		public override void OnNetworkDespawn()
		{
			base.OnNetworkDespawn();
			if (base.IsServer)
			{
				if (NetworkBehaviourSingleton<GameStateManager>.Instance)
				{
					NetworkBehaviourSingleton<GameStateManager>.Instance.OnGameStateChanged.RemoveListener(new UnityAction<GameState, GameState>(this.HandleGameStateChanged));
				}
				if (this.currentCreatorCharacter)
				{
					global::UnityEngine.Object.Destroy(this.currentCreatorCharacter.gameObject);
				}
				if (this.currentGameplayCharacter)
				{
					global::UnityEngine.Object.Destroy(this.currentGameplayCharacter);
				}
				if (MonoBehaviourSingleton<EndlessLoop>.Instance)
				{
					MonoBehaviourSingleton<EndlessLoop>.Instance.OnCharacterSpawnRequested.RemoveListener(new UnityAction(this.SpawnGameplayCharacter));
				}
			}
			if (base.IsClient)
			{
				MonoBehaviourSingleton<ScreenshotAPI>.Instance.OnToggleCharacterVisibilty.RemoveListener(new UnityAction<bool>(this.HandleScreenshotCharacterVisibilityToggle));
			}
		}

		// Token: 0x060000D6 RID: 214 RVA: 0x00006620 File Offset: 0x00004820
		private void HandleScreenshotCharacterVisibilityToggle(bool hide)
		{
			if (PlayerReferenceManager.LocalInstance && PlayerReferenceManager.LocalInstance.ApperanceController)
			{
				this.ToggleRenderMeshes(PlayerReferenceManager.LocalInstance.ApperanceController.gameObject, !hide);
			}
		}

		// Token: 0x060000D7 RID: 215 RVA: 0x00006658 File Offset: 0x00004858
		private void ToggleRenderMeshes(GameObject targetObject, bool status)
		{
			MeshRenderer[] componentsInChildren = targetObject.GetComponentsInChildren<MeshRenderer>();
			SkinnedMeshRenderer[] componentsInChildren2 = targetObject.GetComponentsInChildren<SkinnedMeshRenderer>();
			ParticleSystem[] componentsInChildren3 = targetObject.GetComponentsInChildren<ParticleSystem>();
			MeshRenderer[] array = componentsInChildren;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enabled = status;
			}
			SkinnedMeshRenderer[] array2 = componentsInChildren2;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].enabled = status;
			}
			ParticleSystem[] array3 = componentsInChildren3;
			for (int i = 0; i < array3.Length; i++)
			{
				array3[i].gameObject.SetActive(status);
			}
			DecalProjector componentInChildren = targetObject.GetComponentInChildren<DecalProjector>();
			if (componentInChildren)
			{
				componentInChildren.enabled = status;
			}
		}

		// Token: 0x060000D8 RID: 216 RVA: 0x000066F4 File Offset: 0x000048F4
		public async void SpawnGameplayCharacter()
		{
			int num = await NetworkBehaviourSingleton<UserIdManager>.Instance.GetUserIdAsync(base.OwnerClientId);
			if (num != -1)
			{
				this.userSlot = MonoBehaviourSingleton<UserSlotManager>.Instance.GetUserSlot(num);
				Vector3 vector = Vector3.zero;
				Quaternion quaternion = Quaternion.identity;
				ISpawnPoint spawnPoint;
				int num2;
				this.GetSpawnPoint(out spawnPoint, out num2);
				if (spawnPoint != null)
				{
					Transform spawnPosition = spawnPoint.GetSpawnPosition(num2);
					vector = spawnPosition.position;
					quaternion = spawnPosition.rotation;
				}
				else
				{
					vector = UserController.GetCenterStageFallSpawnPoint();
				}
				if (!(this.currentGameplayCharacter != null))
				{
					this.currentGameplayCharacter = global::UnityEngine.Object.Instantiate<GameObject>(this.gameplayCharacterPrefab, vector, quaternion);
					PlayerReferenceManager component = this.currentGameplayCharacter.GetComponent<PlayerReferenceManager>();
					component.PlayerController.SetInitialState(vector, quaternion.eulerAngles.y);
					component.UserSlot = this.userSlot;
					this.currentGameplayCharacter.GetComponent<NetworkObject>().SpawnWithOwnership(base.OwnerClientId, false);
					if (spawnPoint != null)
					{
						spawnPoint.ConfigurePlayer(this.currentGameplayCharacter.GetComponent<GameplayPlayerReferenceManager>());
					}
					this.currentGameplayCharacter.GetComponent<PlayerReferenceManager>().LevelChangeTeleport_ClientRpc(vector, quaternion.eulerAngles.y);
				}
				else
				{
					this.currentGameplayCharacter.GetComponent<PlayerReferenceManager>().LevelChangeTeleport_ClientRpc(vector, quaternion.eulerAngles.y);
					if (spawnPoint != null)
					{
						spawnPoint.HandlePlayerEnteredLevel(this.currentGameplayCharacter.GetComponent<GameplayPlayerReferenceManager>());
					}
				}
			}
			else
			{
				Debug.LogWarning("Failed to spawn a gameplay character before user id came back. Could be expected if the match was abandoned quickly");
			}
		}

		// Token: 0x060000D9 RID: 217 RVA: 0x0000672C File Offset: 0x0000492C
		private static Vector3 GetCenterStageFallSpawnPoint()
		{
			Vector3 vector = (MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.MaximumExtents + MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.MinimumExtents) / 2f;
			vector.y = (float)(MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.MaximumExtents.y + 10);
			return vector;
		}

		// Token: 0x060000DA RID: 218 RVA: 0x00006794 File Offset: 0x00004994
		public async void SpawnCreatorCharacter()
		{
			int num = await NetworkBehaviourSingleton<UserIdManager>.Instance.GetUserIdAsync(base.OwnerClientId);
			if (num != -1)
			{
				this.userSlot = MonoBehaviourSingleton<UserSlotManager>.Instance.GetUserSlot(num);
				Vector3 vector = Vector3.zero;
				Quaternion quaternion = Quaternion.identity;
				ISpawnPoint spawnPoint;
				int num2;
				this.GetSpawnPoint(out spawnPoint, out num2);
				Vector3 vector2 = Vector3.zero - Vector3.up;
				if (spawnPoint != null)
				{
					Transform spawnPosition = spawnPoint.GetSpawnPosition(num2);
					vector = spawnPosition.position;
					quaternion = spawnPosition.rotation;
					vector2 = Stage.WorldSpacePointToGridCoordinate(vector + Vector3.up * 0.01f) + Vector3Int.down;
				}
				else
				{
					vector = UserController.GetCenterStageFallSpawnPoint();
				}
				bool flag = this.currentCreatorCharacter != null;
				if (!flag)
				{
					this.currentCreatorCharacter = global::UnityEngine.Object.Instantiate<CreatorPlayerReferenceManager>(this.creatorCharacterPrefab, vector, quaternion);
					this.currentCreatorCharacter.UserSlot = this.userSlot;
				}
				else
				{
					this.currentCreatorCharacter.LevelChangeTeleport_ClientRpc(vector, quaternion.eulerAngles.y);
				}
				Vector3Int vector3Int = Stage.WorldSpacePointToGridCoordinate(vector2);
				Cell cellFromCoordinate = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetCellFromCoordinate(vector3Int);
				bool flag2 = spawnPoint != null && cellFromCoordinate is TerrainCell;
				this.currentCreatorCharacter.PlayerNetworkController.DefaultGhostMode = !flag2;
				if (!flag)
				{
					this.currentCreatorCharacter.PlayerController.SetInitialState(vector, quaternion.eulerAngles.y);
					this.currentCreatorCharacter.GetComponent<NetworkObject>().SpawnWithOwnership(base.OwnerClientId, false);
					this.currentCreatorCharacter.LevelChangeTeleport_ClientRpc(vector, quaternion.eulerAngles.y);
				}
			}
			else
			{
				Debug.LogWarning("Failed to spawn a creator character before user id came back. Could be expected if the match was abandoned quickly");
			}
		}

		// Token: 0x060000DB RID: 219 RVA: 0x000067CB File Offset: 0x000049CB
		private void HandleGameStateChanged(GameState oldState, GameState newState)
		{
			this.HandleStateExited(oldState);
			this.HandleStateEntered(newState);
		}

		// Token: 0x060000DC RID: 220 RVA: 0x000067DC File Offset: 0x000049DC
		private void HandleStateEntered(GameState newState)
		{
			switch (newState)
			{
			case GameState.Default:
				this.DestroyCreatorCharacter();
				this.DestroyGameplayCharacter();
				return;
			case GameState.ValidatingLibrary:
			case GameState.Creator:
			case GameState.LoadedGameplay:
			case GameState.StartingGameplay:
			case GameState.Gameplay:
				break;
			case GameState.LoadingCreator:
				this.DestroyGameplayCharacter();
				return;
			case GameState.LoadingGameplay:
				this.DestroyCreatorCharacter();
				break;
			default:
				return;
			}
		}

		// Token: 0x060000DD RID: 221 RVA: 0x0000229D File Offset: 0x0000049D
		private void HandleStateExited(GameState oldState)
		{
		}

		// Token: 0x060000DE RID: 222 RVA: 0x0000682A File Offset: 0x00004A2A
		private void DestroyCreatorCharacter()
		{
			if (this.currentCreatorCharacter)
			{
				global::UnityEngine.Object.Destroy(this.currentCreatorCharacter.gameObject);
				this.currentCreatorCharacter = null;
			}
		}

		// Token: 0x060000DF RID: 223 RVA: 0x00006850 File Offset: 0x00004A50
		private void DestroyGameplayCharacter()
		{
			if (this.currentGameplayCharacter)
			{
				global::UnityEngine.Object.Destroy(this.currentGameplayCharacter);
				this.currentGameplayCharacter = null;
			}
		}

		// Token: 0x060000E0 RID: 224 RVA: 0x00006874 File Offset: 0x00004A74
		private void GetSpawnPoint(out ISpawnPoint spawnPoint, out int spawnPositionIndex)
		{
			SerializableGuid serializableGuid;
			MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.GetSpawnId(this.userSlot, MonoBehaviourSingleton<GameplayManager>.Instance.AvailableSpawnPoints, out serializableGuid, out spawnPositionIndex);
			if (serializableGuid == SerializableGuid.Empty)
			{
				Debug.LogWarning("Failed to find spawn point. Using fallback behavior..");
				spawnPoint = null;
				return;
			}
			GameObject gameObjectFromInstanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetGameObjectFromInstanceId(serializableGuid);
			if (gameObjectFromInstanceId != null)
			{
				spawnPoint = gameObjectFromInstanceId.GetComponentInChildren<ISpawnPoint>();
				return;
			}
			spawnPoint = null;
		}

		// Token: 0x060000E2 RID: 226 RVA: 0x000068F4 File Offset: 0x00004AF4
		protected override void __initializeVariables()
		{
			base.__initializeVariables();
		}

		// Token: 0x060000E3 RID: 227 RVA: 0x0000690A File Offset: 0x00004B0A
		protected override void __initializeRpcs()
		{
			base.__initializeRpcs();
		}

		// Token: 0x060000E4 RID: 228 RVA: 0x00006914 File Offset: 0x00004B14
		protected internal override string __getTypeName()
		{
			return "UserController";
		}

		// Token: 0x04000085 RID: 133
		[SerializeField]
		private GameObject gameplayCharacterPrefab;

		// Token: 0x04000086 RID: 134
		[SerializeField]
		private CreatorPlayerReferenceManager creatorCharacterPrefab;

		// Token: 0x04000087 RID: 135
		private GameObject currentGameplayCharacter;

		// Token: 0x04000088 RID: 136
		private CreatorPlayerReferenceManager currentCreatorCharacter;

		// Token: 0x04000089 RID: 137
		private int userSlot;
	}
}
