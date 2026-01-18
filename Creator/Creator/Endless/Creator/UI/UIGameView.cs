using System;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x020000AE RID: 174
	public class UIGameView : UIAssetWithScreenshotsView<Game>
	{
		// Token: 0x060002BA RID: 698 RVA: 0x00012420 File Offset: 0x00010620
		public override void SetLocalUserCanInteract(bool localUserCanInteract)
		{
			base.SetLocalUserCanInteract(localUserCanInteract);
			UIButton[] array = this.addScreenshotButtons;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].interactable = localUserCanInteract;
			}
		}

		// Token: 0x040002F8 RID: 760
		[Header("UIGameView")]
		[SerializeField]
		private UIButton[] addScreenshotButtons = Array.Empty<UIButton>();
	}
}
