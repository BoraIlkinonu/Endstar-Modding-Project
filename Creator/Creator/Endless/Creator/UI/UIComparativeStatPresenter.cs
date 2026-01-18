using System;
using Endless.Gameplay.Stats;
using Endless.Shared.Debugging;

namespace Endless.Creator.UI
{
	// Token: 0x0200024E RID: 590
	public class UIComparativeStatPresenter : UIStatBasePresenter<ComparativeStat>
	{
		// Token: 0x06000985 RID: 2437 RVA: 0x0002C634 File Offset: 0x0002A834
		protected override void Start()
		{
			base.Start();
			this.comparativeStatView = base.View.Interface as UIComparativeStatView;
			this.comparativeStatView.ComparisonChanged += this.SetComparison;
			this.comparativeStatView.DisplayFormatChanged += this.SetDisplayFormat;
		}

		// Token: 0x06000986 RID: 2438 RVA: 0x0002C68B File Offset: 0x0002A88B
		protected override void OnDestroy()
		{
			base.OnDestroy();
			this.comparativeStatView.ComparisonChanged -= this.SetComparison;
			this.comparativeStatView.DisplayFormatChanged -= this.SetDisplayFormat;
		}

		// Token: 0x06000987 RID: 2439 RVA: 0x0002C6C1 File Offset: 0x0002A8C1
		private void SetComparison(ComparativeStat.ValueComparison comparison)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetComparison", new object[] { comparison });
			}
			base.Model.Comparison = comparison;
			base.InvokeOnModelChanged();
		}

		// Token: 0x06000988 RID: 2440 RVA: 0x0002C6F7 File Offset: 0x0002A8F7
		private void SetDisplayFormat(NumericDisplayFormat displayFormat)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetDisplayFormat", new object[] { displayFormat });
			}
			base.Model.DisplayFormat = displayFormat;
			base.InvokeOnModelChanged();
		}

		// Token: 0x040007E3 RID: 2019
		private UIComparativeStatView comparativeStatView;
	}
}
