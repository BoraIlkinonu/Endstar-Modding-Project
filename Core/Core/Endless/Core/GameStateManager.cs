using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Core.GameStates;
using Endless.Creator;
using Endless.Data;
using Endless.Gameplay;
using Endless.GraphQl;
using Endless.Matchmaking;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Runtime.Core.GameStates;
using Runtime.Shared.Matchmaking;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.CrashReportHandler;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Endless.Core
{
	// Token: 0x0200002A RID: 42
	public class GameStateManager : NetworkBehaviourSingleton<GameStateManager>
	{
		// Token: 0x17000009 RID: 9
		// (get) Token: 0x0600008B RID: 139 RVA: 0x00004A44 File Offset: 0x00002C44
		public GameState SharedGameState
		{
			get
			{
				return this.sharedGameState.Value;
			}
		}

		// Token: 0x1700000A RID: 10
		// (get) Token: 0x0600008C RID: 140 RVA: 0x00004A51 File Offset: 0x00002C51
		public bool IsEditEnabledSession
		{
			get
			{
				return this.isEditEnabledSession;
			}
		}

		// Token: 0x1700000B RID: 11
		// (get) Token: 0x0600008D RID: 141 RVA: 0x00004A59 File Offset: 0x00002C59
		public SerializableGuid GameId
		{
			get
			{
				return this.gameId.Value;
			}
		}

		// Token: 0x1700000C RID: 12
		// (get) Token: 0x0600008E RID: 142 RVA: 0x00004A66 File Offset: 0x00002C66
		public SerializableGuid LevelId
		{
			get
			{
				return this.levelId.Value;
			}
		}

		// Token: 0x1700000D RID: 13
		// (get) Token: 0x0600008F RID: 143 RVA: 0x00004A73 File Offset: 0x00002C73
		// (set) Token: 0x06000090 RID: 144 RVA: 0x00004A7B File Offset: 0x00002C7B
		public GameStateBase CurrentlyProcessingState { get; set; }

		// Token: 0x1700000E RID: 14
		// (get) Token: 0x06000091 RID: 145 RVA: 0x00004A84 File Offset: 0x00002C84
		public GameState CurrentState
		{
			get
			{
				if (this.currentGameState == null)
				{
					return GameState.None;
				}
				return this.currentGameState.StateType;
			}
		}

		// Token: 0x14000001 RID: 1
		// (add) Token: 0x06000092 RID: 146 RVA: 0x00004A9C File Offset: 0x00002C9C
		// (remove) Token: 0x06000093 RID: 147 RVA: 0x00004AD4 File Offset: 0x00002CD4
		public event Action OnClientStateCollectionUpdated;

		// Token: 0x06000094 RID: 148 RVA: 0x00004B09 File Offset: 0x00002D09
		public void EnterValidateGameLibraryState(MatchData matchData, GameState followUpState)
		{
			this.HandleGameStateChange(new ValidateGameLibraryGameState(matchData, followUpState));
		}

		// Token: 0x06000095 RID: 149 RVA: 0x00004B18 File Offset: 0x00002D18
		private void HandleGameStateChange(GameStateBase newGameState)
		{
			Debug.Log("HandleGameStateChange:: Target State: " + newGameState.GetType().Name);
			GameState gameState = GameState.None;
			if (this.currentGameState != null)
			{
				gameState = this.currentGameState.StateType;
				Debug.Log(string.Format("Leaving state {0}", gameState));
				this.currentGameState.HandleExitingState(newGameState.StateType);
			}
			else
			{
				Debug.Log(string.Format("Leaving state {0}", gameState));
			}
			this.currentGameState = newGameState;
			if (base.IsServer)
			{
				this.sharedGameState.Value = newGameState.StateType;
			}
			else
			{
				this.RecordClientState_ServerRpc(newGameState.StateType, default(ServerRpcParams));
			}
			Debug.Log(string.Format("Entering state {0}", newGameState.StateType));
			this.currentGameState.StartEnteringState(gameState);
			Debug.Log(string.Format("Changed states from {0} to {1}", gameState, newGameState.StateType));
			this.OnGameStateChanged.Invoke(gameState, newGameState.StateType);
			CrashReportHandler.SetUserMetadata("gameState", newGameState.StateType.ToString());
			this.currentGameState.OnChangeRequestCallback = new Action<GameStateBase>(this.HandleGameStateChange);
			this.currentGameState.StateEntered(gameState);
		}

		// Token: 0x06000096 RID: 150 RVA: 0x00004C64 File Offset: 0x00002E64
		[ServerRpc(RequireOwnership = false)]
		public void SpawnCreatorCharacter_ServerRpc(ServerRpcParams rpcParams = default(ServerRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendServerRpc(2133864599U, rpcParams, RpcDelivery.Reliable);
				base.__endSendServerRpc(ref fastBufferWriter, 2133864599U, rpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			ulong senderClientId = rpcParams.Receive.SenderClientId;
			if (base.NetworkManager.ConnectedClients.ContainsKey(senderClientId))
			{
				base.NetworkManager.ConnectedClients[senderClientId].PlayerObject.GetComponent<UserController>().SpawnCreatorCharacter();
			}
		}

		// Token: 0x06000097 RID: 151 RVA: 0x00004D78 File Offset: 0x00002F78
		[ServerRpc(RequireOwnership = false)]
		public void SpawnGameplayCharacter_ServerRpc(ServerRpcParams rpcParams = default(ServerRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendServerRpc(4000813114U, rpcParams, RpcDelivery.Reliable);
				base.__endSendServerRpc(ref fastBufferWriter, 4000813114U, rpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			if (MonoBehaviourSingleton<EndlessLoop>.Instance.HasAwoken)
			{
				ulong senderClientId = rpcParams.Receive.SenderClientId;
				if (base.NetworkManager.ConnectedClients.ContainsKey(senderClientId))
				{
					base.NetworkManager.ConnectedClients[senderClientId].PlayerObject.GetComponent<UserController>().SpawnGameplayCharacter();
				}
			}
		}

		// Token: 0x06000098 RID: 152 RVA: 0x00004E98 File Offset: 0x00003098
		private void Start()
		{
			GraphQlRequest.OnWebsocketReconnected += this.OnWebsocketReconnected;
			MatchSession.OnMatchSessionStart += this.MatchSession_OnMatchSessionStart;
			MatchSession.OnMatchSessionClose += this.MatchSession_OnMatchSessionClose;
			NetworkManager.Singleton.OnClientStopped += this.HandleServerStopped;
			CrashReportHandler.SetUserMetadata("executionEnvironment", "production");
			this.HandleGameStateChange(new DefaultGameState());
		}

		// Token: 0x06000099 RID: 153 RVA: 0x00004F08 File Offset: 0x00003108
		private void HandleServerStopped(bool obj)
		{
			this.ForceExitState();
		}

		// Token: 0x0600009A RID: 154 RVA: 0x00004F10 File Offset: 0x00003110
		private async void OnWebsocketReconnected()
		{
			if (this.isEditEnabledSession && this.CurrentState.IsInCreatorCategory())
			{
				GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.SubscribeAllNotificationsAsync(new string[] { "assetupdated" });
				if (graphQlResult.HasErrors)
				{
					ErrorHandler.HandleError(ErrorCodes.GameStateManager_SubscribeAllObjectNotificationsReconnect, graphQlResult.GetErrorMessage(0), true, false);
				}
				else if (MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame != null)
				{
					GraphQlResult graphQlResult2 = await EndlessServices.Instance.CloudService.SubscribeToObjectNotificationsAsync(MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame.AssetID);
					if (graphQlResult2.HasErrors)
					{
						ErrorHandler.HandleError(ErrorCodes.GameStateManager_SubscribeToObjectNotificationsReconnect, graphQlResult2.GetErrorMessage(0), true, false);
					}
				}
			}
		}

		// Token: 0x0600009B RID: 155 RVA: 0x00004F48 File Offset: 0x00003148
		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();
			if (base.IsServer)
			{
				MatchSession.OnClientConnected += this.HandleClientJoined;
				MatchSession.OnClientLeft += this.HandleClientLeft;
			}
			NetworkVariable<SerializableGuid> networkVariable = this.gameId;
			networkVariable.OnValueChanged = (NetworkVariable<SerializableGuid>.OnValueChangedDelegate)Delegate.Combine(networkVariable.OnValueChanged, new NetworkVariable<SerializableGuid>.OnValueChangedDelegate(this.HandleGameIdUpdated));
			NetworkVariable<SerializableGuid> networkVariable2 = this.levelId;
			networkVariable2.OnValueChanged = (NetworkVariable<SerializableGuid>.OnValueChangedDelegate)Delegate.Combine(networkVariable2.OnValueChanged, new NetworkVariable<SerializableGuid>.OnValueChangedDelegate(this.HandleLevelIdUpdated));
			CrashReportHandler.SetUserMetadata("gameId", this.GameId.ToString());
			CrashReportHandler.SetUserMetadata("levelId", this.LevelId.ToString());
		}

		// Token: 0x0600009C RID: 156 RVA: 0x0000500F File Offset: 0x0000320F
		public override void OnNetworkDespawn()
		{
			base.OnNetworkDespawn();
			if (base.IsServer)
			{
				MatchSession.OnClientConnected -= this.HandleClientJoined;
				MatchSession.OnClientLeft -= this.HandleClientLeft;
			}
		}

		// Token: 0x0600009D RID: 157 RVA: 0x00005041 File Offset: 0x00003241
		private void HandleLevelIdUpdated(SerializableGuid previousValue, SerializableGuid newValue)
		{
			CrashReportHandler.SetUserMetadata("gameId", newValue.ToString());
		}

		// Token: 0x0600009E RID: 158 RVA: 0x0000505A File Offset: 0x0000325A
		private void HandleGameIdUpdated(SerializableGuid previousValue, SerializableGuid newValue)
		{
			CrashReportHandler.SetUserMetadata("levelId", newValue.ToString());
		}

		// Token: 0x0600009F RID: 159 RVA: 0x00005073 File Offset: 0x00003273
		private void HandleClientLeft(ulong clientId)
		{
			Debug.Log(string.Format("Client disconnected {0}", clientId));
			this.clientLoadStateMap.Remove(clientId);
			Action onClientStateCollectionUpdated = this.OnClientStateCollectionUpdated;
			if (onClientStateCollectionUpdated == null)
			{
				return;
			}
			onClientStateCollectionUpdated();
		}

		// Token: 0x060000A0 RID: 160 RVA: 0x000050A8 File Offset: 0x000032A8
		private void HandleClientJoined(ulong clientId)
		{
			if (base.IsServer && NetworkManager.Singleton.LocalClientId == clientId)
			{
				return;
			}
			Debug.Log(string.Format("Client connected {0}", clientId));
			if (!this.clientLoadStateMap.ContainsKey(clientId))
			{
				this.clientLoadStateMap.Add(clientId, GameState.None);
				Action onClientStateCollectionUpdated = this.OnClientStateCollectionUpdated;
				if (onClientStateCollectionUpdated == null)
				{
					return;
				}
				onClientStateCollectionUpdated();
			}
		}

		// Token: 0x060000A1 RID: 161 RVA: 0x0000510B File Offset: 0x0000330B
		public void FlipGameState()
		{
			if (this.isEditEnabledSession)
			{
				this.OnGameplayFlipRequested.Invoke();
			}
		}

		// Token: 0x060000A2 RID: 162 RVA: 0x00005120 File Offset: 0x00003320
		[ClientRpc]
		public void ChangeClientState_ClientRpc(GameState newValue, SerializableGuid levelId)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(2419514561U, clientRpcParams, RpcDelivery.Reliable);
				fastBufferWriter.WriteValueSafe<GameState>(in newValue, default(FastBufferWriter.ForEnums));
				fastBufferWriter.WriteValueSafe<SerializableGuid>(in levelId, default(FastBufferWriter.ForNetworkSerializable));
				base.__endSendClientRpc(ref fastBufferWriter, 2419514561U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			Debug.Log(string.Format("{0} - Receiving Change Client State call from server. Are we the server? {1}", "ChangeClientState_ClientRpc", base.IsServer));
			if (base.IsServer)
			{
				return;
			}
			Debug.Log(string.Format("Server switched client's state to: {0}", newValue));
			if (this.currentGameState is ValidateGameLibraryGameState)
			{
				return;
			}
			if (newValue != GameState.LoadingCreator)
			{
				if (newValue != GameState.LoadingGameplay)
				{
					if (newValue == GameState.Gameplay)
					{
						this.HandleGameStateChange(new GameplayGameState());
						return;
					}
				}
				else
				{
					this.HandleGameStateChange(new LoadingGameplayGameState(levelId, MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame.GetLevelReferenceById(levelId).AssetVersion));
				}
				return;
			}
			this.HandleGameStateChange(new LoadingCreatorGameState(levelId));
		}

		// Token: 0x060000A3 RID: 163 RVA: 0x000052BC File Offset: 0x000034BC
		[ClientRpc]
		public void SetClientsToValidateGameLibrary_ClientRpc(GameState followUpState)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(1564664599U, clientRpcParams, RpcDelivery.Reliable);
				fastBufferWriter.WriteValueSafe<GameState>(in followUpState, default(FastBufferWriter.ForEnums));
				base.__endSendClientRpc(ref fastBufferWriter, 1564664599U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			if (base.IsHost)
			{
				return;
			}
			this.HandleGameStateChange(new ValidateGameLibraryGameState(MatchSession.Instance.MatchData, followUpState));
		}

		// Token: 0x060000A4 RID: 164 RVA: 0x000053CC File Offset: 0x000035CC
		public override void OnDestroy()
		{
			base.OnDestroy();
			if (!ExitManager.IsQuitting)
			{
				GraphQlRequest.OnWebsocketReconnected -= this.OnWebsocketReconnected;
				MatchSession.OnMatchSessionStart -= this.MatchSession_OnMatchSessionStart;
				MatchSession.OnMatchSessionClose -= this.MatchSession_OnMatchSessionClose;
			}
		}

		// Token: 0x060000A5 RID: 165 RVA: 0x0000541C File Offset: 0x0000361C
		private void MatchSession_OnMatchSessionClose(MatchSession matchSession)
		{
			if (EndlessServices.Instance)
			{
				EndlessServices.Instance.CloudService.UnsubscribeToObjectNotifications(matchSession.MatchData.ProjectId, delegate(object _)
				{
				}, delegate(Exception e)
				{
					ErrorHandler.HandleError(ErrorCodes.GameStateManager_UnsubscribeToObjectNotifications, e, true, false);
				});
				EndlessServices.Instance.CloudService.RemoveWebSocketCallback(WebSocketMessageId.AssetUpdated, new Action<WebSocketPayload>(this.OnAssetUpdate));
			}
			Debug.Log(string.Format("Match session closed. MatchId: {0} ProjectId: {1}, Edit {2}", matchSession.MatchData.MatchId, matchSession.ProjectId, matchSession.MatchData.IsEditSession));
			this.clientLoadStateMap.Clear();
			this.ForceExitState();
		}

		// Token: 0x060000A6 RID: 166 RVA: 0x000054EF File Offset: 0x000036EF
		private void ForceExitState()
		{
			if (this.currentGameState != null && this.currentGameState.StateType != GameState.Default && !ExitManager.IsQuitting)
			{
				this.currentGameState.ForceExitState();
			}
		}

		// Token: 0x060000A7 RID: 167 RVA: 0x00005518 File Offset: 0x00003718
		private void MatchSession_OnMatchSessionStart(MatchSession matchSession)
		{
			this.isEditEnabledSession = matchSession.MatchData.IsEditSession;
			Debug.Log(string.Format("Match session started. MatchId: {0} ProjectId: {1}, Level Id: {2}, Edit {3}", new object[]
			{
				matchSession.MatchData.MatchId,
				matchSession.ProjectId,
				matchSession.MatchData.LevelId,
				matchSession.MatchData.IsEditSession
			}));
			if (this.isEditEnabledSession)
			{
				EndlessServices.Instance.CloudService.SubscribeToObjectNotifications(matchSession.MatchData.ProjectId, delegate(object _)
				{
					EndlessServices.Instance.CloudService.AddWebSocketCallback(WebSocketMessageId.AssetUpdated, new Action<WebSocketPayload>(this.OnAssetUpdate));
				}, delegate(Exception e)
				{
					ErrorHandler.HandleError(ErrorCodes.GameStateManager_SubscribeToObjectNotifications, e, true, false);
				});
			}
			if (base.IsServer)
			{
				this.gameId.Value = matchSession.ProjectId;
				this.levelId.Value = matchSession.MatchData.LevelId;
			}
		}

		// Token: 0x060000A8 RID: 168 RVA: 0x0000560C File Offset: 0x0000380C
		private void OnAssetUpdate(WebSocketPayload webSocketPayload)
		{
			AssetUpdatedMetaData metaDataAs = webSocketPayload.GetMetaDataAs<AssetUpdatedMetaData>();
			NetworkBehaviourSingleton<CreatorManager>.Instance.AssetUpdated(metaDataAs);
			Debug.Log(string.Concat(new string[] { "OnAssetUpdate: Asset Id (", metaDataAs.AssetId, ", ", metaDataAs.LastAssetVersion, ")" }));
		}

		// Token: 0x060000A9 RID: 169 RVA: 0x00005665 File Offset: 0x00003865
		internal void SetNewLevelId(SerializableGuid newLevelId)
		{
			this.levelId.Value = newLevelId;
		}

		// Token: 0x060000AA RID: 170 RVA: 0x00005674 File Offset: 0x00003874
		[ServerRpc(RequireOwnership = false)]
		internal void RecordClientState_ServerRpc(GameState gameState, ServerRpcParams serverRpcParams = default(ServerRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendServerRpc(1671968724U, serverRpcParams, RpcDelivery.Reliable);
				fastBufferWriter.WriteValueSafe<GameState>(in gameState, default(FastBufferWriter.ForEnums));
				base.__endSendServerRpc(ref fastBufferWriter, 1671968724U, serverRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			ulong senderClientId = serverRpcParams.Receive.SenderClientId;
			if (!NetworkManager.Singleton.ConnectedClientsIds.Contains(senderClientId))
			{
				return;
			}
			if (!this.clientLoadStateMap.ContainsKey(senderClientId))
			{
				this.clientLoadStateMap.Add(senderClientId, gameState);
			}
			else
			{
				this.clientLoadStateMap[senderClientId] = gameState;
			}
			Action onClientStateCollectionUpdated = this.OnClientStateCollectionUpdated;
			if (onClientStateCollectionUpdated == null)
			{
				return;
			}
			onClientStateCollectionUpdated();
		}

		// Token: 0x060000AB RID: 171 RVA: 0x000057BC File Offset: 0x000039BC
		internal bool AreAllClientsInState(GameState stateToCheck, out int validStates, out int totalStates)
		{
			validStates = 0;
			totalStates = this.clientLoadStateMap.Count;
			using (Dictionary<ulong, GameState>.ValueCollection.Enumerator enumerator = this.clientLoadStateMap.Values.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current == stateToCheck)
					{
						validStates++;
					}
				}
			}
			return validStates == totalStates;
		}

		// Token: 0x060000AC RID: 172 RVA: 0x0000582C File Offset: 0x00003A2C
		internal bool AreAllClientsInState(GameState stateToCheck)
		{
			using (Dictionary<ulong, GameState>.ValueCollection.Enumerator enumerator = this.clientLoadStateMap.Values.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current != stateToCheck)
					{
						return false;
					}
				}
			}
			return true;
		}

		// Token: 0x060000AD RID: 173 RVA: 0x00005888 File Offset: 0x00003A88
		internal void UpdateClientStates(GameState stateToForce)
		{
			if (base.IsServer)
			{
				Debug.Log(string.Format("Telling clients to join state {0}", stateToForce));
				this.ChangeClientState_ClientRpc(stateToForce, SerializableGuid.Empty);
			}
		}

		// Token: 0x060000AE RID: 174 RVA: 0x000058B4 File Offset: 0x00003AB4
		internal void DisconnectClientsNotInState(GameState stateType, string message)
		{
			List<ulong> list = new List<ulong>();
			foreach (ulong num in this.clientLoadStateMap.Keys)
			{
				if (this.clientLoadStateMap[num] != stateType)
				{
					list.Add(num);
				}
			}
			default(ClientRpcParams).Send = new ClientRpcSendParams
			{
				TargetClientIds = new List<ulong>(list)
			};
			foreach (ulong num2 in list)
			{
				Debug.Log(string.Format("Kicking client due to invalid state {0}", num2));
				MatchSession.Instance.KickClient(num2, message);
				this.clientLoadStateMap.Remove(num2);
			}
			Action onClientStateCollectionUpdated = this.OnClientStateCollectionUpdated;
			if (onClientStateCollectionUpdated == null)
			{
				return;
			}
			onClientStateCollectionUpdated();
		}

		// Token: 0x060000AF RID: 175 RVA: 0x000059C0 File Offset: 0x00003BC0
		private void FlipGameState(InputAction.CallbackContext context)
		{
			if (!this.isEditEnabledSession || !base.IsServer)
			{
				return;
			}
			this.FlipGameState();
		}

		// Token: 0x060000B2 RID: 178 RVA: 0x00005A60 File Offset: 0x00003C60
		protected override void __initializeVariables()
		{
			bool flag = this.sharedGameState == null;
			if (flag)
			{
				throw new Exception("GameStateManager.sharedGameState cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.sharedGameState.Initialize(this);
			base.__nameNetworkVariable(this.sharedGameState, "sharedGameState");
			this.NetworkVariableFields.Add(this.sharedGameState);
			flag = this.gameId == null;
			if (flag)
			{
				throw new Exception("GameStateManager.gameId cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.gameId.Initialize(this);
			base.__nameNetworkVariable(this.gameId, "gameId");
			this.NetworkVariableFields.Add(this.gameId);
			flag = this.levelId == null;
			if (flag)
			{
				throw new Exception("GameStateManager.levelId cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.levelId.Initialize(this);
			base.__nameNetworkVariable(this.levelId, "levelId");
			this.NetworkVariableFields.Add(this.levelId);
			base.__initializeVariables();
		}

		// Token: 0x060000B3 RID: 179 RVA: 0x00005B60 File Offset: 0x00003D60
		protected override void __initializeRpcs()
		{
			base.__registerRpc(2133864599U, new NetworkBehaviour.RpcReceiveHandler(GameStateManager.__rpc_handler_2133864599), "SpawnCreatorCharacter_ServerRpc");
			base.__registerRpc(4000813114U, new NetworkBehaviour.RpcReceiveHandler(GameStateManager.__rpc_handler_4000813114), "SpawnGameplayCharacter_ServerRpc");
			base.__registerRpc(2419514561U, new NetworkBehaviour.RpcReceiveHandler(GameStateManager.__rpc_handler_2419514561), "ChangeClientState_ClientRpc");
			base.__registerRpc(1564664599U, new NetworkBehaviour.RpcReceiveHandler(GameStateManager.__rpc_handler_1564664599), "SetClientsToValidateGameLibrary_ClientRpc");
			base.__registerRpc(1671968724U, new NetworkBehaviour.RpcReceiveHandler(GameStateManager.__rpc_handler_1671968724), "RecordClientState_ServerRpc");
			base.__initializeRpcs();
		}

		// Token: 0x060000B4 RID: 180 RVA: 0x00005C04 File Offset: 0x00003E04
		private static void __rpc_handler_2133864599(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((GameStateManager)target).SpawnCreatorCharacter_ServerRpc(server);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x060000B5 RID: 181 RVA: 0x00005C64 File Offset: 0x00003E64
		private static void __rpc_handler_4000813114(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((GameStateManager)target).SpawnGameplayCharacter_ServerRpc(server);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x060000B6 RID: 182 RVA: 0x00005CC4 File Offset: 0x00003EC4
		private static void __rpc_handler_2419514561(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			GameState gameState;
			reader.ReadValueSafe<GameState>(out gameState, default(FastBufferWriter.ForEnums));
			SerializableGuid serializableGuid;
			reader.ReadValueSafe<SerializableGuid>(out serializableGuid, default(FastBufferWriter.ForNetworkSerializable));
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((GameStateManager)target).ChangeClientState_ClientRpc(gameState, serializableGuid);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x060000B7 RID: 183 RVA: 0x00005D54 File Offset: 0x00003F54
		private static void __rpc_handler_1564664599(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			GameState gameState;
			reader.ReadValueSafe<GameState>(out gameState, default(FastBufferWriter.ForEnums));
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((GameStateManager)target).SetClientsToValidateGameLibrary_ClientRpc(gameState);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x060000B8 RID: 184 RVA: 0x00005DC4 File Offset: 0x00003FC4
		private static void __rpc_handler_1671968724(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			GameState gameState;
			reader.ReadValueSafe<GameState>(out gameState, default(FastBufferWriter.ForEnums));
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((GameStateManager)target).RecordClientState_ServerRpc(gameState, server);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x060000B9 RID: 185 RVA: 0x00005E42 File Offset: 0x00004042
		protected internal override string __getTypeName()
		{
			return "GameStateManager";
		}

		// Token: 0x04000067 RID: 103
		private NetworkVariable<GameState> sharedGameState = new NetworkVariable<GameState>(GameState.None, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x04000068 RID: 104
		private bool isEditEnabledSession;

		// Token: 0x04000069 RID: 105
		private NetworkVariable<SerializableGuid> gameId = new NetworkVariable<SerializableGuid>(SerializableGuid.Empty, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x0400006A RID: 106
		private NetworkVariable<SerializableGuid> levelId = new NetworkVariable<SerializableGuid>(SerializableGuid.Empty, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x0400006B RID: 107
		public UnityEvent<GameState, GameState> OnGameStateChanged = new UnityEvent<GameState, GameState>();

		// Token: 0x0400006C RID: 108
		private Dictionary<ulong, GameState> clientLoadStateMap = new Dictionary<ulong, GameState>();

		// Token: 0x0400006D RID: 109
		private GameStateBase currentGameState;

		// Token: 0x0400006F RID: 111
		public UnityEvent OnGameplayFlipRequested = new UnityEvent();
	}
}
