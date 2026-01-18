using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Gameplay.UI;

public class UIGameplayPlayerListView : UIBaseListView<PlayerReferenceManager>
{
	protected override void Start()
	{
		base.Start();
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		base.ScrollerScrolledUnityEvent.AddListener(OnScroll);
	}

	private void OnScroll(Vector2 arg0, float arg1)
	{
		foreach (UIBaseListItemView<PlayerReferenceManager> visibleCellView in GetVisibleCellViews(ifRowReturnCellsInstead: true))
		{
			(visibleCellView as UIGameplayPlayerListCellView).SetSpacingForSocialButton();
		}
	}
}
