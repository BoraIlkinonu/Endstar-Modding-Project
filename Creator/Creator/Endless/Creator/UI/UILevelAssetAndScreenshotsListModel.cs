using System;
using System.Collections.Generic;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared.Debugging;
using Endless.Shared.UI;

namespace Endless.Creator.UI
{
	// Token: 0x02000132 RID: 306
	public class UILevelAssetAndScreenshotsListModel : UIBaseLocalFilterableListModel<UILevelAssetAndScreenshotsListModelEntry>
	{
		// Token: 0x1700007A RID: 122
		// (get) Token: 0x060004D0 RID: 1232 RVA: 0x0001B5ED File Offset: 0x000197ED
		// (set) Token: 0x060004D1 RID: 1233 RVA: 0x0001B5F5 File Offset: 0x000197F5
		public Dictionary<ScreenshotFileInstances, int> ExteriorSelected { get; private set; } = new Dictionary<ScreenshotFileInstances, int>();

		// Token: 0x1700007B RID: 123
		// (get) Token: 0x060004D2 RID: 1234 RVA: 0x0001B5FE File Offset: 0x000197FE
		protected override Comparison<UILevelAssetAndScreenshotsListModelEntry> DefaultSort
		{
			get
			{
				return (UILevelAssetAndScreenshotsListModelEntry x, UILevelAssetAndScreenshotsListModelEntry y) => string.Compare(x.LevelAsset.Name, y.LevelAsset.Name, StringComparison.Ordinal);
			}
		}

		// Token: 0x060004D3 RID: 1235 RVA: 0x0001B61F File Offset: 0x0001981F
		public override void Clear(bool triggerEvents)
		{
			base.Clear(triggerEvents);
			this.ExteriorSelected.Clear();
		}

		// Token: 0x060004D4 RID: 1236 RVA: 0x0001B634 File Offset: 0x00019834
		public void SetExteriorSelected(Dictionary<ScreenshotFileInstances, int> newValue)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetExteriorSelected", new object[] { newValue.Count });
			}
			this.ExteriorSelected = newValue;
			Action<UIBaseListModel<UILevelAssetAndScreenshotsListModelEntry>> modelChangedAction = UIBaseListModel<UILevelAssetAndScreenshotsListModelEntry>.ModelChangedAction;
			if (modelChangedAction != null)
			{
				modelChangedAction(this);
			}
			base.ModelChangedUnityEvent.Invoke();
		}
	}
}
