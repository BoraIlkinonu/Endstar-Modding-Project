using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using MatchmakingClientSDK.Events;

namespace MatchmakingClientSDK
{
	// Token: 0x02000051 RID: 81
	public class EventsService
	{
		// Token: 0x14000012 RID: 18
		// (add) Token: 0x060002E2 RID: 738 RVA: 0x0000CC14 File Offset: 0x0000AE14
		// (remove) Token: 0x060002E3 RID: 739 RVA: 0x0000CC4C File Offset: 0x0000AE4C
		internal event Func<EventData, bool> OnGroupCreated;

		// Token: 0x14000013 RID: 19
		// (add) Token: 0x060002E4 RID: 740 RVA: 0x0000CC84 File Offset: 0x0000AE84
		// (remove) Token: 0x060002E5 RID: 741 RVA: 0x0000CCBC File Offset: 0x0000AEBC
		internal event Func<EventData, bool> OnGroupUpdated;

		// Token: 0x14000014 RID: 20
		// (add) Token: 0x060002E6 RID: 742 RVA: 0x0000CCF4 File Offset: 0x0000AEF4
		// (remove) Token: 0x060002E7 RID: 743 RVA: 0x0000CD2C File Offset: 0x0000AF2C
		internal event Func<EventData, bool> OnGroupDeleted;

		// Token: 0x14000015 RID: 21
		// (add) Token: 0x060002E8 RID: 744 RVA: 0x0000CD64 File Offset: 0x0000AF64
		// (remove) Token: 0x060002E9 RID: 745 RVA: 0x0000CD9C File Offset: 0x0000AF9C
		internal event Func<EventData, bool> OnMatchCreated;

		// Token: 0x14000016 RID: 22
		// (add) Token: 0x060002EA RID: 746 RVA: 0x0000CDD4 File Offset: 0x0000AFD4
		// (remove) Token: 0x060002EB RID: 747 RVA: 0x0000CE0C File Offset: 0x0000B00C
		internal event Func<EventData, bool> OnMatchUpdated;

		// Token: 0x14000017 RID: 23
		// (add) Token: 0x060002EC RID: 748 RVA: 0x0000CE44 File Offset: 0x0000B044
		// (remove) Token: 0x060002ED RID: 749 RVA: 0x0000CE7C File Offset: 0x0000B07C
		internal event Func<EventData, bool> OnMatchDeleted;

		// Token: 0x14000018 RID: 24
		// (add) Token: 0x060002EE RID: 750 RVA: 0x0000CEB4 File Offset: 0x0000B0B4
		// (remove) Token: 0x060002EF RID: 751 RVA: 0x0000CEEC File Offset: 0x0000B0EC
		internal event Func<EventData, bool> OnUserConnected;

		// Token: 0x14000019 RID: 25
		// (add) Token: 0x060002F0 RID: 752 RVA: 0x0000CF24 File Offset: 0x0000B124
		// (remove) Token: 0x060002F1 RID: 753 RVA: 0x0000CF5C File Offset: 0x0000B15C
		internal event Func<EventData, bool> OnUserUpdated;

		// Token: 0x1400001A RID: 26
		// (add) Token: 0x060002F2 RID: 754 RVA: 0x0000CF94 File Offset: 0x0000B194
		// (remove) Token: 0x060002F3 RID: 755 RVA: 0x0000CFCC File Offset: 0x0000B1CC
		internal event Func<EventData, bool> OnUserDisconnected;

		// Token: 0x060002F4 RID: 756 RVA: 0x0000D001 File Offset: 0x0000B201
		public EventsService(IMatchmakingClient client)
		{
			this.client = client;
			IMatchmakingClient matchmakingClient = this.client;
			if (matchmakingClient == null)
			{
				return;
			}
			MatchmakingWebsocketClient websocketClient = matchmakingClient.WebsocketClient;
			if (websocketClient == null)
			{
				return;
			}
			websocketClient.AddServiceHandler("events", new Action<Document>(this.OnMessageReceived));
		}

		// Token: 0x060002F5 RID: 757 RVA: 0x0000D03C File Offset: 0x0000B23C
		internal void OnMessageReceived(Document document)
		{
			DynamoDBEntry dynamoDBEntry;
			if (!document.TryGetValue("eventName", out dynamoDBEntry))
			{
				this.client.Log("[eventName] not found in incoming message.", true);
				return;
			}
			string text = dynamoDBEntry.AsString();
			DynamoDBEntry dynamoDBEntry2;
			if (!document.TryGetValue("sequenceNum", out dynamoDBEntry2))
			{
				this.client.Log("[sequenceNum] not found in incoming message.", true);
				return;
			}
			string text2 = dynamoDBEntry2.AsString();
			DynamoDBEntry dynamoDBEntry3;
			if (!document.TryGetValue("data", out dynamoDBEntry3))
			{
				this.client.Log("[data] not found in incoming message.", true);
				return;
			}
			Document document2 = dynamoDBEntry3.AsDocument();
			Dictionary<string, AttributeValue> dictionary = document2["old"].AsDocument().ToAttributeMap();
			Dictionary<string, AttributeValue> dictionary2 = document2["new"].AsDocument().ToAttributeMap();
			EventData eventData = new EventData(text, text2, dictionary, dictionary2);
			if (text != null)
			{
				switch (text.Length)
				{
				case 11:
				{
					if (!(text == "userUpdated"))
					{
						return;
					}
					Func<EventData, bool> onUserUpdated = this.OnUserUpdated;
					if (onUserUpdated == null)
					{
						return;
					}
					onUserUpdated(eventData);
					return;
				}
				case 12:
				{
					char c = text[5];
					if (c != 'C')
					{
						if (c != 'D')
						{
							if (c != 'U')
							{
								return;
							}
							if (!(text == "groupUpdated"))
							{
								if (!(text == "matchUpdated"))
								{
									return;
								}
								Func<EventData, bool> onMatchUpdated = this.OnMatchUpdated;
								if (onMatchUpdated == null)
								{
									return;
								}
								onMatchUpdated(eventData);
								return;
							}
							else
							{
								Func<EventData, bool> onGroupUpdated = this.OnGroupUpdated;
								if (onGroupUpdated == null)
								{
									return;
								}
								onGroupUpdated(eventData);
								return;
							}
						}
						else if (!(text == "groupDeleted"))
						{
							if (!(text == "matchDeleted"))
							{
								return;
							}
							Func<EventData, bool> onMatchDeleted = this.OnMatchDeleted;
							if (onMatchDeleted == null)
							{
								return;
							}
							onMatchDeleted(eventData);
						}
						else
						{
							Func<EventData, bool> onGroupDeleted = this.OnGroupDeleted;
							if (onGroupDeleted == null)
							{
								return;
							}
							onGroupDeleted(eventData);
							return;
						}
					}
					else if (!(text == "groupCreated"))
					{
						if (!(text == "matchCreated"))
						{
							return;
						}
						Func<EventData, bool> onMatchCreated = this.OnMatchCreated;
						if (onMatchCreated == null)
						{
							return;
						}
						onMatchCreated(eventData);
						return;
					}
					else
					{
						Func<EventData, bool> onGroupCreated = this.OnGroupCreated;
						if (onGroupCreated == null)
						{
							return;
						}
						onGroupCreated(eventData);
						return;
					}
					break;
				}
				case 13:
				{
					if (!(text == "userConnected"))
					{
						return;
					}
					Func<EventData, bool> onUserConnected = this.OnUserConnected;
					if (onUserConnected == null)
					{
						return;
					}
					onUserConnected(eventData);
					return;
				}
				case 14:
				case 15:
					break;
				case 16:
				{
					if (!(text == "userDisconnected"))
					{
						return;
					}
					Func<EventData, bool> onUserDisconnected = this.OnUserDisconnected;
					if (onUserDisconnected == null)
					{
						return;
					}
					onUserDisconnected(eventData);
					return;
				}
				default:
					return;
				}
			}
		}

		// Token: 0x060002F6 RID: 758 RVA: 0x0000D29A File Offset: 0x0000B49A
		internal void Clear()
		{
		}

		// Token: 0x040001BE RID: 446
		private readonly IMatchmakingClient client;
	}
}
