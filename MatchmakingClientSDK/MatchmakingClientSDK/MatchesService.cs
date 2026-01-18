using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using MatchmakingClientSDK.Errors;
using MatchmakingClientSDK.Events;

namespace MatchmakingClientSDK
{
	// Token: 0x02000055 RID: 85
	public class MatchesService
	{
		// Token: 0x1400001B RID: 27
		// (add) Token: 0x06000304 RID: 772 RVA: 0x0000D2EC File Offset: 0x0000B4EC
		// (remove) Token: 0x06000305 RID: 773 RVA: 0x0000D324 File Offset: 0x0000B524
		public event Action OnMatchJoined;

		// Token: 0x1400001C RID: 28
		// (add) Token: 0x06000306 RID: 774 RVA: 0x0000D35C File Offset: 0x0000B55C
		// (remove) Token: 0x06000307 RID: 775 RVA: 0x0000D394 File Offset: 0x0000B594
		public event Action<string> OnMatchJoin;

		// Token: 0x1400001D RID: 29
		// (add) Token: 0x06000308 RID: 776 RVA: 0x0000D3CC File Offset: 0x0000B5CC
		// (remove) Token: 0x06000309 RID: 777 RVA: 0x0000D404 File Offset: 0x0000B604
		public event Action<string> OnMatchLeave;

		// Token: 0x1400001E RID: 30
		// (add) Token: 0x0600030A RID: 778 RVA: 0x0000D43C File Offset: 0x0000B63C
		// (remove) Token: 0x0600030B RID: 779 RVA: 0x0000D474 File Offset: 0x0000B674
		public event Action<string> OnMatchHostChanged;

		// Token: 0x1400001F RID: 31
		// (add) Token: 0x0600030C RID: 780 RVA: 0x0000D4AC File Offset: 0x0000B6AC
		// (remove) Token: 0x0600030D RID: 781 RVA: 0x0000D4E4 File Offset: 0x0000B6E4
		public event Action<ReadOnlyDictionary<string, AttributeValue>> OnMatchUpdated;

		// Token: 0x14000020 RID: 32
		// (add) Token: 0x0600030E RID: 782 RVA: 0x0000D51C File Offset: 0x0000B71C
		// (remove) Token: 0x0600030F RID: 783 RVA: 0x0000D554 File Offset: 0x0000B754
		public event Action<string, string, string, int, string, string> OnMatchAllocated;

		// Token: 0x14000021 RID: 33
		// (add) Token: 0x06000310 RID: 784 RVA: 0x0000D58C File Offset: 0x0000B78C
		// (remove) Token: 0x06000311 RID: 785 RVA: 0x0000D5C4 File Offset: 0x0000B7C4
		public event Action<int, string> OnMatchAllocationError;

		// Token: 0x14000022 RID: 34
		// (add) Token: 0x06000312 RID: 786 RVA: 0x0000D5FC File Offset: 0x0000B7FC
		// (remove) Token: 0x06000313 RID: 787 RVA: 0x0000D634 File Offset: 0x0000B834
		public event Action OnHostMigration;

		// Token: 0x14000023 RID: 35
		// (add) Token: 0x06000314 RID: 788 RVA: 0x0000D66C File Offset: 0x0000B86C
		// (remove) Token: 0x06000315 RID: 789 RVA: 0x0000D6A4 File Offset: 0x0000B8A4
		public event Action OnMatchLeft;

		// Token: 0x06000316 RID: 790 RVA: 0x0000D6DC File Offset: 0x0000B8DC
		public MatchesService(IMatchmakingClient client)
		{
			this.client = client;
			IMatchmakingClient matchmakingClient = this.client;
			if (matchmakingClient != null)
			{
				MatchmakingWebsocketClient websocketClient = matchmakingClient.WebsocketClient;
				if (websocketClient != null)
				{
					websocketClient.AddServiceHandler("matches", new Action<Document>(this.OnMessageReceived));
				}
			}
			this.AddCreateTreeNode();
			this.AddUpdateTreeNode();
			this.AddDeleteTreeNode();
		}

		// Token: 0x06000317 RID: 791 RVA: 0x0000D756 File Offset: 0x0000B956
		private void OnMessageReceived(Document doc)
		{
		}

		// Token: 0x06000318 RID: 792 RVA: 0x0000D758 File Offset: 0x0000B958
		private void AddCreateTreeNode()
		{
			TreeNode<EventData> treeNode = TreeNode<EventData>.New(new Func<EventData, bool>(this.HasLocalMatch), TreeNode<EventData>.New(new Func<EventData, bool>(this.EnqueueEvent)), TreeNode<EventData>.New(new Func<EventData, bool>(this.ApplyEventData)));
			this.decisionTree.RootNodes.Add("matchCreated", treeNode);
		}

		// Token: 0x06000319 RID: 793 RVA: 0x0000D7B0 File Offset: 0x0000B9B0
		private void AddUpdateTreeNode()
		{
			TreeNode<EventData> treeNode = TreeNode<EventData>.New(new Func<EventData, bool>(this.HasLocalMatch), TreeNode<EventData>.New(new Func<EventData, bool>(this.IsLocalMatchIdEqualToEventMatchId), TreeNode<EventData>.New(new Func<EventData, bool>(this.IsEventNewerThanLocal), TreeNode<EventData>.New(new Func<EventData, bool>(this.ApplyEventData)), TreeNode<EventData>.New(new Func<EventData, bool>(this.DiscardEvent))), TreeNode<EventData>.New(new Func<EventData, bool>(this.EnqueueEvent))), TreeNode<EventData>.New(new Func<EventData, bool>(this.ApplyEventData)));
			this.decisionTree.RootNodes.Add("matchUpdated", treeNode);
		}

		// Token: 0x0600031A RID: 794 RVA: 0x0000D84C File Offset: 0x0000BA4C
		private void AddDeleteTreeNode()
		{
			TreeNode<EventData> treeNode = TreeNode<EventData>.New(new Func<EventData, bool>(this.HasLocalMatch), TreeNode<EventData>.New(new Func<EventData, bool>(this.IsLocalMatchIdEqualToEventMatchId), TreeNode<EventData>.New(new Func<EventData, bool>(this.ApplyEventData)), TreeNode<EventData>.New(new Func<EventData, bool>(this.EnqueueEvent))), TreeNode<EventData>.New(new Func<EventData, bool>(this.EnqueueEvent)));
			this.decisionTree.RootNodes.Add("matchDeleted", treeNode);
		}

		// Token: 0x0600031B RID: 795 RVA: 0x0000D8C5 File Offset: 0x0000BAC5
		private bool HasLocalMatch(EventData data)
		{
			return this.MatchData.Attributes.Count > 0;
		}

		// Token: 0x0600031C RID: 796 RVA: 0x0000D8DC File Offset: 0x0000BADC
		private bool IsLocalMatchIdEqualToEventMatchId(EventData data)
		{
			if (this.MatchData.Attributes == null)
			{
				return false;
			}
			string stringProperty = this.MatchData.GetStringProperty("matchId");
			AttributeValue attributeValue;
			if (data.Old.TryGetValue("matchId", out attributeValue))
			{
				return stringProperty == attributeValue.S;
			}
			AttributeValue attributeValue2;
			return data.New.TryGetValue("matchId", out attributeValue2) && stringProperty == attributeValue2.S;
		}

		// Token: 0x0600031D RID: 797 RVA: 0x0000D94C File Offset: 0x0000BB4C
		private bool IsEventNewerThanLocal(EventData data)
		{
			return string.Compare(data.SequenceNum, this.MatchData.CurrentSequenceNum, StringComparison.Ordinal) > 0;
		}

		// Token: 0x0600031E RID: 798 RVA: 0x0000D968 File Offset: 0x0000BB68
		private bool IsLocalGroupInEventMatch(EventData data)
		{
			string text = string.Empty;
			AttributeValue attributeValue;
			AttributeValue attributeValue2;
			if (data.New.TryGetValue("matchId", out attributeValue))
			{
				text = attributeValue.S;
			}
			else if (data.Old.TryGetValue("matchId", out attributeValue2))
			{
				text = attributeValue2.S;
			}
			return this.client.GroupsService.GroupData.GetStringProperty("matchId") == text;
		}

		// Token: 0x0600031F RID: 799 RVA: 0x0000D9D4 File Offset: 0x0000BBD4
		private bool IsGroupAddedToMatch(EventData data)
		{
			string stringProperty = this.client.GroupsService.GroupData.GetStringProperty("groupId");
			bool flag = false;
			AttributeValue attributeValue;
			if (data.Old.TryGetValue("matchMembers", out attributeValue))
			{
				flag = attributeValue.L.Select((AttributeValue e) => e.S).Contains(stringProperty);
			}
			bool flag2 = false;
			AttributeValue attributeValue2;
			if (data.New.TryGetValue("matchMembers", out attributeValue2))
			{
				flag2 = attributeValue2.L.Select((AttributeValue e) => e.S).Contains(stringProperty);
			}
			return !flag && flag2;
		}

		// Token: 0x06000320 RID: 800 RVA: 0x0000DA94 File Offset: 0x0000BC94
		private bool IsGroupRemovedFromMatch(EventData data)
		{
			string stringProperty = this.client.GroupsService.GroupData.GetStringProperty("groupId");
			bool flag = false;
			AttributeValue attributeValue;
			if (data.Old.TryGetValue("matchMembers", out attributeValue))
			{
				flag = attributeValue.L.Select((AttributeValue e) => e.S).Contains(stringProperty);
			}
			bool flag2 = false;
			AttributeValue attributeValue2;
			if (data.New.TryGetValue("matchMembers", out attributeValue2))
			{
				flag2 = attributeValue2.L.Select((AttributeValue e) => e.S).Contains(stringProperty);
			}
			return flag && !flag2;
		}

		// Token: 0x06000321 RID: 801 RVA: 0x0000DB54 File Offset: 0x0000BD54
		private bool EnqueueEvent(EventData @event)
		{
			this.matchEventsQueue.Enqueue(@event);
			return false;
		}

		// Token: 0x06000322 RID: 802 RVA: 0x0000DB63 File Offset: 0x0000BD63
		private bool DiscardEvent(EventData @event)
		{
			this.client.Log("Match event discarded: " + @event.EventType + " - " + @event.SequenceNum, false);
			return false;
		}

		// Token: 0x06000323 RID: 803 RVA: 0x0000DB94 File Offset: 0x0000BD94
		private bool ApplyEventData(EventData data)
		{
			if (data.EventType == "matchCreated")
			{
				if (this.IsLocalGroupInEventMatch(data))
				{
					return this.SetMatchData(data.SequenceNum, new Dictionary<string, AttributeValue>(data.New));
				}
				this.matchEventsQueue.Enqueue(data);
				return false;
			}
			else
			{
				if (!(data.EventType == "matchUpdated"))
				{
					return data.EventType == "matchDeleted" && this.SetMatchData(string.Empty, new Dictionary<string, AttributeValue>());
				}
				if (this.MatchData.Attributes.Count == 0 && this.IsGroupAddedToMatch(data))
				{
					if (this.IsLocalGroupInEventMatch(data))
					{
						return this.SetMatchData(data.SequenceNum, new Dictionary<string, AttributeValue>(data.New));
					}
					this.matchEventsQueue.Enqueue(data);
					return false;
				}
				else
				{
					if (this.MatchData.Attributes.Count > 0 && this.IsGroupRemovedFromMatch(data))
					{
						return this.SetMatchData(string.Empty, new Dictionary<string, AttributeValue>());
					}
					if (this.MatchData.Attributes.Count > 0)
					{
						return this.SetMatchData(data.SequenceNum, new Dictionary<string, AttributeValue>(data.New));
					}
					this.matchEventsQueue.Enqueue(data);
					return false;
				}
			}
		}

		// Token: 0x06000324 RID: 804 RVA: 0x0000DCCC File Offset: 0x0000BECC
		internal void ProcessEventsQueue()
		{
			int count = this.matchEventsQueue.Count;
			for (int i = 0; i < count; i++)
			{
				EventData eventData = this.matchEventsQueue.Dequeue();
				DateTime receivedAt = eventData.ReceivedAt;
				if (!(DateTime.Now - receivedAt > TimeSpan.FromSeconds(5.0)))
				{
					if (eventData.EventType == "matchCreated")
					{
						if (this.OnCreateEventReceived(eventData))
						{
							return;
						}
					}
					else if (eventData.EventType == "matchUpdated")
					{
						if (this.OnUpdateEventReceived(eventData))
						{
							return;
						}
					}
					else if (eventData.EventType == "matchDeleted" && this.OnDeleteEventReceived(eventData))
					{
						return;
					}
				}
			}
		}

		// Token: 0x06000325 RID: 805 RVA: 0x0000DD7E File Offset: 0x0000BF7E
		internal bool OnCreateEventReceived(EventData eventData)
		{
			return this.decisionTree.TriggerRoot("matchCreated", eventData);
		}

		// Token: 0x06000326 RID: 806 RVA: 0x0000DD91 File Offset: 0x0000BF91
		internal bool OnUpdateEventReceived(EventData eventData)
		{
			return this.decisionTree.TriggerRoot("matchUpdated", eventData);
		}

		// Token: 0x06000327 RID: 807 RVA: 0x0000DDA4 File Offset: 0x0000BFA4
		internal bool OnDeleteEventReceived(EventData eventData)
		{
			return this.decisionTree.TriggerRoot("matchDeleted", eventData);
		}

		// Token: 0x06000328 RID: 808 RVA: 0x0000DDB8 File Offset: 0x0000BFB8
		internal bool SetMatchData(string sequenceNum, Dictionary<string, AttributeValue> newAttributes)
		{
			Dictionary<string, AttributeValue> attributes = this.MatchData.Attributes;
			this.MatchData.CurrentSequenceNum = sequenceNum;
			this.MatchData.Attributes = newAttributes;
			if (attributes.Count == 0 && newAttributes.Count > 0)
			{
				Action onMatchJoined = this.OnMatchJoined;
				if (onMatchJoined != null)
				{
					onMatchJoined();
				}
				this.client.Log("OnMatchJoined: " + Document.FromAttributeMap(newAttributes).ToJsonPretty(), false);
				this.CheckAllocation(attributes);
			}
			else if (attributes.Count > 0 && newAttributes.Count > 0)
			{
				AttributeValue attributeValue;
				if (attributes.TryGetValue("matchHost", out attributeValue))
				{
					string s = attributeValue.S;
					AttributeValue attributeValue2;
					if (newAttributes.TryGetValue("matchHost", out attributeValue2))
					{
						string s2 = attributeValue2.S;
						if (s != s2)
						{
							this.client.Log("OnMatchHostChanged()", false);
							Action<string> onMatchHostChanged = this.OnMatchHostChanged;
							if (onMatchHostChanged != null)
							{
								onMatchHostChanged(s2);
							}
						}
					}
				}
				AttributeValue attributeValue3;
				AttributeValue attributeValue4;
				if (attributes.TryGetValue("matchMembers", out attributeValue3) && newAttributes.TryGetValue("matchMembers", out attributeValue4))
				{
					using (List<AttributeValue>.Enumerator enumerator = attributeValue4.L.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							AttributeValue groupIdAttribute2 = enumerator.Current;
							if (attributeValue3.L.All((AttributeValue x) => x.S != groupIdAttribute2.S))
							{
								this.client.Log("OnMatchJoin()", false);
								Action<string> onMatchJoin = this.OnMatchJoin;
								if (onMatchJoin != null)
								{
									onMatchJoin(groupIdAttribute2.S);
								}
							}
						}
					}
				}
				if (newAttributes.TryGetValue("matchMembers", out attributeValue4) && attributes.TryGetValue("matchMembers", out attributeValue3))
				{
					using (List<AttributeValue>.Enumerator enumerator = attributeValue3.L.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							AttributeValue groupIdAttribute = enumerator.Current;
							if (attributeValue4.L.All((AttributeValue x) => x.S != groupIdAttribute.S))
							{
								this.client.Log("OnMatchLeave()", false);
								Action<string> onMatchLeave = this.OnMatchLeave;
								if (onMatchLeave != null)
								{
									onMatchLeave(groupIdAttribute.S);
								}
							}
						}
					}
				}
				Action<ReadOnlyDictionary<string, AttributeValue>> onMatchUpdated = this.OnMatchUpdated;
				if (onMatchUpdated != null)
				{
					onMatchUpdated(new ReadOnlyDictionary<string, AttributeValue>(newAttributes));
				}
				this.CheckAllocation(attributes);
			}
			else if (attributes.Count > 0)
			{
				Action onMatchLeft = this.OnMatchLeft;
				if (onMatchLeft != null)
				{
					onMatchLeft();
				}
				this.client.Log("OnMatchLeft: " + Document.FromAttributeMap(attributes).ToJsonPretty(), false);
			}
			this.ProcessEventsQueue();
			return true;
		}

		// Token: 0x06000329 RID: 809 RVA: 0x0000E098 File Offset: 0x0000C298
		private void InvokeHostMigration()
		{
			this.client.Log("OnHostMigration()", false);
			Action onHostMigration = this.OnHostMigration;
			if (onHostMigration == null)
			{
				return;
			}
			onHostMigration();
		}

		// Token: 0x0600032A RID: 810 RVA: 0x0000E0C0 File Offset: 0x0000C2C0
		private void CheckAllocation(Dictionary<string, AttributeValue> oldAttributes)
		{
			Dictionary<string, AttributeValue> attributes = this.MatchData.Attributes;
			string text = string.Empty;
			AttributeValue attributeValue;
			if (oldAttributes.TryGetValue("allocationData", out attributeValue))
			{
				text = attributeValue.S;
			}
			string text2 = string.Empty;
			AttributeValue attributeValue2;
			if (attributes.TryGetValue("allocationData", out attributeValue2))
			{
				text2 = attributeValue2.S;
			}
			if (string.IsNullOrWhiteSpace(text) && !string.IsNullOrWhiteSpace(text2))
			{
				Document document = Document.FromJson(text2);
				string text3;
				HttpStatusCode httpStatusCode;
				int num;
				if (ErrorsService.TryGetError(document, out text3, out httpStatusCode, out num))
				{
					this.client.Log(string.Format("Error allocating match! Code: {0}, Message: {1}, Index: {2}", httpStatusCode, text3, num), true);
					Action<int, string> onMatchAllocationError = this.OnMatchAllocationError;
					if (onMatchAllocationError == null)
					{
						return;
					}
					onMatchAllocationError((int)httpStatusCode, text3);
					return;
				}
				else
				{
					string text4 = document["publicIp"].AsString();
					string text5 = document["localIp"].AsString();
					string text6 = document["name"].AsString();
					int num2 = document["port"].AsInt();
					string text7 = document["key"].AsString();
					string text8 = document["serverType"].AsString();
					this.client.Log("OnMatchAllocated()", false);
					Action<string, string, string, int, string, string> onMatchAllocated = this.OnMatchAllocated;
					if (onMatchAllocated == null)
					{
						return;
					}
					onMatchAllocated(text4, text5, text6, num2, text7, text8);
					return;
				}
			}
			else
			{
				if (!string.IsNullOrWhiteSpace(text) && string.IsNullOrWhiteSpace(text2))
				{
					this.InvokeHostMigration();
					return;
				}
				if (text != text2)
				{
					this.InvokeHostMigration();
					Document document2 = Document.FromJson(text2);
					string text9;
					HttpStatusCode httpStatusCode2;
					int num3;
					if (ErrorsService.TryGetError(document2, out text9, out httpStatusCode2, out num3))
					{
						this.client.Log(string.Format("Error allocating match! Code: {0}, Message: {1}, Index: {2}", httpStatusCode2, text9, num3), true);
						Action<int, string> onMatchAllocationError2 = this.OnMatchAllocationError;
						if (onMatchAllocationError2 == null)
						{
							return;
						}
						onMatchAllocationError2((int)httpStatusCode2, text9);
						return;
					}
					else
					{
						string text10 = document2["publicIp"].AsString();
						string text11 = document2["localIp"].AsString();
						string text12 = document2["name"].AsString();
						int num4 = document2["port"].AsInt();
						string text13 = document2["key"].AsString();
						string text14 = document2["serverType"].AsString();
						this.client.Log("OnMatchAllocated()", false);
						Action<string, string, string, int, string, string> onMatchAllocated2 = this.OnMatchAllocated;
						if (onMatchAllocated2 == null)
						{
							return;
						}
						onMatchAllocated2(text10, text11, text12, num4, text13, text14);
					}
				}
				return;
			}
		}

		// Token: 0x0600032B RID: 811 RVA: 0x0000E348 File Offset: 0x0000C548
		public void SyncMatch(string matchId = null, Action<Document> callback = null)
		{
			Document document = new Document();
			document["action"] = "matches";
			document["method"] = "sync";
			Document document2 = document;
			if (!string.IsNullOrWhiteSpace(matchId))
			{
				document2["matchId"] = matchId;
			}
			this.client.WebsocketClient.SendRequest(document2, callback);
		}

		// Token: 0x0600032C RID: 812 RVA: 0x0000E3B0 File Offset: 0x0000C5B0
		public string GetStringProperty(string key)
		{
			return this.MatchData.GetStringProperty(key);
		}

		// Token: 0x0600032D RID: 813 RVA: 0x0000E3BE File Offset: 0x0000C5BE
		public ReadOnlyDictionary<string, AttributeValue> GetLocalAttributes()
		{
			return new ReadOnlyDictionary<string, AttributeValue>(this.MatchData.Attributes);
		}

		// Token: 0x0600032E RID: 814 RVA: 0x0000E3D0 File Offset: 0x0000C5D0
		public void StartMatch(string gameId, string levelId, bool isEditSession, string version, string serverType, string matchData, Action<int, string> onError = null)
		{
			MatchmakingWebsocketClient websocketClient = this.client.WebsocketClient;
			Document document = new Document();
			document["action"] = "matches";
			document["method"] = "start";
			document["gameId"] = gameId;
			document["levelId"] = levelId;
			document["isEditSession"] = isEditSession;
			document["version"] = version;
			document["serverType"] = serverType;
			document["matchData"] = matchData;
			websocketClient.SendRequest(document, delegate(Document doc)
			{
				string text;
				HttpStatusCode httpStatusCode;
				int num;
				if (ErrorsService.TryGetError(doc, out text, out httpStatusCode, out num))
				{
					Action<int, string> onError2 = onError;
					if (onError2 == null)
					{
						return;
					}
					onError2((int)httpStatusCode, text);
				}
			});
		}

		// Token: 0x0600032F RID: 815 RVA: 0x0000E4A0 File Offset: 0x0000C6A0
		public void GetServerWhitelist(Action<List<string>> onSuccess, Action<int, string> onError = null)
		{
			MatchmakingWebsocketClient websocketClient = this.client.WebsocketClient;
			Document document = new Document();
			document["action"] = "matches";
			document["method"] = "serverWhitelist";
			websocketClient.SendRequest(document, delegate(Document doc)
			{
				string text;
				HttpStatusCode httpStatusCode;
				int num;
				if (!ErrorsService.TryGetError(doc, out text, out httpStatusCode, out num))
				{
					DynamoDBEntry dynamoDBEntry;
					if (doc.TryGetValue("whitelist", out dynamoDBEntry))
					{
						List<string> list = dynamoDBEntry.AsListOfString();
						Action<List<string>> onSuccess2 = onSuccess;
						if (onSuccess2 == null)
						{
							return;
						}
						onSuccess2(list);
					}
					return;
				}
				Action<int, string> onError2 = onError;
				if (onError2 == null)
				{
					return;
				}
				onError2((int)httpStatusCode, text);
			});
		}

		// Token: 0x06000330 RID: 816 RVA: 0x0000E50C File Offset: 0x0000C70C
		public void AllocateMatch(string publicIp, string localIp, string name, int port, string authKey, string serverType, Action<int, string> onError)
		{
			MatchmakingWebsocketClient websocketClient = this.client.WebsocketClient;
			Document document = new Document();
			document["action"] = "matches";
			document["method"] = "allocate";
			string text = "data";
			Document document2 = new Document();
			document2["publicIp"] = publicIp;
			document2["localIp"] = localIp;
			document2["name"] = name;
			document2["port"] = port;
			document2["key"] = authKey;
			document2["serverType"] = serverType;
			document[text] = document2;
			websocketClient.SendRequest(document, delegate(Document doc)
			{
				string text2;
				HttpStatusCode httpStatusCode;
				int num;
				if (ErrorsService.TryGetError(doc, out text2, out httpStatusCode, out num))
				{
					Action<int, string> onError2 = onError;
					if (onError2 == null)
					{
						return;
					}
					onError2((int)httpStatusCode, text2);
				}
			});
		}

		// Token: 0x06000331 RID: 817 RVA: 0x0000E5EC File Offset: 0x0000C7EC
		public void ChangeHost(string newHostId, Action<int, string> onError = null)
		{
			MatchmakingWebsocketClient websocketClient = this.client.WebsocketClient;
			Document document = new Document();
			document["action"] = "matches";
			document["method"] = "changeHost";
			document["matchHost"] = newHostId;
			websocketClient.SendRequest(document, delegate(Document doc)
			{
				string text;
				HttpStatusCode httpStatusCode;
				int num;
				if (ErrorsService.TryGetError(doc, out text, out httpStatusCode, out num))
				{
					Action<int, string> onError2 = onError;
					if (onError2 == null)
					{
						return;
					}
					onError2((int)httpStatusCode, text);
				}
			});
		}

		// Token: 0x06000332 RID: 818 RVA: 0x0000E664 File Offset: 0x0000C864
		public void RemoveFromMatch(string groupId, Action<int, string> onError = null)
		{
			MatchmakingWebsocketClient websocketClient = this.client.WebsocketClient;
			Document document = new Document();
			document["action"] = "matches";
			document["method"] = "leave";
			document["groupId"] = groupId;
			websocketClient.SendRequest(document, delegate(Document doc)
			{
				string text;
				HttpStatusCode httpStatusCode;
				int num;
				if (ErrorsService.TryGetError(doc, out text, out httpStatusCode, out num))
				{
					Action<int, string> onError2 = onError;
					if (onError2 == null)
					{
						return;
					}
					onError2((int)httpStatusCode, text);
				}
			});
		}

		// Token: 0x06000333 RID: 819 RVA: 0x0000E6DC File Offset: 0x0000C8DC
		public void ChangeMatch(string gameId, string levelId, bool isEditSession, string version, string serverType, string matchData, Action<int, string> onError = null, bool endMatch = false, params string[] userIds)
		{
			MatchmakingWebsocketClient websocketClient = this.client.WebsocketClient;
			Document document = new Document();
			document["action"] = "matches";
			document["method"] = "change";
			document["gameId"] = gameId;
			document["levelId"] = levelId;
			document["isEditSession"] = isEditSession;
			document["version"] = version;
			document["serverType"] = serverType;
			document["matchData"] = matchData;
			document["endMatch"] = endMatch;
			document["changeMatchUsers"] = userIds;
			websocketClient.SendRequest(document, delegate(Document doc)
			{
				string text;
				HttpStatusCode httpStatusCode;
				int num;
				if (ErrorsService.TryGetError(doc, out text, out httpStatusCode, out num))
				{
					Action<int, string> onError2 = onError;
					if (onError2 == null)
					{
						return;
					}
					onError2((int)httpStatusCode, text);
				}
			});
		}

		// Token: 0x06000334 RID: 820 RVA: 0x0000E7D0 File Offset: 0x0000C9D0
		public void EndMatch(Action<int, string> onError = null)
		{
			MatchmakingWebsocketClient websocketClient = this.client.WebsocketClient;
			Document document = new Document();
			document["action"] = "matches";
			document["method"] = "leave";
			websocketClient.SendRequest(document, delegate(Document doc)
			{
				string text;
				HttpStatusCode httpStatusCode;
				int num;
				if (ErrorsService.TryGetError(doc, out text, out httpStatusCode, out num))
				{
					Action<int, string> onError2 = onError;
					if (onError2 == null)
					{
						return;
					}
					onError2((int)httpStatusCode, text);
				}
			});
		}

		// Token: 0x06000335 RID: 821 RVA: 0x0000E835 File Offset: 0x0000CA35
		internal void Clear()
		{
			this.MatchData.CurrentSequenceNum = string.Empty;
			this.MatchData.Attributes = null;
			this.matchEventsQueue.Clear();
		}

		// Token: 0x040001C6 RID: 454
		internal readonly MatchData MatchData = new MatchData();

		// Token: 0x040001C7 RID: 455
		private readonly IMatchmakingClient client;

		// Token: 0x040001C8 RID: 456
		private readonly Queue<EventData> matchEventsQueue = new Queue<EventData>();

		// Token: 0x040001C9 RID: 457
		private readonly SequentialDecisionTree<EventData> decisionTree = new SequentialDecisionTree<EventData>();
	}
}
