using System;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x0200016A RID: 362
	public class UIScreenshotFileInstancesListCellController : UIBaseListCellController<ScreenshotFileInstances>
	{
		// Token: 0x06000561 RID: 1377 RVA: 0x0001CF52 File Offset: 0x0001B152
		protected override void Start()
		{
			base.Start();
			this.selectButton.onClick.AddListener(new UnityAction(this.Select));
		}

		// Token: 0x06000562 RID: 1378 RVA: 0x0001CF77 File Offset: 0x0001B177
		protected override void OnAddButton()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnAddButton", Array.Empty<object>());
			}
			throw new NotImplementedException();
		}

		// Token: 0x06000563 RID: 1379 RVA: 0x0001CF96 File Offset: 0x0001B196
		protected override void Select()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Select", Array.Empty<object>());
			}
			base.ListModel.ToggleSelected(base.DataIndex, true);
		}

		// Token: 0x040004D6 RID: 1238
		[Header("UIScreenshotFileInstancesListCellController")]
		[SerializeField]
		private UIButton selectButton;
	}
}
