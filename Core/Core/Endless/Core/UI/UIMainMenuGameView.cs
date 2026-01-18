using System;
using Endless.Creator.UI;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.UI;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Core.UI
{
	// Token: 0x02000062 RID: 98
	public class UIMainMenuGameView : UIGameObject
	{
		// Token: 0x17000030 RID: 48
		// (get) Token: 0x060001C8 RID: 456 RVA: 0x0000ABA4 File Offset: 0x00008DA4
		// (set) Token: 0x060001C9 RID: 457 RVA: 0x0000ABAC File Offset: 0x00008DAC
		public MainMenuGameModel MainMenuGameModel { get; private set; }

		// Token: 0x17000031 RID: 49
		// (get) Token: 0x060001CA RID: 458 RVA: 0x0000ABB5 File Offset: 0x00008DB5
		// (set) Token: 0x060001CB RID: 459 RVA: 0x0000ABBD File Offset: 0x00008DBD
		public MainMenuGameContext Context { get; private set; }

		// Token: 0x060001CC RID: 460 RVA: 0x0000ABC6 File Offset: 0x00008DC6
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.nameText.maxVisibleLines = 2;
			this.nameShadowText.maxVisibleLines = 2;
		}

		// Token: 0x060001CD RID: 461 RVA: 0x0000ABF8 File Offset: 0x00008DF8
		public void View(MainMenuGameModel mainMenuGameModel, MainMenuGameContext context)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "View", new object[]
				{
					(mainMenuGameModel == null) ? "null" : mainMenuGameModel.Name,
					context
				});
			}
			if (mainMenuGameModel != this.MainMenuGameModel)
			{
				this.Clear();
			}
			if (mainMenuGameModel == null)
			{
				this.ViewSkeletonData();
				return;
			}
			this.MainMenuGameModel = mainMenuGameModel;
			bool flag = this.MainMenuGameModel.Screenshots.Count > 0;
			this.screenshot.gameObject.SetActive(flag);
			if (flag)
			{
				this.screenshot.Display(this.MainMenuGameModel.Screenshots[0]);
			}
			this.defaultScreenshot.enabled = !flag;
			this.nameText.text = mainMenuGameModel.Name;
			this.nameShadowText.text = mainMenuGameModel.Name;
			this.Context = context;
		}

		// Token: 0x060001CE RID: 462 RVA: 0x0000ACD6 File Offset: 0x00008ED6
		public void Clear()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Clear", Array.Empty<object>());
			}
			this.screenshot.Clear();
		}

		// Token: 0x060001CF RID: 463 RVA: 0x0000ACFC File Offset: 0x00008EFC
		private void ViewSkeletonData()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ViewSkeletonData", Array.Empty<object>());
			}
			this.Clear();
			this.screenshot.gameObject.SetActive(false);
			this.defaultScreenshot.enabled = true;
			this.nameText.text = "Loading...";
			this.nameShadowText.text = "Loading...";
		}

		// Token: 0x04000145 RID: 325
		[SerializeField]
		private UIScreenshotView screenshot;

		// Token: 0x04000146 RID: 326
		[SerializeField]
		private Image defaultScreenshot;

		// Token: 0x04000147 RID: 327
		[SerializeField]
		private TextMeshProUGUI nameText;

		// Token: 0x04000148 RID: 328
		[SerializeField]
		private TextMeshProUGUI nameShadowText;

		// Token: 0x04000149 RID: 329
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
