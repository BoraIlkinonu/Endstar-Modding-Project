using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Endless.Matchmaking;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared
{
	// Token: 0x02000092 RID: 146
	public class UserIdManager : NetworkBehaviourSingleton<UserIdManager>
	{
		// Token: 0x170000BA RID: 186
		// (get) Token: 0x06000425 RID: 1061 RVA: 0x00011C62 File Offset: 0x0000FE62
		public IEnumerable<int> ConnectedUserIds
		{
			get
			{
				return this.userMap.Forward.Values;
			}
		}

		// Token: 0x170000BB RID: 187
		// (get) Token: 0x06000426 RID: 1062 RVA: 0x00011C74 File Offset: 0x0000FE74
		public int UserCount
		{
			get
			{
				return this.userMap.Forward.Keys.Count<ulong>();
			}
		}

		// Token: 0x06000427 RID: 1063 RVA: 0x00011C8B File Offset: 0x0000FE8B
		protected override void Awake()
		{
			base.Awake();
			this.currentToken = this.cancellationSource.Token;
		}

		// Token: 0x06000428 RID: 1064 RVA: 0x00011CA4 File Offset: 0x0000FEA4
		private void Start()
		{
			NetworkManager.Singleton.OnConnectionEvent += this.HandleConnectionEvent;
		}

		// Token: 0x06000429 RID: 1065 RVA: 0x00011CBC File Offset: 0x0000FEBC
		private void HandleConnectionEvent(NetworkManager arg1, ConnectionEventData data)
		{
			IEnumerable<string> enumerable = data.PeerClientIds.Select((ulong id) => id.ToString());
			Debug.LogWarning(string.Format("Connection event: {0}: {1}, {2}", data.EventType, data.ClientId, string.Join(", ", enumerable)));
			if (data.EventType == ConnectionEvent.ClientDisconnected)
			{
				if (base.IsServer)
				{
					this.UnregisterClientUser(data.ClientId);
				}
				if (NetworkManager.Singleton.LocalClientId == data.ClientId)
				{
					this.ClearUserIds();
				}
			}
		}

		// Token: 0x0600042A RID: 1066 RVA: 0x00011D60 File Offset: 0x0000FF60
		protected override void OnSynchronize<T>(ref BufferSerializer<T> serializer)
		{
			base.OnSynchronize<T>(ref serializer);
			int num = 0;
			if (serializer.IsWriter)
			{
				num = this.ConnectedUserIds.Count<int>();
			}
			serializer.SerializeValue<int>(ref num, default(FastBufferWriter.ForPrimitives));
			if (num > 0)
			{
				ulong[] array;
				int[] array2;
				if (serializer.IsWriter)
				{
					array = this.userMap.Forward.Keys.ToArray<ulong>();
					array2 = this.userMap.Forward.Values.ToArray<int>();
				}
				else
				{
					array = new ulong[num];
					array2 = new int[num];
				}
				for (int i = 0; i < num; i++)
				{
					serializer.SerializeValue<ulong>(ref array[i], default(FastBufferWriter.ForPrimitives));
					serializer.SerializeValue<int>(ref array2[i], default(FastBufferWriter.ForPrimitives));
					if (serializer.IsReader)
					{
						this.userMap.Add(array[i], array2[i]);
						this.OnUserIdAdded.Invoke(array2[i]);
					}
				}
			}
		}

		// Token: 0x0600042B RID: 1067 RVA: 0x00011E4E File Offset: 0x0001004E
		public void RegisterClientUser()
		{
			MatchmakingClientController.Instance.GetEncryptedKey(MatchSession.Instance.MatchData.MatchAuthKey, new Action<string>(this.ClientUserKeyRetreived));
		}

		// Token: 0x0600042C RID: 1068 RVA: 0x00011E75 File Offset: 0x00010075
		public void UnregisterClientUser(ulong clientId)
		{
			this.UntrackUserByClientId(clientId);
		}

		// Token: 0x0600042D RID: 1069 RVA: 0x00011E7E File Offset: 0x0001007E
		public ulong GetClientId(int userId)
		{
			return this.userMap.Reverse[userId];
		}

		// Token: 0x0600042E RID: 1070 RVA: 0x00011E91 File Offset: 0x00010091
		public bool TryGetClientId(int userId, out ulong clientId)
		{
			return this.userMap.Reverse.TryGetValue(userId, out clientId);
		}

		// Token: 0x0600042F RID: 1071 RVA: 0x00011EA5 File Offset: 0x000100A5
		public int GetUserId(ulong clientId)
		{
			return this.userMap.Forward[clientId];
		}

		// Token: 0x06000430 RID: 1072 RVA: 0x00011EB8 File Offset: 0x000100B8
		public bool TryGetUserId(ulong clientId, out int userId)
		{
			return this.userMap.Forward.TryGetValue(clientId, out userId);
		}

		// Token: 0x06000431 RID: 1073 RVA: 0x00011ECC File Offset: 0x000100CC
		public async Task<int> GetUserIdAsync(ulong clientId)
		{
			int num = -1;
			while (!this.userMap.Forward.TryGetValue(clientId, out num))
			{
				if (ExitManager.IsQuitting || this.currentToken.IsCancellationRequested)
				{
					return -1;
				}
				await Task.Yield();
			}
			return num;
		}

		// Token: 0x06000432 RID: 1074 RVA: 0x00011F18 File Offset: 0x00010118
		public async Task<int> GetUserIdAsync(ulong clientId, CancellationToken cancellationToken)
		{
			TaskCompletionSource<int> taskCompletionSource = new TaskCompletionSource<int>();
			int num;
			using (cancellationToken.Register(delegate
			{
				taskCompletionSource.TrySetCanceled();
			}))
			{
				Task<int> userIdTask = this.GetUserIdAsync(clientId);
				TaskAwaiter<Task<int>> taskAwaiter = Task.WhenAny<int>(new Task<int>[] { userIdTask, taskCompletionSource.Task }).GetAwaiter();
				if (!taskAwaiter.IsCompleted)
				{
					await taskAwaiter;
					TaskAwaiter<Task<int>> taskAwaiter2;
					taskAwaiter = taskAwaiter2;
					taskAwaiter2 = default(TaskAwaiter<Task<int>>);
				}
				if (taskAwaiter.GetResult() == taskCompletionSource.Task)
				{
					throw new OperationCanceledException();
				}
				num = await userIdTask;
			}
			return num;
		}

		// Token: 0x06000433 RID: 1075 RVA: 0x00011F6C File Offset: 0x0001016C
		public void ClearUserIds()
		{
			this.cancellationSource.Cancel();
			this.currentToken = this.cancellationSource.Token;
			foreach (int num in this.userMap.Reverse.Keys)
			{
				this.OnUserIdRemoved.Invoke(num);
			}
			this.userMap = new BidirectionalDictionary<ulong, int>();
		}

		// Token: 0x06000434 RID: 1076 RVA: 0x00011FF0 File Offset: 0x000101F0
		private void ClientUserKeyRetreived(string encryptedKey)
		{
			this.RegisterPlayerIdentity_ServerRpc(encryptedKey, default(ServerRpcParams));
		}

		// Token: 0x06000435 RID: 1077 RVA: 0x00012010 File Offset: 0x00010210
		[ServerRpc(RequireOwnership = false)]
		private void RegisterPlayerIdentity_ServerRpc(string encryptedKey, ServerRpcParams serverRpcParams = default(ServerRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendServerRpc(252647596U, serverRpcParams, RpcDelivery.Reliable);
				bool flag = encryptedKey != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag, default(FastBufferWriter.ForPrimitives));
				if (flag)
				{
					fastBufferWriter.WriteValueSafe(encryptedKey, false);
				}
				base.__endSendServerRpc(ref fastBufferWriter, 252647596U, serverRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			ulong clientId = serverRpcParams.Receive.SenderClientId;
			MatchmakingClientController.Instance.GetUserFromEncryptedKey(MatchSession.Instance.MatchData.MatchAuthKey, encryptedKey, delegate(string userIdString)
			{
				this.TrackUser(clientId, userIdString);
			});
		}

		// Token: 0x06000436 RID: 1078 RVA: 0x00012168 File Offset: 0x00010368
		private void TrackUser(ulong clientId, string userIdString)
		{
			int num = int.Parse(userIdString);
			this.userMap.Add(clientId, num);
			this.OnUserIdAdded.Invoke(num);
			this.TrackUser_ClientRpc(clientId, num);
		}

		// Token: 0x06000437 RID: 1079 RVA: 0x000121A0 File Offset: 0x000103A0
		[ClientRpc]
		private void TrackUser_ClientRpc(ulong clientId, int userId)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(600628695U, clientRpcParams, RpcDelivery.Reliable);
				BytePacker.WriteValueBitPacked(fastBufferWriter, clientId);
				BytePacker.WriteValueBitPacked(fastBufferWriter, userId);
				base.__endSendClientRpc(ref fastBufferWriter, 600628695U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			if (base.IsServer)
			{
				return;
			}
			this.userMap.Add(clientId, userId);
			this.OnUserIdAdded.Invoke(userId);
		}

		// Token: 0x06000438 RID: 1080 RVA: 0x000122B0 File Offset: 0x000104B0
		private void UntrackUserByClientId(ulong clientId)
		{
			int num;
			bool flag = this.userMap.Forward.TryGetValue(clientId, out num);
			this.userMap.Remove(clientId);
			if (flag)
			{
				this.OnUserIdRemoved.Invoke(num);
			}
			this.UntrackUser_ClientRpc(clientId);
		}

		// Token: 0x06000439 RID: 1081 RVA: 0x000122F4 File Offset: 0x000104F4
		[ClientRpc]
		private void UntrackUser_ClientRpc(ulong clientId)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(1640557737U, clientRpcParams, RpcDelivery.Reliable);
				BytePacker.WriteValueBitPacked(fastBufferWriter, clientId);
				base.__endSendClientRpc(ref fastBufferWriter, 1640557737U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			if (base.IsServer)
			{
				return;
			}
			int num;
			bool flag = this.userMap.Forward.TryGetValue(clientId, out num);
			this.userMap.Remove(clientId);
			if (flag)
			{
				this.OnUserIdRemoved.Invoke(num);
			}
		}

		// Token: 0x0600043B RID: 1083 RVA: 0x00012440 File Offset: 0x00010640
		protected override void __initializeVariables()
		{
			base.__initializeVariables();
		}

		// Token: 0x0600043C RID: 1084 RVA: 0x00012458 File Offset: 0x00010658
		protected override void __initializeRpcs()
		{
			base.__registerRpc(252647596U, new NetworkBehaviour.RpcReceiveHandler(UserIdManager.__rpc_handler_252647596), "RegisterPlayerIdentity_ServerRpc");
			base.__registerRpc(600628695U, new NetworkBehaviour.RpcReceiveHandler(UserIdManager.__rpc_handler_600628695), "TrackUser_ClientRpc");
			base.__registerRpc(1640557737U, new NetworkBehaviour.RpcReceiveHandler(UserIdManager.__rpc_handler_1640557737), "UntrackUser_ClientRpc");
			base.__initializeRpcs();
		}

		// Token: 0x0600043D RID: 1085 RVA: 0x000124C4 File Offset: 0x000106C4
		private static void __rpc_handler_252647596(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			bool flag;
			reader.ReadValueSafe<bool>(out flag, default(FastBufferWriter.ForPrimitives));
			string text = null;
			if (flag)
			{
				reader.ReadValueSafe(out text, false);
			}
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((UserIdManager)target).RegisterPlayerIdentity_ServerRpc(text, server);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x0600043E RID: 1086 RVA: 0x00012560 File Offset: 0x00010760
		private static void __rpc_handler_600628695(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			ulong num;
			ByteUnpacker.ReadValueBitPacked(reader, out num);
			int num2;
			ByteUnpacker.ReadValueBitPacked(reader, out num2);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((UserIdManager)target).TrackUser_ClientRpc(num, num2);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x0600043F RID: 1087 RVA: 0x000125D4 File Offset: 0x000107D4
		private static void __rpc_handler_1640557737(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			ulong num;
			ByteUnpacker.ReadValueBitPacked(reader, out num);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((UserIdManager)target).UntrackUser_ClientRpc(num);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06000440 RID: 1088 RVA: 0x00012636 File Offset: 0x00010836
		protected internal override string __getTypeName()
		{
			return "UserIdManager";
		}

		// Token: 0x040001FA RID: 506
		private BidirectionalDictionary<ulong, int> userMap = new BidirectionalDictionary<ulong, int>();

		// Token: 0x040001FB RID: 507
		public UnityEvent<int> OnUserIdAdded = new UnityEvent<int>();

		// Token: 0x040001FC RID: 508
		public UnityEvent<int> OnUserIdRemoved = new UnityEvent<int>();

		// Token: 0x040001FD RID: 509
		private CancellationTokenSource cancellationSource = new CancellationTokenSource();

		// Token: 0x040001FE RID: 510
		private CancellationToken currentToken;
	}
}
