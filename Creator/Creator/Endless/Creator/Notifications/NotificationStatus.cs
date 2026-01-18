using System;

namespace Endless.Creator.Notifications
{
	// Token: 0x02000313 RID: 787
	public class NotificationStatus
	{
		// Token: 0x1700020D RID: 525
		// (get) Token: 0x06000E40 RID: 3648 RVA: 0x00043A33 File Offset: 0x00041C33
		// (set) Token: 0x06000E41 RID: 3649 RVA: 0x00043A3B File Offset: 0x00041C3B
		public NotificationState Status { get; set; } = NotificationState.New;
	}
}
