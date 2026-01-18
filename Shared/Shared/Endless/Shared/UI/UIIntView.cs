using System;
using Endless.Shared.Debugging;

namespace Endless.Shared.UI
{
	// Token: 0x02000217 RID: 535
	public class UIIntView : UIBaseIntView
	{
		// Token: 0x06000DCA RID: 3530 RVA: 0x0003C33B File Offset: 0x0003A53B
		public override void View(int model)
		{
			base.View(model);
			base.NumericFieldViews[0].SetValue((float)model, false);
		}

		// Token: 0x06000DCB RID: 3531 RVA: 0x0003C358 File Offset: 0x0003A558
		protected override void ApplyNumericFieldViewValuesToModel(float fieldModel)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ApplyNumericFieldViewValuesToModel", new object[] { fieldModel });
			}
			base.OnValueChanged.Invoke((int)fieldModel);
		}
	}
}
