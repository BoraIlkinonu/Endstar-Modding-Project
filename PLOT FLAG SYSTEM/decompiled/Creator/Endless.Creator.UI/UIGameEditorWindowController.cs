using System;
using System.Collections.Generic;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Data;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine.Events;

namespace Endless.Creator.UI;

public class UIGameEditorWindowController : UIWindowController, IUILoadingSpinnerViewCompatible
{
	public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

	public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

	protected override void Start()
	{
		base.Start();
		BaseWindowView.CloseUnityEvent.AddListener(OnClosing);
	}

	private void OnEnable()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnEnable");
		}
		UINewLevelStateModalController.CreateLevel = (Action<string, string, LevelStateTemplateSourceBase>)Delegate.Combine(UINewLevelStateModalController.CreateLevel, new Action<string, string, LevelStateTemplateSourceBase>(CreateLevel));
	}

	private void OnDisable()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnDisable");
		}
		UINewLevelStateModalController.CreateLevel = (Action<string, string, LevelStateTemplateSourceBase>)Delegate.Remove(UINewLevelStateModalController.CreateLevel, new Action<string, string, LevelStateTemplateSourceBase>(CreateLevel));
	}

	public override void Close()
	{
		base.Close();
		MonoBehaviourSingleton<ToolManager>.Instance.SetActiveTool(ToolType.Empty);
	}

	private async void CreateLevel(string levelName, string levelDescription, LevelStateTemplateSourceBase levelStateTemplate)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "CreateLevel", levelName, levelDescription);
		}
		OnLoadingStarted.Invoke();
		Game activeGame = MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame;
		LevelState newLevel;
		try
		{
			newLevel = await levelStateTemplate.CreateLevelState(activeGame, levelName, levelDescription, useGameEditor: true);
		}
		catch (Exception innerException)
		{
			OnLoadingEnded.Invoke();
			ErrorHandler.HandleError(ErrorCodes.UIGameEditorWindowController_LevelCreationFail, new Exception("Failed creating level from template " + levelStateTemplate.name, innerException));
			return;
		}
		OnLoadingEnded.Invoke();
		if (await MonoBehaviourSingleton<GameEditor>.Instance.AddLevel(newLevel))
		{
			UIGameEditorWindowView obj = (UIGameEditorWindowView)BaseWindowView;
			List<LevelReference> levels = MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame.levels;
			newLevel.AssetID = levels[levels.Count - 1].AssetID;
			obj.OnLevelAdded(newLevel);
		}
	}

	private void OnClosing()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnClosing");
		}
		if (MonoBehaviourSingleton<ToolManager>.Instance.ActiveTool.ToolType == ToolType.GameEditor)
		{
			MonoBehaviourSingleton<ToolManager>.Instance.SetActiveTool(ToolType.Empty);
		}
	}
}
