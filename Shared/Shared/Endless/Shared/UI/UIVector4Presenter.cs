using System;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x02000229 RID: 553
	public class UIVector4Presenter : UIBaseFloatNumericPresenter<Vector4>
	{
		// Token: 0x1700029B RID: 667
		// (get) Token: 0x06000E18 RID: 3608 RVA: 0x0003C391 File Offset: 0x0003A591
		public override int FieldCount
		{
			get
			{
				return 4;
			}
		}

		// Token: 0x06000E19 RID: 3609 RVA: 0x0003D26F File Offset: 0x0003B46F
		protected override void Start()
		{
			base.Start();
			(base.View.Interface as UIVector4View).OnValueChanged.AddListener(new UnityAction<Vector4>(base.SetModelAndTriggerOnModelChanged));
		}

		// Token: 0x06000E1A RID: 3610 RVA: 0x0003D2A0 File Offset: 0x0003B4A0
		protected override Vector4 Clamp(Vector4 value)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "Clamp", "value", value), this);
			}
			value = new Vector4(Mathf.Clamp(value.x, base.Min, base.Max), Mathf.Clamp(value.y, base.Min, base.Max), Mathf.Clamp(value.z, base.Min, base.Max), Mathf.Clamp(value.w, base.Min, base.Max));
			return value;
		}
	}
}
