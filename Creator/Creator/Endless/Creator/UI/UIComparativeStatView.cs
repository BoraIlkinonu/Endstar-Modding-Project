using System;
using Endless.Gameplay.Stats;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x0200024F RID: 591
	public class UIComparativeStatView : UIStatBaseView<ComparativeStat>
	{
		// Token: 0x1400001F RID: 31
		// (add) Token: 0x0600098A RID: 2442 RVA: 0x0002C738 File Offset: 0x0002A938
		// (remove) Token: 0x0600098B RID: 2443 RVA: 0x0002C770 File Offset: 0x0002A970
		public event Action<ComparativeStat.ValueComparison> ComparisonChanged;

		// Token: 0x14000020 RID: 32
		// (add) Token: 0x0600098C RID: 2444 RVA: 0x0002C7A8 File Offset: 0x0002A9A8
		// (remove) Token: 0x0600098D RID: 2445 RVA: 0x0002C7E0 File Offset: 0x0002A9E0
		public event Action<NumericDisplayFormat> DisplayFormatChanged;

		// Token: 0x0600098E RID: 2446 RVA: 0x0002C815 File Offset: 0x0002AA15
		protected override void Start()
		{
			base.Start();
			this.comparisonControl.OnModelChanged += this.InvokeComparisonChanged;
			this.displayFormatControl.OnModelChanged += this.InvokeDisplayFormatChanged;
		}

		// Token: 0x0600098F RID: 2447 RVA: 0x0002C84B File Offset: 0x0002AA4B
		protected override void OnDestroy()
		{
			base.OnDestroy();
			this.comparisonControl.OnModelChanged -= this.InvokeComparisonChanged;
			this.displayFormatControl.OnModelChanged -= this.InvokeDisplayFormatChanged;
		}

		// Token: 0x06000990 RID: 2448 RVA: 0x0002C881 File Offset: 0x0002AA81
		public override void View(ComparativeStat model)
		{
			base.View(model);
			this.comparisonControl.SetModel(model.Comparison, false);
			this.displayFormatControl.SetModel(model.DisplayFormat, false);
		}

		// Token: 0x06000991 RID: 2449 RVA: 0x0002C8B8 File Offset: 0x0002AAB8
		private void InvokeComparisonChanged(object comparison)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "InvokeComparisonChanged", "comparison", comparison), this);
			}
			ComparativeStat.ValueComparison valueComparison = (ComparativeStat.ValueComparison)comparison;
			Action<ComparativeStat.ValueComparison> comparisonChanged = this.ComparisonChanged;
			if (comparisonChanged == null)
			{
				return;
			}
			comparisonChanged(valueComparison);
		}

		// Token: 0x06000992 RID: 2450 RVA: 0x0002C900 File Offset: 0x0002AB00
		private void InvokeDisplayFormatChanged(object displayFormat)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "InvokeDisplayFormatChanged", "displayFormat", displayFormat), this);
			}
			NumericDisplayFormat numericDisplayFormat = (NumericDisplayFormat)displayFormat;
			Action<NumericDisplayFormat> displayFormatChanged = this.DisplayFormatChanged;
			if (displayFormatChanged == null)
			{
				return;
			}
			displayFormatChanged(numericDisplayFormat);
		}

		// Token: 0x040007E4 RID: 2020
		[Header("UIComparativeStatView")]
		[SerializeField]
		private UIEnumPresenter comparisonControl;

		// Token: 0x040007E5 RID: 2021
		[SerializeField]
		private UIEnumPresenter displayFormatControl;
	}
}
