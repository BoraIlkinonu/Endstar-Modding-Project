using System;
using Amazon.DynamoDBv2.DocumentModel;
using Endless.Networking;
using Endless.Shared;
using MatchmakingAPI.Notifications;
using UnityEngine;

namespace Endless.Matchmaking
{
	// Token: 0x02000042 RID: 66
	[DefaultExecutionOrder(2147483647)]
	public class MatchmakingClientControllerDebugger : MonoBehaviour
	{
		// Token: 0x0600022C RID: 556 RVA: 0x0000BF24 File Offset: 0x0000A124
		private void Start()
		{
			MatchmakingClientController.OnInitialized += delegate
			{
				Debug.Log("OnInitialized", this);
			};
			MatchmakingClientController.OnMissingIdentity += delegate
			{
				Debug.Log("OnMissingIdentity", this);
			};
			MatchmakingClientController.OnStartedConnectionToServer += delegate
			{
				Debug.Log("OnStartedConnectionToServer", this);
			};
			MatchmakingClientController.OnConnectionToServerFailed += delegate(string error)
			{
				Debug.Log("OnConnectionToServerFailed | error: " + error, this);
			};
			MatchmakingClientController.OnConnectedToServer += delegate
			{
				Debug.Log("OnConnectedToServer", this);
			};
			MatchmakingClientController.OnDisconnectedFromServer += delegate(string error)
			{
				Debug.Log("OnDisconnectedFromServer | error: " + error, this);
			};
			MatchmakingClientController.OnAuthenticationProcessStarted += delegate
			{
				Debug.Log("OnAuthenticationProcessStarted", this);
			};
			MatchmakingClientController.OnAuthenticationProcessFailed = (Action<string>)Delegate.Combine(MatchmakingClientController.OnAuthenticationProcessFailed, new Action<string>(delegate(string error)
			{
				Debug.Log("OnAuthenticationProcessFailed | error: " + error, this);
			}));
			MatchmakingClientController.OnAuthenticationProcessSuccessful = (Action<ClientData>)Delegate.Combine(MatchmakingClientController.OnAuthenticationProcessSuccessful, new Action<ClientData>(delegate(ClientData clientData)
			{
				Debug.Log("OnAuthenticationProcessSuccessful | clientData: " + clientData.ToPrettyString(), this);
			}));
			MatchmakingClientController.OnMatchmakingStarted += delegate
			{
				Debug.Log("OnMatchmakingStarted", this);
			};
			MatchmakingClientController.GroupInviteReceived += delegate(string inviteId, string hostId)
			{
				Debug.Log("GroupInviteReceived | inviteId: " + inviteId + ", hostId: " + hostId, this);
			};
			MatchmakingClientController.GroupJoined += delegate
			{
				Debug.Log("GroupJoined", this);
			};
			MatchmakingClientController.GroupJoin += delegate(string joinedUserId)
			{
				Debug.Log("GroupJoin | joinedUserId: " + new CoreClientData(joinedUserId, TargetPlatforms.Endless).ToPrettyString(), this);
			};
			MatchmakingClientController.GroupHostChanged += delegate(string newHostId)
			{
				Debug.Log("GroupHostChanged | newHostId: " + newHostId, this);
			};
			MatchmakingClientController.GroupLeave += delegate(string leaverUserId)
			{
				Debug.Log("GroupLeave | leaverUserId: " + new CoreClientData(leaverUserId, TargetPlatforms.Endless).ToPrettyString(), this);
			};
			MatchmakingClientController.GroupLeft += delegate
			{
				Debug.Log("GroupLeft", this);
			};
			MatchmakingClientController.MatchStart += delegate
			{
				Debug.Log("MatchStart", this);
			};
			MatchmakingClientController.MatchAllocated += delegate
			{
				Debug.Log("MatchAllocated", this);
			};
			MatchmakingClientController.MatchAllocationError += delegate(int errorCode, string errorMessage)
			{
				Debug.Log(string.Format("{0} | {1}: {2}, {3}: {4}", new object[] { "MatchAllocationError", "errorCode", errorCode, "errorMessage", errorMessage }), this);
			};
			MatchmakingClientController.MatchJoin += delegate(string error)
			{
				Debug.Log("MatchJoin | error: " + error, this);
			};
			MatchmakingClientController.MatchHostChanged += delegate(string groupId)
			{
				Debug.Log("MatchHostChanged | groupId: " + groupId, this);
			};
			MatchmakingClientController.MatchHostMigration += delegate
			{
				Debug.Log("MatchHostMigration", this);
			};
			MatchmakingClientController.MatchLeave += delegate(string groupId)
			{
				Debug.Log("MatchLeave | groupId: " + groupId, this);
			};
			MatchmakingClientController.MatchLeft += delegate
			{
				Debug.Log("MatchLeft", this);
			};
			MatchmakingClientController.UserChatReceived += delegate(string senderId, string message)
			{
				Debug.Log("UserChatReceived | senderId: " + senderId + ", message: " + message, this);
			};
			MatchmakingClientController.GroupChatReceived += delegate(string senderId, string message)
			{
				Debug.Log("GroupChatReceived | senderId: " + senderId + ", message: " + message, this);
			};
			MatchmakingClientController.MatchChatReceived += delegate(string senderId, string message)
			{
				Debug.Log("MatchChatReceived | senderId: " + senderId + ", message: " + message, this);
			};
			MatchmakingClientController.NotificationReceived += delegate(NotificationTypes notificationType, Document document)
			{
				Debug.Log(string.Format("{0} | {1}: {2}, {3}: {4}", new object[]
				{
					"NotificationReceived",
					"notificationType",
					notificationType,
					"document",
					document.ToJson()
				}), this);
			};
			MatchmakingClientController.OnShutdown += delegate
			{
				Debug.Log("OnShutdown", this);
			};
			MatchSession.OnMatchSessionInitialized += delegate(MatchSession matchSession)
			{
				Debug.Log(string.Format("{0} | {1}: {2}", "OnMatchSessionInitialized", "matchSession", matchSession), this);
			};
			MatchSession.OnMatchSessionStart += delegate(MatchSession matchSession)
			{
				Debug.Log(string.Format("{0} | {1}: {2}", "OnMatchSessionStart", "matchSession", matchSession), this);
			};
			MatchSession.OnMatchSessionStop += delegate(MatchSession matchSession)
			{
				Debug.Log(string.Format("{0} | {1}: {2}", "OnMatchSessionStop", "matchSession", matchSession), this);
			};
			MatchSession.OnMatchSessionClose += delegate(MatchSession matchSession)
			{
				Debug.Log(string.Format("{0} | {1}: {2}", "OnMatchSessionClose", "matchSession", matchSession), this);
			};
			MatchSession.OnClientConnected += delegate(ulong clientId)
			{
				Debug.Log(string.Format("{0} | {1}: {2}", "OnClientConnected", "clientId", clientId), this);
			};
			MatchSession.OnClientUnexpectedDisconnection += delegate(ulong clientId)
			{
				Debug.Log(string.Format("{0} | {1}: {2}", "OnClientUnexpectedDisconnection", "clientId", clientId), this);
			};
			MatchSession.OnClientKicked += delegate(ulong clientId)
			{
				Debug.Log(string.Format("{0} | {1}: {2}", "OnClientKicked", "clientId", clientId), this);
			};
			MatchSession.OnClientLeft += delegate(ulong clientId)
			{
				Debug.Log(string.Format("{0} | {1}: {2}", "OnClientLeft", "clientId", clientId), this);
			};
			this.matchmakingClientController = MatchmakingClientController.Instance;
		}

		// Token: 0x0400012F RID: 303
		private MatchmakingClientController matchmakingClientController;
	}
}
