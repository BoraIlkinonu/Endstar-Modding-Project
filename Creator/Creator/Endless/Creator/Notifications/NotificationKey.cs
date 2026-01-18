using System;

namespace Endless.Creator.Notifications
{
	// Token: 0x02000315 RID: 789
	public abstract class NotificationKey<T> where T : NotificationStatus
	{
		// Token: 0x06000E46 RID: 3654 RVA: 0x00043A73 File Offset: 0x00041C73
		public bool CompareStatus(T oldStatus, T newStatus)
		{
			if (this.IsNewer(oldStatus, newStatus))
			{
				newStatus.Status = NotificationState.New;
				return true;
			}
			return false;
		}

		// Token: 0x06000E47 RID: 3655
		protected abstract bool IsNewer(T oldStatus, T newStatus);
	}
}
