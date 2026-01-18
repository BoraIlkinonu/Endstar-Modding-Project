using System;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.Social;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Gameplay.UI;

public class UIBlockedUserView : UIBaseSocialView<BlockedUser>
{
	[Header("UIBlockedUserView")]
	[SerializeField]
	private UIUserView userView;

	[SerializeField]
	private UIButton unblockButton;

	public event Action Unblock;

	private void Start()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		unblockButton.onClick.AddListener(InvokeUnblock);
	}

	public override void View(BlockedUser model)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "View", model);
		}
		User model2 = UISocialUtility.ExtractNonActiveUser(model);
		userView.View(model2);
	}

	private void InvokeUnblock()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "InvokeUnblock");
		}
		this.Unblock?.Invoke();
	}
}
