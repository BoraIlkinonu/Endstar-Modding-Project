using System;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x02000218 RID: 536
	public class UIQuaternionPresenter : UIBaseFloatNumericPresenter<Quaternion>
	{
		// Token: 0x17000288 RID: 648
		// (get) Token: 0x06000DCD RID: 3533 RVA: 0x0003C391 File Offset: 0x0003A591
		public override int FieldCount
		{
			get
			{
				return 4;
			}
		}

		// Token: 0x06000DCE RID: 3534 RVA: 0x0003C394 File Offset: 0x0003A594
		protected override void Start()
		{
			base.Start();
			(base.View.Interface as UIQuaternionView).OnValueChanged.AddListener(new UnityAction<Quaternion>(base.SetModelAndTriggerOnModelChanged));
		}

		// Token: 0x06000DCF RID: 3535 RVA: 0x0003C3C4 File Offset: 0x0003A5C4
		protected override Quaternion Clamp(Quaternion value)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "Clamp", "value", value), this);
			}
			value = new Quaternion(Mathf.Clamp(value.x, base.Min, base.Max), Mathf.Clamp(value.y, base.Min, base.Max), Mathf.Clamp(value.z, base.Min, base.Max), Mathf.Clamp(value.w, base.Min, base.Max));
			return value;
		}
	}
}
