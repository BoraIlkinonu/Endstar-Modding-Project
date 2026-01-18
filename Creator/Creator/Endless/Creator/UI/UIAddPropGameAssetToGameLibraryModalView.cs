using System;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;

namespace Endless.Creator.UI
{
	// Token: 0x0200019F RID: 415
	public class UIAddPropGameAssetToGameLibraryModalView : UIBaseModalView
	{
		// Token: 0x0600060E RID: 1550 RVA: 0x0001F330 File Offset: 0x0001D530
		public override void OnBack()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("OnBack", this);
			}
			MonoBehaviourSingleton<UIModalManager>.Instance.CloseWithoutClearingStack();
		}
	}
}
