using System;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x02000198 RID: 408
	public class UIVersionListCellController : UIBaseListCellController<string>
	{
		// Token: 0x060005F3 RID: 1523 RVA: 0x0001E7D4 File Offset: 0x0001C9D4
		protected override void Start()
		{
			base.Start();
			this.pointerUpHandler.PointerUpUnityEvent.AddListener(new UnityAction(this.Select));
			this.revertButton.onClick.AddListener(new UnityAction(this.ConfirmRevert));
		}

		// Token: 0x060005F4 RID: 1524 RVA: 0x0001E820 File Offset: 0x0001CA20
		protected override void OnAddButton()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnAddButton", Array.Empty<object>());
			}
			throw new NotImplementedException();
		}

		// Token: 0x060005F5 RID: 1525 RVA: 0x0001E83F File Offset: 0x0001CA3F
		protected override void Select()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Select", Array.Empty<object>());
			}
			base.ListModel.Select(base.DataIndex, true);
		}

		// Token: 0x060005F6 RID: 1526 RVA: 0x0001E86C File Offset: 0x0001CA6C
		private void ConfirmRevert()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ConfirmRevert", Array.Empty<object>());
			}
			MonoBehaviourSingleton<UIModalManager>.Instance.Confirm("Are you sure you want to revert to v" + base.Model + "?", new Action(this.Revert), new Action(MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack), UIModalManagerStackActions.MaintainStack);
		}

		// Token: 0x060005F7 RID: 1527 RVA: 0x0001E8D0 File Offset: 0x0001CAD0
		private void Revert()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethodWithAppension(this, "Revert", "Model: " + base.Model, Array.Empty<object>());
			}
			NetworkBehaviourSingleton<CreatorManager>.Instance.LevelEditor.RevertLevelToVersion_ServerRpc(base.Model, default(ServerRpcParams));
			MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
		}

		// Token: 0x04000530 RID: 1328
		[Header("UIVersionListCellView")]
		[SerializeField]
		private PointerUpHandler pointerUpHandler;

		// Token: 0x04000531 RID: 1329
		[SerializeField]
		private UIButton revertButton;
	}
}
