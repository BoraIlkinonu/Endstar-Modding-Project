using System;
using System.Collections.Generic;
using Endless.Matchmaking;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Netcode;

namespace Endless.Gameplay.Scripting;

public class UserScriptingConsole : NetworkBehaviourSingleton<UserScriptingConsole>
{
	private Dictionary<SerializableGuid, List<ConsoleMessage>> instanceIdSortedMessageMap = new Dictionary<SerializableGuid, List<ConsoleMessage>>();

	private Dictionary<SerializableGuid, List<ConsoleMessage>> assetIdSortedMessageMap = new Dictionary<SerializableGuid, List<ConsoleMessage>>();

	private List<ConsoleMessage> messages = new List<ConsoleMessage>();

	private string projectId;

	public event Action<ConsoleMessage, List<ConsoleMessage>> OnMessageReceived;

	public event Action OnMessagesCleared;

	private void Start()
	{
		MatchSession.OnMatchSessionStart += HandleMatchSessionStarted;
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		MatchSession.OnMatchSessionStart -= HandleMatchSessionStarted;
	}

	private void HandleMatchSessionStarted(MatchSession session)
	{
		if ((SerializableGuid)projectId != session.ProjectId)
		{
			ClearMessages();
			projectId = session.ProjectId;
		}
	}

	public IReadOnlyList<ConsoleMessage> GetConsoleMessages()
	{
		return messages;
	}

	public IReadOnlyList<ConsoleMessage> GetConsoleMessagesForInstanceId(SerializableGuid instanceId)
	{
		if (instanceIdSortedMessageMap.TryGetValue(instanceId, out var value))
		{
			return value;
		}
		return new List<ConsoleMessage>();
	}

	public IReadOnlyList<ConsoleMessage> GetConsoleMessagesForAssetId(SerializableGuid assetId)
	{
		if (assetIdSortedMessageMap.TryGetValue(assetId, out var value))
		{
			return value;
		}
		return new List<ConsoleMessage>();
	}

	public void AddMessage(ConsoleMessage consoleMessage)
	{
		AddMessage_ClientRpc(consoleMessage);
	}

	[ClientRpc]
	private void AddMessage_ClientRpc(ConsoleMessage consoleMessage)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			ClientRpcParams clientRpcParams = default(ClientRpcParams);
			FastBufferWriter bufferWriter = __beginSendClientRpc(57075717u, clientRpcParams, RpcDelivery.Reliable);
			bool value = consoleMessage != null;
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			if (value)
			{
				bufferWriter.WriteValueSafe(in consoleMessage, default(FastBufferWriter.ForNetworkSerializable));
			}
			__endSendClientRpc(ref bufferWriter, 57075717u, clientRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			AddMessageLocal(consoleMessage);
		}
	}

	private void AddMessageLocal(ConsoleMessage consoleMessage)
	{
		instanceIdSortedMessageMap.TryAdd(consoleMessage.InstanceId, new List<ConsoleMessage>());
		instanceIdSortedMessageMap[consoleMessage.InstanceId].Add(consoleMessage);
		assetIdSortedMessageMap.TryAdd(consoleMessage.AssetId, new List<ConsoleMessage>());
		assetIdSortedMessageMap[consoleMessage.AssetId].Add(consoleMessage);
		messages.Add(consoleMessage);
		this.OnMessageReceived?.Invoke(consoleMessage, messages);
	}

	public void ClearMessages()
	{
		instanceIdSortedMessageMap.Clear();
		messages.Clear();
		this.OnMessagesCleared?.Invoke();
	}

	protected override void __initializeVariables()
	{
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		__registerRpc(57075717u, __rpc_handler_57075717, "AddMessage_ClientRpc");
		base.__initializeRpcs();
	}

	private static void __rpc_handler_57075717(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out bool value, default(FastBufferWriter.ForPrimitives));
			ConsoleMessage value2 = null;
			if (value)
			{
				reader.ReadValueSafe(out value2, default(FastBufferWriter.ForNetworkSerializable));
			}
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((UserScriptingConsole)target).AddMessage_ClientRpc(value2);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	protected internal override string __getTypeName()
	{
		return "UserScriptingConsole";
	}
}
