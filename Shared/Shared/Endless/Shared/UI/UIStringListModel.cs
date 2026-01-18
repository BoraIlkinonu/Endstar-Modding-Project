using System;
using Endless.Shared.Debugging;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x020001BA RID: 442
	public class UIStringListModel : UIBaseLocalFilterableListModel<string>
	{
		// Token: 0x1700021B RID: 539
		// (get) Token: 0x06000B2A RID: 2858 RVA: 0x00030A19 File Offset: 0x0002EC19
		// (set) Token: 0x06000B2B RID: 2859 RVA: 0x00030A21 File Offset: 0x0002EC21
		public bool CanRemove { get; private set; }

		// Token: 0x1700021C RID: 540
		// (get) Token: 0x06000B2C RID: 2860 RVA: 0x00030A2A File Offset: 0x0002EC2A
		// (set) Token: 0x06000B2D RID: 2861 RVA: 0x00030A32 File Offset: 0x0002EC32
		public bool CanEditEntryValue { get; private set; }

		// Token: 0x1700021D RID: 541
		// (get) Token: 0x06000B2E RID: 2862 RVA: 0x00030A3B File Offset: 0x0002EC3B
		// (set) Token: 0x06000B2F RID: 2863 RVA: 0x00030A43 File Offset: 0x0002EC43
		public string DefaultValueOnAdd { get; private set; } = string.Empty;

		// Token: 0x1700021E RID: 542
		// (get) Token: 0x06000B30 RID: 2864 RVA: 0x00030A4C File Offset: 0x0002EC4C
		protected override Comparison<string> DefaultSort
		{
			get
			{
				return (string x, string y) => string.Compare(x, y, StringComparison.Ordinal);
			}
		}

		// Token: 0x06000B31 RID: 2865 RVA: 0x00030A6D File Offset: 0x0002EC6D
		public void SetCanEditEntryValue(bool newValue)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetCanEditEntryValue", new object[] { newValue });
			}
			this.CanEditEntryValue = newValue;
			base.TriggerModelChanged();
		}

		// Token: 0x06000B32 RID: 2866 RVA: 0x00030AA0 File Offset: 0x0002ECA0
		public void TestAdd()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "TestAdd", Array.Empty<object>());
			}
			this.Add(this.Count.ToString(), true);
		}

		// Token: 0x06000B33 RID: 2867 RVA: 0x00030ADC File Offset: 0x0002ECDC
		public void TestChange()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "TestChange", Array.Empty<object>());
			}
			int num = global::UnityEngine.Random.Range(0, this.Count);
			this.Insert(num, global::UnityEngine.Random.Range(-1000, 1000).ToString(), true);
		}

		// Token: 0x06000B34 RID: 2868 RVA: 0x00030B2D File Offset: 0x0002ED2D
		public void TestRemove()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "TestRemove", Array.Empty<object>());
			}
			base.RemoveFilteredAt(this.Count - 1, true);
		}
	}
}
