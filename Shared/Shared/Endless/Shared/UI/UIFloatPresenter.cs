using System;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x02000211 RID: 529
	public class UIFloatPresenter : UIBaseFloatNumericPresenter<float>
	{
		// Token: 0x17000282 RID: 642
		// (get) Token: 0x06000DB7 RID: 3511 RVA: 0x000050D2 File Offset: 0x000032D2
		public override int FieldCount
		{
			get
			{
				return 1;
			}
		}

		// Token: 0x06000DB8 RID: 3512 RVA: 0x0003C182 File Offset: 0x0003A382
		protected override void Start()
		{
			base.Start();
			(base.View.Interface as UIFloatView).OnValueChanged.AddListener(new UnityAction<float>(base.SetModelAndTriggerOnModelChanged));
		}

		// Token: 0x06000DB9 RID: 3513 RVA: 0x0003C1B0 File Offset: 0x0003A3B0
		protected override float Clamp(float value)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "Clamp", "value", value), this);
			}
			return Mathf.Clamp(value, base.Min, base.Max);
		}
	}
}
