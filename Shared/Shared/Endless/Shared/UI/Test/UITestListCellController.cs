using System;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared.UI.Test
{
	// Token: 0x02000298 RID: 664
	public class UITestListCellController : UIBaseListCellController<int>
	{
		// Token: 0x0600108E RID: 4238 RVA: 0x00046B2C File Offset: 0x00044D2C
		protected override void Start()
		{
			base.Start();
			this.toggleSelectedButton.onClick.AddListener(new UnityAction(this.ToggleSelected));
			this.removeButton.onClick.AddListener(new UnityAction(this.Remove));
		}

		// Token: 0x0600108F RID: 4239 RVA: 0x00046B79 File Offset: 0x00044D79
		protected override void OnAddButton()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnAddButton", Array.Empty<object>());
			}
			base.ListModel.Add(base.ListModel.Count, true);
		}

		// Token: 0x04000A75 RID: 2677
		[Header("UITestListCellController")]
		[SerializeField]
		private UIButton toggleSelectedButton;

		// Token: 0x04000A76 RID: 2678
		[SerializeField]
		private UIButton removeButton;
	}
}
