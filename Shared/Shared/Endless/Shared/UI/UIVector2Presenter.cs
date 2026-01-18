using System;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x0200021D RID: 541
	public class UIVector2Presenter : UIBaseFloatNumericPresenter<Vector2>
	{
		// Token: 0x1700028F RID: 655
		// (get) Token: 0x06000DF0 RID: 3568 RVA: 0x0003CC05 File Offset: 0x0003AE05
		public override int FieldCount
		{
			get
			{
				return 2;
			}
		}

		// Token: 0x06000DF1 RID: 3569 RVA: 0x0003CC08 File Offset: 0x0003AE08
		protected override void Start()
		{
			base.Start();
			(base.View.Interface as UIVector2View).OnValueChanged.AddListener(new UnityAction<Vector2>(base.SetModelAndTriggerOnModelChanged));
		}

		// Token: 0x06000DF2 RID: 3570 RVA: 0x0003CC38 File Offset: 0x0003AE38
		protected override Vector2 Clamp(Vector2 value)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "Clamp", "value", value), this);
			}
			value = new Vector2(Mathf.Clamp(value.x, base.Min, base.Max), Mathf.Clamp(value.y, base.Min, base.Max));
			return value;
		}
	}
}
