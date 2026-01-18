using System;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x02000171 RID: 369
	public class UIScreenshotFileInstancesGameAdditionListCellController : UIBaseListCellController<ScreenshotFileInstances>
	{
		// Token: 0x0600057B RID: 1403 RVA: 0x0001D45E File Offset: 0x0001B65E
		protected override void Start()
		{
			base.Start();
			this.selectButton.onClick.AddListener(new UnityAction(this.ToggleSelected));
		}

		// Token: 0x0600057C RID: 1404 RVA: 0x0001CF77 File Offset: 0x0001B177
		protected override void OnAddButton()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnAddButton", Array.Empty<object>());
			}
			throw new NotImplementedException();
		}

		// Token: 0x0600057D RID: 1405 RVA: 0x0001D483 File Offset: 0x0001B683
		protected override void ToggleSelected()
		{
			base.ToggleSelected();
			Action<ScreenshotFileInstances> selectAction = UIScreenshotFileInstancesGameAdditionListCellController.SelectAction;
			if (selectAction == null)
			{
				return;
			}
			selectAction(base.Model);
		}

		// Token: 0x040004E0 RID: 1248
		public static Action<ScreenshotFileInstances> SelectAction;

		// Token: 0x040004E1 RID: 1249
		[Header("UIScreenshotFileInstancesGameAdditionListCellController")]
		[SerializeField]
		private UIButton selectButton;
	}
}
