using System;

namespace Endless.Core.UI.Settings
{
	// Token: 0x020000AC RID: 172
	public abstract class UIBasePresetChangingQualityLevelHandler : UIBaseQualityLevelHandler
	{
		// Token: 0x17000065 RID: 101
		// (get) Token: 0x060003C0 RID: 960 RVA: 0x000027B9 File Offset: 0x000009B9
		public override bool IsMobileSupported
		{
			get
			{
				return true;
			}
		}

		// Token: 0x040002BF RID: 703
		public static Action DeviatedFromPresetAction;
	}
}
