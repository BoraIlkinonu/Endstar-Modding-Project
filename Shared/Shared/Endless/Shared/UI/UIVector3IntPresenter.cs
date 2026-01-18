using System;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x02000226 RID: 550
	public class UIVector3IntPresenter : UIBaseIntNumericPresenter<Vector3Int>
	{
		// Token: 0x17000298 RID: 664
		// (get) Token: 0x06000E0E RID: 3598 RVA: 0x0003CEED File Offset: 0x0003B0ED
		public override int FieldCount
		{
			get
			{
				return 3;
			}
		}

		// Token: 0x06000E0F RID: 3599 RVA: 0x0003D0A8 File Offset: 0x0003B2A8
		protected override void Start()
		{
			base.Start();
			(base.View.Interface as UIVector3IntView).OnValueChanged.AddListener(new UnityAction<Vector3Int>(base.SetModelAndTriggerOnModelChanged));
		}

		// Token: 0x06000E10 RID: 3600 RVA: 0x0003D0D8 File Offset: 0x0003B2D8
		protected override Vector3Int Clamp(Vector3Int value)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "Clamp", "value", value), this);
			}
			value = new Vector3Int(Mathf.Clamp(value.x, base.Min, base.Max), Mathf.Clamp(value.y, base.Min, base.Max), Mathf.Clamp(value.z, base.Min, base.Max));
			return value;
		}
	}
}
