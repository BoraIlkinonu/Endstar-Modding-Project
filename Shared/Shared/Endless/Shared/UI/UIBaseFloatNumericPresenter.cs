using System;
using Endless.Shared.Debugging;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x0200020B RID: 523
	public abstract class UIBaseFloatNumericPresenter<TModel> : UIBaseNumericPresenter<TModel>, IUIFloatClampable, IUIUnclampable
	{
		// Token: 0x17000278 RID: 632
		// (get) Token: 0x06000D8C RID: 3468 RVA: 0x0003BA44 File Offset: 0x00039C44
		// (set) Token: 0x06000D8D RID: 3469 RVA: 0x0003BA4C File Offset: 0x00039C4C
		public float Min { get; private set; } = -2E+09f;

		// Token: 0x17000279 RID: 633
		// (get) Token: 0x06000D8E RID: 3470 RVA: 0x0003BA55 File Offset: 0x00039C55
		// (set) Token: 0x06000D8F RID: 3471 RVA: 0x0003BA5D File Offset: 0x00039C5D
		public float Max { get; private set; } = 2E+09f;

		// Token: 0x06000D90 RID: 3472 RVA: 0x0003BA68 File Offset: 0x00039C68
		public override void SetModel(TModel model, bool triggerOnModelChanged)
		{
			for (int i = 0; i < this.FieldCount; i++)
			{
				this.SetMinMax(i, this.Min, this.Max);
			}
			base.SetModel(model, triggerOnModelChanged);
		}

		// Token: 0x06000D91 RID: 3473 RVA: 0x0003BAA4 File Offset: 0x00039CA4
		public void SetMinMax(int index, float min, float max)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4}, {5}: {6} )", new object[] { "SetMinMax", "index", index, "min", min, "max", max }), this);
			}
			this.Min = min;
			this.Max = max;
			this.viewFloatClampable.Interface.SetMinMax(index, min, max);
		}

		// Token: 0x06000D92 RID: 3474 RVA: 0x0003BB2C File Offset: 0x00039D2C
		public override void Unclamp()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("Unclamp", this);
			}
			for (int i = 0; i < this.FieldCount; i++)
			{
				this.SetMinMax(i, float.MinValue, float.MaxValue);
			}
		}

		// Token: 0x040008C4 RID: 2244
		[SerializeField]
		private InterfaceReference<IUIFloatClampable> viewFloatClampable;
	}
}
