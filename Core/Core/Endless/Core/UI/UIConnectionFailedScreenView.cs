using System;
using Endless.Shared;
using Endless.Shared.UI;

namespace Endless.Core.UI
{
	// Token: 0x02000079 RID: 121
	public class UIConnectionFailedScreenView : UIBaseScreenView
	{
		// Token: 0x06000261 RID: 609 RVA: 0x0000D39C File Offset: 0x0000B59C
		public static UIConnectionFailedScreenView Display(UIScreenManager.DisplayStackActions displayStackAction)
		{
			return (UIConnectionFailedScreenView)MonoBehaviourSingleton<UIScreenManager>.Instance.Display<UIConnectionFailedScreenView>(displayStackAction, null);
		}
	}
}
