using System;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x02000227 RID: 551
	public class UIVector3IntView : UIBaseIntNumericView<Vector3Int, UIVector3IntView.Styles>
	{
		// Token: 0x17000299 RID: 665
		// (get) Token: 0x06000E12 RID: 3602 RVA: 0x0003D165 File Offset: 0x0003B365
		// (set) Token: 0x06000E13 RID: 3603 RVA: 0x0003D16D File Offset: 0x0003B36D
		public override UIVector3IntView.Styles Style { get; protected set; }

		// Token: 0x1700029A RID: 666
		// (get) Token: 0x06000E14 RID: 3604 RVA: 0x0003D176 File Offset: 0x0003B376
		public UnityEvent<Vector3Int> OnValueChanged { get; } = new UnityEvent<Vector3Int>();

		// Token: 0x06000E15 RID: 3605 RVA: 0x0003D180 File Offset: 0x0003B380
		public override void View(Vector3Int model)
		{
			base.View(model);
			base.NumericFieldViews[0].SetValue((float)model.x, false);
			base.NumericFieldViews[1].SetValue((float)model.y, false);
			base.NumericFieldViews[2].SetValue((float)model.z, false);
		}

		// Token: 0x06000E16 RID: 3606 RVA: 0x0003D1E4 File Offset: 0x0003B3E4
		protected override void ApplyNumericFieldViewValuesToModel(float fieldModel)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ApplyNumericFieldViewValuesToModel", new object[] { fieldModel });
			}
			Vector3Int vector3Int = new Vector3Int((int)base.NumericFieldViews[0].Value, (int)base.NumericFieldViews[1].Value, (int)base.NumericFieldViews[2].Value);
			this.OnValueChanged.Invoke(vector3Int);
		}

		// Token: 0x02000228 RID: 552
		public enum Styles
		{
			// Token: 0x040008FF RID: 2303
			Vertical,
			// Token: 0x04000900 RID: 2304
			Horizontal
		}
	}
}
