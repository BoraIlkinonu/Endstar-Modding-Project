using System;

namespace Endless.Gameplay.Screenshotting
{
	// Token: 0x02000428 RID: 1064
	public class ScreenshotOptions
	{
		// Token: 0x06001A63 RID: 6755 RVA: 0x0007942C File Offset: 0x0007762C
		public ScreenshotOptions()
		{
			this.HideCharacter = false;
			this.HideUi = false;
		}

		// Token: 0x06001A64 RID: 6756 RVA: 0x00079450 File Offset: 0x00077650
		public ScreenshotOptions(bool hideCharacter, bool hideUi = true, bool hidePlayerTags = true)
		{
			this.HideCharacter = hideCharacter;
			this.HideUi = hideUi;
			this.HidePlayerTags = hidePlayerTags;
		}

		// Token: 0x17000553 RID: 1363
		// (get) Token: 0x06001A65 RID: 6757 RVA: 0x0007947B File Offset: 0x0007767B
		// (set) Token: 0x06001A66 RID: 6758 RVA: 0x00079483 File Offset: 0x00077683
		public bool HideCharacter { get; private set; }

		// Token: 0x17000554 RID: 1364
		// (get) Token: 0x06001A67 RID: 6759 RVA: 0x0007948C File Offset: 0x0007768C
		// (set) Token: 0x06001A68 RID: 6760 RVA: 0x00079494 File Offset: 0x00077694
		public bool HideUi { get; private set; } = true;

		// Token: 0x17000555 RID: 1365
		// (get) Token: 0x06001A69 RID: 6761 RVA: 0x0007949D File Offset: 0x0007769D
		// (set) Token: 0x06001A6A RID: 6762 RVA: 0x000794A5 File Offset: 0x000776A5
		public bool HidePlayerTags { get; private set; } = true;
	}
}
