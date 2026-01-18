using System;
using Endless.Shared.Debugging;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x0200026B RID: 619
	public class UIStringTab : UIBaseTab<string>
	{
		// Token: 0x06000F9E RID: 3998 RVA: 0x000432B6 File Offset: 0x000414B6
		protected override void ViewOption(string option)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ViewOption", new object[] { option });
			}
			this.text.Value = option;
		}

		// Token: 0x040009F5 RID: 2549
		[Header("UIStringTab")]
		[SerializeField]
		private UIText text;
	}
}
