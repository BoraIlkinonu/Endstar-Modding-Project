using System;

namespace Endless.Creator.UI
{
	// Token: 0x0200009B RID: 155
	public abstract class UIAssetWithScreenshotsController : UIAssetReadAndWriteController
	{
		// Token: 0x0600026F RID: 623
		protected abstract void RemoveScreenshot(int index);

		// Token: 0x06000270 RID: 624
		protected abstract void RearrangeScreenshots(int oldIndex, int newIndex);
	}
}
