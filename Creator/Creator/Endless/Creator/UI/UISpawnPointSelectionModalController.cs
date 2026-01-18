using System;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020001D9 RID: 473
	public class UISpawnPointSelectionModalController : UIGameObject
	{
		// Token: 0x0600071C RID: 1820 RVA: 0x00023E79 File Offset: 0x00022079
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.confirmButton.onClick.AddListener(new UnityAction(this.Confirm));
		}

		// Token: 0x0600071D RID: 1821 RVA: 0x00023EAF File Offset: 0x000220AF
		private void Confirm()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Confirm", Array.Empty<object>());
			}
			this.view.LevelDestinationPresenter.SetSpawnPoints(this.spawnPointListModel.SelectedTypedList);
			MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
		}

		// Token: 0x0400066F RID: 1647
		[SerializeField]
		private UISpawnPointSelectionModalView view;

		// Token: 0x04000670 RID: 1648
		[SerializeField]
		private UIButton confirmButton;

		// Token: 0x04000671 RID: 1649
		[SerializeField]
		private UISpawnPointListModel spawnPointListModel;

		// Token: 0x04000672 RID: 1650
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
