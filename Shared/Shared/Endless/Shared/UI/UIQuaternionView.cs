using System;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x02000219 RID: 537
	public class UIQuaternionView : UIBaseFloatNumericView<Quaternion, UIQuaternionView.Styles>
	{
		// Token: 0x17000289 RID: 649
		// (get) Token: 0x06000DD1 RID: 3537 RVA: 0x0003C465 File Offset: 0x0003A665
		// (set) Token: 0x06000DD2 RID: 3538 RVA: 0x0003C46D File Offset: 0x0003A66D
		public override UIQuaternionView.Styles Style { get; protected set; }

		// Token: 0x1700028A RID: 650
		// (get) Token: 0x06000DD3 RID: 3539 RVA: 0x0003C476 File Offset: 0x0003A676
		public UnityEvent<Quaternion> OnValueChanged { get; } = new UnityEvent<Quaternion>();

		// Token: 0x06000DD4 RID: 3540 RVA: 0x0003C480 File Offset: 0x0003A680
		public override void View(Quaternion model)
		{
			base.View(model);
			base.NumericFieldViews[0].SetValue(model.x, false);
			base.NumericFieldViews[1].SetValue(model.y, false);
			base.NumericFieldViews[2].SetValue(model.z, false);
			base.NumericFieldViews[3].SetValue(model.w, false);
		}

		// Token: 0x06000DD5 RID: 3541 RVA: 0x0003C4F4 File Offset: 0x0003A6F4
		protected override void ApplyNumericFieldViewValuesToModel(float fieldModel)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ApplyNumericFieldViewValuesToModel", new object[] { fieldModel });
			}
			Quaternion quaternion = new Quaternion(base.NumericFieldViews[0].Value, base.NumericFieldViews[1].Value, base.NumericFieldViews[2].Value, base.NumericFieldViews[3].Value);
			this.OnValueChanged.Invoke(quaternion);
		}

		// Token: 0x0200021A RID: 538
		public enum Styles
		{
			// Token: 0x040008D7 RID: 2263
			Vertical,
			// Token: 0x040008D8 RID: 2264
			Horizontal
		}
	}
}
