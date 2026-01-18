using System;
using Endless.Shared.Debugging;
using TMPro;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x020001C7 RID: 455
	public class UIErrorModalView : UIBaseModalView
	{
		// Token: 0x06000B54 RID: 2900 RVA: 0x00030FCD File Offset: 0x0002F1CD
		public override void OnBack()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnBack", Array.Empty<object>());
			}
		}

		// Token: 0x06000B55 RID: 2901 RVA: 0x00030FE8 File Offset: 0x0002F1E8
		public override void OnDisplay(params object[] modalData)
		{
			base.OnDisplay(modalData);
			string text = modalData[0] as string;
			this.text.text = text;
		}

		// Token: 0x04000740 RID: 1856
		[Header("UIErrorModalView")]
		[SerializeField]
		private TextMeshProUGUI text;
	}
}
