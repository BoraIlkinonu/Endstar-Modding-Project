using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using MatchmakingAPI.Notifications;
using MatchmakingClientSDK.Errors;
using MatchmakingClientSDK.Events;

namespace MatchmakingClientSDK.Users
{
	// Token: 0x02000061 RID: 97
	public class UsersService
	{
		// Token: 0x14000033 RID: 51
		// (add) Token: 0x060003AE RID: 942 RVA: 0x000102CC File Offset: 0x0000E4CC
		// (remove) Token: 0x060003AF RID: 943 RVA: 0x00010304 File Offset: 0x0000E504
		public event Action OnAuthenticationSuccess;

		// Token: 0x14000034 RID: 52
		// (add) Token: 0x060003B0 RID: 944 RVA: 0x0001033C File Offset: 0x0000E53C
		// (remove) Token: 0x060003B1 RID: 945 RVA: 0x00010374 File Offset: 0x0000E574
		public event Action OnAuthenticationFailed;

		// Token: 0x14000035 RID: 53
		// (add) Token: 0x060003B2 RID: 946 RVA: 0x000103AC File Offset: 0x0000E5AC
		// (remove) Token: 0x060003B3 RID: 947 RVA: 0x000103E4 File Offset: 0x0000E5E4
		public event Action<ReadOnlyDictionary<string, AttributeValue>> OnLocalUserUpdated;

		// Token: 0x060003B4 RID: 948 RVA: 0x00010419 File Offset: 0x0000E619
		public UsersService(IMatchmakingClient client)
		{
			this.client = client;
			if (client != null)
			{
				MatchmakingWebsocketClient websocketClient = client.WebsocketClient;
				if (websocketClient == null)
				{
					return;
				}
				websocketClient.AddServiceHandler("users", new Action<Document>(this.OnMessageReceived));
			}
		}

		// Token: 0x060003B5 RID: 949 RVA: 0x00010458 File Offset: 0x0000E658
		internal void OnAuthentication(Document authDoc)
		{
			string text;
			HttpStatusCode httpStatusCode;
			int num;
			if (ErrorsService.TryGetError(authDoc, out text, out httpStatusCode, out num))
			{
				Action onAuthenticationFailed = this.OnAuthenticationFailed;
				if (onAuthenticationFailed != null)
				{
					onAuthenticationFailed();
				}
				this.client.Log(string.Format("Authentication failed! Code: {0}, Message: {1}, Index: {2}", httpStatusCode, text, num), true);
				return;
			}
			Dictionary<string, AttributeValue> dictionary = authDoc.ToAttributeMap();
			this.UserData.CurrentSequenceNum = "0";
			this.UserData.Attributes = dictionary;
			Action onAuthenticationSuccess = this.OnAuthenticationSuccess;
			if (onAuthenticationSuccess != null)
			{
				onAuthenticationSuccess();
			}
			this.client.Log("Authentication success: " + authDoc.ToJsonPretty(), false);
		}

		// Token: 0x060003B6 RID: 950 RVA: 0x00010502 File Offset: 0x0000E702
		public ReadOnlyDictionary<string, AttributeValue> GetLocalAttributes()
		{
			return new ReadOnlyDictionary<string, AttributeValue>(this.UserData.Attributes);
		}

		// Token: 0x060003B7 RID: 951 RVA: 0x00010514 File Offset: 0x0000E714
		public string GetUserId()
		{
			return this.UserData.GetStringProperty("userId");
		}

		// Token: 0x060003B8 RID: 952 RVA: 0x00010526 File Offset: 0x0000E726
		public string GetUserName()
		{
			return this.UserData.GetStringProperty("userName");
		}

		// Token: 0x060003B9 RID: 953 RVA: 0x00010538 File Offset: 0x0000E738
		public string GetGroupId()
		{
			return this.UserData.GetStringProperty("groupId");
		}

		// Token: 0x060003BA RID: 954 RVA: 0x0001054C File Offset: 0x0000E74C
		public void SyncUser(string userId, Action<Document> callback)
		{
			Document document = new Document();
			document["action"] = "users";
			document["method"] = "sync";
			if (!string.IsNullOrWhiteSpace(userId))
			{
				document["userId"] = userId;
			}
			this.client.WebsocketClient.SendRequest(document, callback);
		}

		// Token: 0x060003BB RID: 955 RVA: 0x000105B4 File Offset: 0x0000E7B4
		public void EncryptUser(string publicKey, Action<Document> callback)
		{
			Document document = new Document();
			document["action"] = "users";
			document["method"] = "encryptUser";
			document["publicKey"] = publicKey;
			Document document2 = document;
			this.client.WebsocketClient.SendRequest(document2, callback);
		}

		// Token: 0x060003BC RID: 956 RVA: 0x00010614 File Offset: 0x0000E814
		public void DecryptUser(string publicKey, string encryptedKey, Action<Document> callback)
		{
			Document document = new Document();
			document["action"] = "users";
			document["method"] = "decryptUser";
			document["publicKey"] = publicKey;
			document["encryptedKey"] = encryptedKey;
			Document document2 = document;
			this.client.WebsocketClient.SendRequest(document2, callback);
		}

		// Token: 0x060003BD RID: 957 RVA: 0x00010688 File Offset: 0x0000E888
		internal bool OnConnectedEventReceived(EventData eventData)
		{
			AttributeValue attributeValue;
			if (eventData.New.TryGetValue("userId", out attributeValue))
			{
				NotificationsService notificationsService = this.client.NotificationsService;
				NotificationTypes notificationTypes = NotificationTypes.UserConnected;
				Document document = new Document();
				string text = "data";
				Document document2 = new Document();
				document2["userId"] = attributeValue.S;
				document[text] = document2;
				notificationsService.PushNotification(notificationTypes, document);
			}
			return true;
		}

		// Token: 0x060003BE RID: 958 RVA: 0x000106E8 File Offset: 0x0000E8E8
		internal bool OnUpdateEventReceived(EventData eventData)
		{
			ReadOnlyDictionary<string, AttributeValue> @new = eventData.New;
			AttributeValue attributeValue;
			if (!@new.TryGetValue("userId", out attributeValue))
			{
				this.client.Log("[userId] not found in incoming message.", true);
				return true;
			}
			if (attributeValue.S != this.GetUserId())
			{
				NotificationsService notificationsService = this.client.NotificationsService;
				NotificationTypes notificationTypes = NotificationTypes.UserUpdated;
				Document document = new Document();
				document["data"] = Document.FromAttributeMap(new Dictionary<string, AttributeValue>(@new));
				notificationsService.PushNotification(notificationTypes, document);
				return true;
			}
			string sequenceNum = eventData.SequenceNum;
			if (string.Compare(sequenceNum, this.UserData.CurrentSequenceNum, StringComparison.Ordinal) > 0)
			{
				Dictionary<string, AttributeValue> dictionary = new Dictionary<string, AttributeValue>(eventData.New);
				this.UserData.CurrentSequenceNum = sequenceNum;
				this.UserData.Attributes = dictionary;
				this.client.GroupsService.ProcessEventsQueue();
				if (this.UserData.GetStringProperty("groupId") == null && this.client.MatchesService.MatchData.GetStringProperty("matchId") != null)
				{
					this.client.MatchesService.SetMatchData("1", new Dictionary<string, AttributeValue>());
				}
				Action<ReadOnlyDictionary<string, AttributeValue>> onLocalUserUpdated = this.OnLocalUserUpdated;
				if (onLocalUserUpdated != null)
				{
					onLocalUserUpdated(new ReadOnlyDictionary<string, AttributeValue>(dictionary));
				}
			}
			return true;
		}

		// Token: 0x060003BF RID: 959 RVA: 0x00010818 File Offset: 0x0000EA18
		internal bool OnDisconnectedEventReceived(EventData eventData)
		{
			AttributeValue attributeValue;
			if (eventData.New.TryGetValue("userId", out attributeValue))
			{
				NotificationsService notificationsService = this.client.NotificationsService;
				NotificationTypes notificationTypes = NotificationTypes.UserDisconnected;
				Document document = new Document();
				string text = "data";
				Document document2 = new Document();
				document2["userId"] = attributeValue.S;
				document[text] = document2;
				notificationsService.PushNotification(notificationTypes, document);
			}
			return true;
		}

		// Token: 0x060003C0 RID: 960 RVA: 0x00010876 File Offset: 0x0000EA76
		private void OnMessageReceived(Document doc)
		{
		}

		// Token: 0x060003C1 RID: 961 RVA: 0x00010878 File Offset: 0x0000EA78
		internal void Clear()
		{
			this.UserData.CurrentSequenceNum = string.Empty;
			this.UserData.Attributes = null;
		}

		// Token: 0x0400026B RID: 619
		internal readonly UserData UserData = new UserData();

		// Token: 0x0400026C RID: 620
		private readonly IMatchmakingClient client;
	}
}
