using System;
using Endless.Shared.Debugging;

namespace Endless.Shared.UI
{
	// Token: 0x0200020E RID: 526
	public abstract class UIBaseFloatNumericView<TModel, TViewStyle> : UIBaseNumericView<TModel, TViewStyle>, IUIFloatClampable, IUIUnclampable where TViewStyle : Enum
	{
		// Token: 0x1700027D RID: 637
		// (get) Token: 0x06000DA3 RID: 3491 RVA: 0x000050D2 File Offset: 0x000032D2
		protected override UINumericFieldView.Types UINumericFieldViewType
		{
			get
			{
				return UINumericFieldView.Types.Float;
			}
		}

		// Token: 0x06000DA4 RID: 3492 RVA: 0x0003BD9C File Offset: 0x00039F9C
		public virtual void SetMinMax(int index, float min, float max)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4}, {5}: {6} )", new object[] { "SetMinMax", "index", index, "min", min, "max", max }), this);
			}
			base.NumericFieldViews[index].SetMinAndMax(min, max);
		}

		// Token: 0x06000DA5 RID: 3493 RVA: 0x0003BE18 File Offset: 0x0003A018
		public void Unclamp()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("Unclamp", this);
			}
			foreach (UINumericFieldView uinumericFieldView in base.NumericFieldViews)
			{
				uinumericFieldView.SetMinAndMax(float.MinValue, float.MaxValue);
			}
		}
	}
}
