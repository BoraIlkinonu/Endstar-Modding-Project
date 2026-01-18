using System;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x02000221 RID: 545
	public class UIVector2IntView : UIBaseIntNumericView<Vector2Int, UIVector2IntView.Styles>
	{
		// Token: 0x17000293 RID: 659
		// (get) Token: 0x06000DFE RID: 3582 RVA: 0x0003CE1D File Offset: 0x0003B01D
		// (set) Token: 0x06000DFF RID: 3583 RVA: 0x0003CE25 File Offset: 0x0003B025
		public override UIVector2IntView.Styles Style { get; protected set; }

		// Token: 0x17000294 RID: 660
		// (get) Token: 0x06000E00 RID: 3584 RVA: 0x0003CE2E File Offset: 0x0003B02E
		public UnityEvent<Vector2Int> OnValueChanged { get; } = new UnityEvent<Vector2Int>();

		// Token: 0x06000E01 RID: 3585 RVA: 0x0003CE36 File Offset: 0x0003B036
		public override void View(Vector2Int model)
		{
			base.View(model);
			base.NumericFieldViews[0].SetValue((float)model.x, false);
			base.NumericFieldViews[1].SetValue((float)model.y, false);
		}

		// Token: 0x06000E02 RID: 3586 RVA: 0x0003CE74 File Offset: 0x0003B074
		protected override void ApplyNumericFieldViewValuesToModel(float fieldModel)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ApplyNumericFieldViewValuesToModel", new object[] { fieldModel });
			}
			Vector2Int vector2Int = new Vector2Int((int)base.NumericFieldViews[0].Value, (int)base.NumericFieldViews[1].Value);
			this.OnValueChanged.Invoke(vector2Int);
		}

		// Token: 0x02000222 RID: 546
		public enum Styles
		{
			// Token: 0x040008F5 RID: 2293
			Vertical,
			// Token: 0x040008F6 RID: 2294
			Horizontal
		}
	}
}
