using System;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x0200022A RID: 554
	public class UIVector4View : UIBaseFloatNumericView<Vector4, UIVector4View.Styles>
	{
		// Token: 0x1700029C RID: 668
		// (get) Token: 0x06000E1C RID: 3612 RVA: 0x0003D341 File Offset: 0x0003B541
		// (set) Token: 0x06000E1D RID: 3613 RVA: 0x0003D349 File Offset: 0x0003B549
		public override UIVector4View.Styles Style { get; protected set; }

		// Token: 0x1700029D RID: 669
		// (get) Token: 0x06000E1E RID: 3614 RVA: 0x0003D352 File Offset: 0x0003B552
		public UnityEvent<Vector4> OnValueChanged { get; } = new UnityEvent<Vector4>();

		// Token: 0x06000E1F RID: 3615 RVA: 0x0003D35C File Offset: 0x0003B55C
		public override void View(Vector4 model)
		{
			base.View(model);
			base.NumericFieldViews[0].SetValue(model.x, false);
			base.NumericFieldViews[1].SetValue(model.y, false);
			base.NumericFieldViews[2].SetValue(model.z, false);
			base.NumericFieldViews[3].SetValue(model.w, false);
		}

		// Token: 0x06000E20 RID: 3616 RVA: 0x0003D3D0 File Offset: 0x0003B5D0
		protected override void ApplyNumericFieldViewValuesToModel(float fieldModel)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ApplyNumericFieldViewValuesToModel", new object[] { fieldModel });
			}
			Vector4 vector = new Vector4(base.NumericFieldViews[0].Value, base.NumericFieldViews[1].Value, base.NumericFieldViews[2].Value, base.NumericFieldViews[3].Value);
			this.OnValueChanged.Invoke(vector);
		}

		// Token: 0x0200022B RID: 555
		public enum Styles
		{
			// Token: 0x04000904 RID: 2308
			Vertical,
			// Token: 0x04000905 RID: 2309
			Horizontal
		}
	}
}
