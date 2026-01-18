using System;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x02000216 RID: 534
	public class UIIntPresenter : UIBaseIntNumericPresenter<int>
	{
		// Token: 0x17000287 RID: 647
		// (get) Token: 0x06000DC6 RID: 3526 RVA: 0x000050D2 File Offset: 0x000032D2
		public override int FieldCount
		{
			get
			{
				return 1;
			}
		}

		// Token: 0x06000DC7 RID: 3527 RVA: 0x0003C2C9 File Offset: 0x0003A4C9
		protected override void Start()
		{
			base.Start();
			(base.View.Interface as UIBaseIntView).OnValueChanged.AddListener(new UnityAction<int>(base.SetModelAndTriggerOnModelChanged));
		}

		// Token: 0x06000DC8 RID: 3528 RVA: 0x0003C2F7 File Offset: 0x0003A4F7
		protected override int Clamp(int value)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "Clamp", "value", value), this);
			}
			return Mathf.Clamp(value, base.Min, base.Max);
		}
	}
}
