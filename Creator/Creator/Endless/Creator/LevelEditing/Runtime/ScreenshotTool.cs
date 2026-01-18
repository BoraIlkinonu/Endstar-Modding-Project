using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Endless.Assets;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.Screenshotting;
using Endless.Shared;
using Endless.Shared.Debugging;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.LevelEditing.Runtime
{
	// Token: 0x0200038D RID: 909
	public class ScreenshotTool : EndlessTool, IBackable
	{
		// Token: 0x17000298 RID: 664
		// (get) Token: 0x06001187 RID: 4487 RVA: 0x000558EA File Offset: 0x00053AEA
		public override ToolType ToolType
		{
			get
			{
				return ToolType.Screenshot;
			}
		}

		// Token: 0x17000299 RID: 665
		// (get) Token: 0x06001188 RID: 4488 RVA: 0x000558EE File Offset: 0x00053AEE
		// (set) Token: 0x06001189 RID: 4489 RVA: 0x000558F6 File Offset: 0x00053AF6
		public ScreenshotOptions ScreenshotOptions { get; private set; } = new ScreenshotOptions(false, false, true);

		// Token: 0x1700029A RID: 666
		// (get) Token: 0x0600118A RID: 4490 RVA: 0x0001BF89 File Offset: 0x0001A189
		public override bool PerformsLineCast
		{
			get
			{
				return false;
			}
		}

		// Token: 0x1700029B RID: 667
		// (get) Token: 0x0600118B RID: 4491 RVA: 0x000558FF File Offset: 0x00053AFF
		// (set) Token: 0x0600118C RID: 4492 RVA: 0x00055907 File Offset: 0x00053B07
		public UnityEvent OnScreenshotRemoved { get; private set; } = new UnityEvent();

		// Token: 0x0600118D RID: 4493 RVA: 0x00055910 File Offset: 0x00053B10
		public override void HandleSelected()
		{
			base.HandleSelected();
			MonoBehaviourSingleton<CellMarker>.Instance.SetActiveState(false);
			MonoBehaviourSingleton<WorldBoundaryMarker>.Instance.SetActiveState(false);
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "HandleSelected", Array.Empty<object>());
			}
			MonoBehaviourSingleton<ScreenshotAPI>.Instance.StartScreenshotMode(this.ScreenshotOptions);
			if (!MonoBehaviourSingleton<BackManager>.Instance.HasContext(this))
			{
				MonoBehaviourSingleton<BackManager>.Instance.ClaimContext(this);
			}
		}

		// Token: 0x0600118E RID: 4494 RVA: 0x0005597C File Offset: 0x00053B7C
		public override void HandleDeselected()
		{
			base.HandleDeselected();
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "HandleDeselected", Array.Empty<object>());
			}
			MonoBehaviourSingleton<CellMarker>.Instance.SetActiveState(true);
			MonoBehaviourSingleton<WorldBoundaryMarker>.Instance.SetActiveState(true);
			MonoBehaviourSingleton<ScreenshotAPI>.Instance.StopScreenshotMode();
			if (MonoBehaviourSingleton<BackManager>.Instance.HasContext(this))
			{
				MonoBehaviourSingleton<BackManager>.Instance.UnclaimContext(this);
			}
		}

		// Token: 0x0600118F RID: 4495 RVA: 0x000559DF File Offset: 0x00053BDF
		public void OnBack()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnBack", Array.Empty<object>());
			}
			MonoBehaviourSingleton<ToolManager>.Instance.SetActiveTool(ToolType.Empty);
		}

		// Token: 0x06001190 RID: 4496 RVA: 0x00055A04 File Offset: 0x00053C04
		public void SetHideCharacter(bool hide)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetHideCharacter", new object[] { hide });
			}
			this.ScreenshotOptions = new ScreenshotOptions(hide, this.ScreenshotOptions.HideUi, true);
			MonoBehaviourSingleton<ScreenshotAPI>.Instance.SetupScreenshotOptions(this.ScreenshotOptions);
		}

		// Token: 0x06001191 RID: 4497 RVA: 0x00055A5C File Offset: 0x00053C5C
		public void SetHideUi(bool hide)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetHideCharacter", new object[] { hide });
			}
			this.ScreenshotOptions = new ScreenshotOptions(this.ScreenshotOptions.HideCharacter, hide, true);
			MonoBehaviourSingleton<ScreenshotAPI>.Instance.SetupScreenshotOptions(this.ScreenshotOptions);
		}

		// Token: 0x06001192 RID: 4498 RVA: 0x00055AB4 File Offset: 0x00053CB4
		[ServerRpc(RequireOwnership = false)]
		public void AddScreenshotsToLevel_ServerRPC(ScreenshotFileInstances screenshotFileInstance, ServerRpcParams serverRpcParams = default(ServerRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendServerRpc(1879625076U, serverRpcParams, RpcDelivery.Reliable);
				bool flag = screenshotFileInstance != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag, default(FastBufferWriter.ForPrimitives));
				if (flag)
				{
					fastBufferWriter.WriteValueSafe<ScreenshotFileInstances>(in screenshotFileInstance, default(FastBufferWriter.ForNetworkSerializable));
				}
				base.__endSendServerRpc(ref fastBufferWriter, 1879625076U, serverRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "AddScreenshotsToLevel_ServerRPC", new object[] { screenshotFileInstance, serverRpcParams });
			}
			this.AddScreenshotsToLevelTask(screenshotFileInstance, serverRpcParams);
		}

		// Token: 0x06001193 RID: 4499 RVA: 0x00055C04 File Offset: 0x00053E04
		private async Task AddScreenshotsToLevelTask(ScreenshotFileInstances screenshotFileInstance, ServerRpcParams serverRpcParams)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "AddScreenshotsToLevelTask", new object[] { screenshotFileInstance, serverRpcParams });
			}
			if (base.IsServer)
			{
				int userId;
				if (!NetworkBehaviourSingleton<UserIdManager>.Instance.TryGetUserId(serverRpcParams.Receive.SenderClientId, out userId))
				{
					Debug.LogException(new Exception(string.Format("Unable to determine User Id for Client Id: {0}", serverRpcParams.Receive.SenderClientId)));
				}
				else
				{
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
						MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RevisionMetaData.RevisionTimestamp = DateTime.UtcNow.Ticks;
						MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RevisionMetaData.Changes.Add(new ChangeData
						{
							ChangeType = ChangeType.LevelScreenshotsAdded,
							UserId = userId
						});
						this.AddScreenshotsToLevel_ClientRpc(screenshotFileInstance);
						if (!base.IsClient)
						{
							this.AddScreenshotsToLevel(screenshotFileInstance);
						}
						NetworkBehaviourSingleton<CreatorManager>.Instance.SaveLoadManager.SaveLevel();
					}
				}
			}
		}

		// Token: 0x06001194 RID: 4500 RVA: 0x00055C58 File Offset: 0x00053E58
		[ClientRpc]
		private void AddScreenshotsToLevel_ClientRpc(ScreenshotFileInstances screenshotFileInstance)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(492376081U, clientRpcParams, RpcDelivery.Reliable);
				bool flag = screenshotFileInstance != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag, default(FastBufferWriter.ForPrimitives));
				if (flag)
				{
					fastBufferWriter.WriteValueSafe<ScreenshotFileInstances>(in screenshotFileInstance, default(FastBufferWriter.ForNetworkSerializable));
				}
				base.__endSendClientRpc(ref fastBufferWriter, 492376081U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "AddScreenshotsToLevel_ClientRpc", new object[] { screenshotFileInstance });
			}
			this.AddScreenshotsToLevel(screenshotFileInstance);
		}

		// Token: 0x06001195 RID: 4501 RVA: 0x00055D9C File Offset: 0x00053F9C
		private void AddScreenshotsToLevel(ScreenshotFileInstances screenshotFileInstance)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "AddScreenshotsToLevel", new object[] { screenshotFileInstance });
			}
			if (NetworkBehaviourSingleton<CreatorManager>.Instance.RpcReceiveState == RpcReceiveState.Allow)
			{
				MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.Screenshots.Add(screenshotFileInstance);
				return;
			}
			NetworkBehaviourSingleton<CreatorManager>.Instance.AddCachedRpc(delegate
			{
				MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.Screenshots.Add(screenshotFileInstance);
			});
		}

		// Token: 0x06001196 RID: 4502 RVA: 0x00055E1C File Offset: 0x0005401C
		[ServerRpc(RequireOwnership = false)]
		public void RemoveScreenshotFromLevel_ServerRPC(int index, ServerRpcParams serverRpcParams = default(ServerRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendServerRpc(2184509282U, serverRpcParams, RpcDelivery.Reliable);
				BytePacker.WriteValueBitPacked(fastBufferWriter, index);
				base.__endSendServerRpc(ref fastBufferWriter, 2184509282U, serverRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "RemoveScreenshotFromLevel_ServerRPC", new object[] { index, serverRpcParams });
			}
			this.RemoveScreenshotFromLevelTask(index, serverRpcParams);
		}

		// Token: 0x06001197 RID: 4503 RVA: 0x00055F30 File Offset: 0x00054130
		private async Task RemoveScreenshotFromLevelTask(int index, ServerRpcParams serverRpcParams)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "RemoveScreenshotFromLevelTask", new object[] { index, serverRpcParams });
			}
			if (base.IsServer)
			{
				int userId;
				if (!NetworkBehaviourSingleton<UserIdManager>.Instance.TryGetUserId(serverRpcParams.Receive.SenderClientId, out userId))
				{
					Debug.LogException(new Exception(string.Format("Unable to determine User Id for Client Id: {0}", serverRpcParams.Receive.SenderClientId)));
				}
				else
				{
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
						MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RevisionMetaData.RevisionTimestamp = DateTime.UtcNow.Ticks;
						MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RevisionMetaData.Changes.Remove(new ChangeData
						{
							ChangeType = ChangeType.LevelScreenshotsRemoved,
							UserId = userId
						});
						this.RemoveScreenshotFromLevel_ClientRpc(index);
						if (!base.IsClient)
						{
							this.RemoveScreenshotFromLevel(index);
						}
						NetworkBehaviourSingleton<CreatorManager>.Instance.SaveLoadManager.SaveLevel();
					}
				}
			}
		}

		// Token: 0x06001198 RID: 4504 RVA: 0x00055F84 File Offset: 0x00054184
		[ClientRpc]
		private void RemoveScreenshotFromLevel_ClientRpc(int index)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(945843350U, clientRpcParams, RpcDelivery.Reliable);
				BytePacker.WriteValueBitPacked(fastBufferWriter, index);
				base.__endSendClientRpc(ref fastBufferWriter, 945843350U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "RemoveScreenshotFromLevel_ClientRpc", new object[] { index });
			}
			this.RemoveScreenshotFromLevel(index);
		}

		// Token: 0x06001199 RID: 4505 RVA: 0x00056090 File Offset: 0x00054290
		private void RemoveScreenshotFromLevel(int index)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "RemoveScreenshotFromLevel", new object[] { index });
			}
			if (NetworkBehaviourSingleton<CreatorManager>.Instance.RpcReceiveState == RpcReceiveState.Allow)
			{
				MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.Screenshots.RemoveAt(index);
				return;
			}
			NetworkBehaviourSingleton<CreatorManager>.Instance.AddCachedRpc(delegate
			{
				MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.Screenshots.RemoveAt(index);
			});
		}

		// Token: 0x0600119A RID: 4506 RVA: 0x00056114 File Offset: 0x00054314
		[ServerRpc(RequireOwnership = false)]
		public void RearrangeScreenshotsToLevel_ServerRPC(ScreenshotFileInstances[] newOrder, ServerRpcParams serverRpcParams = default(ServerRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendServerRpc(3203229569U, serverRpcParams, RpcDelivery.Reliable);
				bool flag = newOrder != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag, default(FastBufferWriter.ForPrimitives));
				if (flag)
				{
					fastBufferWriter.WriteValueSafe<ScreenshotFileInstances>(newOrder, default(FastBufferWriter.ForNetworkSerializable));
				}
				base.__endSendServerRpc(ref fastBufferWriter, 3203229569U, serverRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "RearrangeScreenshotsToLevel_ServerRPC", new object[] { newOrder.Length, serverRpcParams });
			}
			this.RearrangeScreenshotsToLevelTask(newOrder, serverRpcParams);
		}

		// Token: 0x0600119B RID: 4507 RVA: 0x00056268 File Offset: 0x00054468
		private async Task RearrangeScreenshotsToLevelTask(ScreenshotFileInstances[] newOrder, ServerRpcParams serverRpcParams)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "RearrangeScreenshotsToLevelTask", new object[] { newOrder.Length, serverRpcParams });
			}
			if (base.IsServer)
			{
				int userId;
				if (!NetworkBehaviourSingleton<UserIdManager>.Instance.TryGetUserId(serverRpcParams.Receive.SenderClientId, out userId))
				{
					Debug.LogException(new Exception(string.Format("Unable to determine User Id for Client Id: {0}", serverRpcParams.Receive.SenderClientId)));
				}
				else
				{
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
						MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RevisionMetaData.RevisionTimestamp = DateTime.UtcNow.Ticks;
						MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RevisionMetaData.Changes.Add(new ChangeData
						{
							ChangeType = ChangeType.LevelScreenshotsReorder,
							UserId = userId
						});
						this.RearrangeScreenshotsToLevel_ClientRpc(newOrder);
						if (!base.IsClient)
						{
							this.RearrangeScreenshotsToLevel(newOrder);
						}
						NetworkBehaviourSingleton<CreatorManager>.Instance.SaveLoadManager.SaveLevel();
					}
				}
			}
		}

		// Token: 0x0600119C RID: 4508 RVA: 0x000562BC File Offset: 0x000544BC
		[ClientRpc]
		private void RearrangeScreenshotsToLevel_ClientRpc(ScreenshotFileInstances[] newOrder)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(1804873295U, clientRpcParams, RpcDelivery.Reliable);
				bool flag = newOrder != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag, default(FastBufferWriter.ForPrimitives));
				if (flag)
				{
					fastBufferWriter.WriteValueSafe<ScreenshotFileInstances>(newOrder, default(FastBufferWriter.ForNetworkSerializable));
				}
				base.__endSendClientRpc(ref fastBufferWriter, 1804873295U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "RearrangeScreenshotsToLevel_ClientRpc", new object[] { newOrder.Length });
			}
			this.RearrangeScreenshotsToLevel(newOrder);
		}

		// Token: 0x0600119D RID: 4509 RVA: 0x00056408 File Offset: 0x00054608
		private void RearrangeScreenshotsToLevel(ScreenshotFileInstances[] newOrder)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "RearrangeScreenshotsToLevel", new object[] { newOrder.Length });
			}
			if (NetworkBehaviourSingleton<CreatorManager>.Instance.RpcReceiveState == RpcReceiveState.Allow)
			{
				MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.Screenshots = newOrder.ToList<ScreenshotFileInstances>();
				return;
			}
			NetworkBehaviourSingleton<CreatorManager>.Instance.AddCachedRpc(delegate
			{
				MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.Screenshots = newOrder.ToList<ScreenshotFileInstances>();
			});
		}

		// Token: 0x0600119F RID: 4511 RVA: 0x000564B0 File Offset: 0x000546B0
		protected override void __initializeVariables()
		{
			base.__initializeVariables();
		}

		// Token: 0x060011A0 RID: 4512 RVA: 0x000564C8 File Offset: 0x000546C8
		protected override void __initializeRpcs()
		{
			base.__registerRpc(1879625076U, new NetworkBehaviour.RpcReceiveHandler(ScreenshotTool.__rpc_handler_1879625076), "AddScreenshotsToLevel_ServerRPC");
			base.__registerRpc(492376081U, new NetworkBehaviour.RpcReceiveHandler(ScreenshotTool.__rpc_handler_492376081), "AddScreenshotsToLevel_ClientRpc");
			base.__registerRpc(2184509282U, new NetworkBehaviour.RpcReceiveHandler(ScreenshotTool.__rpc_handler_2184509282), "RemoveScreenshotFromLevel_ServerRPC");
			base.__registerRpc(945843350U, new NetworkBehaviour.RpcReceiveHandler(ScreenshotTool.__rpc_handler_945843350), "RemoveScreenshotFromLevel_ClientRpc");
			base.__registerRpc(3203229569U, new NetworkBehaviour.RpcReceiveHandler(ScreenshotTool.__rpc_handler_3203229569), "RearrangeScreenshotsToLevel_ServerRPC");
			base.__registerRpc(1804873295U, new NetworkBehaviour.RpcReceiveHandler(ScreenshotTool.__rpc_handler_1804873295), "RearrangeScreenshotsToLevel_ClientRpc");
			base.__initializeRpcs();
		}

		// Token: 0x060011A1 RID: 4513 RVA: 0x00056588 File Offset: 0x00054788
		private static void __rpc_handler_1879625076(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			bool flag;
			reader.ReadValueSafe<bool>(out flag, default(FastBufferWriter.ForPrimitives));
			ScreenshotFileInstances screenshotFileInstances = null;
			if (flag)
			{
				reader.ReadValueSafe<ScreenshotFileInstances>(out screenshotFileInstances, default(FastBufferWriter.ForNetworkSerializable));
			}
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((ScreenshotTool)target).AddScreenshotsToLevel_ServerRPC(screenshotFileInstances, server);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x060011A2 RID: 4514 RVA: 0x00056630 File Offset: 0x00054830
		private static void __rpc_handler_492376081(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			bool flag;
			reader.ReadValueSafe<bool>(out flag, default(FastBufferWriter.ForPrimitives));
			ScreenshotFileInstances screenshotFileInstances = null;
			if (flag)
			{
				reader.ReadValueSafe<ScreenshotFileInstances>(out screenshotFileInstances, default(FastBufferWriter.ForNetworkSerializable));
			}
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((ScreenshotTool)target).AddScreenshotsToLevel_ClientRpc(screenshotFileInstances);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x060011A3 RID: 4515 RVA: 0x000566CC File Offset: 0x000548CC
		private static void __rpc_handler_2184509282(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			int num;
			ByteUnpacker.ReadValueBitPacked(reader, out num);
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((ScreenshotTool)target).RemoveScreenshotFromLevel_ServerRPC(num, server);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x060011A4 RID: 4516 RVA: 0x0005673C File Offset: 0x0005493C
		private static void __rpc_handler_945843350(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			int num;
			ByteUnpacker.ReadValueBitPacked(reader, out num);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((ScreenshotTool)target).RemoveScreenshotFromLevel_ClientRpc(num);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x060011A5 RID: 4517 RVA: 0x000567A0 File Offset: 0x000549A0
		private static void __rpc_handler_3203229569(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			bool flag;
			reader.ReadValueSafe<bool>(out flag, default(FastBufferWriter.ForPrimitives));
			ScreenshotFileInstances[] array = null;
			if (flag)
			{
				reader.ReadValueSafe<ScreenshotFileInstances>(out array, default(FastBufferWriter.ForNetworkSerializable));
			}
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((ScreenshotTool)target).RearrangeScreenshotsToLevel_ServerRPC(array, server);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x060011A6 RID: 4518 RVA: 0x00056848 File Offset: 0x00054A48
		private static void __rpc_handler_1804873295(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			bool flag;
			reader.ReadValueSafe<bool>(out flag, default(FastBufferWriter.ForPrimitives));
			ScreenshotFileInstances[] array = null;
			if (flag)
			{
				reader.ReadValueSafe<ScreenshotFileInstances>(out array, default(FastBufferWriter.ForNetworkSerializable));
			}
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((ScreenshotTool)target).RearrangeScreenshotsToLevel_ClientRpc(array);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x060011A7 RID: 4519 RVA: 0x000568E2 File Offset: 0x00054AE2
		protected internal override string __getTypeName()
		{
			return "ScreenshotTool";
		}

		// Token: 0x04000E63 RID: 3683
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
