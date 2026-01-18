using System;
using System.Net;
using Amazon.DynamoDBv2.DocumentModel;
using MatchmakingClientSDK.Errors;

namespace MatchmakingClientSDK
{
	// Token: 0x0200004E RID: 78
	public class ChatService
	{
		// Token: 0x1400000F RID: 15
		// (add) Token: 0x0600028F RID: 655 RVA: 0x0000BDC0 File Offset: 0x00009FC0
		// (remove) Token: 0x06000290 RID: 656 RVA: 0x0000BDF8 File Offset: 0x00009FF8
		public event Action<string, string> OnUserChatReceived;

		// Token: 0x14000010 RID: 16
		// (add) Token: 0x06000291 RID: 657 RVA: 0x0000BE30 File Offset: 0x0000A030
		// (remove) Token: 0x06000292 RID: 658 RVA: 0x0000BE68 File Offset: 0x0000A068
		public event Action<string, string> OnGroupChatReceived;

		// Token: 0x14000011 RID: 17
		// (add) Token: 0x06000293 RID: 659 RVA: 0x0000BEA0 File Offset: 0x0000A0A0
		// (remove) Token: 0x06000294 RID: 660 RVA: 0x0000BED8 File Offset: 0x0000A0D8
		public event Action<string, string> OnMatchChatReceived;

		// Token: 0x06000295 RID: 661 RVA: 0x0000BF0D File Offset: 0x0000A10D
		public ChatService(IMatchmakingClient client)
		{
			this.client = client;
			if (client != null)
			{
				MatchmakingWebsocketClient websocketClient = client.WebsocketClient;
				if (websocketClient == null)
				{
					return;
				}
				websocketClient.AddServiceHandler("chat", new Action<Document>(this.OnMessageReceived));
			}
		}

		// Token: 0x06000296 RID: 662 RVA: 0x0000BF40 File Offset: 0x0000A140
		private void OnMessageReceived(Document message)
		{
			DynamoDBEntry dynamoDBEntry;
			if (!message.TryGetValue("channel", out dynamoDBEntry))
			{
				return;
			}
			string text = dynamoDBEntry.AsString();
			if (!(text == "user"))
			{
				DynamoDBEntry dynamoDBEntry5;
				DynamoDBEntry dynamoDBEntry6;
				DynamoDBEntry dynamoDBEntry7;
				if (!(text == "group"))
				{
					if (!(text == "match"))
					{
						return;
					}
					DynamoDBEntry dynamoDBEntry2;
					DynamoDBEntry dynamoDBEntry3;
					DynamoDBEntry dynamoDBEntry4;
					if (message.TryGetValue("matchId", out dynamoDBEntry2) && message.TryGetValue("userId", out dynamoDBEntry3) && message.TryGetValue("message", out dynamoDBEntry4))
					{
						string text2 = dynamoDBEntry3.AsString();
						string userId = this.client.UsersService.GetUserId();
						if (text2 == userId)
						{
							return;
						}
						string text3 = dynamoDBEntry2.AsString();
						string stringProperty = this.client.MatchesService.GetStringProperty("matchId");
						if (text3 != stringProperty)
						{
							return;
						}
						Action<string, string> onMatchChatReceived = this.OnMatchChatReceived;
						if (onMatchChatReceived == null)
						{
							return;
						}
						onMatchChatReceived(text2, dynamoDBEntry4.AsString());
					}
				}
				else if (message.TryGetValue("groupId", out dynamoDBEntry5) && message.TryGetValue("userId", out dynamoDBEntry6) && message.TryGetValue("message", out dynamoDBEntry7))
				{
					string text4 = dynamoDBEntry6.AsString();
					string userId2 = this.client.UsersService.GetUserId();
					if (text4 == userId2)
					{
						return;
					}
					string text5 = dynamoDBEntry5.AsString();
					string stringProperty2 = this.client.GroupsService.GetStringProperty("groupId");
					if (text5 != stringProperty2)
					{
						return;
					}
					Action<string, string> onGroupChatReceived = this.OnGroupChatReceived;
					if (onGroupChatReceived == null)
					{
						return;
					}
					onGroupChatReceived(text4, dynamoDBEntry7.AsString());
					return;
				}
				return;
			}
			DynamoDBEntry dynamoDBEntry8;
			DynamoDBEntry dynamoDBEntry9;
			if (!message.TryGetValue("userId", out dynamoDBEntry8) || !message.TryGetValue("message", out dynamoDBEntry9))
			{
				this.client.Log("User chat message is missing required fields.", true);
				return;
			}
			string text6 = dynamoDBEntry8.AsString();
			string userId3 = this.client.UsersService.GetUserId();
			if (text6 == userId3)
			{
				return;
			}
			Action<string, string> onUserChatReceived = this.OnUserChatReceived;
			if (onUserChatReceived == null)
			{
				return;
			}
			onUserChatReceived(text6, dynamoDBEntry9.AsString());
		}

		// Token: 0x06000297 RID: 663 RVA: 0x0000C140 File Offset: 0x0000A340
		public void SendChatMessage(string message, ChatChannel chatChannel, string targetId = null, Action<int, string> onError = null)
		{
			Document document = new Document
			{
				{ "action", "chat" },
				{
					"method",
					chatChannel.ToString().ToLower()
				},
				{ "message", message }
			};
			if (chatChannel == ChatChannel.User)
			{
				if (string.IsNullOrWhiteSpace(targetId))
				{
					this.client.Log("Cannot send chat message to user. [userId] property is not provided", true);
					return;
				}
				document["userId"] = targetId;
			}
			this.client.WebsocketClient.SendRequest(document, delegate(Document responseDocument)
			{
				string text;
				HttpStatusCode httpStatusCode;
				int num;
				if (ErrorsService.TryGetError(responseDocument, out text, out httpStatusCode, out num))
				{
					Action<int, string> onError2 = onError;
					if (onError2 != null)
					{
						onError2((int)httpStatusCode, text);
					}
					this.client.Log(string.Format("Error sending chat message: Code: {0}, Message: {1}, Index: {2}", httpStatusCode, text, num), true);
				}
			});
		}

		// Token: 0x06000298 RID: 664 RVA: 0x0000C200 File Offset: 0x0000A400
		internal void Clear()
		{
		}

		// Token: 0x0400019D RID: 413
		private readonly IMatchmakingClient client;
	}
}
