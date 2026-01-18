using System;
using Endless.Gameplay;
using Endless.Shared.Debugging;
using Endless.Shared.UI;

namespace Endless.Creator.UI
{
	// Token: 0x020001BA RID: 442
	public class UILevelDestinationSelectionModalView : UIEscapableModalView
	{
		// Token: 0x06000693 RID: 1683 RVA: 0x00021DED File Offset: 0x0001FFED
		public override void OnDisplay(params object[] modalData)
		{
			base.OnDisplay(modalData);
			this.levelDestinationPresenterToApplyTo = modalData[0] as UILevelDestinationPresenter;
		}

		// Token: 0x06000694 RID: 1684 RVA: 0x00021E04 File Offset: 0x00020004
		public void ApplyToProperty(LevelDestination levelDestination)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ApplyToProperty", new object[] { levelDestination.TargetLevelId });
			}
			this.levelDestinationPresenterToApplyTo.SetLevelDestination(levelDestination);
		}

		// Token: 0x040005E5 RID: 1509
		private UILevelDestinationPresenter levelDestinationPresenterToApplyTo;
	}
}
