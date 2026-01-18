using System;
using Endless.Shared.Debugging;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x02000212 RID: 530
	public class UIFloatView : UIBaseFloatNumericView<float, UIFloatView.Styles>
	{
		// Token: 0x17000283 RID: 643
		// (get) Token: 0x06000DBB RID: 3515 RVA: 0x0003C1F4 File Offset: 0x0003A3F4
		// (set) Token: 0x06000DBC RID: 3516 RVA: 0x0003C1FC File Offset: 0x0003A3FC
		public override UIFloatView.Styles Style { get; protected set; }

		// Token: 0x17000284 RID: 644
		// (get) Token: 0x06000DBD RID: 3517 RVA: 0x0003C205 File Offset: 0x0003A405
		public UnityEvent<float> OnValueChanged { get; } = new UnityEvent<float>();

		// Token: 0x06000DBE RID: 3518 RVA: 0x0003C20D File Offset: 0x0003A40D
		public override void View(float model)
		{
			base.View(model);
			base.NumericFieldViews[0].SetValue(model, false);
		}

		// Token: 0x06000DBF RID: 3519 RVA: 0x0003C229 File Offset: 0x0003A429
		public void OverrideInputField(string input)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OverrideInputField", new object[] { input });
			}
			base.NumericFieldViews[0].OverrideInputField(input);
		}

		// Token: 0x06000DC0 RID: 3520 RVA: 0x0003C25A File Offset: 0x0003A45A
		protected override void ApplyNumericFieldViewValuesToModel(float fieldModel)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ApplyNumericFieldViewValuesToModel", new object[] { fieldModel });
			}
			this.OnValueChanged.Invoke(fieldModel);
		}

		// Token: 0x02000213 RID: 531
		public enum Styles
		{
			// Token: 0x040008CE RID: 2254
			Default
		}
	}
}
