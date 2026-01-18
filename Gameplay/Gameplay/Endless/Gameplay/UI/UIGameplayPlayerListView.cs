using System;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay.UI
{
	// Token: 0x020003C1 RID: 961
	public class UIGameplayPlayerListView : UIBaseListView<PlayerReferenceManager>
	{
		// Token: 0x06001877 RID: 6263 RVA: 0x00071B6B File Offset: 0x0006FD6B
		protected override void Start()
		{
			base.Start();
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			base.ScrollerScrolledUnityEvent.AddListener(new UnityAction<Vector2, float>(this.OnScroll));
		}

		// Token: 0x06001878 RID: 6264 RVA: 0x00071BA4 File Offset: 0x0006FDA4
		private void OnScroll(Vector2 arg0, float arg1)
		{
			foreach (UIBaseListItemView<PlayerReferenceManager> uibaseListItemView in base.GetVisibleCellViews(true, 0.1f))
			{
				(uibaseListItemView as UIGameplayPlayerListCellView).SetSpacingForSocialButton();
			}
		}
	}
}
