using System;
using Endless.Props.Assets;
using Endless.Shared.Debugging;
using Endless.Shared.UI;

namespace Endless.Creator.UI
{
	// Token: 0x020001E2 RID: 482
	public class UIAudioAssetView : UIBaseView<AudioAsset, UIAudioAssetView.Styles>
	{
		// Token: 0x170000E1 RID: 225
		// (get) Token: 0x06000778 RID: 1912 RVA: 0x000253FF File Offset: 0x000235FF
		// (set) Token: 0x06000779 RID: 1913 RVA: 0x00025407 File Offset: 0x00023607
		public override UIAudioAssetView.Styles Style { get; protected set; }

		// Token: 0x0600077A RID: 1914 RVA: 0x00025410 File Offset: 0x00023610
		public override void View(AudioAsset model)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "View", new object[] { model });
			}
		}

		// Token: 0x0600077B RID: 1915 RVA: 0x0002542F File Offset: 0x0002362F
		public override void Clear()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Clear", Array.Empty<object>());
			}
		}

		// Token: 0x020001E3 RID: 483
		public enum Styles
		{
			// Token: 0x040006BF RID: 1727
			Default
		}
	}
}
