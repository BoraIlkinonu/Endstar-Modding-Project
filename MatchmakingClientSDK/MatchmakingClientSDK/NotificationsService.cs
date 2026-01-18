using System;
using Amazon.DynamoDBv2.DocumentModel;
using MatchmakingAPI.Notifications;

namespace MatchmakingClientSDK
{
	// Token: 0x02000059 RID: 89
	public class NotificationsService
	{
		// Token: 0x14000028 RID: 40
		// (add) Token: 0x0600036D RID: 877 RVA: 0x0000F4CC File Offset: 0x0000D6CC
		// (remove) Token: 0x0600036E RID: 878 RVA: 0x0000F504 File Offset: 0x0000D704
		public event Action<NotificationTypes, Document> OnNotificationReceived;

		// Token: 0x0600036F RID: 879 RVA: 0x0000F539 File Offset: 0x0000D739
		public NotificationsService(IMatchmakingClient client)
		{
			this.client = client;
			if (client != null)
			{
				MatchmakingWebsocketClient websocketClient = client.WebsocketClient;
				if (websocketClient == null)
				{
					return;
				}
				websocketClient.AddServiceHandler("notifications", new Action<Document>(this.OnMessageReceived));
			}
		}

		// Token: 0x06000370 RID: 880 RVA: 0x0000F56C File Offset: 0x0000D76C
		internal void OnMessageReceived(Document message)
		{
		}

		// Token: 0x06000371 RID: 881 RVA: 0x0000F56E File Offset: 0x0000D76E
		internal void PushNotification(NotificationTypes notificationType, Document notificationData)
		{
			Action<NotificationTypes, Document> onNotificationReceived = this.OnNotificationReceived;
			if (onNotificationReceived == null)
			{
				return;
			}
			onNotificationReceived(notificationType, notificationData);
		}

		// Token: 0x06000372 RID: 882 RVA: 0x0000F582 File Offset: 0x0000D782
		internal void Clear()
		{
		}

		// Token: 0x040001F9 RID: 505
		private readonly IMatchmakingClient client;
	}
}
