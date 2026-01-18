using System;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x02000220 RID: 544
	public class UIVector2IntPresenter : UIBaseIntNumericPresenter<Vector2Int>
	{
		// Token: 0x17000292 RID: 658
		// (get) Token: 0x06000DFA RID: 3578 RVA: 0x0003CC05 File Offset: 0x0003AE05
		public override int FieldCount
		{
			get
			{
				return 2;
			}
		}

		// Token: 0x06000DFB RID: 3579 RVA: 0x0003CD77 File Offset: 0x0003AF77
		protected override void Start()
		{
			base.Start();
			(base.View.Interface as UIVector2IntView).OnValueChanged.AddListener(new UnityAction<Vector2Int>(base.SetModelAndTriggerOnModelChanged));
		}

		// Token: 0x06000DFC RID: 3580 RVA: 0x0003CDA8 File Offset: 0x0003AFA8
		protected override Vector2Int Clamp(Vector2Int value)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "Clamp", "value", value), this);
			}
			value = new Vector2Int(Mathf.Clamp(value.x, base.Min, base.Max), Mathf.Clamp(value.y, base.Min, base.Max));
			return value;
		}
	}
}
