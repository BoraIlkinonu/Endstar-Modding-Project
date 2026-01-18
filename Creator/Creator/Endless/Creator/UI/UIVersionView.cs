using System;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x0200025E RID: 606
	public class UIVersionView : UIBaseView<UIVersion, UIVersionView.Styles>
	{
		// Token: 0x1700013E RID: 318
		// (get) Token: 0x060009DE RID: 2526 RVA: 0x0002D81F File Offset: 0x0002BA1F
		// (set) Token: 0x060009DF RID: 2527 RVA: 0x0002D827 File Offset: 0x0002BA27
		public override UIVersionView.Styles Style { get; protected set; }

		// Token: 0x060009E0 RID: 2528 RVA: 0x0002D830 File Offset: 0x0002BA30
		public override void View(UIVersion model)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "View", new object[] { model });
			}
			this.versionText.Value = model.UserFacingVersion;
		}

		// Token: 0x060009E1 RID: 2529 RVA: 0x0002D866 File Offset: 0x0002BA66
		public override void Clear()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Clear", Array.Empty<object>());
			}
			this.versionText.Clear();
		}

		// Token: 0x04000815 RID: 2069
		[SerializeField]
		private UIText versionText;

		// Token: 0x04000816 RID: 2070
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x0200025F RID: 607
		public enum Styles
		{
			// Token: 0x04000819 RID: 2073
			Default,
			// Token: 0x0400081A RID: 2074
			Publish
		}
	}
}
