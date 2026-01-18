using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI;

public class UILevelEditorWindowView : UIBaseWindowView, IUILoadingSpinnerViewCompatible
{
	public UnityEvent OnDisplayLevel = new UnityEvent();

	[Header("UILevelEditorWindowView")]
	[SerializeField]
	private UILevelModelHandler levelModelHandler;

	[SerializeField]
	private UISpawnPointListModel spawnPointListModel;

	[Header("User Roles")]
	[SerializeField]
	private UIUserRolesModel levelUserRolesModel;

	[SerializeField]
	private UIUserRolesModel gameUserRolesModel;

	[SerializeField]
	private UIRevisionsView revisions;

	[SerializeField]
	private InterfaceReference<IUILayoutable>[] layoutables = Array.Empty<InterfaceReference<IUILayoutable>>();

	private ScreenshotTool screenshotTool;

	public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

	public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

	public Roles LocalClientRole { get; private set; } = Roles.None;

	protected override void Start()
	{
		base.Start();
		screenshotTool = MonoBehaviourSingleton<ToolManager>.Instance.RequestToolInstance<ScreenshotTool>();
		screenshotTool.OnScreenshotRemoved.AddListener(OnScreenshotRemoved);
		layoutables.RequestLayout();
	}

	public static UILevelEditorWindowView Display(Transform parent = null)
	{
		return (UILevelEditorWindowView)MonoBehaviourSingleton<UIWindowManager>.Instance.Display<UILevelEditorWindowView>(parent);
	}

	public override void OnDespawn()
	{
		base.OnDespawn();
		LocalClientRole = Roles.None;
		spawnPointListModel.Clear(triggerEvents: true);
		MonoBehaviourSingleton<UIUserRoleWizard>.Instance.OnComplete.RemoveListener(InitializeUserRoles);
		UIScreenObserver.OnSizeChange = (Action)Delegate.Remove(UIScreenObserver.OnSizeChange, new Action(layoutables.RequestLayout));
	}

	public override void Initialize(Dictionary<string, object> supplementalData)
	{
		base.Initialize(supplementalData);
		LevelState levelState = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState;
		MonoBehaviourSingleton<UIUserRoleWizard>.Instance.OnComplete.AddListener(InitializeUserRoles);
		InitializeUserRoles();
		levelModelHandler.Set(levelState);
		ViewSpawnPoints();
		revisions.Initialize(levelState);
		UIScreenObserver.OnSizeChange = (Action)Delegate.Combine(UIScreenObserver.OnSizeChange, new Action(layoutables.RequestLayout));
	}

	private void ViewSpawnPoints()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "ViewSpawnPoints");
		}
		LevelState levelState = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState;
		foreach (SerializableGuid spawnPointId in levelState.SpawnPointIds)
		{
			PropEntry propEntry = levelState.PropEntries.FirstOrDefault((PropEntry propEntry2) => propEntry2.InstanceId == spawnPointId);
			if (propEntry == null)
			{
				DebugUtility.LogError(string.Format("Could not find the {0} that was a Spawn Point with an {1} of {2}!", "PropEntry", "InstanceId", spawnPointId), this);
				continue;
			}
			bool num = levelState.SelectedSpawnPointIds.Contains(spawnPointId);
			UISpawnPoint item = new UISpawnPoint(spawnPointId, propEntry.Label);
			spawnPointListModel.Add(item, triggerEvents: false);
			if (num)
			{
				spawnPointListModel.Select(spawnPointListModel.Count - 1, triggerEvents: false);
			}
		}
		spawnPointListModel.TriggerModelChanged();
	}

	private void InitializeUserRoles()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "InitializeUserRoles");
		}
		AssetContexts context = AssetContexts.GameOrLevelEditor;
		_ = (SerializableGuid)MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame.AssetID;
		SerializableGuid activeLevelGuid = MonoBehaviourSingleton<StageManager>.Instance.ActiveLevelGuid;
		string assetName = MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame.Name;
		gameUserRolesModel.Initialize(activeLevelGuid, assetName, SerializableGuid.Empty, context);
		string assetName2 = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.Name;
		levelUserRolesModel.Initialize(activeLevelGuid, assetName2, SerializableGuid.Empty, context);
	}

	private void OnScreenshotRemoved()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnScreenshotRemoved");
		}
		LevelState levelState = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState;
		levelModelHandler.Set(levelState);
	}
}
