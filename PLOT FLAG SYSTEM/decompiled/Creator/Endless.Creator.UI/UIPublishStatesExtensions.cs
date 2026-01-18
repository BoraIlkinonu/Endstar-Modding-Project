namespace Endless.Creator.UI;

public static class UIPublishStatesExtensions
{
	public static string ToEndlessCloudServicesCompatibleString(this UIPublishStates publishState)
	{
		return publishState.ToString().ToLower();
	}
}
