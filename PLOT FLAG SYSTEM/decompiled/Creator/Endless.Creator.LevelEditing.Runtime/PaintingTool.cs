using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Endless.Assets;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.LevelEditing.Tilesets;
using Endless.Shared;
using Unity.Netcode;
using Unity.Services.Analytics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Endless.Creator.LevelEditing.Runtime;

public class PaintingTool : EndlessTool
{
	public static int NoSelection = -1;

	public UnityEvent<int> OnActiveTilesetIndexChanged = new UnityEvent<int>();

	[SerializeField]
	[Tooltip("Max Void Distance is used when no object is collided against, how far out do we let the user paint. Essentially, in an empty level, how far into the void can they paint with no other props.")]
	private float maxVoidDistance = 10f;

	[SerializeField]
	private float toolDragDeadZone = 0.01f;

	[SerializeField]
	private int maximumPaintedCellsPerStroke = 100;

	private Vector3Int initialCoordinate;

	private Vector3Int lastValidStrokeCoordinate;

	private int activeTilesetIndex = NoSelection;

	private Vector3 previousRayOrigin;

	private bool painting;

	private int paintingModeIndex;

	private CellCollectionMode[] planePaintingModes = new CellCollectionMode[3]
	{
		new HorizontalPlaneCellCollectionMode(),
		new FacingPlaneCellCollectionMode(),
		new SidePlaneCellCollectionMode()
	};

	public override ToolType ToolType => ToolType.Painting;

	public int ActiveTilesetIndex => activeTilesetIndex;

	private void Awake()
	{
		Application.focusChanged += OnFocusChanged;
	}

	private void OnFocusChanged(bool obj)
	{
		Set3DCursorUsesIntersection(val: false);
	}

	public override void HandleSelected()
	{
		if (activeTilesetIndex == NoSelection)
		{
			activeTilesetIndex = MonoBehaviourSingleton<StageManager>.Instance.ActiveTerrainPalette.GetIndexOfFirstNonRedirect();
		}
		Set3DCursorUsesIntersection(val: true);
		UpdateToolPrompter();
	}

	private void UpdateToolPrompter()
	{
		if (HasActiveTilesetIndex())
		{
			string displayName = planePaintingModes[paintingModeIndex].DisplayName;
			base.UIToolPrompter.Display("Painting " + displayName + " (Q/E)");
		}
		else
		{
			base.UIToolPrompter.Hide();
		}
	}

	public override void HandleDeselected()
	{
		painting = false;
		MonoBehaviourSingleton<MarkerManager>.Instance.ReleaseAllMarkers();
	}

	public override void ToolPressed()
	{
		if (EndlessInput.GetKey(Key.LeftAlt))
		{
			LineCastHit activeLineCastResult = base.ActiveLineCastResult;
			if (activeLineCastResult.IntersectionOccured)
			{
				TerrainCell cellFromCoordinateAs = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetCellFromCoordinateAs<TerrainCell>(activeLineCastResult.IntersectedObjectPosition);
				if (cellFromCoordinateAs != null)
				{
					SetActiveTilesetIndex(cellFromCoordinateAs.TilesetIndex);
				}
			}
		}
		else if (HasActiveTilesetIndex())
		{
			base.AutoPlace3DCursor = false;
			previousRayOrigin = base.ActiveRay.origin;
			initialCoordinate = base.ActiveLineCastResult.NearestPositionToObject;
			for (int i = 0; i < planePaintingModes.Length; i++)
			{
				planePaintingModes[i].InputDown(initialCoordinate);
			}
			lastValidStrokeCoordinate = initialCoordinate;
			painting = true;
		}
	}

	private CellCollectionMode GetActivePaintingMode()
	{
		return planePaintingModes[paintingModeIndex];
	}

	public override void ToolHeld()
	{
		if (!HasActiveTilesetIndex() || !painting)
		{
			return;
		}
		List<Vector3Int> list;
		if ((base.ActiveRay.origin - previousRayOrigin).magnitude >= toolDragDeadZone)
		{
			list = GetActivePaintingMode().CollectPaintedPositions(base.ActiveRay, maximumPaintedCellsPerStroke);
			MonoBehaviourSingleton<CellMarker>.Instance.MoveTo(GetActivePaintingMode().CurrentEndCoordinate);
		}
		else
		{
			list = new List<Vector3Int> { initialCoordinate };
			lastValidStrokeCoordinate = initialCoordinate;
			MonoBehaviourSingleton<CellMarker>.Instance.MoveTo(initialCoordinate);
		}
		List<Vector3Int> list2 = new List<Vector3Int>();
		List<Vector3Int> list3 = new List<Vector3Int>();
		for (int i = 0; i < list.Count; i++)
		{
			if (MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.CanPaintCellPosition(list[i]))
			{
				list2.Add(list[i]);
			}
			else
			{
				list3.Add(list[i]);
			}
		}
		MonoBehaviourSingleton<MarkerManager>.Instance.MarkCoordinates(list2, MarkerType.Create);
		MonoBehaviourSingleton<MarkerManager>.Instance.MarkCoordinates(list3, MarkerType.Ignore);
	}

	public override void ToolReleased()
	{
		if (!HasActiveTilesetIndex() || !painting)
		{
			return;
		}
		GetActivePaintingMode().InputReleased();
		if (GetActivePaintingMode().IsComplete())
		{
			base.AutoPlace3DCursor = true;
			List<Vector3Int> list;
			if ((base.ActiveRay.origin - previousRayOrigin).magnitude >= toolDragDeadZone)
			{
				list = GetActivePaintingMode().CollectPaintedPositions(base.ActiveRay, maximumPaintedCellsPerStroke);
			}
			else
			{
				list = new List<Vector3Int>();
				list.Add(initialCoordinate);
				lastValidStrokeCoordinate = initialCoordinate;
			}
			if (list.Count <= maximumPaintedCellsPerStroke)
			{
				AttemptPainting_ServerRPC(list.ToArray(), activeTilesetIndex);
			}
			painting = false;
			GetActivePaintingMode().Reset();
			MonoBehaviourSingleton<MarkerManager>.Instance.ReleaseAllMarkers();
		}
	}

	public void SetActiveTilesetIndex(int newActiveTilesetIndex)
	{
		activeTilesetIndex = newActiveTilesetIndex;
		UpdateToolPrompter();
		OnActiveTilesetIndexChanged.Invoke(activeTilesetIndex);
	}

	[ServerRpc(RequireOwnership = false)]
	private void AttemptPainting_ServerRPC(Vector3Int[] positions, int tilesetIndex, ServerRpcParams serverRpcParams = default(ServerRpcParams))
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			FastBufferWriter bufferWriter = __beginSendServerRpc(787849911u, serverRpcParams, RpcDelivery.Reliable);
			bool value = positions != null;
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			if (value)
			{
				bufferWriter.WriteValueSafe(positions);
			}
			BytePacker.WriteValueBitPacked(bufferWriter, tilesetIndex);
			__endSendServerRpc(ref bufferWriter, 787849911u, serverRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			AttemptPainting(positions, tilesetIndex, serverRpcParams);
		}
	}

	private async Task AttemptPainting(Vector3Int[] positions, int tilesetIndex, ServerRpcParams serverRpcParams)
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
		for (int i = 0; i < positions.Length; i++)
		{
			if (MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.CanPaintCellPosition(positions[i]))
			{
				list.Add(positions[i]);
			}
		}
		if (list.Count > 0)
		{
			Tileset tileset = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.TilesetAtIndex(tilesetIndex);
			if (tileset == null)
			{
				Debug.LogError($"Null tileset at {tilesetIndex}");
			}
			CustomEvent e = new CustomEvent("terrainPainted")
			{
				{ "cellChangedCount", list.Count },
				{
					"terrainType",
					tilesetIndex.ToString()
				},
				{ "terrainName", tileset.DisplayName }
			};
			AnalyticsService.Instance.RecordEvent(e);
			Vector3Int[] affectedCellPositions = list.ToArray();
			ApplyPaintingResult_ClientRPC(affectedCellPositions, tilesetIndex);
			if (!base.IsClient)
			{
				ApplyPaintingResult(affectedCellPositions, tilesetIndex);
			}
			MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RevisionMetaData.RevisionTimestamp = DateTime.UtcNow.Ticks;
			MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RevisionMetaData.Changes.Add(new ChangeData
			{
				ChangeType = ChangeType.TerrainPainted,
				UserId = userId
			});
			NetworkBehaviourSingleton<CreatorManager>.Instance.SaveLoadManager.SaveLevel();
		}
	}

	[ClientRpc]
	private void ApplyPaintingResult_ClientRPC(Vector3Int[] affectedCellPositions, int tilesetIndex)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			ClientRpcParams clientRpcParams = default(ClientRpcParams);
			FastBufferWriter bufferWriter = __beginSendClientRpc(3535967038u, clientRpcParams, RpcDelivery.Reliable);
			bool value = affectedCellPositions != null;
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			if (value)
			{
				bufferWriter.WriteValueSafe(affectedCellPositions);
			}
			BytePacker.WriteValueBitPacked(bufferWriter, tilesetIndex);
			__endSendClientRpc(ref bufferWriter, 3535967038u, clientRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			ApplyPaintingResult(affectedCellPositions, tilesetIndex);
		}
	}

	private void ApplyPaintingResult(Vector3Int[] affectedCellPositions, int tilesetIndex)
	{
		if (NetworkBehaviourSingleton<CreatorManager>.Instance.RpcReceiveState == RpcReceiveState.Allow)
		{
			MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.PaintCellPositions(affectedCellPositions, tilesetIndex, updateLevelState: true, updateNeighbors: true, generateChanges: true);
			if (!MonoBehaviourSingleton<StageManager>.Instance.ActiveTerrainPalette.IsTilesetLoaded(tilesetIndex))
			{
				MonoBehaviourSingleton<StageManager>.Instance.LoadTilesetByIndex(tilesetIndex);
			}
		}
		else
		{
			Debug.Log("Adding a cached rpc!");
			NetworkBehaviourSingleton<CreatorManager>.Instance.AddCachedRpc(delegate
			{
				MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.PaintCellPositions(affectedCellPositions, tilesetIndex, updateLevelState: true, updateNeighbors: true, generateChanges: true);
			});
		}
	}

	public override void UpdateTool()
	{
		base.UpdateTool();
		Set3DCursorUsesIntersection(EndlessInput.GetKey(Key.LeftAlt));
		if (!HasActiveTilesetIndex())
		{
			return;
		}
		if (EndlessInput.GetKeyDown(Key.E))
		{
			paintingModeIndex = (paintingModeIndex + 1) % planePaintingModes.Length;
			Debug.Log("Switched to Painting Mode: " + GetActivePaintingMode().GetType().Name);
			UpdateToolPrompter();
		}
		else if (EndlessInput.GetKeyDown(Key.Q))
		{
			paintingModeIndex--;
			if (paintingModeIndex < 0)
			{
				paintingModeIndex = planePaintingModes.Length - 1;
			}
			UpdateToolPrompter();
			Debug.Log("Switched to Painting Mode: " + GetActivePaintingMode().GetType().Name);
		}
	}

	private bool HasActiveTilesetIndex()
	{
		return activeTilesetIndex >= 0;
	}

	public override void Reset()
	{
		base.Reset();
		painting = false;
		MonoBehaviourSingleton<MarkerManager>.Instance.ReleaseAllMarkers();
	}

	public override void CreatorExited()
	{
		base.CreatorExited();
		activeTilesetIndex = NoSelection;
	}

	protected override void __initializeVariables()
	{
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		__registerRpc(787849911u, __rpc_handler_787849911, "AttemptPainting_ServerRPC");
		__registerRpc(3535967038u, __rpc_handler_3535967038, "ApplyPaintingResult_ClientRPC");
		base.__initializeRpcs();
	}

	private static void __rpc_handler_787849911(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
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
			ByteUnpacker.ReadValueBitPacked(reader, out int value3);
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((PaintingTool)target).AttemptPainting_ServerRPC(value2, value3, server);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_3535967038(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
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
			ByteUnpacker.ReadValueBitPacked(reader, out int value3);
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((PaintingTool)target).ApplyPaintingResult_ClientRPC(value2, value3);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	protected internal override string __getTypeName()
	{
		return "PaintingTool";
	}
}
