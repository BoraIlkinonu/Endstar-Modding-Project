using System;
using Endless.Gameplay;
using Endless.Shared.Debugging;
using Endless.Shared.UI;

namespace Endless.Creator.UI
{
	// Token: 0x020001E4 RID: 484
	public class UIAutoStatPresenter : UIBasePresenter<GameEndBlock.AutoStat>
	{
		// Token: 0x0600077D RID: 1917 RVA: 0x00025454 File Offset: 0x00023654
		protected override void Start()
		{
			base.Start();
			this.autoStatView = base.View.Interface as UIAutoStatView;
			this.autoStatView.StatChanged += this.SetStat;
			this.autoStatView.PriorityChanged += this.SetPriority;
			this.autoStatView.StatTypeChanged += this.SetStatType;
		}

		// Token: 0x0600077E RID: 1918 RVA: 0x000254C4 File Offset: 0x000236C4
		private void OnDestroy()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDestroy", Array.Empty<object>());
			}
			this.autoStatView.StatChanged -= this.SetStat;
			this.autoStatView.PriorityChanged -= this.SetPriority;
			this.autoStatView.StatTypeChanged -= this.SetStatType;
		}

		// Token: 0x0600077F RID: 1919 RVA: 0x0002552E File Offset: 0x0002372E
		private void SetStat(GameEndBlock.Stats stat)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetStat", new object[] { stat });
			}
			base.Model.Stat = stat;
			base.InvokeOnModelChanged();
		}

		// Token: 0x06000780 RID: 1920 RVA: 0x00025564 File Offset: 0x00023764
		private void SetPriority(int priority)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetPriority", new object[] { priority });
			}
			base.Model.Order = priority;
			base.InvokeOnModelChanged();
		}

		// Token: 0x06000781 RID: 1921 RVA: 0x0002559A File Offset: 0x0002379A
		private void SetStatType(GameEndBlock.StatType statType)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetStatType", new object[] { statType });
			}
			base.Model.StatType = statType;
			base.InvokeOnModelChanged();
		}

		// Token: 0x040006C0 RID: 1728
		private UIAutoStatView autoStatView;
	}
}
