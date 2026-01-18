using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Endless.Assets;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Data;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020002CE RID: 718
	public class UIGameEditorWindowController : UIWindowController, IUILoadingSpinnerViewCompatible
	{
		// Token: 0x1700018F RID: 399
		// (get) Token: 0x06000C21 RID: 3105 RVA: 0x0003A071 File Offset: 0x00038271
		public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

		// Token: 0x17000190 RID: 400
		// (get) Token: 0x06000C22 RID: 3106 RVA: 0x0003A079 File Offset: 0x00038279
		public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

		// Token: 0x06000C23 RID: 3107 RVA: 0x0003A081 File Offset: 0x00038281
		protected override void Start()
		{
			base.Start();
			this.BaseWindowView.CloseUnityEvent.AddListener(new UnityAction(this.OnClosing));
		}

		// Token: 0x06000C24 RID: 3108 RVA: 0x0003A0A5 File Offset: 0x000382A5
		private void OnEnable()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEnable", Array.Empty<object>());
			}
			UINewLevelStateModalController.CreateLevel = (Action<string, string, LevelStateTemplateSourceBase>)Delegate.Combine(UINewLevelStateModalController.CreateLevel, new Action<string, string, LevelStateTemplateSourceBase>(this.CreateLevel));
		}

		// Token: 0x06000C25 RID: 3109 RVA: 0x0003A0DF File Offset: 0x000382DF
		private void OnDisable()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDisable", Array.Empty<object>());
			}
			UINewLevelStateModalController.CreateLevel = (Action<string, string, LevelStateTemplateSourceBase>)Delegate.Remove(UINewLevelStateModalController.CreateLevel, new Action<string, string, LevelStateTemplateSourceBase>(this.CreateLevel));
		}

		// Token: 0x06000C26 RID: 3110 RVA: 0x0003A119 File Offset: 0x00038319
		public override void Close()
		{
			base.Close();
			MonoBehaviourSingleton<ToolManager>.Instance.SetActiveTool(ToolType.Empty);
		}

		// Token: 0x06000C27 RID: 3111 RVA: 0x0003A12C File Offset: 0x0003832C
		private async void CreateLevel(string levelName, string levelDescription, LevelStateTemplateSourceBase levelStateTemplate)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "CreateLevel", new object[] { levelName, levelDescription });
			}
			this.OnLoadingStarted.Invoke();
			Game activeGame = MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame;
			LevelState newLevel;
			try
			{
				LevelState levelState = await levelStateTemplate.CreateLevelState(activeGame, levelName, levelDescription, true);
				newLevel = levelState;
			}
			catch (Exception ex)
			{
				this.OnLoadingEnded.Invoke();
				ErrorHandler.HandleError(ErrorCodes.UIGameEditorWindowController_LevelCreationFail, new Exception("Failed creating level from template " + levelStateTemplate.name, ex), true, false);
				return;
			}
			this.OnLoadingEnded.Invoke();
			TaskAwaiter<bool> taskAwaiter = MonoBehaviourSingleton<GameEditor>.Instance.AddLevel(newLevel).GetAwaiter();
			if (!taskAwaiter.IsCompleted)
			{
				await taskAwaiter;
				TaskAwaiter<bool> taskAwaiter2;
				taskAwaiter = taskAwaiter2;
				taskAwaiter2 = default(TaskAwaiter<bool>);
			}
			if (taskAwaiter.GetResult())
			{
				UIGameEditorWindowView uigameEditorWindowView = (UIGameEditorWindowView)this.BaseWindowView;
				AssetCore assetCore = newLevel;
				List<LevelReference> levels = MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame.levels;
				assetCore.AssetID = levels[levels.Count - 1].AssetID;
				uigameEditorWindowView.OnLevelAdded(newLevel);
			}
		}

		// Token: 0x06000C28 RID: 3112 RVA: 0x0003A17B File Offset: 0x0003837B
		private void OnClosing()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnClosing", Array.Empty<object>());
			}
			if (MonoBehaviourSingleton<ToolManager>.Instance.ActiveTool.ToolType == ToolType.GameEditor)
			{
				MonoBehaviourSingleton<ToolManager>.Instance.SetActiveTool(ToolType.Empty);
			}
		}
	}
}
