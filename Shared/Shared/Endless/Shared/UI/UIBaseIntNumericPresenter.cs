using System;
using Endless.Shared.Debugging;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x0200020C RID: 524
	public abstract class UIBaseIntNumericPresenter<TModel> : UIBaseNumericPresenter<TModel>, IUIIntClampable, IUIUnclampable
	{
		// Token: 0x1700027A RID: 634
		// (get) Token: 0x06000D94 RID: 3476 RVA: 0x0003BB8C File Offset: 0x00039D8C
		// (set) Token: 0x06000D95 RID: 3477 RVA: 0x0003BB94 File Offset: 0x00039D94
		public int Min { get; private set; } = -2000000000;

		// Token: 0x1700027B RID: 635
		// (get) Token: 0x06000D96 RID: 3478 RVA: 0x0003BB9D File Offset: 0x00039D9D
		// (set) Token: 0x06000D97 RID: 3479 RVA: 0x0003BBA5 File Offset: 0x00039DA5
		public int Max { get; private set; } = 2000000000;

		// Token: 0x06000D98 RID: 3480 RVA: 0x0003BBB0 File Offset: 0x00039DB0
		public override void SetModel(TModel model, bool triggerOnModelChanged)
		{
			for (int i = 0; i < this.FieldCount; i++)
			{
				this.SetMinMax(i, this.Min, this.Max);
			}
			base.SetModel(model, triggerOnModelChanged);
		}

		// Token: 0x06000D99 RID: 3481 RVA: 0x0003BBEC File Offset: 0x00039DEC
		public void SetMinMax(int index, int min, int max)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4}, {5}: {6} )", new object[] { "SetMinMax", "index", index, "min", min, "max", max }), this);
			}
			this.Min = min;
			this.Max = max;
			this.viewIntClampable.Interface.SetMinMax(index, min, max);
		}

		// Token: 0x06000D9A RID: 3482 RVA: 0x0003BC74 File Offset: 0x00039E74
		public override void Unclamp()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("Unclamp", this);
			}
			for (int i = 0; i < this.FieldCount; i++)
			{
				this.SetMinMax(i, int.MinValue, int.MaxValue);
			}
		}

		// Token: 0x040008C7 RID: 2247
		[SerializeField]
		private InterfaceReference<IUIIntClampable> viewIntClampable;
	}
}
