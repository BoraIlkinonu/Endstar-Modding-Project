using System;
using System.Collections.Generic;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared.Debugging;
using Endless.Shared.UI;

namespace Endless.Creator.UI
{
	// Token: 0x0200016E RID: 366
	public class UIScreenshotFileInstancesListModel : UIBaseRearrangeableListModel<ScreenshotFileInstances>
	{
		// Token: 0x17000089 RID: 137
		// (get) Token: 0x0600056C RID: 1388 RVA: 0x0001D0AD File Offset: 0x0001B2AD
		// (set) Token: 0x0600056D RID: 1389 RVA: 0x0001D0B5 File Offset: 0x0001B2B5
		public ScreenshotTypes ScreenshotType { get; private set; }

		// Token: 0x1700008A RID: 138
		// (get) Token: 0x0600056E RID: 1390 RVA: 0x0001D0BE File Offset: 0x0001B2BE
		public int ExteriorSelectedCount
		{
			get
			{
				return this.exteriorSelected.Count;
			}
		}

		// Token: 0x0600056F RID: 1391 RVA: 0x0001D0CB File Offset: 0x0001B2CB
		public override void Clear(bool triggerEvents)
		{
			base.Clear(triggerEvents);
			this.exteriorSelected.Clear();
		}

		// Token: 0x06000570 RID: 1392 RVA: 0x0001D0DF File Offset: 0x0001B2DF
		public void SetExteriorSelected(Dictionary<ScreenshotFileInstances, int> newValue)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetExteriorSelected", new object[] { newValue.Count });
			}
			this.exteriorSelected = newValue;
		}

		// Token: 0x06000571 RID: 1393 RVA: 0x0001D110 File Offset: 0x0001B310
		public int GetExteriorSelectedValue(ScreenshotFileInstances screenshotFileInstances)
		{
			if (base.SuperVerboseLogging)
			{
				DebugUtility.LogMethod(this, "GetExteriorSelectedValue", new object[] { screenshotFileInstances });
				DebugUtility.DebugEnumerable<KeyValuePair<ScreenshotFileInstances, int>>("exteriorSelected", this.exteriorSelected, this);
			}
			if (!this.exteriorSelected.ContainsKey(screenshotFileInstances))
			{
				return -1;
			}
			return this.exteriorSelected[screenshotFileInstances];
		}

		// Token: 0x040004DA RID: 1242
		private Dictionary<ScreenshotFileInstances, int> exteriorSelected = new Dictionary<ScreenshotFileInstances, int>();
	}
}
