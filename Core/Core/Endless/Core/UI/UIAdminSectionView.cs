using System;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Core.UI
{
	// Token: 0x02000088 RID: 136
	public class UIAdminSectionView : UIGameObject
	{
		// Token: 0x060002B1 RID: 689 RVA: 0x0000ED76 File Offset: 0x0000CF76
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.displayAndHideHandler.OnDisplayStart.AddListener(new UnityAction(this.View));
		}

		// Token: 0x060002B2 RID: 690 RVA: 0x0000EDAC File Offset: 0x0000CFAC
		private void View()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "View", Array.Empty<object>());
			}
			if (!this.userReportedMainMenuGameModelListModel.IsInitialized)
			{
				this.userReportedMainMenuGameModelListModel.Request(null);
			}
		}

		// Token: 0x04000204 RID: 516
		[SerializeField]
		private UIDisplayAndHideHandler displayAndHideHandler;

		// Token: 0x04000205 RID: 517
		[SerializeField]
		private UIMainMenuGameModelPaginatedListModel userReportedMainMenuGameModelListModel;

		// Token: 0x04000206 RID: 518
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
