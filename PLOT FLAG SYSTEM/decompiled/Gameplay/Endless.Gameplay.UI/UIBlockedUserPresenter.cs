using System;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.Social;
using Endless.Shared.UI;
using Runtime.Shared.Matchmaking;

namespace Endless.Gameplay.UI;

public class UIBlockedUserPresenter : UIBasePresenter<BlockedUser>
{
	public static Action<BlockedUser> OnUnblock;

	private UIBlockedUserView typedView;

	protected override void Start()
	{
		base.Start();
		typedView = base.View.Interface as UIBlockedUserView;
		typedView.Unblock += Unblock;
	}

	private void OnDestroy()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnDestroy");
		}
		typedView.Unblock -= Unblock;
	}

	private void Unblock()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Unblock");
		}
		User user = UISocialUtility.ExtractNonActiveUser(base.Model);
		EndlessServices.Instance.CloudService.UnblockUserAsync(user.Id, base.VerboseLogging);
		OnUnblock?.Invoke(base.Model);
	}
}
