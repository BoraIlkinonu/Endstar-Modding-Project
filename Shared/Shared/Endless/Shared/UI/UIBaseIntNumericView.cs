using System;
using Endless.Shared.Debugging;

namespace Endless.Shared.UI
{
	// Token: 0x0200020F RID: 527
	public abstract class UIBaseIntNumericView<TModel, TViewStyle> : UIBaseNumericView<TModel, TViewStyle>, IUIIntClampable, IUIUnclampable where TViewStyle : Enum
	{
		// Token: 0x1700027E RID: 638
		// (get) Token: 0x06000DA7 RID: 3495 RVA: 0x000043C6 File Offset: 0x000025C6
		protected override UINumericFieldView.Types UINumericFieldViewType
		{
			get
			{
				return UINumericFieldView.Types.Int;
			}
		}

		// Token: 0x06000DA8 RID: 3496 RVA: 0x0003BE88 File Offset: 0x0003A088
		public void SetMinMax(int index, int min, int max)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4}, {5}: {6} )", new object[] { "SetMinMax", "index", index, "min", min, "max", max }), this);
			}
			base.NumericFieldViews[index].SetMinAndMax((float)min, (float)max);
		}

		// Token: 0x06000DA9 RID: 3497 RVA: 0x0003BF04 File Offset: 0x0003A104
		public void Unclamp()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("Unclamp", this);
			}
			foreach (UINumericFieldView uinumericFieldView in base.NumericFieldViews)
			{
				uinumericFieldView.SetMinAndMax(-2.1474836E+09f, 2.1474836E+09f);
			}
		}
	}
}
