using System;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x020001AC RID: 428
	public class UILogsListCellController : UIBaseListCellController<UILog>
	{
		// Token: 0x06000B08 RID: 2824 RVA: 0x0003061F File Offset: 0x0002E81F
		protected override void Start()
		{
			base.Start();
			this.inspectButton.onClick.AddListener(new UnityAction(this.Inspect));
		}

		// Token: 0x06000B09 RID: 2825 RVA: 0x00030643 File Offset: 0x0002E843
		protected override void OnAddButton()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnAddButton", Array.Empty<object>());
			}
			throw new NotImplementedException();
		}

		// Token: 0x06000B0A RID: 2826 RVA: 0x00030662 File Offset: 0x0002E862
		private void Inspect()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Inspect", Array.Empty<object>());
			}
			Action<UILog> selectAction = UILogsListCellController.SelectAction;
			if (selectAction == null)
			{
				return;
			}
			selectAction(base.Model);
		}

		// Token: 0x04000718 RID: 1816
		public static Action<UILog> SelectAction;

		// Token: 0x04000719 RID: 1817
		[Header("UILogsListCellController")]
		[SerializeField]
		private UIButton inspectButton;
	}
}
