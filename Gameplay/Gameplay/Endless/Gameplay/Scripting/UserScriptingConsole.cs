using System;
using System.Collections.Generic;
using Endless.Matchmaking;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Netcode;

namespace Endless.Gameplay.Scripting
{
	// Token: 0x020004B2 RID: 1202
	public class UserScriptingConsole : NetworkBehaviourSingleton<UserScriptingConsole>
	{
		// Token: 0x14000044 RID: 68
		// (add) Token: 0x06001DC9 RID: 7625 RVA: 0x00082CC8 File Offset: 0x00080EC8
		// (remove) Token: 0x06001DCA RID: 7626 RVA: 0x00082D00 File Offset: 0x00080F00
		public event Action<ConsoleMessage, List<ConsoleMessage>> OnMessageReceived;

		// Token: 0x14000045 RID: 69
		// (add) Token: 0x06001DCB RID: 7627 RVA: 0x00082D38 File Offset: 0x00080F38
		// (remove) Token: 0x06001DCC RID: 7628 RVA: 0x00082D70 File Offset: 0x00080F70
		public event Action OnMessagesCleared;

		// Token: 0x06001DCD RID: 7629 RVA: 0x00082DA5 File Offset: 0x00080FA5
		private void Start()
		{
			MatchSession.OnMatchSessionStart += this.HandleMatchSessionStarted;
		}

		// Token: 0x06001DCE RID: 7630 RVA: 0x00082DB8 File Offset: 0x00080FB8
		public override void OnDestroy()
		{
			base.OnDestroy();
			MatchSession.OnMatchSessionStart -= this.HandleMatchSessionStarted;
		}

		// Token: 0x06001DCF RID: 7631 RVA: 0x00082DD1 File Offset: 0x00080FD1
		private void HandleMatchSessionStarted(MatchSession session)
		{
			if (this.projectId != session.ProjectId)
			{
				this.ClearMessages();
				this.projectId = session.ProjectId;
			}
		}

		// Token: 0x06001DD0 RID: 7632 RVA: 0x00082E02 File Offset: 0x00081002
		public IReadOnlyList<ConsoleMessage> GetConsoleMessages()
		{
			return this.messages;
		}

		// Token: 0x06001DD1 RID: 7633 RVA: 0x00082E0C File Offset: 0x0008100C
		public IReadOnlyList<ConsoleMessage> GetConsoleMessagesForInstanceId(SerializableGuid instanceId)
		{
			List<ConsoleMessage> list;
			if (this.instanceIdSortedMessageMap.TryGetValue(instanceId, out list))
			{
				return list;
			}
			return new List<ConsoleMessage>();
		}

		// Token: 0x06001DD2 RID: 7634 RVA: 0x00082E30 File Offset: 0x00081030
		public IReadOnlyList<ConsoleMessage> GetConsoleMessagesForAssetId(SerializableGuid assetId)
		{
			List<ConsoleMessage> list;
			if (this.assetIdSortedMessageMap.TryGetValue(assetId, out list))
			{
				return list;
			}
			return new List<ConsoleMessage>();
		}

		// Token: 0x06001DD3 RID: 7635 RVA: 0x00082E54 File Offset: 0x00081054
		public void AddMessage(ConsoleMessage consoleMessage)
		{
			this.AddMessage_ClientRpc(consoleMessage);
		}

		// Token: 0x06001DD4 RID: 7636 RVA: 0x00082E60 File Offset: 0x00081060
		[ClientRpc]
		private void AddMessage_ClientRpc(ConsoleMessage consoleMessage)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(57075717U, clientRpcParams, RpcDelivery.Reliable);
				bool flag = consoleMessage != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag, default(FastBufferWriter.ForPrimitives));
				if (flag)
				{
					fastBufferWriter.WriteValueSafe<ConsoleMessage>(in consoleMessage, default(FastBufferWriter.ForNetworkSerializable));
				}
				base.__endSendClientRpc(ref fastBufferWriter, 57075717U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			this.AddMessageLocal(consoleMessage);
		}

		// Token: 0x06001DD5 RID: 7637 RVA: 0x00082F88 File Offset: 0x00081188
		private void AddMessageLocal(ConsoleMessage consoleMessage)
		{
			this.instanceIdSortedMessageMap.TryAdd(consoleMessage.InstanceId, new List<ConsoleMessage>());
			this.instanceIdSortedMessageMap[consoleMessage.InstanceId].Add(consoleMessage);
			this.assetIdSortedMessageMap.TryAdd(consoleMessage.AssetId, new List<ConsoleMessage>());
			this.assetIdSortedMessageMap[consoleMessage.AssetId].Add(consoleMessage);
			this.messages.Add(consoleMessage);
			Action<ConsoleMessage, List<ConsoleMessage>> onMessageReceived = this.OnMessageReceived;
			if (onMessageReceived == null)
			{
				return;
			}
			onMessageReceived(consoleMessage, this.messages);
		}

		// Token: 0x06001DD6 RID: 7638 RVA: 0x00083014 File Offset: 0x00081214
		public void ClearMessages()
		{
			this.instanceIdSortedMessageMap.Clear();
			this.messages.Clear();
			Action onMessagesCleared = this.OnMessagesCleared;
			if (onMessagesCleared == null)
			{
				return;
			}
			onMessagesCleared();
		}

		// Token: 0x06001DD8 RID: 7640 RVA: 0x00083068 File Offset: 0x00081268
		protected override void __initializeVariables()
		{
			base.__initializeVariables();
		}

		// Token: 0x06001DD9 RID: 7641 RVA: 0x0008307E File Offset: 0x0008127E
		protected override void __initializeRpcs()
		{
			base.__registerRpc(57075717U, new NetworkBehaviour.RpcReceiveHandler(UserScriptingConsole.__rpc_handler_57075717), "AddMessage_ClientRpc");
			base.__initializeRpcs();
		}

		// Token: 0x06001DDA RID: 7642 RVA: 0x000830A4 File Offset: 0x000812A4
		private static void __rpc_handler_57075717(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			bool flag;
			reader.ReadValueSafe<bool>(out flag, default(FastBufferWriter.ForPrimitives));
			ConsoleMessage consoleMessage = null;
			if (flag)
			{
				reader.ReadValueSafe<ConsoleMessage>(out consoleMessage, default(FastBufferWriter.ForNetworkSerializable));
			}
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((UserScriptingConsole)target).AddMessage_ClientRpc(consoleMessage);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06001DDB RID: 7643 RVA: 0x0008313E File Offset: 0x0008133E
		protected internal override string __getTypeName()
		{
			return "UserScriptingConsole";
		}

		// Token: 0x04001747 RID: 5959
		private Dictionary<SerializableGuid, List<ConsoleMessage>> instanceIdSortedMessageMap = new Dictionary<SerializableGuid, List<ConsoleMessage>>();

		// Token: 0x04001748 RID: 5960
		private Dictionary<SerializableGuid, List<ConsoleMessage>> assetIdSortedMessageMap = new Dictionary<SerializableGuid, List<ConsoleMessage>>();

		// Token: 0x04001749 RID: 5961
		private List<ConsoleMessage> messages = new List<ConsoleMessage>();

		// Token: 0x0400174C RID: 5964
		private string projectId;
	}
}
