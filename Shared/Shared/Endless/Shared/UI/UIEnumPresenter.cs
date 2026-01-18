using System;
using Endless.Shared.Debugging;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x020001EA RID: 490
	public class UIEnumPresenter : UIBasePresenter<Enum>
	{
		// Token: 0x06000C1C RID: 3100 RVA: 0x0003457F File Offset: 0x0003277F
		protected override void Start()
		{
			base.Start();
			this.enumView.OnEnumChanged += base.SetModelAndTriggerOnModelChanged;
		}

		// Token: 0x06000C1D RID: 3101 RVA: 0x0003459E File Offset: 0x0003279E
		private void OnDestroy()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDestroy", Array.Empty<object>());
			}
			this.enumView.OnEnumChanged -= base.SetModelAndTriggerOnModelChanged;
		}

		// Token: 0x040007CF RID: 1999
		[Header("UIEnumPresenter")]
		[SerializeField]
		private UIBaseEnumView enumView;
	}
}
