using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using MatchmakingClientSDK.Errors;
using MatchmakingClientSDK.Events;

namespace MatchmakingClientSDK.Groups
{
	// Token: 0x02000065 RID: 101
	public class GroupsService
	{
		// Token: 0x14000036 RID: 54
		// (add) Token: 0x060003D6 RID: 982 RVA: 0x00010E9C File Offset: 0x0000F09C
		// (remove) Token: 0x060003D7 RID: 983 RVA: 0x00010ED4 File Offset: 0x0000F0D4
		public event Action<string, string> OnGroupInvite;

		// Token: 0x14000037 RID: 55
		// (add) Token: 0x060003D8 RID: 984 RVA: 0x00010F0C File Offset: 0x0000F10C
		// (remove) Token: 0x060003D9 RID: 985 RVA: 0x00010F44 File Offset: 0x0000F144
		public event Action OnGroupJoined;

		// Token: 0x14000038 RID: 56
		// (add) Token: 0x060003DA RID: 986 RVA: 0x00010F7C File Offset: 0x0000F17C
		// (remove) Token: 0x060003DB RID: 987 RVA: 0x00010FB4 File Offset: 0x0000F1B4
		public event Action<string> OnGroupJoin;

		// Token: 0x14000039 RID: 57
		// (add) Token: 0x060003DC RID: 988 RVA: 0x00010FEC File Offset: 0x0000F1EC
		// (remove) Token: 0x060003DD RID: 989 RVA: 0x00011024 File Offset: 0x0000F224
		public event Action<string> OnGroupHostChanged;

		// Token: 0x1400003A RID: 58
		// (add) Token: 0x060003DE RID: 990 RVA: 0x0001105C File Offset: 0x0000F25C
		// (remove) Token: 0x060003DF RID: 991 RVA: 0x00011094 File Offset: 0x0000F294
		public event Action<ReadOnlyDictionary<string, AttributeValue>> OnGroupUpdated;

		// Token: 0x1400003B RID: 59
		// (add) Token: 0x060003E0 RID: 992 RVA: 0x000110CC File Offset: 0x0000F2CC
		// (remove) Token: 0x060003E1 RID: 993 RVA: 0x00011104 File Offset: 0x0000F304
		public event Action<string> OnGroupLeave;

		// Token: 0x1400003C RID: 60
		// (add) Token: 0x060003E2 RID: 994 RVA: 0x0001113C File Offset: 0x0000F33C
		// (remove) Token: 0x060003E3 RID: 995 RVA: 0x00011174 File Offset: 0x0000F374
		public event Action OnGroupLeft;

		// Token: 0x060003E4 RID: 996 RVA: 0x000111AC File Offset: 0x0000F3AC
		public GroupsService(IMatchmakingClient client)
		{
			this.client = client;
			IMatchmakingClient matchmakingClient = this.client;
			if (matchmakingClient != null)
			{
				MatchmakingWebsocketClient websocketClient = matchmakingClient.WebsocketClient;
				if (websocketClient != null)
				{
					websocketClient.AddServiceHandler("groups", new Action<Document>(this.OnMessageReceived));
				}
			}
			this.AddCreateTreeNode();
			this.AddUpdateTreeNode();
			this.AddDeleteTreeNode();
		}

		// Token: 0x060003E5 RID: 997 RVA: 0x00011228 File Offset: 0x0000F428
		private void OnMessageReceived(Document doc)
		{
			DynamoDBEntry dynamoDBEntry;
			if (!doc.TryGetValue("method", out dynamoDBEntry))
			{
				this.client.Log("[method] not found in incoming groups message.", true);
				return;
			}
			string text = dynamoDBEntry.AsString();
			if (!(text == "invite"))
			{
				this.client.Log("Method [" + text + "] not supported by GroupsService.", true);
				return;
			}
			DynamoDBEntry dynamoDBEntry2;
			if (!doc.TryGetValue("inviteId", out dynamoDBEntry2))
			{
				this.client.Log("[inviteId] not found in incoming group invite message.", true);
				return;
			}
			string text2 = dynamoDBEntry2.AsString();
			DynamoDBEntry dynamoDBEntry3;
			if (!doc.TryGetValue("groupHost", out dynamoDBEntry3))
			{
				this.client.Log("[groupHost] not found in incoming group invite message.", true);
				return;
			}
			string text3 = dynamoDBEntry3.AsString();
			Action<string, string> onGroupInvite = this.OnGroupInvite;
			if (onGroupInvite == null)
			{
				return;
			}
			onGroupInvite(text2, text3);
		}

		// Token: 0x060003E6 RID: 998 RVA: 0x00011300 File Offset: 0x0000F500
		private void AddCreateTreeNode()
		{
			TreeNode<EventData> treeNode = TreeNode<EventData>.New(new Func<EventData, bool>(this.HasLocalGroup), TreeNode<EventData>.New(new Func<EventData, bool>(this.EnqueueEvent)), TreeNode<EventData>.New(new Func<EventData, bool>(this.ApplyEventData)));
			this.decisionTree.RootNodes.Add("groupCreated", treeNode);
		}

		// Token: 0x060003E7 RID: 999 RVA: 0x00011358 File Offset: 0x0000F558
		private void AddUpdateTreeNode()
		{
			TreeNode<EventData> treeNode = TreeNode<EventData>.New(new Func<EventData, bool>(this.HasLocalGroup), TreeNode<EventData>.New(new Func<EventData, bool>(this.IsLocalGroupIdEqualToEventGroupId), TreeNode<EventData>.New(new Func<EventData, bool>(this.IsEventNewerThanLocal), TreeNode<EventData>.New(new Func<EventData, bool>(this.ApplyEventData)), TreeNode<EventData>.New(new Func<EventData, bool>(this.DiscardEvent))), TreeNode<EventData>.New(new Func<EventData, bool>(this.EnqueueEvent))), TreeNode<EventData>.New(new Func<EventData, bool>(this.ApplyEventData)));
			this.decisionTree.RootNodes.Add("groupUpdated", treeNode);
		}

		// Token: 0x060003E8 RID: 1000 RVA: 0x000113F4 File Offset: 0x0000F5F4
		private void AddDeleteTreeNode()
		{
			TreeNode<EventData> treeNode = TreeNode<EventData>.New(new Func<EventData, bool>(this.HasLocalGroup), TreeNode<EventData>.New(new Func<EventData, bool>(this.IsLocalGroupIdEqualToEventGroupId), TreeNode<EventData>.New(new Func<EventData, bool>(this.ApplyEventData)), TreeNode<EventData>.New(new Func<EventData, bool>(this.EnqueueEvent))), TreeNode<EventData>.New(new Func<EventData, bool>(this.EnqueueEvent)));
			this.decisionTree.RootNodes.Add("groupDeleted", treeNode);
		}

		// Token: 0x060003E9 RID: 1001 RVA: 0x0001146D File Offset: 0x0000F66D
		private bool HasLocalGroup(EventData data)
		{
			return this.GroupData.Attributes.Count > 0;
		}

		// Token: 0x060003EA RID: 1002 RVA: 0x00011484 File Offset: 0x0000F684
		private bool IsLocalGroupIdEqualToEventGroupId(EventData data)
		{
			if (this.GroupData.Attributes == null)
			{
				return false;
			}
			string stringProperty = this.GroupData.GetStringProperty("groupId");
			AttributeValue attributeValue;
			if (data.Old.TryGetValue("groupId", out attributeValue))
			{
				return stringProperty == attributeValue.S;
			}
			AttributeValue attributeValue2;
			return data.New.TryGetValue("groupId", out attributeValue2) && stringProperty == attributeValue2.S;
		}

		// Token: 0x060003EB RID: 1003 RVA: 0x000114F4 File Offset: 0x0000F6F4
		private bool IsEventNewerThanLocal(EventData data)
		{
			return string.Compare(data.SequenceNum, this.GroupData.CurrentSequenceNum, StringComparison.Ordinal) > 0;
		}

		// Token: 0x060003EC RID: 1004 RVA: 0x00011510 File Offset: 0x0000F710
		private bool IsLocalUserInEventGroup(EventData data)
		{
			string text = string.Empty;
			AttributeValue attributeValue;
			AttributeValue attributeValue2;
			if (data.New.TryGetValue("groupId", out attributeValue))
			{
				text = attributeValue.S;
			}
			else if (data.Old.TryGetValue("groupId", out attributeValue2))
			{
				text = attributeValue2.S;
			}
			return this.client.UsersService.UserData.GetStringProperty("groupId") == text;
		}

		// Token: 0x060003ED RID: 1005 RVA: 0x0001157C File Offset: 0x0000F77C
		private bool IsUserAddedToGroup(EventData data)
		{
			string stringProperty = this.client.UsersService.UserData.GetStringProperty("userId");
			bool flag = false;
			AttributeValue attributeValue;
			if (data.Old.TryGetValue("groupMembers", out attributeValue))
			{
				flag = attributeValue.L.Select((AttributeValue e) => e.S).Contains(stringProperty);
			}
			bool flag2 = false;
			AttributeValue attributeValue2;
			if (data.New.TryGetValue("groupMembers", out attributeValue2))
			{
				flag2 = attributeValue2.L.Select((AttributeValue e) => e.S).Contains(stringProperty);
			}
			return !flag && flag2;
		}

		// Token: 0x060003EE RID: 1006 RVA: 0x0001163C File Offset: 0x0000F83C
		private bool IsUserRemovedFromGroup(EventData data)
		{
			string stringProperty = this.client.UsersService.UserData.GetStringProperty("userId");
			bool flag = false;
			AttributeValue attributeValue;
			if (data.Old.TryGetValue("groupMembers", out attributeValue))
			{
				flag = attributeValue.L.Select((AttributeValue e) => e.S).Contains(stringProperty);
			}
			bool flag2 = false;
			AttributeValue attributeValue2;
			if (data.New.TryGetValue("groupMembers", out attributeValue2))
			{
				flag2 = attributeValue2.L.Select((AttributeValue e) => e.S).Contains(stringProperty);
			}
			return flag && !flag2;
		}

		// Token: 0x060003EF RID: 1007 RVA: 0x000116FC File Offset: 0x0000F8FC
		private bool EnqueueEvent(EventData @event)
		{
			this.groupEventsQueue.Enqueue(@event);
			return false;
		}

		// Token: 0x060003F0 RID: 1008 RVA: 0x0001170B File Offset: 0x0000F90B
		private bool DiscardEvent(EventData @event)
		{
			this.client.Log("Group event discarded: " + @event.EventType + " - " + @event.SequenceNum, false);
			return false;
		}

		// Token: 0x060003F1 RID: 1009 RVA: 0x0001173C File Offset: 0x0000F93C
		private bool ApplyEventData(EventData data)
		{
			if (data.EventType == "groupCreated")
			{
				if (this.IsLocalUserInEventGroup(data))
				{
					return this.SetGroupData(data.SequenceNum, new Dictionary<string, AttributeValue>(data.New), true);
				}
				return this.EnqueueEvent(data);
			}
			else
			{
				if (!(data.EventType == "groupUpdated"))
				{
					return data.EventType == "groupDeleted" && this.SetGroupData(string.Empty, new Dictionary<string, AttributeValue>(), true);
				}
				if (this.GroupData.Attributes.Count == 0 && this.IsUserAddedToGroup(data))
				{
					if (this.IsLocalUserInEventGroup(data))
					{
						return this.SetGroupData(data.SequenceNum, new Dictionary<string, AttributeValue>(data.New), true);
					}
					return this.EnqueueEvent(data);
				}
				else
				{
					if (this.GroupData.Attributes.Count > 0 && this.IsUserRemovedFromGroup(data))
					{
						return this.SetGroupData(string.Empty, new Dictionary<string, AttributeValue>(), true);
					}
					if (this.GroupData.Attributes.Count > 0)
					{
						return this.SetGroupData(data.SequenceNum, new Dictionary<string, AttributeValue>(data.New), true);
					}
					return this.EnqueueEvent(data);
				}
			}
		}

		// Token: 0x060003F2 RID: 1010 RVA: 0x00011868 File Offset: 0x0000FA68
		internal void ProcessEventsQueue()
		{
			int count = this.groupEventsQueue.Count;
			for (int i = 0; i < count; i++)
			{
				EventData eventData = this.groupEventsQueue.Dequeue();
				DateTime receivedAt = eventData.ReceivedAt;
				if (eventData.EventType == "groupCreated")
				{
					if (this.OnCreateEventReceived(eventData))
					{
						return;
					}
				}
				else if (eventData.EventType == "groupUpdated")
				{
					if (this.OnUpdateEventReceived(eventData))
					{
						return;
					}
				}
				else if (eventData.EventType == "groupDeleted" && this.OnDeleteEventReceived(eventData))
				{
					return;
				}
			}
		}

		// Token: 0x060003F3 RID: 1011 RVA: 0x000118F4 File Offset: 0x0000FAF4
		internal bool OnCreateEventReceived(EventData eventData)
		{
			return this.decisionTree.TriggerRoot("groupCreated", eventData);
		}

		// Token: 0x060003F4 RID: 1012 RVA: 0x00011907 File Offset: 0x0000FB07
		internal bool OnUpdateEventReceived(EventData eventData)
		{
			return this.decisionTree.TriggerRoot("groupUpdated", eventData);
		}

		// Token: 0x060003F5 RID: 1013 RVA: 0x0001191A File Offset: 0x0000FB1A
		internal bool OnDeleteEventReceived(EventData eventData)
		{
			return this.decisionTree.TriggerRoot("groupDeleted", eventData);
		}

		// Token: 0x060003F6 RID: 1014 RVA: 0x0001192D File Offset: 0x0000FB2D
		public string GetStringProperty(string key)
		{
			return this.GroupData.GetStringProperty(key);
		}

		// Token: 0x060003F7 RID: 1015 RVA: 0x0001193B File Offset: 0x0000FB3B
		public ReadOnlyDictionary<string, AttributeValue> GetLocalAttributes()
		{
			return new ReadOnlyDictionary<string, AttributeValue>(this.GroupData.Attributes);
		}

		// Token: 0x060003F8 RID: 1016 RVA: 0x00011950 File Offset: 0x0000FB50
		public void SyncGroup(string groupId, Action<Dictionary<string, AttributeValue>> onSuccess, Action<int, string> onError = null)
		{
			Document document3 = new Document();
			document3["action"] = "groups";
			document3["method"] = "sync";
			Document document2 = document3;
			if (!string.IsNullOrWhiteSpace(groupId))
			{
				document2["groupId"] = groupId;
			}
			this.client.WebsocketClient.SendRequest(document2, delegate(Document document)
			{
				string text;
				HttpStatusCode httpStatusCode;
				int num;
				if (ErrorsService.TryGetError(document, out text, out httpStatusCode, out num))
				{
					this.client.Log(string.Format("Error syncing user group [{0}]! Code: {1}, Message: {2}, Index: {3}", new object[] { groupId, httpStatusCode, text, num }), true);
					Action<int, string> onError2 = onError;
					if (onError2 == null)
					{
						return;
					}
					onError2((int)httpStatusCode, text);
					return;
				}
				else
				{
					DynamoDBEntry dynamoDBEntry;
					if (document.TryGetValue("data", out dynamoDBEntry))
					{
						Document document4 = dynamoDBEntry as Document;
						if (document4 != null)
						{
							Dictionary<string, AttributeValue> dictionary = document4.ToAttributeMap();
							Action<Dictionary<string, AttributeValue>> onSuccess2 = onSuccess;
							if (onSuccess2 == null)
							{
								return;
							}
							onSuccess2(dictionary);
							return;
						}
					}
					this.client.Log("No [data] found in sync response for group [" + groupId + "].", true);
					Action<int, string> onError3 = onError;
					if (onError3 == null)
					{
						return;
					}
					onError3(500, "Server sent invalid data.");
					return;
				}
			});
		}

		// Token: 0x060003F9 RID: 1017 RVA: 0x000119F0 File Offset: 0x0000FBF0
		public void InviteToGroup(string userId, Action<int, string> onError = null)
		{
			MatchmakingWebsocketClient websocketClient = this.client.WebsocketClient;
			Document document = new Document();
			document["action"] = "groups";
			document["method"] = "invite";
			document["userId"] = userId;
			websocketClient.SendRequest(document, delegate(Document responseDocument)
			{
				string text;
				HttpStatusCode httpStatusCode;
				int num;
				if (ErrorsService.TryGetError(responseDocument, out text, out httpStatusCode, out num))
				{
					this.client.Log(string.Format("Error inviting user [{0}] group! Code: {1}, Message: {2}, Index: {3}", new object[] { userId, httpStatusCode, text, num }), true);
					Action<int, string> onError2 = onError;
					if (onError2 == null)
					{
						return;
					}
					onError2((int)httpStatusCode, text);
				}
			});
		}

		// Token: 0x060003FA RID: 1018 RVA: 0x00011A7C File Offset: 0x0000FC7C
		public void JoinGroup(string inviteId, Action<int, string> onError = null)
		{
			MatchmakingWebsocketClient websocketClient = this.client.WebsocketClient;
			Document document = new Document();
			document["action"] = "groups";
			document["method"] = "join";
			document["inviteId"] = inviteId;
			websocketClient.SendRequest(document, delegate(Document responseDocument)
			{
				string text;
				HttpStatusCode httpStatusCode;
				int num;
				if (ErrorsService.TryGetError(responseDocument, out text, out httpStatusCode, out num))
				{
					this.client.Log(string.Format("Error joining group! Code: {0}, Message: {1}, Index: {2}", httpStatusCode, text, num), true);
					Action<int, string> onError2 = onError;
					if (onError2 == null)
					{
						return;
					}
					onError2((int)httpStatusCode, text);
				}
			});
		}

		// Token: 0x060003FB RID: 1019 RVA: 0x00011AFC File Offset: 0x0000FCFC
		public void ChangeHost(string newHostId, Action<int, string> onError = null)
		{
			MatchmakingWebsocketClient websocketClient = this.client.WebsocketClient;
			Document document = new Document();
			document["action"] = "groups";
			document["method"] = "changeHost";
			document["groupHost"] = newHostId;
			websocketClient.SendRequest(document, delegate(Document responseDocument)
			{
				string text;
				HttpStatusCode httpStatusCode;
				int num;
				if (ErrorsService.TryGetError(responseDocument, out text, out httpStatusCode, out num))
				{
					this.client.Log(string.Format("Error changing group host! Code: {0}, Message: {1}, Index: {2}", httpStatusCode, text, num), true);
					Action<int, string> onError2 = onError;
					if (onError2 == null)
					{
						return;
					}
					onError2((int)httpStatusCode, text);
				}
			});
		}

		// Token: 0x060003FC RID: 1020 RVA: 0x00011B7C File Offset: 0x0000FD7C
		public void RemoveFromGroup(string userId, Action<int, string> onError = null)
		{
			MatchmakingWebsocketClient websocketClient = this.client.WebsocketClient;
			Document document = new Document();
			document["action"] = "groups";
			document["method"] = "leave";
			document["userId"] = userId;
			websocketClient.SendRequest(document, delegate(Document responseDocument)
			{
				string text;
				HttpStatusCode httpStatusCode;
				int num;
				if (ErrorsService.TryGetError(responseDocument, out text, out httpStatusCode, out num))
				{
					this.client.Log(string.Format("Error removing user [{0}] from group! Code: {1}, Message: {2}, Index: {3}", new object[] { userId, httpStatusCode, text, num }), true);
					Action<int, string> onError2 = onError;
					if (onError2 == null)
					{
						return;
					}
					onError2((int)httpStatusCode, text);
				}
			});
		}

		// Token: 0x060003FD RID: 1021 RVA: 0x00011C08 File Offset: 0x0000FE08
		public void LeaveGroup(bool stayInMatch = true, Action<int, string> onError = null)
		{
			MatchmakingWebsocketClient websocketClient = this.client.WebsocketClient;
			Document document = new Document();
			document["action"] = "groups";
			document["method"] = "leave";
			document["stayInMatch"] = stayInMatch;
			websocketClient.SendRequest(document, delegate(Document responseDocument)
			{
				string text;
				HttpStatusCode httpStatusCode;
				int num;
				if (ErrorsService.TryGetError(responseDocument, out text, out httpStatusCode, out num))
				{
					this.client.Log(string.Format("Error leaving group! Code: {0}, Message: {1}, Index: {2}", httpStatusCode, text, num), true);
					Action<int, string> onError2 = onError;
					if (onError2 == null)
					{
						return;
					}
					onError2((int)httpStatusCode, text);
				}
			});
		}

		// Token: 0x060003FE RID: 1022 RVA: 0x00011C88 File Offset: 0x0000FE88
		private bool SetGroupData(string sequenceNum, Dictionary<string, AttributeValue> newAttributes, bool triggerMatchEvents = true)
		{
			Dictionary<string, AttributeValue> attributes = this.GroupData.Attributes;
			this.GroupData.CurrentSequenceNum = sequenceNum;
			this.GroupData.Attributes = new Dictionary<string, AttributeValue>(newAttributes);
			if (attributes.Count == 0 && newAttributes.Count > 0)
			{
				Action onGroupJoined = this.OnGroupJoined;
				if (onGroupJoined != null)
				{
					onGroupJoined();
				}
				this.client.Log("OnGroupJoined()", false);
				if (triggerMatchEvents)
				{
					this.client.MatchesService.ProcessEventsQueue();
					AttributeValue attributeValue;
					if (newAttributes.TryGetValue("matchId", out attributeValue))
					{
						string stringProperty = this.client.MatchesService.MatchData.GetStringProperty("matchId");
						if (stringProperty != attributeValue.S)
						{
							if (stringProperty != null)
							{
								this.client.MatchesService.SetMatchData("1", new Dictionary<string, AttributeValue>());
							}
							Document document = new Document();
							string text = "data";
							Dictionary<string, AttributeValue> dictionary = new Dictionary<string, AttributeValue>();
							dictionary["matchId"] = new AttributeValue
							{
								S = (attributeValue.S ?? Guid.NewGuid().ToString())
							};
							dictionary["matchMembers"] = new AttributeValue
							{
								L = new List<AttributeValue>
								{
									new AttributeValue(this.client.GroupsService.GroupData.GetStringProperty("groupId"))
								}
							};
							document[text] = Document.FromAttributeMap(dictionary);
							this.client.MatchesService.SyncMatch(attributeValue.S, new Action<Document>(this.OnMatchData));
						}
					}
					else if (this.client.MatchesService.MatchData.GetStringProperty("matchId") != null)
					{
						this.client.MatchesService.SetMatchData("1", new Dictionary<string, AttributeValue>());
					}
				}
			}
			else if (attributes.Count > 0 && newAttributes.Count > 0)
			{
				AttributeValue attributeValue2;
				AttributeValue attributeValue3;
				if (attributes.TryGetValue("groupHost", out attributeValue2) && newAttributes.TryGetValue("groupHost", out attributeValue3) && attributeValue2.S != attributeValue3.S)
				{
					this.client.Log("OnGroupHostChanged()", false);
					Action<string> onGroupHostChanged = this.OnGroupHostChanged;
					if (onGroupHostChanged != null)
					{
						onGroupHostChanged(attributeValue3.S);
					}
				}
				AttributeValue attributeValue4;
				AttributeValue attributeValue5;
				if (attributes.TryGetValue("groupMembers", out attributeValue4) && newAttributes.TryGetValue("groupMembers", out attributeValue5))
				{
					using (List<AttributeValue>.Enumerator enumerator = attributeValue5.L.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							AttributeValue userIdAttribute2 = enumerator.Current;
							if (attributeValue4.L.All((AttributeValue x) => x.S != userIdAttribute2.S))
							{
								this.client.Log("OnGroupJoin()", false);
								Action<string> onGroupJoin = this.OnGroupJoin;
								if (onGroupJoin != null)
								{
									onGroupJoin(userIdAttribute2.S);
								}
							}
						}
					}
				}
				if (newAttributes.TryGetValue("groupMembers", out attributeValue5) && attributes.TryGetValue("groupMembers", out attributeValue4))
				{
					using (List<AttributeValue>.Enumerator enumerator = attributeValue4.L.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							AttributeValue userIdAttribute = enumerator.Current;
							if (attributeValue5.L.All((AttributeValue x) => x.S != userIdAttribute.S))
							{
								this.client.Log("OnGroupLeave()", false);
								Action<string> onGroupLeave = this.OnGroupLeave;
								if (onGroupLeave != null)
								{
									onGroupLeave(userIdAttribute.S);
								}
							}
						}
					}
				}
				Action<ReadOnlyDictionary<string, AttributeValue>> onGroupUpdated = this.OnGroupUpdated;
				if (onGroupUpdated != null)
				{
					onGroupUpdated(new ReadOnlyDictionary<string, AttributeValue>(attributes));
				}
				this.client.MatchesService.ProcessEventsQueue();
			}
			else if (attributes.Count > 0)
			{
				Action onGroupLeft = this.OnGroupLeft;
				if (onGroupLeft != null)
				{
					onGroupLeft();
				}
				this.client.Log("OnGroupLeft()", false);
			}
			this.ProcessEventsQueue();
			return true;
		}

		// Token: 0x060003FF RID: 1023 RVA: 0x000120B8 File Offset: 0x000102B8
		private void OnMatchData(Document document)
		{
			string text;
			HttpStatusCode httpStatusCode;
			int num;
			if (ErrorsService.TryGetError(document, out text, out httpStatusCode, out num))
			{
				this.client.Log(string.Format("Error syncing match data! Code: {0}, Message: {1}, Index: {2}", httpStatusCode, text, num), true);
				return;
			}
			string stringProperty = this.GroupData.GetStringProperty("groupId");
			if (string.IsNullOrWhiteSpace(stringProperty))
			{
				return;
			}
			Dictionary<string, AttributeValue> dictionary = document["data"].AsDocument().ToAttributeMap();
			AttributeValue attributeValue;
			if (!dictionary.TryGetValue("matchMembers", out attributeValue))
			{
				return;
			}
			IEnumerable<string> enumerable = attributeValue.L.Select((AttributeValue e) => e.S);
			string s = dictionary["matchId"].S;
			string stringProperty2 = this.client.MatchesService.MatchData.GetStringProperty("matchId");
			string stringProperty3 = this.GroupData.GetStringProperty("matchId");
			if (string.IsNullOrWhiteSpace(stringProperty3))
			{
				return;
			}
			if (s == stringProperty3 && s != stringProperty2 && enumerable.Contains(stringProperty))
			{
				EventData eventData = new EventData("matchCreated", "0", dictionary, dictionary);
				this.client.MatchesService.OnCreateEventReceived(eventData);
			}
		}

		// Token: 0x06000400 RID: 1024 RVA: 0x000121FE File Offset: 0x000103FE
		internal void Clear()
		{
			this.GroupData.CurrentSequenceNum = string.Empty;
			this.GroupData.Attributes = null;
			this.groupEventsQueue.Clear();
		}

		// Token: 0x04000287 RID: 647
		internal readonly GroupData GroupData = new GroupData();

		// Token: 0x04000288 RID: 648
		private readonly IMatchmakingClient client;

		// Token: 0x04000289 RID: 649
		private readonly Queue<EventData> groupEventsQueue = new Queue<EventData>();

		// Token: 0x0400028A RID: 650
		private readonly SequentialDecisionTree<EventData> decisionTree = new SequentialDecisionTree<EventData>();
	}
}
