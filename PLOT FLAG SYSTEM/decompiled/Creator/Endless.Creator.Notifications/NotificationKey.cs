namespace Endless.Creator.Notifications;

public abstract class NotificationKey<T> where T : NotificationStatus
{
	public bool CompareStatus(T oldStatus, T newStatus)
	{
		if (IsNewer(oldStatus, newStatus))
		{
			newStatus.Status = NotificationState.New;
			return true;
		}
		return false;
	}

	protected abstract bool IsNewer(T oldStatus, T newStatus);
}
