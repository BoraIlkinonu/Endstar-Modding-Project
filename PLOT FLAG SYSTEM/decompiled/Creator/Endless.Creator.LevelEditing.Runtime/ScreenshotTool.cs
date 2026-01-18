using System;
using System.Linq;
using System.Threading.Tasks;
using Endless.Assets;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.Screenshotting;
using Endless.Shared;
using Endless.Shared.Debugging;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.LevelEditing.Runtime;

public class ScreenshotTool : EndlessTool, IBackable
{
	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	public override ToolType ToolType => ToolType.Screenshot;

	public ScreenshotOptions ScreenshotOptions { get; private set; } = new ScreenshotOptions(hideCharacter: false, hideUi: false);

	public override bool PerformsLineCast => false;

	public UnityEvent OnScreenshotRemoved { get; private set; } = new UnityEvent();

	public override void HandleSelected()
	{
		base.HandleSelected();
		MonoBehaviourSingleton<CellMarker>.Instance.SetActiveState(active: false);
		MonoBehaviourSingleton<WorldBoundaryMarker>.Instance.SetActiveState(active: false);
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "HandleSelected");
		}
		MonoBehaviourSingleton<ScreenshotAPI>.Instance.StartScreenshotMode(ScreenshotOptions);
		if (!MonoBehaviourSingleton<BackManager>.Instance.HasContext(this))
		{
			MonoBehaviourSingleton<BackManager>.Instance.ClaimContext(this);
		}
	}

	public override void HandleDeselected()
	{
		base.HandleDeselected();
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "HandleDeselected");
		}
		MonoBehaviourSingleton<CellMarker>.Instance.SetActiveState(active: true);
		MonoBehaviourSingleton<WorldBoundaryMarker>.Instance.SetActiveState(active: true);
		MonoBehaviourSingleton<ScreenshotAPI>.Instance.StopScreenshotMode();
		if (MonoBehaviourSingleton<BackManager>.Instance.HasContext(this))
		{
			MonoBehaviourSingleton<BackManager>.Instance.UnclaimContext(this);
		}
	}

	public void OnBack()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnBack");
		}
		MonoBehaviourSingleton<ToolManager>.Instance.SetActiveTool(ToolType.Empty);
	}

	public void SetHideCharacter(bool hide)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "SetHideCharacter", hide);
		}
		ScreenshotOptions = new ScreenshotOptions(hide, ScreenshotOptions.HideUi);
		MonoBehaviourSingleton<ScreenshotAPI>.Instance.SetupScreenshotOptions(ScreenshotOptions);
	}

	public void SetHideUi(bool hide)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "SetHideCharacter", hide);
		}
		ScreenshotOptions = new ScreenshotOptions(ScreenshotOptions.HideCharacter, hide);
		MonoBehaviourSingleton<ScreenshotAPI>.Instance.SetupScreenshotOptions(ScreenshotOptions);
	}

	[ServerRpc(RequireOwnership = false)]
	public void AddScreenshotsToLevel_ServerRPC(ScreenshotFileInstances screenshotFileInstance, ServerRpcParams serverRpcParams = default(ServerRpcParams))
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			FastBufferWriter bufferWriter = __beginSendServerRpc(1879625076u, serverRpcParams, RpcDelivery.Reliable);
			bool value = screenshotFileInstance != null;
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			if (value)
			{
				bufferWriter.WriteValueSafe(in screenshotFileInstance, default(FastBufferWriter.ForNetworkSerializable));
			}
			__endSendServerRpc(ref bufferWriter, 1879625076u, serverRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			if (verboseLogging)
			{
				DebugUtility.LogMethod(this, "AddScreenshotsToLevel_ServerRPC", screenshotFileInstance, serverRpcParams);
			}
			AddScreenshotsToLevelTask(screenshotFileInstance, serverRpcParams);
		}
	}

	private async Task AddScreenshotsToLevelTask(ScreenshotFileInstances screenshotFileInstance, ServerRpcParams serverRpcParams)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "AddScreenshotsToLevelTask", screenshotFileInstance, serverRpcParams);
		}
		if (!base.IsServer)
		{
			return;
		}
		if (!NetworkBehaviourSingleton<UserIdManager>.Instance.TryGetUserId(serverRpcParams.Receive.SenderClientId, out var userId))
		{
			Debug.LogException(new Exception($"Unable to determine User Id for Client Id: {serverRpcParams.Receive.SenderClientId}"));
		}
		else if (await NetworkBehaviourSingleton<CreatorManager>.Instance.UserCanEditLevel(userId))
		{
			MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RevisionMetaData.RevisionTimestamp = DateTime.UtcNow.Ticks;
			MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RevisionMetaData.Changes.Add(new ChangeData
			{
				ChangeType = ChangeType.LevelScreenshotsAdded,
				UserId = userId
			});
			AddScreenshotsToLevel_ClientRpc(screenshotFileInstance);
			if (!base.IsClient)
			{
				AddScreenshotsToLevel(screenshotFileInstance);
			}
			NetworkBehaviourSingleton<CreatorManager>.Instance.SaveLoadManager.SaveLevel();
		}
	}

	[ClientRpc]
	private void AddScreenshotsToLevel_ClientRpc(ScreenshotFileInstances screenshotFileInstance)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			ClientRpcParams clientRpcParams = default(ClientRpcParams);
			FastBufferWriter bufferWriter = __beginSendClientRpc(492376081u, clientRpcParams, RpcDelivery.Reliable);
			bool value = screenshotFileInstance != null;
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			if (value)
			{
				bufferWriter.WriteValueSafe(in screenshotFileInstance, default(FastBufferWriter.ForNetworkSerializable));
			}
			__endSendClientRpc(ref bufferWriter, 492376081u, clientRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			if (verboseLogging)
			{
				DebugUtility.LogMethod(this, "AddScreenshotsToLevel_ClientRpc", screenshotFileInstance);
			}
			AddScreenshotsToLevel(screenshotFileInstance);
		}
	}

	private void AddScreenshotsToLevel(ScreenshotFileInstances screenshotFileInstance)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "AddScreenshotsToLevel", screenshotFileInstance);
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

	[ServerRpc(RequireOwnership = false)]
	public void RemoveScreenshotFromLevel_ServerRPC(int index, ServerRpcParams serverRpcParams = default(ServerRpcParams))
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			FastBufferWriter bufferWriter = __beginSendServerRpc(2184509282u, serverRpcParams, RpcDelivery.Reliable);
			BytePacker.WriteValueBitPacked(bufferWriter, index);
			__endSendServerRpc(ref bufferWriter, 2184509282u, serverRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			if (verboseLogging)
			{
				DebugUtility.LogMethod(this, "RemoveScreenshotFromLevel_ServerRPC", index, serverRpcParams);
			}
			RemoveScreenshotFromLevelTask(index, serverRpcParams);
		}
	}

	private async Task RemoveScreenshotFromLevelTask(int index, ServerRpcParams serverRpcParams)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "RemoveScreenshotFromLevelTask", index, serverRpcParams);
		}
		if (!base.IsServer)
		{
			return;
		}
		if (!NetworkBehaviourSingleton<UserIdManager>.Instance.TryGetUserId(serverRpcParams.Receive.SenderClientId, out var userId))
		{
			Debug.LogException(new Exception($"Unable to determine User Id for Client Id: {serverRpcParams.Receive.SenderClientId}"));
		}
		else if (await NetworkBehaviourSingleton<CreatorManager>.Instance.UserCanEditLevel(userId))
		{
			MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RevisionMetaData.RevisionTimestamp = DateTime.UtcNow.Ticks;
			MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RevisionMetaData.Changes.Remove(new ChangeData
			{
				ChangeType = ChangeType.LevelScreenshotsRemoved,
				UserId = userId
			});
			RemoveScreenshotFromLevel_ClientRpc(index);
			if (!base.IsClient)
			{
				RemoveScreenshotFromLevel(index);
			}
			NetworkBehaviourSingleton<CreatorManager>.Instance.SaveLoadManager.SaveLevel();
		}
	}

	[ClientRpc]
	private void RemoveScreenshotFromLevel_ClientRpc(int index)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			ClientRpcParams clientRpcParams = default(ClientRpcParams);
			FastBufferWriter bufferWriter = __beginSendClientRpc(945843350u, clientRpcParams, RpcDelivery.Reliable);
			BytePacker.WriteValueBitPacked(bufferWriter, index);
			__endSendClientRpc(ref bufferWriter, 945843350u, clientRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			if (verboseLogging)
			{
				DebugUtility.LogMethod(this, "RemoveScreenshotFromLevel_ClientRpc", index);
			}
			RemoveScreenshotFromLevel(index);
		}
	}

	private void RemoveScreenshotFromLevel(int index)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "RemoveScreenshotFromLevel", index);
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

	[ServerRpc(RequireOwnership = false)]
	public void RearrangeScreenshotsToLevel_ServerRPC(ScreenshotFileInstances[] newOrder, ServerRpcParams serverRpcParams = default(ServerRpcParams))
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			FastBufferWriter bufferWriter = __beginSendServerRpc(3203229569u, serverRpcParams, RpcDelivery.Reliable);
			bool value = newOrder != null;
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			if (value)
			{
				bufferWriter.WriteValueSafe(newOrder, default(FastBufferWriter.ForNetworkSerializable));
			}
			__endSendServerRpc(ref bufferWriter, 3203229569u, serverRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			if (verboseLogging)
			{
				DebugUtility.LogMethod(this, "RearrangeScreenshotsToLevel_ServerRPC", newOrder.Length, serverRpcParams);
			}
			RearrangeScreenshotsToLevelTask(newOrder, serverRpcParams);
		}
	}

	private async Task RearrangeScreenshotsToLevelTask(ScreenshotFileInstances[] newOrder, ServerRpcParams serverRpcParams)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "RearrangeScreenshotsToLevelTask", newOrder.Length, serverRpcParams);
		}
		if (!base.IsServer)
		{
			return;
		}
		if (!NetworkBehaviourSingleton<UserIdManager>.Instance.TryGetUserId(serverRpcParams.Receive.SenderClientId, out var userId))
		{
			Debug.LogException(new Exception($"Unable to determine User Id for Client Id: {serverRpcParams.Receive.SenderClientId}"));
		}
		else if (await NetworkBehaviourSingleton<CreatorManager>.Instance.UserCanEditLevel(userId))
		{
			MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RevisionMetaData.RevisionTimestamp = DateTime.UtcNow.Ticks;
			MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RevisionMetaData.Changes.Add(new ChangeData
			{
				ChangeType = ChangeType.LevelScreenshotsReorder,
				UserId = userId
			});
			RearrangeScreenshotsToLevel_ClientRpc(newOrder);
			if (!base.IsClient)
			{
				RearrangeScreenshotsToLevel(newOrder);
			}
			NetworkBehaviourSingleton<CreatorManager>.Instance.SaveLoadManager.SaveLevel();
		}
	}

	[ClientRpc]
	private void RearrangeScreenshotsToLevel_ClientRpc(ScreenshotFileInstances[] newOrder)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			ClientRpcParams clientRpcParams = default(ClientRpcParams);
			FastBufferWriter bufferWriter = __beginSendClientRpc(1804873295u, clientRpcParams, RpcDelivery.Reliable);
			bool value = newOrder != null;
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			if (value)
			{
				bufferWriter.WriteValueSafe(newOrder, default(FastBufferWriter.ForNetworkSerializable));
			}
			__endSendClientRpc(ref bufferWriter, 1804873295u, clientRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			if (verboseLogging)
			{
				DebugUtility.LogMethod(this, "RearrangeScreenshotsToLevel_ClientRpc", newOrder.Length);
			}
			RearrangeScreenshotsToLevel(newOrder);
		}
	}

	private void RearrangeScreenshotsToLevel(ScreenshotFileInstances[] newOrder)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "RearrangeScreenshotsToLevel", newOrder.Length);
		}
		if (NetworkBehaviourSingleton<CreatorManager>.Instance.RpcReceiveState == RpcReceiveState.Allow)
		{
			MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.Screenshots = newOrder.ToList();
			return;
		}
		NetworkBehaviourSingleton<CreatorManager>.Instance.AddCachedRpc(delegate
		{
			MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.Screenshots = newOrder.ToList();
		});
	}

	protected override void __initializeVariables()
	{
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		__registerRpc(1879625076u, __rpc_handler_1879625076, "AddScreenshotsToLevel_ServerRPC");
		__registerRpc(492376081u, __rpc_handler_492376081, "AddScreenshotsToLevel_ClientRpc");
		__registerRpc(2184509282u, __rpc_handler_2184509282, "RemoveScreenshotFromLevel_ServerRPC");
		__registerRpc(945843350u, __rpc_handler_945843350, "RemoveScreenshotFromLevel_ClientRpc");
		__registerRpc(3203229569u, __rpc_handler_3203229569, "RearrangeScreenshotsToLevel_ServerRPC");
		__registerRpc(1804873295u, __rpc_handler_1804873295, "RearrangeScreenshotsToLevel_ClientRpc");
		base.__initializeRpcs();
	}

	private static void __rpc_handler_1879625076(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out bool value, default(FastBufferWriter.ForPrimitives));
			ScreenshotFileInstances value2 = null;
			if (value)
			{
				reader.ReadValueSafe(out value2, default(FastBufferWriter.ForNetworkSerializable));
			}
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((ScreenshotTool)target).AddScreenshotsToLevel_ServerRPC(value2, server);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_492376081(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out bool value, default(FastBufferWriter.ForPrimitives));
			ScreenshotFileInstances value2 = null;
			if (value)
			{
				reader.ReadValueSafe(out value2, default(FastBufferWriter.ForNetworkSerializable));
			}
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((ScreenshotTool)target).AddScreenshotsToLevel_ClientRpc(value2);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_2184509282(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			ByteUnpacker.ReadValueBitPacked(reader, out int value);
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((ScreenshotTool)target).RemoveScreenshotFromLevel_ServerRPC(value, server);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_945843350(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			ByteUnpacker.ReadValueBitPacked(reader, out int value);
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((ScreenshotTool)target).RemoveScreenshotFromLevel_ClientRpc(value);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_3203229569(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out bool value, default(FastBufferWriter.ForPrimitives));
			ScreenshotFileInstances[] value2 = null;
			if (value)
			{
				reader.ReadValueSafe(out value2, default(FastBufferWriter.ForNetworkSerializable));
			}
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((ScreenshotTool)target).RearrangeScreenshotsToLevel_ServerRPC(value2, server);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_1804873295(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out bool value, default(FastBufferWriter.ForPrimitives));
			ScreenshotFileInstances[] value2 = null;
			if (value)
			{
				reader.ReadValueSafe(out value2, default(FastBufferWriter.ForNetworkSerializable));
			}
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((ScreenshotTool)target).RearrangeScreenshotsToLevel_ClientRpc(value2);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	protected internal override string __getTypeName()
	{
		return "ScreenshotTool";
	}
}
