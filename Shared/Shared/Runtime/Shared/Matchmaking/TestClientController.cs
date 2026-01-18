using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Endless.GraphQl;
using Endless.Matchmaking;
using MatchmakingAPI.Notifications;
using MatchmakingClientSDK;
using Unity.Services.Relay.Models;
using UnityEngine;

namespace Runtime.Shared.Matchmaking
{
	// Token: 0x02000020 RID: 32
	public class TestClientController : ClientController
	{
		// Token: 0x1700002C RID: 44
		// (get) Token: 0x060000E3 RID: 227 RVA: 0x00006286 File Offset: 0x00004486
		public override global::MatchmakingClientSDK.NetworkEnvironment NetworkEnv
		{
			get
			{
				return this.networkEnv;
			}
		}

		// Token: 0x1700002D RID: 45
		// (get) Token: 0x060000E4 RID: 228 RVA: 0x0000628E File Offset: 0x0000448E
		public override Func<string, string, Task<string>> SignIn { get; } = async delegate(string userName, string password)
		{
			GraphQlResult graphQlResult = await EndlessCloudService.DirectLoginAsync(userName, password);
			if (graphQlResult.HasErrors)
			{
				throw graphQlResult.GetErrorMessage(0);
			}
			string text = ((graphQlResult != null) ? graphQlResult.GetDataMember().ToString() : null);
			Debug.Log("User token: " + text);
			return text;
		};

		// Token: 0x1700002E RID: 46
		// (get) Token: 0x060000E5 RID: 229 RVA: 0x00006296 File Offset: 0x00004496
		public override Action<string, bool> Log { get; } = delegate(string message, bool isError)
		{
			if (isError)
			{
				Debug.LogError(message);
				return;
			}
			Debug.Log(message);
		};

		// Token: 0x1700002F RID: 47
		// (get) Token: 0x060000E6 RID: 230 RVA: 0x0000629E File Offset: 0x0000449E
		public override Func<float> Time { get; } = () => global::UnityEngine.Time.time;

		// Token: 0x17000030 RID: 48
		// (get) Token: 0x060000E7 RID: 231 RVA: 0x000062A6 File Offset: 0x000044A6
		public override Action AllocateMatch
		{
			get
			{
				return async delegate
				{
					this.allocator.Allocate();
				};
			}
		}

		// Token: 0x060000E8 RID: 232 RVA: 0x000062B4 File Offset: 0x000044B4
		public TestClientController(global::MatchmakingClientSDK.NetworkEnvironment networkEnv)
		{
			this.networkEnv = networkEnv;
			this.allocator.OnMatchAllocated += delegate
			{
				if (base.client != null && this.allocator.LastAllocation != null)
				{
					string userName = base.client.UsersService.GetUserName();
					string publicIp = this.allocator.PublicIp;
					string localIp = this.allocator.LocalIp;
					string text2 = userName + "'s Server";
					int port = this.allocator.Port;
					string key = this.allocator.Key;
					base.client.MatchesService.AllocateMatch(publicIp, localIp, text2, port, key, "UnityRelay", delegate(int code, string message)
					{
						Debug.LogError(string.Format("Match allocation failed! {0} - {1}", (HttpStatusCode)code, message));
					});
				}
			};
		}

		// Token: 0x060000E9 RID: 233 RVA: 0x000063A4 File Offset: 0x000045A4
		protected override void LoginGui()
		{
			if (!base.Connecting)
			{
				this.UserName = GUILayout.TextField(this.UserName, new GUILayoutOption[] { GUILayout.Width(200f) });
				this.Password = GUILayout.PasswordField(this.Password, '*', new GUILayoutOption[] { GUILayout.Width(200f) });
				GUILayout.Label(this.UserPlatform.ToString(), new GUILayoutOption[] { GUILayout.Width(200f) });
				if (GUILayout.Button("Connect", new GUILayoutOption[] { GUILayout.Width(200f) }))
				{
					base.Connect();
					return;
				}
			}
			else
			{
				GUILayout.Label("Connecting...", Array.Empty<GUILayoutOption>());
			}
		}

		// Token: 0x060000EA RID: 234 RVA: 0x00006464 File Offset: 0x00004664
		protected override void ConnectedGui()
		{
			string userId = base.client.UsersService.GetUserId();
			string userName = base.client.UsersService.GetUserName();
			GUILayout.Label(string.Concat(new string[] { "Connected as ", userName, " [", userId, "]" }), Array.Empty<GUILayoutOption>());
			GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
			GUILayout.BeginVertical(Array.Empty<GUILayoutOption>());
			this.ChatGui();
			GUILayout.EndVertical();
			GUILayout.BeginVertical(Array.Empty<GUILayoutOption>());
			this.GroupsGui();
			GUILayout.EndVertical();
			GUILayout.BeginVertical(Array.Empty<GUILayoutOption>());
			this.InvitesGui();
			GUILayout.EndVertical();
			GUILayout.BeginVertical(Array.Empty<GUILayoutOption>());
			this.MatchesGui();
			GUILayout.EndVertical();
			GUILayout.BeginVertical(Array.Empty<GUILayoutOption>());
			this.NotificationsGui();
			GUILayout.EndVertical();
			GUILayout.BeginVertical(Array.Empty<GUILayoutOption>());
			this.CryptographyGui();
			GUILayout.EndVertical();
			GUILayout.EndHorizontal();
		}

		// Token: 0x060000EB RID: 235 RVA: 0x00006558 File Offset: 0x00004758
		private void ChatGui()
		{
			this.ChatMessage = GUILayout.TextArea(this.ChatMessage, Array.Empty<GUILayoutOption>());
			GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
			this.Channel = GUILayout.TextField(this.Channel, 1, new GUILayoutOption[] { GUILayout.Width(24f) });
			this.TargetId = GUILayout.TextField(this.TargetId, new GUILayoutOption[] { GUILayout.Width(100f) });
			int num;
			if (int.TryParse(this.Channel, out num) && num >= 0 && num < 3 && GUILayout.Button("Send", Array.Empty<GUILayoutOption>()))
			{
				base.client.ChatService.SendChatMessage(this.ChatMessage, (ChatChannel)num, this.TargetId, null);
			}
			GUILayout.EndHorizontal();
		}

		// Token: 0x060000EC RID: 236 RVA: 0x0000661C File Offset: 0x0000481C
		private void GroupsGui()
		{
			bool flag = true;
			if (string.IsNullOrWhiteSpace(base.client.GroupsService.GetStringProperty("groupId")))
			{
				GUILayout.Label("No group", Array.Empty<GUILayoutOption>());
			}
			else
			{
				Dictionary<string, AttributeValue> dictionary = new Dictionary<string, AttributeValue>(base.client.GroupsService.GetLocalAttributes());
				AttributeValue attributeValue;
				if (dictionary.TryGetValue("groupId", out attributeValue))
				{
					GUILayout.Label("GroupId: " + attributeValue.S, Array.Empty<GUILayoutOption>());
				}
				AttributeValue attributeValue2;
				if (dictionary.TryGetValue("groupHost", out attributeValue2))
				{
					GUILayout.Label("Host: " + attributeValue2.S, Array.Empty<GUILayoutOption>());
					flag = attributeValue2.S == base.client.UsersService.GetUserId();
				}
				AttributeValue attributeValue3;
				if (dictionary.TryGetValue("groupMembers", out attributeValue3))
				{
					GUILayout.Label(string.Format("Members: {0}", attributeValue3.L.Count), Array.Empty<GUILayoutOption>());
					foreach (AttributeValue attributeValue4 in attributeValue3.L)
					{
						GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
						GUILayout.Label(attributeValue4.S, Array.Empty<GUILayoutOption>());
						if (attributeValue4.S == base.client.UsersService.GetUserId())
						{
							if (GUILayout.Button("Leave", Array.Empty<GUILayoutOption>()))
							{
								base.LeaveGroup(true);
							}
						}
						else if (flag)
						{
							if (GUILayout.Button("Set Host", Array.Empty<GUILayoutOption>()))
							{
								base.ChangeGroupHost(attributeValue4.S);
							}
							if (GUILayout.Button("Kick", Array.Empty<GUILayoutOption>()))
							{
								base.RemoveFromGroup(attributeValue4.S);
							}
						}
						GUILayout.EndHorizontal();
					}
				}
			}
			if (flag)
			{
				GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
				this.inviteUserId = GUILayout.TextField(this.inviteUserId, Array.Empty<GUILayoutOption>());
				if (GUILayout.Button("Invite", Array.Empty<GUILayoutOption>()))
				{
					base.InviteToGroup(this.inviteUserId);
				}
				GUILayout.EndHorizontal();
			}
		}

		// Token: 0x060000ED RID: 237 RVA: 0x00006838 File Offset: 0x00004A38
		private void MatchesGui()
		{
			if (string.IsNullOrWhiteSpace(base.client.MatchesService.GetStringProperty("matchId")))
			{
				GUILayout.Label("No match", Array.Empty<GUILayoutOption>());
				GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
				GUILayout.Label("GameId: ", Array.Empty<GUILayoutOption>());
				this.GameId = GUILayout.TextField(this.GameId, Array.Empty<GUILayoutOption>());
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
				GUILayout.Label("LevelId: ", Array.Empty<GUILayoutOption>());
				this.LevelId = GUILayout.TextField(this.LevelId, Array.Empty<GUILayoutOption>());
				GUILayout.EndHorizontal();
				this.IsEditSession = GUILayout.Toggle(this.IsEditSession, "Edit Session", Array.Empty<GUILayoutOption>());
				GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
				GUILayout.Label("ServerType: (USER/DEDICATED)", Array.Empty<GUILayoutOption>());
				this.ServerType = GUILayout.TextField(this.ServerType, Array.Empty<GUILayoutOption>());
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
				GUILayout.Label("Version: ", Array.Empty<GUILayoutOption>());
				this.Version = GUILayout.TextField(this.Version, Array.Empty<GUILayoutOption>());
				GUILayout.EndHorizontal();
				if (!string.IsNullOrWhiteSpace(this.GameId) && !string.IsNullOrWhiteSpace(this.LevelId) && !string.IsNullOrWhiteSpace(this.ServerType) && !string.IsNullOrWhiteSpace(this.Version) && GUILayout.Button("Start Match", Array.Empty<GUILayoutOption>()))
				{
					base.StartMatch();
					return;
				}
			}
			else
			{
				Dictionary<string, AttributeValue> dictionary = new Dictionary<string, AttributeValue>(base.client.MatchesService.GetLocalAttributes());
				AttributeValue attributeValue;
				if (dictionary.TryGetValue("matchId", out attributeValue))
				{
					GUILayout.Label("MatchId: " + attributeValue.S, Array.Empty<GUILayoutOption>());
				}
				AttributeValue attributeValue2;
				if (dictionary.TryGetValue("gameId", out attributeValue2))
				{
					GUILayout.Label("GameId: " + attributeValue2.S, Array.Empty<GUILayoutOption>());
				}
				AttributeValue attributeValue3;
				if (dictionary.TryGetValue("levelId", out attributeValue3))
				{
					GUILayout.Label("LevelId: " + attributeValue3.S, Array.Empty<GUILayoutOption>());
				}
				AttributeValue attributeValue4;
				if (dictionary.TryGetValue("isEditSession", out attributeValue4))
				{
					GUILayout.Label(string.Format("IsEditSession: {0}", attributeValue4.BOOL), Array.Empty<GUILayoutOption>());
				}
				string stringProperty = base.client.GroupsService.GetStringProperty("groupId");
				string stringProperty2 = base.client.GroupsService.GetStringProperty("groupHost");
				bool flag = stringProperty2 == base.client.UsersService.GetUserId();
				AttributeValue attributeValue5;
				if (dictionary.TryGetValue("matchHost", out attributeValue5))
				{
					flag = flag && attributeValue5.S == stringProperty;
					GUILayout.Label("Host: " + attributeValue5.S, Array.Empty<GUILayoutOption>());
				}
				AttributeValue attributeValue6;
				if (dictionary.TryGetValue("matchMembers", out attributeValue6))
				{
					GUILayout.Label(string.Format("Members: {0}", attributeValue6.L.Count), Array.Empty<GUILayoutOption>());
					foreach (AttributeValue attributeValue7 in attributeValue6.L)
					{
						GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
						GUILayout.Label(attributeValue7.S, Array.Empty<GUILayoutOption>());
						if (attributeValue7.S == stringProperty)
						{
							if (stringProperty2 == base.client.UsersService.GetUserId() && GUILayout.Button("Leave", Array.Empty<GUILayoutOption>()))
							{
								base.EndMatch();
							}
						}
						else if (flag)
						{
							if (GUILayout.Button("Set Host", Array.Empty<GUILayoutOption>()))
							{
								base.ChangeMatchHost(attributeValue7.S);
							}
							if (GUILayout.Button("Kick", Array.Empty<GUILayoutOption>()))
							{
								base.RemoveFromMatch(attributeValue7.S);
							}
						}
						GUILayout.EndHorizontal();
					}
				}
				AttributeValue attributeValue8;
				if (dictionary.TryGetValue("allocationData", out attributeValue8))
				{
					GUILayout.Label("AllocationData: " + attributeValue8.S, Array.Empty<GUILayoutOption>());
					if (this.udpClient != null && this.udpClient.Connected)
					{
						GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
						this.message = GUILayout.TextField(this.message, Array.Empty<GUILayoutOption>());
						if (GUILayout.Button("Send", Array.Empty<GUILayoutOption>()))
						{
							this.udpClient.Send(this.message, Array.Empty<ulong>());
						}
						GUILayout.EndHorizontal();
						GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
						GUILayout.Label("Received: ", Array.Empty<GUILayoutOption>());
						GUILayout.Label(this.udpClient.LastMessage, Array.Empty<GUILayoutOption>());
						GUILayout.EndHorizontal();
					}
				}
				GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
				GUILayout.Label("GameId: ", Array.Empty<GUILayoutOption>());
				this.GameId = GUILayout.TextField(this.GameId, Array.Empty<GUILayoutOption>());
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
				GUILayout.Label("LevelId: ", Array.Empty<GUILayoutOption>());
				this.LevelId = GUILayout.TextField(this.LevelId, Array.Empty<GUILayoutOption>());
				GUILayout.EndHorizontal();
				this.IsEditSession = GUILayout.Toggle(this.IsEditSession, "Edit Session", Array.Empty<GUILayoutOption>());
				GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
				GUILayout.Label("ServerType: (USER/DEDICATED)", Array.Empty<GUILayoutOption>());
				this.ServerType = GUILayout.TextField(this.ServerType, Array.Empty<GUILayoutOption>());
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
				GUILayout.Label("Version: ", Array.Empty<GUILayoutOption>());
				this.Version = GUILayout.TextField(this.Version, Array.Empty<GUILayoutOption>());
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
				GUILayout.Label("Change Match Users: e.g.('5555,4444,3333')", Array.Empty<GUILayoutOption>());
				this.changeMatchUsers = GUILayout.TextField(this.changeMatchUsers, Array.Empty<GUILayoutOption>());
				GUILayout.EndHorizontal();
				if (!string.IsNullOrWhiteSpace(this.GameId) && !string.IsNullOrWhiteSpace(this.LevelId) && !string.IsNullOrWhiteSpace(this.ServerType) && !string.IsNullOrWhiteSpace(this.Version) && GUILayout.Button("Change Match", Array.Empty<GUILayoutOption>()))
				{
					base.ChangeMatch(this.changeMatchUsers.Split(',', StringSplitOptions.None));
				}
			}
		}

		// Token: 0x060000EE RID: 238 RVA: 0x00006E60 File Offset: 0x00005060
		protected override void OnMatchAllocated(string publicIp, string localIp, string name, int port, string key, string serverType)
		{
			base.OnMatchAllocated(publicIp, localIp, name, port, key, serverType);
			if (this.udpClient != null)
			{
				this.udpClient.Dispose();
			}
			bool flag = true;
			string userId = base.client.UsersService.GetUserId();
			string stringProperty = base.client.GroupsService.GetStringProperty("groupId");
			string stringProperty2 = base.client.GroupsService.GetStringProperty("groupHost");
			if (userId != stringProperty2)
			{
				flag = false;
			}
			string stringProperty3 = base.client.MatchesService.GetStringProperty("matchHost");
			if (stringProperty != stringProperty3)
			{
				flag = false;
			}
			string stringProperty4 = base.client.MatchesService.GetStringProperty("serverType");
			if (flag)
			{
				if (stringProperty4 == "USER")
				{
					if (this.allocator.LastAllocation != null)
					{
						this.udpClient = new UDPClient(this.allocator.LastAllocation as Allocation);
						return;
					}
					Debug.LogError("Cannot allocate. Invalid state.");
					return;
				}
				else
				{
					if (stringProperty4 == "DEDICATED")
					{
						this.udpClient = new UDPClient(publicIp, localIp, name, port, key);
						return;
					}
					Debug.LogError("Invalid server type");
					return;
				}
			}
			else
			{
				if (stringProperty4 == "USER")
				{
					this.udpClient = new UDPClient(key);
					return;
				}
				if (stringProperty4 == "DEDICATED")
				{
					this.udpClient = new UDPClient(publicIp, localIp, name, port, key);
					return;
				}
				Debug.LogError("Invalid server type");
				return;
			}
		}

		// Token: 0x060000EF RID: 239 RVA: 0x00006FC7 File Offset: 0x000051C7
		protected override void OnMatchAllocationError(int code, string message)
		{
			base.OnMatchAllocationError(code, message);
			if (this.udpClient != null)
			{
				this.udpClient.Dispose();
			}
		}

		// Token: 0x060000F0 RID: 240 RVA: 0x00006FE4 File Offset: 0x000051E4
		private void InvitesGui()
		{
			int count = this.groupInvites.Count;
			for (int i = 0; i < count; i++)
			{
				Tuple<string, string, DateTime> tuple = this.groupInvites.Dequeue();
				if (!(DateTime.UtcNow - tuple.Item3 > TimeSpan.FromMinutes(1.0)))
				{
					GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
					GUILayout.Label("Invite from " + tuple.Item2 + ": ", Array.Empty<GUILayoutOption>());
					if (GUILayout.Button("Accept", Array.Empty<GUILayoutOption>()))
					{
						base.JoinGroup(tuple.Item1);
					}
					else
					{
						this.groupInvites.Enqueue(tuple);
					}
					GUILayout.EndHorizontal();
				}
			}
		}

		// Token: 0x060000F1 RID: 241 RVA: 0x0000709C File Offset: 0x0000529C
		private void NotificationsGui()
		{
			int count = this.notifications.Count;
			for (int i = 0; i < count; i++)
			{
				Tuple<NotificationTypes, Document, DateTime> tuple = this.notifications.Dequeue();
				if (!(DateTime.UtcNow - tuple.Item3 > TimeSpan.FromSeconds(10.0)))
				{
					GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
					GUILayout.Label(string.Format("Notification {0}:", tuple.Item1), Array.Empty<GUILayoutOption>());
					GUILayout.Label(tuple.Item2.ToJsonPretty(), Array.Empty<GUILayoutOption>());
					this.notifications.Enqueue(tuple);
					GUILayout.EndHorizontal();
				}
			}
		}

		// Token: 0x060000F2 RID: 242 RVA: 0x00007148 File Offset: 0x00005348
		private void CryptographyGui()
		{
			GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
			this.publicKey = GUILayout.TextField(this.publicKey, Array.Empty<GUILayoutOption>());
			if (GUILayout.Button("Encrypt", Array.Empty<GUILayoutOption>()))
			{
				base.client.UsersService.EncryptUser(this.publicKey, delegate(Document doc)
				{
					this.encryptedKey = doc["encryptedKey"].AsString();
				});
			}
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
			if (!string.IsNullOrWhiteSpace(this.encryptedKey))
			{
				GUILayout.Label("Encrypted Key: " + this.encryptedKey, Array.Empty<GUILayoutOption>());
			}
			if (!string.IsNullOrWhiteSpace(this.publicKey) && !string.IsNullOrWhiteSpace(this.encryptedKey) && GUILayout.Button("Decrypt", Array.Empty<GUILayoutOption>()))
			{
				base.client.UsersService.DecryptUser(this.publicKey, this.encryptedKey, delegate(Document doc)
				{
					Debug.Log(doc.ToJsonPretty());
				});
			}
			GUILayout.EndHorizontal();
		}

		// Token: 0x060000F3 RID: 243 RVA: 0x00007249 File Offset: 0x00005449
		public void OnGUI()
		{
			base.OnGui();
		}

		// Token: 0x060000F4 RID: 244 RVA: 0x00007251 File Offset: 0x00005451
		public new void Disconnect()
		{
			if (this.udpClient != null)
			{
				this.udpClient.Dispose();
			}
			base.Disconnect();
		}

		// Token: 0x0400006A RID: 106
		private readonly global::MatchmakingClientSDK.NetworkEnvironment networkEnv;

		// Token: 0x0400006B RID: 107
		private readonly MatchAllocator allocator = new MatchAllocator();

		// Token: 0x0400006F RID: 111
		public string ChatMessage = string.Empty;

		// Token: 0x04000070 RID: 112
		public string Channel = "0";

		// Token: 0x04000071 RID: 113
		public string TargetId = string.Empty;

		// Token: 0x04000072 RID: 114
		private string inviteUserId = string.Empty;

		// Token: 0x04000073 RID: 115
		private string changeMatchUsers;

		// Token: 0x04000074 RID: 116
		private UDPClient udpClient;

		// Token: 0x04000075 RID: 117
		private string message;

		// Token: 0x04000076 RID: 118
		private string publicKey = string.Empty;

		// Token: 0x04000077 RID: 119
		private string encryptedKey = string.Empty;
	}
}
