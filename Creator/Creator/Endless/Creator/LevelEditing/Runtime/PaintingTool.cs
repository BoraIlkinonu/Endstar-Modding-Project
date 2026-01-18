using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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

namespace Endless.Creator.LevelEditing.Runtime
{
	// Token: 0x02000377 RID: 887
	public class PaintingTool : EndlessTool
	{
		// Token: 0x17000286 RID: 646
		// (get) Token: 0x060010EB RID: 4331 RVA: 0x0002ABC6 File Offset: 0x00028DC6
		public override ToolType ToolType
		{
			get
			{
				return ToolType.Painting;
			}
		}

		// Token: 0x17000287 RID: 647
		// (get) Token: 0x060010EC RID: 4332 RVA: 0x00051AEC File Offset: 0x0004FCEC
		public int ActiveTilesetIndex
		{
			get
			{
				return this.activeTilesetIndex;
			}
		}

		// Token: 0x060010ED RID: 4333 RVA: 0x00051AF4 File Offset: 0x0004FCF4
		private void Awake()
		{
			Application.focusChanged += this.OnFocusChanged;
		}

		// Token: 0x060010EE RID: 4334 RVA: 0x00051B07 File Offset: 0x0004FD07
		private void OnFocusChanged(bool obj)
		{
			base.Set3DCursorUsesIntersection(false);
		}

		// Token: 0x060010EF RID: 4335 RVA: 0x00051B10 File Offset: 0x0004FD10
		public override void HandleSelected()
		{
			if (this.activeTilesetIndex == PaintingTool.NoSelection)
			{
				this.activeTilesetIndex = MonoBehaviourSingleton<StageManager>.Instance.ActiveTerrainPalette.GetIndexOfFirstNonRedirect();
			}
			base.Set3DCursorUsesIntersection(true);
			this.UpdateToolPrompter();
		}

		// Token: 0x060010F0 RID: 4336 RVA: 0x00051B44 File Offset: 0x0004FD44
		private void UpdateToolPrompter()
		{
			if (this.HasActiveTilesetIndex())
			{
				string displayName = this.planePaintingModes[this.paintingModeIndex].DisplayName;
				base.UIToolPrompter.Display("Painting " + displayName + " (Q/E)", false);
				return;
			}
			base.UIToolPrompter.Hide();
		}

		// Token: 0x060010F1 RID: 4337 RVA: 0x00051B94 File Offset: 0x0004FD94
		public override void HandleDeselected()
		{
			this.painting = false;
			MonoBehaviourSingleton<MarkerManager>.Instance.ReleaseAllMarkers();
		}

		// Token: 0x060010F2 RID: 4338 RVA: 0x00051BA8 File Offset: 0x0004FDA8
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
						this.SetActiveTilesetIndex(cellFromCoordinateAs.TilesetIndex);
					}
				}
				return;
			}
			if (!this.HasActiveTilesetIndex())
			{
				return;
			}
			base.AutoPlace3DCursor = false;
			this.previousRayOrigin = base.ActiveRay.origin;
			this.initialCoordinate = base.ActiveLineCastResult.NearestPositionToObject;
			for (int i = 0; i < this.planePaintingModes.Length; i++)
			{
				this.planePaintingModes[i].InputDown(this.initialCoordinate);
			}
			this.lastValidStrokeCoordinate = this.initialCoordinate;
			this.painting = true;
		}

		// Token: 0x060010F3 RID: 4339 RVA: 0x00051C6B File Offset: 0x0004FE6B
		private CellCollectionMode GetActivePaintingMode()
		{
			return this.planePaintingModes[this.paintingModeIndex];
		}

		// Token: 0x060010F4 RID: 4340 RVA: 0x00051C7C File Offset: 0x0004FE7C
		public override void ToolHeld()
		{
			if (!this.HasActiveTilesetIndex() || !this.painting)
			{
				return;
			}
			List<Vector3Int> list;
			if ((base.ActiveRay.origin - this.previousRayOrigin).magnitude >= this.toolDragDeadZone)
			{
				list = this.GetActivePaintingMode().CollectPaintedPositions(base.ActiveRay, this.maximumPaintedCellsPerStroke);
				MonoBehaviourSingleton<CellMarker>.Instance.MoveTo(this.GetActivePaintingMode().CurrentEndCoordinate);
			}
			else
			{
				list = new List<Vector3Int> { this.initialCoordinate };
				this.lastValidStrokeCoordinate = this.initialCoordinate;
				MonoBehaviourSingleton<CellMarker>.Instance.MoveTo(this.initialCoordinate);
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

		// Token: 0x060010F5 RID: 4341 RVA: 0x00051D94 File Offset: 0x0004FF94
		public override void ToolReleased()
		{
			if (!this.HasActiveTilesetIndex() || !this.painting)
			{
				return;
			}
			this.GetActivePaintingMode().InputReleased();
			if (this.GetActivePaintingMode().IsComplete())
			{
				base.AutoPlace3DCursor = true;
				List<Vector3Int> list;
				if ((base.ActiveRay.origin - this.previousRayOrigin).magnitude >= this.toolDragDeadZone)
				{
					list = this.GetActivePaintingMode().CollectPaintedPositions(base.ActiveRay, this.maximumPaintedCellsPerStroke);
				}
				else
				{
					list = new List<Vector3Int>();
					list.Add(this.initialCoordinate);
					this.lastValidStrokeCoordinate = this.initialCoordinate;
				}
				if (list.Count <= this.maximumPaintedCellsPerStroke)
				{
					this.AttemptPainting_ServerRPC(list.ToArray(), this.activeTilesetIndex, default(ServerRpcParams));
				}
				this.painting = false;
				this.GetActivePaintingMode().Reset();
				MonoBehaviourSingleton<MarkerManager>.Instance.ReleaseAllMarkers();
			}
		}

		// Token: 0x060010F6 RID: 4342 RVA: 0x00051E7A File Offset: 0x0005007A
		public void SetActiveTilesetIndex(int newActiveTilesetIndex)
		{
			this.activeTilesetIndex = newActiveTilesetIndex;
			this.UpdateToolPrompter();
			this.OnActiveTilesetIndexChanged.Invoke(this.activeTilesetIndex);
		}

		// Token: 0x060010F7 RID: 4343 RVA: 0x00051E9C File Offset: 0x0005009C
		[ServerRpc(RequireOwnership = false)]
		private void AttemptPainting_ServerRPC(Vector3Int[] positions, int tilesetIndex, ServerRpcParams serverRpcParams = default(ServerRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendServerRpc(787849911U, serverRpcParams, RpcDelivery.Reliable);
				bool flag = positions != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag, default(FastBufferWriter.ForPrimitives));
				if (flag)
				{
					fastBufferWriter.WriteValueSafe(positions);
				}
				BytePacker.WriteValueBitPacked(fastBufferWriter, tilesetIndex);
				base.__endSendServerRpc(ref fastBufferWriter, 787849911U, serverRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			this.AttemptPainting(positions, tilesetIndex, serverRpcParams);
		}

		// Token: 0x060010F8 RID: 4344 RVA: 0x00051FC4 File Offset: 0x000501C4
		private async Task AttemptPainting(Vector3Int[] positions, int tilesetIndex, ServerRpcParams serverRpcParams)
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
							Debug.LogError(string.Format("Null tileset at {0}", tilesetIndex));
						}
						CustomEvent customEvent = new CustomEvent("terrainPainted");
						customEvent.Add("cellChangedCount", list.Count);
						customEvent.Add("terrainType", tilesetIndex.ToString());
						customEvent.Add("terrainName", tileset.DisplayName);
						AnalyticsService.Instance.RecordEvent(customEvent);
						Vector3Int[] array = list.ToArray();
						this.ApplyPaintingResult_ClientRPC(array, tilesetIndex);
						if (!base.IsClient)
						{
							this.ApplyPaintingResult(array, tilesetIndex);
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
			}
		}

		// Token: 0x060010F9 RID: 4345 RVA: 0x00052020 File Offset: 0x00050220
		[ClientRpc]
		private void ApplyPaintingResult_ClientRPC(Vector3Int[] affectedCellPositions, int tilesetIndex)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(3535967038U, clientRpcParams, RpcDelivery.Reliable);
				bool flag = affectedCellPositions != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag, default(FastBufferWriter.ForPrimitives));
				if (flag)
				{
					fastBufferWriter.WriteValueSafe(affectedCellPositions);
				}
				BytePacker.WriteValueBitPacked(fastBufferWriter, tilesetIndex);
				base.__endSendClientRpc(ref fastBufferWriter, 3535967038U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			this.ApplyPaintingResult(affectedCellPositions, tilesetIndex);
		}

		// Token: 0x060010FA RID: 4346 RVA: 0x00052148 File Offset: 0x00050348
		private void ApplyPaintingResult(Vector3Int[] affectedCellPositions, int tilesetIndex)
		{
			if (NetworkBehaviourSingleton<CreatorManager>.Instance.RpcReceiveState == RpcReceiveState.Allow)
			{
				MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.PaintCellPositions(affectedCellPositions, tilesetIndex, true, true, true);
				if (!MonoBehaviourSingleton<StageManager>.Instance.ActiveTerrainPalette.IsTilesetLoaded(tilesetIndex))
				{
					MonoBehaviourSingleton<StageManager>.Instance.LoadTilesetByIndex(tilesetIndex);
					return;
				}
			}
			else
			{
				Debug.Log("Adding a cached rpc!");
				NetworkBehaviourSingleton<CreatorManager>.Instance.AddCachedRpc(delegate
				{
					MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.PaintCellPositions(affectedCellPositions, tilesetIndex, true, true, true);
				});
			}
		}

		// Token: 0x060010FB RID: 4347 RVA: 0x000521DC File Offset: 0x000503DC
		public override void UpdateTool()
		{
			base.UpdateTool();
			base.Set3DCursorUsesIntersection(EndlessInput.GetKey(Key.LeftAlt));
			if (!this.HasActiveTilesetIndex())
			{
				return;
			}
			if (EndlessInput.GetKeyDown(Key.E))
			{
				this.paintingModeIndex = (this.paintingModeIndex + 1) % this.planePaintingModes.Length;
				Debug.Log("Switched to Painting Mode: " + this.GetActivePaintingMode().GetType().Name);
				this.UpdateToolPrompter();
				return;
			}
			if (EndlessInput.GetKeyDown(Key.Q))
			{
				this.paintingModeIndex--;
				if (this.paintingModeIndex < 0)
				{
					this.paintingModeIndex = this.planePaintingModes.Length - 1;
				}
				this.UpdateToolPrompter();
				Debug.Log("Switched to Painting Mode: " + this.GetActivePaintingMode().GetType().Name);
			}
		}

		// Token: 0x060010FC RID: 4348 RVA: 0x000522A0 File Offset: 0x000504A0
		private bool HasActiveTilesetIndex()
		{
			return this.activeTilesetIndex >= 0;
		}

		// Token: 0x060010FD RID: 4349 RVA: 0x000522AE File Offset: 0x000504AE
		public override void Reset()
		{
			base.Reset();
			this.painting = false;
			MonoBehaviourSingleton<MarkerManager>.Instance.ReleaseAllMarkers();
		}

		// Token: 0x060010FE RID: 4350 RVA: 0x000522C7 File Offset: 0x000504C7
		public override void CreatorExited()
		{
			base.CreatorExited();
			this.activeTilesetIndex = PaintingTool.NoSelection;
		}

		// Token: 0x06001101 RID: 4353 RVA: 0x00052350 File Offset: 0x00050550
		protected override void __initializeVariables()
		{
			base.__initializeVariables();
		}

		// Token: 0x06001102 RID: 4354 RVA: 0x00052368 File Offset: 0x00050568
		protected override void __initializeRpcs()
		{
			base.__registerRpc(787849911U, new NetworkBehaviour.RpcReceiveHandler(PaintingTool.__rpc_handler_787849911), "AttemptPainting_ServerRPC");
			base.__registerRpc(3535967038U, new NetworkBehaviour.RpcReceiveHandler(PaintingTool.__rpc_handler_3535967038), "ApplyPaintingResult_ClientRPC");
			base.__initializeRpcs();
		}

		// Token: 0x06001103 RID: 4355 RVA: 0x000523B8 File Offset: 0x000505B8
		private static void __rpc_handler_787849911(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			bool flag;
			reader.ReadValueSafe<bool>(out flag, default(FastBufferWriter.ForPrimitives));
			Vector3Int[] array = null;
			if (flag)
			{
				reader.ReadValueSafe(out array);
			}
			int num;
			ByteUnpacker.ReadValueBitPacked(reader, out num);
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((PaintingTool)target).AttemptPainting_ServerRPC(array, num, server);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06001104 RID: 4356 RVA: 0x00052464 File Offset: 0x00050664
		private static void __rpc_handler_3535967038(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			bool flag;
			reader.ReadValueSafe<bool>(out flag, default(FastBufferWriter.ForPrimitives));
			Vector3Int[] array = null;
			if (flag)
			{
				reader.ReadValueSafe(out array);
			}
			int num;
			ByteUnpacker.ReadValueBitPacked(reader, out num);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((PaintingTool)target).ApplyPaintingResult_ClientRPC(array, num);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06001105 RID: 4357 RVA: 0x00052501 File Offset: 0x00050701
		protected internal override string __getTypeName()
		{
			return "PaintingTool";
		}

		// Token: 0x04000DE4 RID: 3556
		public static int NoSelection = -1;

		// Token: 0x04000DE5 RID: 3557
		public UnityEvent<int> OnActiveTilesetIndexChanged = new UnityEvent<int>();

		// Token: 0x04000DE6 RID: 3558
		[SerializeField]
		[Tooltip("Max Void Distance is used when no object is collided against, how far out do we let the user paint. Essentially, in an empty level, how far into the void can they paint with no other props.")]
		private float maxVoidDistance = 10f;

		// Token: 0x04000DE7 RID: 3559
		[SerializeField]
		private float toolDragDeadZone = 0.01f;

		// Token: 0x04000DE8 RID: 3560
		[SerializeField]
		private int maximumPaintedCellsPerStroke = 100;

		// Token: 0x04000DE9 RID: 3561
		private Vector3Int initialCoordinate;

		// Token: 0x04000DEA RID: 3562
		private Vector3Int lastValidStrokeCoordinate;

		// Token: 0x04000DEB RID: 3563
		private int activeTilesetIndex = PaintingTool.NoSelection;

		// Token: 0x04000DEC RID: 3564
		private Vector3 previousRayOrigin;

		// Token: 0x04000DED RID: 3565
		private bool painting;

		// Token: 0x04000DEE RID: 3566
		private int paintingModeIndex;

		// Token: 0x04000DEF RID: 3567
		private CellCollectionMode[] planePaintingModes = new CellCollectionMode[]
		{
			new HorizontalPlaneCellCollectionMode(),
			new FacingPlaneCellCollectionMode(),
			new SidePlaneCellCollectionMode()
		};
	}
}
