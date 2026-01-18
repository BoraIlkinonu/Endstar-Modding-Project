using System;
using Endless.Gameplay.Screenshotting;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x02000118 RID: 280
	public class UIInMemoryScreenshotListCellController : UIBaseListCellController<ScreenshotAPI.InMemoryScreenShot>
	{
		// Token: 0x06000472 RID: 1138 RVA: 0x0001A50E File Offset: 0x0001870E
		protected override void Start()
		{
			base.Start();
			this.selectButton.onClick.AddListener(new UnityAction(this.ToggleSelected));
		}

		// Token: 0x06000473 RID: 1139 RVA: 0x0001A533 File Offset: 0x00018733
		protected override void OnAddButton()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnAddButton", Array.Empty<object>());
			}
			throw new NotImplementedException();
		}

		// Token: 0x04000446 RID: 1094
		[Header("UIInMemoryScreenshotListCellController")]
		[SerializeField]
		private UIButton selectButton;
	}
}
