using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Endless.Assets;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Props.Assets;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Netcode;
using Unity.Services.Analytics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Endless.Creator.LevelEditing.Runtime;

public class EraseTool : EndlessTool
{
	public UnityEvent<EraseToolFunction> OnFunctionChange = new UnityEvent<EraseToolFunction>();

	[SerializeField]
	private float toolDragDeadZone = 0.01f;

	[SerializeField]
	private int maximumPositions = 100;

	private WiringTool wiringTool;

	private Vector3Int initialCoordinate;

	private Vector3 previousRayOrigin;

	private bool isErasing;

	private EraseToolFunction currentFunction = (EraseToolFunction)3;

	private List<Vector3Int> minimumErasedCells = new List<Vector3Int>();

	private List<Vector3Int> minimumIgnoredCells = new List<Vector3Int>();

	private HashSet<SerializableGuid> allErasedPropGuids = new HashSet<SerializableGuid>();

	private HashSet<Vector3Int> erasedPositions = new HashSet<Vector3Int>();

	private HashSet<Vector3Int> ignoredPositions = new HashSet<Vector3Int>();

	private int erasingModeIndex;

	private CellCollectionMode[] planeErasingModes = new CellCollectionMode[3]
	{
		new HorizontalPlaneCellCollectionMode(),
		new FacingPlaneCellCollectionMode(),
		new SidePlaneCellCollectionMode()
	};

	public override ToolType ToolType => ToolType.Erase;

	public EraseToolFunction CurrentFunction => currentFunction;

	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();
		if (base.IsClient)
		{
			wiringTool = MonoBehaviourSingleton<ToolManager>.Instance.RequestToolInstance<WiringTool>();
		}
	}

	public override void UpdateTool()
	{
		base.UpdateTool();
		if (EndlessInput.GetKeyDown(Key.E))
		{
			erasingModeIndex = (erasingModeIndex + 1) % planeErasingModes.Length;
			Debug.Log("Switched to Painting Mode: " + GetActiveEraseMode().GetType().Name);
			UpdateToolPrompter();
		}
		else if (EndlessInput.GetKeyDown(Key.Q))
		{
			erasingModeIndex--;
			if (erasingModeIndex < 0)
			{
				erasingModeIndex = planeErasingModes.Length - 1;
			}
			Debug.Log("Switched to Painting Mode: " + GetActiveEraseMode().GetType().Name);
			UpdateToolPrompter();
		}
		if (isErasing)
		{
			return;
		}
		LineCastHit activeLineCastResult = base.ActiveLineCastResult;
		minimumErasedCells.Clear();
		minimumIgnoredCells.Clear();
		if (!activeLineCastResult.IntersectionOccured)
		{
			MonoBehaviourSingleton<MarkerManager>.Instance.ReleaseAllMarkers();
			return;
		}
		if (MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetCellFromCoordinate(activeLineCastResult.IntersectedObjectPosition) is PropCell propCell)
		{
			bool flag = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.PropIsWired(propCell.InstanceId);
			if (!MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.CanRemoveProp(MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetAssetIdFromInstanceId(propCell.InstanceId)))
			{
				minimumIgnoredCells.Add(activeLineCastResult.IntersectedObjectPosition);
			}
			else if (((currentFunction & EraseToolFunction.WiredProps) == EraseToolFunction.WiredProps && flag) || ((currentFunction & EraseToolFunction.UnwiredProps) == EraseToolFunction.UnwiredProps && !flag))
			{
				GameObject gameObjectFromInstanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetGameObjectFromInstanceId(propCell.InstanceId);
				if ((bool)gameObjectFromInstanceId && MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetAssetIdFromInstanceId(propCell.InstanceId), out var metadata))
				{
					PropLocationOffset[] rotatedPropLocations = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetRotatedPropLocations(metadata.PropData, gameObjectFromInstanceId.transform.position, gameObjectFromInstanceId.transform.rotation);
					minimumErasedCells.AddRange(rotatedPropLocations.Select((PropLocationOffset x) => x.Offset));
				}
			}
			else
			{
				minimumIgnoredCells.Add(activeLineCastResult.IntersectedObjectPosition);
			}
		}
		else if (MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetCellFromCoordinate(activeLineCastResult.IntersectedObjectPosition) is TerrainCell)
		{
			if ((currentFunction & EraseToolFunction.Terrain) == EraseToolFunction.Terrain)
			{
				minimumErasedCells.Add(activeLineCastResult.IntersectedObjectPosition);
			}
			else
			{
				minimumIgnoredCells.Add(activeLineCastResult.IntersectedObjectPosition);
			}
		}
		MonoBehaviourSingleton<MarkerManager>.Instance.MarkCoordinates(minimumIgnoredCells, MarkerType.Ignore);
		MonoBehaviourSingleton<MarkerManager>.Instance.MarkCoordinates(minimumErasedCells, MarkerType.Erase);
	}

	private void UpdateToolPrompter()
	{
		string displayName = planeErasingModes[erasingModeIndex].DisplayName;
		base.UIToolPrompter.Display("Erasing " + displayName + " (Q/E)");
	}

	public override void HandleSelected()
	{
		Set3DCursorUsesIntersection(val: true);
		UpdateToolPrompter();
	}

	public override void HandleDeselected()
	{
		MonoBehaviourSingleton<MarkerManager>.Instance.ReleaseAllMarkers();
		isErasing = false;
	}

	public override void ToolPressed()
	{
		LineCastHit activeLineCastResult = base.ActiveLineCastResult;
		if (activeLineCastResult.IntersectionOccured)
		{
			base.AutoPlace3DCursor = false;
			initialCoordinate = activeLineCastResult.IntersectedObjectPosition;
			previousRayOrigin = base.ActiveRay.origin;
			isErasing = true;
			for (int i = 0; i < planeErasingModes.Length; i++)
			{
				planeErasingModes[i].InputDown(initialCoordinate);
			}
		}
	}

	private CellCollectionMode GetActiveEraseMode()
	{
		return planeErasingModes[erasingModeIndex];
	}

	public override void ToolHeld()
	{
		if (!isErasing)
		{
			return;
		}
		Vector3 origin = base.ActiveRay.origin;
		HashSet<SerializableGuid> hashSet = new HashSet<SerializableGuid>();
		List<Vector3Int> list;
		if ((origin - previousRayOrigin).magnitude >= toolDragDeadZone)
		{
			MonoBehaviourSingleton<CellMarker>.Instance.MoveTo(GetActiveEraseMode().CurrentEndCoordinate);
			list = GetActiveEraseMode().CollectPaintedPositions(base.ActiveRay, maximumPositions);
		}
		else
		{
			MonoBehaviourSingleton<CellMarker>.Instance.MoveTo(initialCoordinate);
			list = new List<Vector3Int> { initialCoordinate };
		}
		erasedPositions.Clear();
		ignoredPositions.Clear();
		allErasedPropGuids.Clear();
		for (int i = 0; i < list.Count; i++)
		{
			Vector3Int vector3Int = list[i];
			Cell cellFromCoordinate = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetCellFromCoordinate(vector3Int);
			if (cellFromCoordinate == null)
			{
				ignoredPositions.Add(vector3Int);
			}
			else if (cellFromCoordinate is TerrainCell)
			{
				if ((currentFunction & EraseToolFunction.Terrain) == EraseToolFunction.Terrain)
				{
					erasedPositions.Add(vector3Int);
				}
				else
				{
					ignoredPositions.Add(vector3Int);
				}
			}
			else
			{
				if (!(cellFromCoordinate is PropCell propCell) || hashSet.Contains(propCell.InstanceId))
				{
					continue;
				}
				hashSet.Add(propCell.InstanceId);
				bool flag = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.PropIsWired(propCell.InstanceId);
				if (!MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.CanRemoveProp(MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetAssetIdFromInstanceId(propCell.InstanceId)))
				{
					ignoredPositions.Add(vector3Int);
				}
				else if (((currentFunction & EraseToolFunction.WiredProps) == EraseToolFunction.WiredProps && flag) || ((currentFunction & EraseToolFunction.UnwiredProps) == EraseToolFunction.UnwiredProps && !flag))
				{
					allErasedPropGuids.Add(propCell.InstanceId);
					GameObject gameObjectFromInstanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetGameObjectFromInstanceId(propCell.InstanceId);
					PropLibrary.RuntimePropInfo runtimePropInfo = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetAssetIdFromInstanceId(propCell.InstanceId));
					PropLocationOffset[] rotatedPropLocations = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetRotatedPropLocations(runtimePropInfo.PropData, gameObjectFromInstanceId.transform.position, gameObjectFromInstanceId.transform.rotation);
					for (int j = 0; j < rotatedPropLocations.Length; j++)
					{
						erasedPositions.Add(rotatedPropLocations[j].Offset);
					}
				}
				else
				{
					ignoredPositions.Add(vector3Int);
				}
			}
		}
		MonoBehaviourSingleton<MarkerManager>.Instance.MarkCoordinates(erasedPositions, MarkerType.Erase);
		MonoBehaviourSingleton<MarkerManager>.Instance.MarkCoordinates(ignoredPositions, MarkerType.Ignore);
	}

	public override void ToolReleased()
	{
		if (isErasing)
		{
			GetActiveEraseMode().InputReleased();
			if (GetActiveEraseMode().IsComplete())
			{
				base.AutoPlace3DCursor = true;
				AttemptErasing_ServerRPC(erasedPositions.ToArray(), allErasedPropGuids.ToArray());
			}
			MonoBehaviourSingleton<MarkerManager>.Instance.ReleaseAllMarkers();
			isErasing = false;
		}
	}

	public void ToggleCurrentFunction(EraseToolFunction function, bool state)
	{
		EraseToolFunction eraseToolFunction = currentFunction;
		eraseToolFunction = ((!state) ? (eraseToolFunction & ~function) : (eraseToolFunction | function));
		if (currentFunction != eraseToolFunction)
		{
			currentFunction = eraseToolFunction;
			OnFunctionChange.Invoke(currentFunction);
		}
	}

	[ServerRpc(RequireOwnership = false)]
	private void AttemptErasing_ServerRPC(Vector3Int[] locations, SerializableGuid[] propsToErase, ServerRpcParams serverRpcParams = default(ServerRpcParams))
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			FastBufferWriter bufferWriter = __beginSendServerRpc(2785434172u, serverRpcParams, RpcDelivery.Reliable);
			bool value = locations != null;
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			if (value)
			{
				bufferWriter.WriteValueSafe(locations);
			}
			bool value2 = propsToErase != null;
			bufferWriter.WriteValueSafe(in value2, default(FastBufferWriter.ForPrimitives));
			if (value2)
			{
				bufferWriter.WriteValueSafe(propsToErase, default(FastBufferWriter.ForNetworkSerializable));
			}
			__endSendServerRpc(ref bufferWriter, 2785434172u, serverRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			AttemptErasing(locations, propsToErase, serverRpcParams);
		}
	}

	private async Task AttemptErasing(Vector3Int[] locations, SerializableGuid[] propsToErase, ServerRpcParams serverRpcParams)
	{
		if (!base.IsServer)
		{
			return;
		}
		if (!NetworkBehaviourSingleton<UserIdManager>.Instance.TryGetUserId(serverRpcParams.Receive.SenderClientId, out var userId))
		{
			Debug.LogException(new Exception($"Unable to determine User Id for Client Id: {serverRpcParams.Receive.SenderClientId}"));
		}
		if (!(await NetworkBehaviourSingleton<CreatorManager>.Instance.UserCanEditLevel(userId)))
		{
			return;
		}
		List<Vector3Int> list = new List<Vector3Int>();
		foreach (Vector3Int vector3Int in locations)
		{
			if (MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.CanEraseTerrainCell(vector3Int))
			{
				list.Add(vector3Int);
			}
		}
		if (list.Count <= 0 && propsToErase.Length == 0)
		{
			return;
		}
		Vector3Int[] array = list.ToArray();
		List<SerializableGuid> list2 = wiringTool.FilterRerouteNodeGuids(propsToErase);
		HashSet<SerializableGuid> hashSet = propsToErase.ToHashSet();
		HashSet<SerializableGuid> wiresUsingProps = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetWiresUsingProps(propsToErase);
		HashSet<SerializableGuid> hashSet2 = new HashSet<SerializableGuid>();
		foreach (SerializableGuid item in wiresUsingProps)
		{
			foreach (SerializableGuid item2 in MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.GetRerouteNodesFromWire(item))
			{
				hashSet2.Add(item2);
				list2.Remove(item2);
			}
		}
		foreach (SerializableGuid item3 in list2.Concat(hashSet2))
		{
			hashSet2.Add(item3);
			hashSet.Remove(item3);
		}
		for (int num = hashSet.Count - 1; num >= 0; num--)
		{
			SerializableGuid instanceId = hashSet.ElementAt(num);
			if (!MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.CanRemoveProp(MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetAssetIdFromInstanceId(instanceId)))
			{
				hashSet.Remove(hashSet.ElementAt(num));
			}
		}
		PropEraseData[] array2 = new PropEraseData[hashSet.Count];
		for (int j = 0; j < array2.Length; j++)
		{
			GameObject gameObjectFromInstanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetGameObjectFromInstanceId(hashSet.ElementAt(j));
			SerializableGuid assetIdFromInstanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetAssetIdFromInstanceId(hashSet.ElementAt(j));
			array2[j] = new PropEraseData
			{
				Position = gameObjectFromInstanceId.transform.position,
				Rotation = gameObjectFromInstanceId.transform.rotation,
				InstanceId = hashSet.ElementAt(j),
				AssetId = assetIdFromInstanceId
			};
			PropLibrary.RuntimePropInfo runtimePropInfo = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(assetIdFromInstanceId);
			CustomEvent e = new CustomEvent("propErased")
			{
				{
					"propName1",
					runtimePropInfo.PropData.Name
				},
				{
					"propId1",
					assetIdFromInstanceId.ToString()
				}
			};
			AnalyticsService.Instance.RecordEvent(e);
		}
		if (array.Length != 0)
		{
			CustomEvent e2 = new CustomEvent("terrainErased") { { "cellChangedCount", array.Length } };
			AnalyticsService.Instance.RecordEvent(e2);
		}
		AttemptErasing_ClientRPC(array, array2);
		wiringTool.EraseRerouteNodes_ServerRpc(list2.ToArray());
		if (!base.IsClient)
		{
			ApplyErasingResult(array, array2);
		}
		if (list.Count > 0)
		{
			MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RevisionMetaData.Changes.Add(new ChangeData
			{
				ChangeType = ChangeType.TerrainErased,
				UserId = userId
			});
		}
		if (propsToErase.Count() > 0)
		{
			MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RevisionMetaData.Changes.Add(new ChangeData
			{
				ChangeType = ChangeType.PropErase,
				UserId = userId
			});
		}
		if (wiresUsingProps.Count() > 0)
		{
			MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RevisionMetaData.Changes.Add(new ChangeData
			{
				ChangeType = ChangeType.WireDeleted,
				UserId = userId
			});
		}
		if (hashSet2.Count() > 0)
		{
			MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RevisionMetaData.Changes.Add(new ChangeData
			{
				ChangeType = ChangeType.WireRerouteDeleted,
				UserId = userId
			});
		}
		MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RevisionMetaData.RevisionTimestamp = DateTime.UtcNow.Ticks;
		NetworkBehaviourSingleton<CreatorManager>.Instance.SaveLoadManager.SaveLevel();
	}

	[ClientRpc]
	private void AttemptErasing_ClientRPC(Vector3Int[] affectedCellPositions, PropEraseData[] propsToErase)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			ClientRpcParams clientRpcParams = default(ClientRpcParams);
			FastBufferWriter bufferWriter = __beginSendClientRpc(2160773166u, clientRpcParams, RpcDelivery.Reliable);
			bool value = affectedCellPositions != null;
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			if (value)
			{
				bufferWriter.WriteValueSafe(affectedCellPositions);
			}
			bool value2 = propsToErase != null;
			bufferWriter.WriteValueSafe(in value2, default(FastBufferWriter.ForPrimitives));
			if (value2)
			{
				PropEraseDataNetworkExtensions.WriteValueSafe(bufferWriter, in propsToErase);
			}
			__endSendClientRpc(ref bufferWriter, 2160773166u, clientRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
		{
			return;
		}
		__rpc_exec_stage = __RpcExecStage.Send;
		Vector3Int[] affectedCellPositions2 = affectedCellPositions;
		PropEraseData[] propsToErase2 = propsToErase;
		if (NetworkBehaviourSingleton<CreatorManager>.Instance.RpcReceiveState == RpcReceiveState.Allow)
		{
			ApplyErasingResult(affectedCellPositions2, propsToErase2);
			return;
		}
		NetworkBehaviourSingleton<CreatorManager>.Instance.AddCachedRpc(delegate
		{
			ApplyErasingResult(affectedCellPositions2, propsToErase2);
		});
	}

	private void ApplyErasingResult(Vector3Int[] affectedTerrainCellPositions, PropEraseData[] propsToErase)
	{
		Debug.Log(string.Format("{0} - props to erase: {1}", "ApplyErasingResult", propsToErase.Length));
		MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.EraseCells(affectedTerrainCellPositions.ToList());
		wiringTool.DestroyWires(MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetWiresUsingProps(propsToErase.Select((PropEraseData propEraseData2) => propEraseData2.InstanceId)));
		foreach (PropEraseData propEraseData in propsToErase)
		{
			MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.RemoveProp(propEraseData.InstanceId, propEraseData.AssetId);
		}
	}

	public override void Reset()
	{
		base.Reset();
		MonoBehaviourSingleton<MarkerManager>.Instance.ReleaseAllMarkers();
		isErasing = false;
	}

	protected override void __initializeVariables()
	{
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		__registerRpc(2785434172u, __rpc_handler_2785434172, "AttemptErasing_ServerRPC");
		__registerRpc(2160773166u, __rpc_handler_2160773166, "AttemptErasing_ClientRPC");
		base.__initializeRpcs();
	}

	private static void __rpc_handler_2785434172(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out bool value, default(FastBufferWriter.ForPrimitives));
			Vector3Int[] value2 = null;
			if (value)
			{
				reader.ReadValueSafe(out value2);
			}
			reader.ReadValueSafe(out bool value3, default(FastBufferWriter.ForPrimitives));
			SerializableGuid[] value4 = null;
			if (value3)
			{
				reader.ReadValueSafe(out value4, default(FastBufferWriter.ForNetworkSerializable));
			}
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((EraseTool)target).AttemptErasing_ServerRPC(value2, value4, server);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_2160773166(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out bool value, default(FastBufferWriter.ForPrimitives));
			Vector3Int[] value2 = null;
			if (value)
			{
				reader.ReadValueSafe(out value2);
			}
			reader.ReadValueSafe(out bool value3, default(FastBufferWriter.ForPrimitives));
			PropEraseData[] propEraseDatas = null;
			if (value3)
			{
				PropEraseDataNetworkExtensions.ReadValueSafe(reader, out propEraseDatas);
			}
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((EraseTool)target).AttemptErasing_ClientRPC(value2, propEraseDatas);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	protected internal override string __getTypeName()
	{
		return "EraseTool";
	}
}
