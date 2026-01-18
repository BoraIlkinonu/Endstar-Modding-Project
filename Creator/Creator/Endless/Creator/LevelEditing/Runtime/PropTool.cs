using System;
using System.Runtime.CompilerServices;
using Endless.Assets;
using Endless.Creator.UI;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.Scripting;
using Endless.Props.Assets;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Endless.Creator.LevelEditing.Runtime
{
	// Token: 0x02000381 RID: 897
	public class PropTool : PropBasedTool
	{
		// Token: 0x17000291 RID: 657
		// (get) Token: 0x0600114E RID: 4430 RVA: 0x00029A30 File Offset: 0x00027C30
		public override ToolType ToolType
		{
			get
			{
				return ToolType.Prop;
			}
		}

		// Token: 0x17000292 RID: 658
		// (get) Token: 0x0600114F RID: 4431 RVA: 0x00053FCE File Offset: 0x000521CE
		public SerializableGuid SelectedAssetId
		{
			get
			{
				return base.ActiveAssetId;
			}
		}

		// Token: 0x17000293 RID: 659
		// (get) Token: 0x06001150 RID: 4432 RVA: 0x00053FD6 File Offset: 0x000521D6
		public SerializableGuid PreviousSelectedAssetId
		{
			get
			{
				return base.PreviousAssetId;
			}
		}

		// Token: 0x06001151 RID: 4433 RVA: 0x00053FE0 File Offset: 0x000521E0
		public override void HandleSelected()
		{
			base.HandleSelected();
			PropLibrary.RuntimePropInfo runtimePropInfo;
			if (MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(this.SelectedAssetId, out runtimePropInfo))
			{
				base.GeneratePropPreview(runtimePropInfo, default(SerializableGuid));
			}
			else
			{
				base.ClearActiveAssetId();
				UnityEvent<SerializableGuid> onSelectedAssetChanged = this.OnSelectedAssetChanged;
				if (onSelectedAssetChanged != null)
				{
					onSelectedAssetChanged.Invoke(SerializableGuid.Empty);
				}
			}
			base.Set3DCursorUsesIntersection(false);
			base.UIToolPrompter.Hide();
		}

		// Token: 0x06001152 RID: 4434 RVA: 0x0005404C File Offset: 0x0005224C
		public override void HandleDeselected()
		{
			base.HandleDeselected();
			base.Set3DCursorUsesIntersection(false);
			if (this.scriptWindow)
			{
				this.scriptWindow.Close();
			}
		}

		// Token: 0x06001153 RID: 4435 RVA: 0x00054074 File Offset: 0x00052274
		public override void ToolPressed()
		{
			if (EndlessInput.GetKey(global::UnityEngine.InputSystem.Key.LeftAlt))
			{
				if (base.PropGhostTransform == null)
				{
					LineCastHit activeLineCastResult = base.ActiveLineCastResult;
					if (activeLineCastResult.IntersectionOccured)
					{
						PropCell cellFromCoordinateAs = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetCellFromCoordinateAs<PropCell>(activeLineCastResult.IntersectedObjectPosition);
						if (cellFromCoordinateAs != null)
						{
							SerializableGuid assetIdFromInstanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetAssetIdFromInstanceId(cellFromCoordinateAs.InstanceId);
							PropLibrary.RuntimePropInfo runtimePropInfo;
							if (MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(assetIdFromInstanceId, out runtimePropInfo))
							{
								base.GeneratePropPreview(runtimePropInfo, default(SerializableGuid));
								return;
							}
						}
					}
				}
			}
			else
			{
				base.ToolPressed();
			}
		}

		// Token: 0x06001154 RID: 4436 RVA: 0x00054104 File Offset: 0x00052304
		public override void ToolReleased()
		{
			if (base.IsLoadingProp || !base.ToolIsPressed)
			{
				return;
			}
			base.ToolReleased();
			if (base.PropGhostTransform && base.NoInvalidOverlapsExist())
			{
				this.AttemptPlaceProp_ServerRPC(base.PropGhostTransform.position, base.PropGhostTransform.rotation.eulerAngles, base.ActiveAssetId, default(ServerRpcParams));
			}
		}

		// Token: 0x06001155 RID: 4437 RVA: 0x00054170 File Offset: 0x00052370
		[ServerRpc(RequireOwnership = false)]
		public void AttemptPlaceProp_ServerRPC(global::UnityEngine.Vector3 position, global::UnityEngine.Vector3 eulerRotation, SerializableGuid assetId, ServerRpcParams serverRpcParams = default(ServerRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendServerRpc(2880201906U, serverRpcParams, RpcDelivery.Reliable);
				fastBufferWriter.WriteValueSafe(in position);
				fastBufferWriter.WriteValueSafe(in eulerRotation);
				fastBufferWriter.WriteValueSafe<SerializableGuid>(in assetId, default(FastBufferWriter.ForNetworkSerializable));
				base.__endSendServerRpc(ref fastBufferWriter, 2880201906U, serverRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			this.AttemptPlaceProp(position, eulerRotation, assetId, serverRpcParams);
		}

		// Token: 0x06001156 RID: 4438 RVA: 0x00054284 File Offset: 0x00052484
		private async void AttemptPlaceProp(global::UnityEngine.Vector3 position, global::UnityEngine.Vector3 eulerRotation, SerializableGuid assetId, ServerRpcParams serverRpcParams)
		{
			if (base.IsServer)
			{
				int userId;
				if (!NetworkBehaviourSingleton<UserIdManager>.Instance.TryGetUserId(serverRpcParams.Receive.SenderClientId, out userId))
				{
					Debug.LogException(new Exception(string.Format("Unable to determine User Id for Client Id: {0}", serverRpcParams.Receive.SenderClientId)));
				}
				TaskAwaiter<bool> taskAwaiter = NetworkBehaviourSingleton<CreatorManager>.Instance.UserCanEditLevel(userId).GetAwaiter();
				if (!taskAwaiter.IsCompleted)
				{
					await taskAwaiter;
					TaskAwaiter<bool> taskAwaiter2;
					taskAwaiter = taskAwaiter2;
					taskAwaiter2 = default(TaskAwaiter<bool>);
				}
				if (taskAwaiter.GetResult())
				{
					PropLibrary.RuntimePropInfo metadata = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary[assetId];
					if (metadata == null)
					{
						Debug.LogException(new Exception("Server was told to spawn invalid asset ID!"));
					}
					else
					{
						if (metadata.IsLoading)
						{
							await MonoBehaviourSingleton<StageManager>.Instance.FetchAndSpawnPropPrefab(metadata.PropData.ToAssetReference());
						}
						if (!metadata.IsMissingObject)
						{
							bool flag = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.PlacementIsValid(metadata.PropData, position, Quaternion.Euler(eulerRotation));
							bool flag2 = true;
							bool flag3 = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.CanAddProp(metadata);
							if (flag && flag2 && flag3)
							{
								base.FirePropPlacedAnalyticEvent(metadata.PropData.AssetID, metadata.PropData.Name);
								base.FirePropEvent("propToolPropPlaced", metadata.PropData.AssetID, metadata.PropData.Name);
								GameObject gameObject = global::UnityEngine.Object.Instantiate<EndlessProp>(metadata.EndlessProp, position, Quaternion.Euler(eulerRotation)).gameObject;
								SerializableGuid serializableGuid = SerializableGuid.NewGuid();
								ulong num = 0UL;
								if (metadata.EndlessProp.IsNetworked)
								{
									NetworkObject component = gameObject.GetComponent<NetworkObject>();
									if (component != null)
									{
										component.Spawn(false);
										num = component.NetworkObjectId;
									}
									MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.TrackPendingNetworkId(num, serializableGuid, true, null);
								}
								MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.TrackNonNetworkedObject(assetId, serializableGuid, gameObject, true);
								this.PlaceProp_ClientRPC(position, eulerRotation, assetId, serializableGuid, num);
								MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RevisionMetaData.RevisionTimestamp = DateTime.UtcNow.Ticks;
								MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RevisionMetaData.Changes.Add(new ChangeData
								{
									ChangeType = ChangeType.PropPainted,
									UserId = userId
								});
								IPropPlacedSubscriber[] componentsInChildren = gameObject.GetComponentsInChildren<IPropPlacedSubscriber>();
								for (int i = 0; i < componentsInChildren.Length; i++)
								{
									componentsInChildren[i].PropPlaced(serializableGuid, false);
								}
								NetworkBehaviourSingleton<CreatorManager>.Instance.SaveLoadManager.SaveLevel();
							}
						}
					}
				}
			}
		}

		// Token: 0x06001157 RID: 4439 RVA: 0x000542DC File Offset: 0x000524DC
		[ClientRpc]
		public void PlaceProp_ClientRPC(global::UnityEngine.Vector3 position, global::UnityEngine.Vector3 eulerRotation, SerializableGuid assetId, SerializableGuid instanceId, ulong networkObjectId)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			global::UnityEngine.Vector3 position;
			global::UnityEngine.Vector3 eulerRotation;
			SerializableGuid assetId;
			SerializableGuid instanceId;
			ulong networkObjectId;
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(47490796U, clientRpcParams, RpcDelivery.Reliable);
				fastBufferWriter.WriteValueSafe(in position);
				fastBufferWriter.WriteValueSafe(in eulerRotation);
				fastBufferWriter.WriteValueSafe<SerializableGuid>(in assetId, default(FastBufferWriter.ForNetworkSerializable));
				fastBufferWriter.WriteValueSafe<SerializableGuid>(in instanceId, default(FastBufferWriter.ForNetworkSerializable));
				BytePacker.WriteValueBitPacked(fastBufferWriter, networkObjectId);
				base.__endSendClientRpc(ref fastBufferWriter, 47490796U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			position = position;
			eulerRotation = eulerRotation;
			assetId = assetId;
			instanceId = instanceId;
			networkObjectId = networkObjectId;
			if (base.IsServer)
			{
				return;
			}
			if (NetworkBehaviourSingleton<CreatorManager>.Instance.RpcReceiveState == RpcReceiveState.Allow)
			{
				this.PlaceProp(position, eulerRotation, assetId, instanceId, networkObjectId);
				return;
			}
			NetworkBehaviourSingleton<CreatorManager>.Instance.AddCachedRpc(delegate
			{
				this.PlaceProp(position, eulerRotation, assetId, instanceId, networkObjectId);
			});
		}

		// Token: 0x06001158 RID: 4440 RVA: 0x00054490 File Offset: 0x00052690
		private async void PlaceProp(global::UnityEngine.Vector3 position, global::UnityEngine.Vector3 eulerRotation, SerializableGuid assetId, SerializableGuid instanceId, ulong networkObjectId)
		{
			PropLibrary.RuntimePropInfo propInfo = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary[assetId];
			if (propInfo.IsLoading)
			{
				await MonoBehaviourSingleton<StageManager>.Instance.FetchAndSpawnPropPrefab(propInfo.PropData.ToAssetReference());
			}
			if (networkObjectId == 0UL)
			{
				GameObject gameObject = global::UnityEngine.Object.Instantiate<EndlessProp>(propInfo.EndlessProp).gameObject;
				gameObject.transform.position = position;
				gameObject.transform.rotation = Quaternion.Euler(eulerRotation);
				MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.TrackNonNetworkedObject(assetId, instanceId, gameObject, true);
			}
			else
			{
				MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.TrackPendingNetworkObject(networkObjectId, assetId, instanceId, true, null);
			}
		}

		// Token: 0x06001159 RID: 4441 RVA: 0x000544EC File Offset: 0x000526EC
		public async void UpdateSelectedAssetId(SerializableGuid selectedAssetId)
		{
			if (base.Rotating)
			{
				base.EndRotating();
			}
			MonoBehaviourSingleton<MarkerManager>.Instance.ReleaseAllMarkers();
			PropLibrary.RuntimePropInfo runtimePropInfo;
			if (selectedAssetId.IsEmpty || !MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(selectedAssetId, out runtimePropInfo))
			{
				if (!base.ActiveAssetId.IsEmpty)
				{
					base.DestroyPreview();
					base.ClearPreviousAssetId();
					this.OnSelectedAssetChanged.Invoke(selectedAssetId);
				}
				base.Set3DCursorUsesIntersection(true);
			}
			else
			{
				if (this.scriptWindow && base.ActiveAssetId != selectedAssetId && this.PreviousSelectedAssetId != selectedAssetId)
				{
					this.scriptWindow.Close();
				}
				base.SetPropDataFromPropInfo(runtimePropInfo, default(SerializableGuid));
				if (runtimePropInfo.IsLoading)
				{
					this.LoadPropPrefab(runtimePropInfo);
				}
				if (!this.scriptWindow)
				{
					base.SpawnPreview(runtimePropInfo);
				}
				this.OnSelectedAssetChanged.Invoke(selectedAssetId);
			}
		}

		// Token: 0x0600115A RID: 4442 RVA: 0x0005452C File Offset: 0x0005272C
		private async void LoadPropPrefab(PropLibrary.RuntimePropInfo runtimePropInfo)
		{
			if (base.InFlightLoads.Add(runtimePropInfo))
			{
				await MonoBehaviourSingleton<StageManager>.Instance.FetchAndSpawnPropPrefab(runtimePropInfo.PropData.ToAssetReference());
				base.InFlightLoads.Remove(runtimePropInfo);
				PropLibrary.RuntimePropInfo runtimePropInfo2;
				if (!this.scriptWindow && MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(runtimePropInfo.PropData.AssetID, out runtimePropInfo2) && runtimePropInfo2.PropData.AssetID == this.SelectedAssetId)
				{
					if (!runtimePropInfo2.IsMissingObject)
					{
						base.SpawnPreview(runtimePropInfo2);
					}
					else
					{
						base.SpawnPreview(runtimePropInfo);
					}
					this.OnSelectedAssetChanged.Invoke(runtimePropInfo.PropData.AssetID);
				}
			}
		}

		// Token: 0x0600115B RID: 4443 RVA: 0x0005456C File Offset: 0x0005276C
		public override void UpdateTool()
		{
			base.UpdateTool();
			if (base.PropGhostTransform == null && EndlessInput.GetKeyDown(global::UnityEngine.InputSystem.Key.LeftAlt))
			{
				base.Set3DCursorUsesIntersection(true);
			}
			if (EndlessInput.GetKeyUp(global::UnityEngine.InputSystem.Key.LeftAlt))
			{
				base.Set3DCursorUsesIntersection(false);
			}
			if (!base.IsMobile && base.ToolState == ToolState.None)
			{
				base.UpdatePropPlacement();
			}
		}

		// Token: 0x0600115C RID: 4444 RVA: 0x000545C3 File Offset: 0x000527C3
		public override void ToolSecondaryPressed()
		{
			base.ToolSecondaryPressed();
			UnityEvent<SerializableGuid> onSelectedAssetChanged = this.OnSelectedAssetChanged;
			if (onSelectedAssetChanged == null)
			{
				return;
			}
			onSelectedAssetChanged.Invoke(SerializableGuid.Empty);
		}

		// Token: 0x0600115D RID: 4445 RVA: 0x000545E0 File Offset: 0x000527E0
		public void EditScript(bool readOnly)
		{
			PropLibrary.RuntimePropInfo runtimePropInfo = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(base.ActiveAssetId);
			Script script = runtimePropInfo.EndlessProp.ScriptComponent.Script;
			base.DestroyPreview();
			this.scriptWindow = UIScriptWindowView.Display(script, runtimePropInfo, readOnly, null);
			this.scriptWindow.OnClosedUnityEvent.AddListener(new UnityAction<SerializableGuid>(this.OnScriptWindowClosed));
		}

		// Token: 0x0600115E RID: 4446 RVA: 0x00054648 File Offset: 0x00052848
		private void OnScriptWindowClosed(SerializableGuid propAssetId)
		{
			this.scriptWindow.OnClosedUnityEvent.RemoveListener(new UnityAction<SerializableGuid>(this.OnScriptWindowClosed));
			this.scriptWindow = null;
			if (!base.IsActive)
			{
				return;
			}
			if (propAssetId == base.PreviousAssetId && (base.ActiveAssetId.IsEmpty || base.PreviousAssetId == base.ActiveAssetId))
			{
				PropLibrary.RuntimePropInfo runtimePropInfo = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(propAssetId);
				base.GeneratePropPreview(runtimePropInfo, propAssetId);
			}
		}

		// Token: 0x06001160 RID: 4448 RVA: 0x000546E0 File Offset: 0x000528E0
		protected override void __initializeVariables()
		{
			base.__initializeVariables();
		}

		// Token: 0x06001161 RID: 4449 RVA: 0x000546F8 File Offset: 0x000528F8
		protected override void __initializeRpcs()
		{
			base.__registerRpc(2880201906U, new NetworkBehaviour.RpcReceiveHandler(PropTool.__rpc_handler_2880201906), "AttemptPlaceProp_ServerRPC");
			base.__registerRpc(47490796U, new NetworkBehaviour.RpcReceiveHandler(PropTool.__rpc_handler_47490796), "PlaceProp_ClientRPC");
			base.__initializeRpcs();
		}

		// Token: 0x06001162 RID: 4450 RVA: 0x00054748 File Offset: 0x00052948
		private static void __rpc_handler_2880201906(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			global::UnityEngine.Vector3 vector;
			reader.ReadValueSafe(out vector);
			global::UnityEngine.Vector3 vector2;
			reader.ReadValueSafe(out vector2);
			SerializableGuid serializableGuid;
			reader.ReadValueSafe<SerializableGuid>(out serializableGuid, default(FastBufferWriter.ForNetworkSerializable));
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((PropTool)target).AttemptPlaceProp_ServerRPC(vector, vector2, serializableGuid, server);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06001163 RID: 4451 RVA: 0x000547E8 File Offset: 0x000529E8
		private static void __rpc_handler_47490796(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			global::UnityEngine.Vector3 vector;
			reader.ReadValueSafe(out vector);
			global::UnityEngine.Vector3 vector2;
			reader.ReadValueSafe(out vector2);
			SerializableGuid serializableGuid;
			reader.ReadValueSafe<SerializableGuid>(out serializableGuid, default(FastBufferWriter.ForNetworkSerializable));
			SerializableGuid serializableGuid2;
			reader.ReadValueSafe<SerializableGuid>(out serializableGuid2, default(FastBufferWriter.ForNetworkSerializable));
			ulong num;
			ByteUnpacker.ReadValueBitPacked(reader, out num);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((PropTool)target).PlaceProp_ClientRPC(vector, vector2, serializableGuid, serializableGuid2, num);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06001164 RID: 4452 RVA: 0x000548AA File Offset: 0x00052AAA
		protected internal override string __getTypeName()
		{
			return "PropTool";
		}

		// Token: 0x04000E24 RID: 3620
		private UIScriptWindowView scriptWindow;

		// Token: 0x04000E25 RID: 3621
		public UnityEvent<SerializableGuid> OnSelectedAssetChanged = new UnityEvent<SerializableGuid>();
	}
}
