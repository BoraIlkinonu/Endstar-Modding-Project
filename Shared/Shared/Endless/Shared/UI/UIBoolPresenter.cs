using System;

namespace Endless.Shared.UI
{
	// Token: 0x020001D9 RID: 473
	public class UIBoolPresenter : UIBasePresenter<bool>
	{
		// Token: 0x06000BA0 RID: 2976 RVA: 0x00032396 File Offset: 0x00030596
		protected override void Start()
		{
			base.Start();
			(base.View.Interface as UIBoolView).OnUserChangedModel += base.SetModelAndTriggerOnModelChanged;
		}
	}
}
