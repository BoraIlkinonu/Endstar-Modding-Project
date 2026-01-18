using System;
using Endless.Shared.Debugging;

namespace Endless.Shared.UI
{
	// Token: 0x020001CE RID: 462
	public abstract class UIEscapableModalView : UIBaseModalView
	{
		// Token: 0x06000B7D RID: 2941 RVA: 0x00031706 File Offset: 0x0002F906
		public override void OnBack()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("OnBack", this);
			}
			MonoBehaviourSingleton<UIModalManager>.Instance.CloseWithoutClearingStack();
			Action<UIBaseModalView> onModalClosedByUser = UIModalManager.OnModalClosedByUser;
			if (onModalClosedByUser == null)
			{
				return;
			}
			onModalClosedByUser(this);
		}
	}
}
