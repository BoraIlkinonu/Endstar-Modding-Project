using System;

namespace Endless.Shared.UI
{
	// Token: 0x0200023A RID: 570
	public class UIStringPresenter : UIBasePresenter<string>
	{
		// Token: 0x06000E88 RID: 3720 RVA: 0x0003F31C File Offset: 0x0003D51C
		protected override void Start()
		{
			base.Start();
			(base.View.Interface as UIStringView).OnValueChanged += base.SetModelAndTriggerOnModelChanged;
		}
	}
}
