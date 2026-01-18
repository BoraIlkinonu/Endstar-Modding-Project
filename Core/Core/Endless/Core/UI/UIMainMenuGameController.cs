using System;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Core.UI
{
	// Token: 0x02000061 RID: 97
	public class UIMainMenuGameController : UIGameObject
	{
		// Token: 0x1700002F RID: 47
		// (get) Token: 0x060001C4 RID: 452 RVA: 0x0000AB1B File Offset: 0x00008D1B
		private MainMenuGameModel Game
		{
			get
			{
				return this.view.MainMenuGameModel;
			}
		}

		// Token: 0x060001C5 RID: 453 RVA: 0x0000AB28 File Offset: 0x00008D28
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.inspectButton.onClick.AddListener(new UnityAction(this.Inspect));
		}

		// Token: 0x060001C6 RID: 454 RVA: 0x0000AB60 File Offset: 0x00008D60
		private void Inspect()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Inspect", Array.Empty<object>());
			}
			UIGameInspectorScreenModel uigameInspectorScreenModel = new UIGameInspectorScreenModel(this.Game, this.view.Context);
			UIGameInspectorScreenView.Display(UIScreenManager.DisplayStackActions.Push, uigameInspectorScreenModel);
		}

		// Token: 0x04000142 RID: 322
		[SerializeField]
		private UIMainMenuGameView view;

		// Token: 0x04000143 RID: 323
		[SerializeField]
		private UIButton inspectButton;

		// Token: 0x04000144 RID: 324
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
