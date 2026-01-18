using System;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x02000223 RID: 547
	public class UIVector3Presenter : UIBaseFloatNumericPresenter<Vector3>
	{
		// Token: 0x17000295 RID: 661
		// (get) Token: 0x06000E04 RID: 3588 RVA: 0x0003CEED File Offset: 0x0003B0ED
		public override int FieldCount
		{
			get
			{
				return 3;
			}
		}

		// Token: 0x06000E05 RID: 3589 RVA: 0x0003CEF0 File Offset: 0x0003B0F0
		protected override void Start()
		{
			base.Start();
			(base.View.Interface as UIVector3View).OnValueChanged.AddListener(new UnityAction<Vector3>(base.SetModelAndTriggerOnModelChanged));
		}

		// Token: 0x06000E06 RID: 3590 RVA: 0x0003CF20 File Offset: 0x0003B120
		protected override Vector3 Clamp(Vector3 value)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "Clamp", "value", value), this);
			}
			value = new Vector3(Mathf.Clamp(value.x, base.Min, base.Max), Mathf.Clamp(value.y, base.Min, base.Max), Mathf.Clamp(value.z, base.Min, base.Max));
			return value;
		}
	}
}
