using System;
using Endless.Shared.Debugging;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x020001C9 RID: 457
	[Serializable]
	public struct UIModalGenericViewAction
	{
		// Token: 0x06000B61 RID: 2913 RVA: 0x00031185 File Offset: 0x0002F385
		public UIModalGenericViewAction(Color color, string text, Action onClick)
		{
			this.Color = color;
			this.Text = text;
			this.OnClick = onClick;
		}

		// Token: 0x06000B62 RID: 2914 RVA: 0x0003119C File Offset: 0x0002F39C
		public UIModalGenericViewAction(UIModalGenericViewAction source)
		{
			this.Color = source.Color;
			this.Text = source.Text;
			this.OnClick = source.OnClick;
		}

		// Token: 0x06000B63 RID: 2915 RVA: 0x000311C4 File Offset: 0x0002F3C4
		public override string ToString()
		{
			return string.Format("{0}: {1}, {2}: {3}, {4}: {5}", new object[]
			{
				"Color",
				this.Color,
				"Text",
				this.Text,
				"OnClick",
				this.OnClick.DebugIsNull()
			});
		}

		// Token: 0x04000746 RID: 1862
		public Color Color;

		// Token: 0x04000747 RID: 1863
		public string Text;

		// Token: 0x04000748 RID: 1864
		public Action OnClick;
	}
}
