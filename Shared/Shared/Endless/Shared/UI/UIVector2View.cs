using System;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x0200021E RID: 542
	public class UIVector2View : UIBaseFloatNumericView<Vector2, UIVector2View.Styles>
	{
		// Token: 0x17000290 RID: 656
		// (get) Token: 0x06000DF4 RID: 3572 RVA: 0x0003CCAB File Offset: 0x0003AEAB
		// (set) Token: 0x06000DF5 RID: 3573 RVA: 0x0003CCB3 File Offset: 0x0003AEB3
		public override UIVector2View.Styles Style { get; protected set; }

		// Token: 0x17000291 RID: 657
		// (get) Token: 0x06000DF6 RID: 3574 RVA: 0x0003CCBC File Offset: 0x0003AEBC
		public UnityEvent<Vector2> OnValueChanged { get; } = new UnityEvent<Vector2>();

		// Token: 0x06000DF7 RID: 3575 RVA: 0x0003CCC4 File Offset: 0x0003AEC4
		public override void View(Vector2 model)
		{
			base.View(model);
			base.NumericFieldViews[0].SetValue(model.x, false);
			base.NumericFieldViews[1].SetValue(model.y, false);
		}

		// Token: 0x06000DF8 RID: 3576 RVA: 0x0003CD00 File Offset: 0x0003AF00
		protected override void ApplyNumericFieldViewValuesToModel(float fieldModel)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ApplyNumericFieldViewValuesToModel", new object[] { fieldModel });
			}
			Vector2 vector = new Vector2(base.NumericFieldViews[0].Value, base.NumericFieldViews[1].Value);
			this.OnValueChanged.Invoke(vector);
		}

		// Token: 0x0200021F RID: 543
		public enum Styles
		{
			// Token: 0x040008F0 RID: 2288
			Vertical,
			// Token: 0x040008F1 RID: 2289
			Horizontal
		}
	}
}
