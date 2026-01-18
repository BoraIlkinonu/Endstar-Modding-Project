using System;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x02000224 RID: 548
	public class UIVector3View : UIBaseFloatNumericView<Vector3, UIVector3View.Styles>
	{
		// Token: 0x17000296 RID: 662
		// (get) Token: 0x06000E08 RID: 3592 RVA: 0x0003CFAA File Offset: 0x0003B1AA
		// (set) Token: 0x06000E09 RID: 3593 RVA: 0x0003CFB2 File Offset: 0x0003B1B2
		public override UIVector3View.Styles Style { get; protected set; }

		// Token: 0x17000297 RID: 663
		// (get) Token: 0x06000E0A RID: 3594 RVA: 0x0003CFBB File Offset: 0x0003B1BB
		public UnityEvent<Vector3> OnValueChanged { get; } = new UnityEvent<Vector3>();

		// Token: 0x06000E0B RID: 3595 RVA: 0x0003CFC4 File Offset: 0x0003B1C4
		public override void View(Vector3 model)
		{
			base.View(model);
			base.NumericFieldViews[0].SetValue(model.x, false);
			base.NumericFieldViews[1].SetValue(model.y, false);
			base.NumericFieldViews[2].SetValue(model.z, false);
		}

		// Token: 0x06000E0C RID: 3596 RVA: 0x0003D020 File Offset: 0x0003B220
		protected override void ApplyNumericFieldViewValuesToModel(float fieldModel)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ApplyNumericFieldViewValuesToModel", new object[] { fieldModel });
			}
			Vector3 vector = new Vector3(base.NumericFieldViews[0].Value, base.NumericFieldViews[1].Value, base.NumericFieldViews[2].Value);
			this.OnValueChanged.Invoke(vector);
		}

		// Token: 0x02000225 RID: 549
		public enum Styles
		{
			// Token: 0x040008FA RID: 2298
			Vertical,
			// Token: 0x040008FB RID: 2299
			Horizontal
		}
	}
}
