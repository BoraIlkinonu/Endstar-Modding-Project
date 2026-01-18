using System;
using Endless.Gameplay.Stats;
using Endless.Shared.Debugging;

namespace Endless.Creator.UI
{
	// Token: 0x02000250 RID: 592
	public class UIPerPlayerStatPresenter : UIStatBasePresenter<PerPlayerStat>
	{
		// Token: 0x06000994 RID: 2452 RVA: 0x0002C950 File Offset: 0x0002AB50
		protected override void Start()
		{
			base.Start();
			this.perPlayerStatView = base.View.Interface as UIPerPlayerStatView;
			this.perPlayerStatView.DefaultValueChanged += this.SetDefaultValue;
			this.perPlayerStatView.DisplayFormatChanged += this.SetDisplayFormat;
		}

		// Token: 0x06000995 RID: 2453 RVA: 0x0002C9A7 File Offset: 0x0002ABA7
		protected override void OnDestroy()
		{
			base.OnDestroy();
			this.perPlayerStatView.DefaultValueChanged -= this.SetDefaultValue;
			this.perPlayerStatView.DisplayFormatChanged -= this.SetDisplayFormat;
		}

		// Token: 0x06000996 RID: 2454 RVA: 0x0002C9DD File Offset: 0x0002ABDD
		private void SetDefaultValue(string newValue)
		{
			base.Model.DefaultValue = newValue;
			base.InvokeOnModelChanged();
		}

		// Token: 0x06000997 RID: 2455 RVA: 0x0002C9F1 File Offset: 0x0002ABF1
		private void SetDisplayFormat(NumericDisplayFormat displayFormat)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetDisplayFormat", new object[] { displayFormat });
			}
			base.Model.DisplayFormat = displayFormat;
			base.InvokeOnModelChanged();
		}

		// Token: 0x040007E8 RID: 2024
		private UIPerPlayerStatView perPlayerStatView;
	}
}
