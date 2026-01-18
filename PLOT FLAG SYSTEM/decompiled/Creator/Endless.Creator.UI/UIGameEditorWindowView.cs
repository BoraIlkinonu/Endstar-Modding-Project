using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Data;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.UI;
using Endless.GraphQl;
using Endless.Matchmaking;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Newtonsoft.Json;
using Runtime.Shared.Matchmaking;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI;

public class UIGameEditorWindowView : UIBaseWindowView, IUILoadingSpinnerViewCompatible
{
	[Header("UIGameEditorWindowView")]
	[SerializeField]
	private UIGameModelHandler gameModelHandler;

	[SerializeField]
	private UILevelAssetListModel levelStateListModel;

	[SerializeField]
	private UIUserRolesModel userRolesModel;

	[SerializeField]
	private InterfaceReference<IUILayoutable>[] layoutables = Array.Empty<InterfaceReference<IUILayoutable>>();

	public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

	public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

	public Game Game { get; private set; }

	public Roles LocalClientRole { get; private set; } = Roles.None;

	protected override void Start()
	{
		base.Start();
		layoutables.RequestLayout();
	}

	public static UIGameEditorWindowView Display(Transform parent = null)
	{
		return (UIGameEditorWindowView)MonoBehaviourSingleton<UIWindowManager>.Instance.Display<UIGameEditorWindowView>(parent);
	}

	public override void OnSpawn()
	{
		base.OnSpawn();
		MonoBehaviourSingleton<UIStartMatchHelper>.Instance.EndMatchAndStartNewMatchWithCachedDataUnityEvent.AddListener(DisplayLoadingSpinner);
		MatchmakingClientController.MatchLeave += OnMatchLeave;
	}

	public override void OnDespawn()
	{
		base.OnDespawn();
		MonoBehaviourSingleton<UIStartMatchHelper>.Instance.EndMatchAndStartNewMatchWithCachedDataUnityEvent.RemoveListener(DisplayLoadingSpinner);
		MatchmakingClientController.MatchLeave -= OnMatchLeave;
		HideLoadingSpinner();
		LocalClientRole = Roles.None;
		Game = null;
		levelStateListModel.Clear(triggerEvents: true);
		userRolesModel.Clear();
		UIScreenObserver.OnSizeChange = (Action)Delegate.Remove(UIScreenObserver.OnSizeChange, new Action(layoutables.RequestLayout));
	}

	public override void Initialize(Dictionary<string, object> supplementalData)
	{
		base.Initialize(supplementalData);
		Game = MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame;
		gameModelHandler.Set(Game);
		levelStateListModel.Clear(triggerEvents: true);
		userRolesModel.Initialize(Game.AssetID, Game.Name, SerializableGuid.Empty, AssetContexts.GameOrLevelEditor);
		LoadLevelData();
		UIScreenObserver.OnSizeChange = (Action)Delegate.Combine(UIScreenObserver.OnSizeChange, new Action(layoutables.RequestLayout));
	}

	private async void LoadLevelData()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "LoadLevelData");
		}
		DisplayLoadingSpinner();
		GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.GetAssetAsync(Game.AssetID, Game.AssetVersion, new AssetParams(LevelAsset.QueryString, populateRefs: true));
		OnLoadingEnded.Invoke();
		if (graphQlResult.HasErrors)
		{
			ErrorHandler.HandleError(ErrorCodes.UIGameEditorWindowView_GetLevels, graphQlResult.GetErrorMessage());
			return;
		}
		var anonymousTypeObject = new
		{
			Levels = Array.Empty<LevelAsset>()
		};
		var anon = JsonConvert.DeserializeAnonymousType(graphQlResult.GetDataMember().ToString(), anonymousTypeObject);
		levelStateListModel.SetGame(Game);
		List<LevelAsset> list = anon.Levels.ToList();
		levelStateListModel.Set(list, triggerEvents: true);
	}

	public void OnLevelAdded(LevelState levelState)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnLevelAdded", levelState);
		}
		LevelAsset item = new LevelAsset(levelState);
		levelStateListModel.Add(item, triggerEvents: true);
	}

	private void DisplayLoadingSpinner()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "DisplayLoadingSpinner");
		}
		OnLoadingStarted.Invoke();
	}

	private void OnMatchLeave(string reason)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnMatchLeave", reason);
		}
		HideLoadingSpinner();
	}

	private void HideLoadingSpinner()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "HideLoadingSpinner");
		}
		OnLoadingEnded.Invoke();
	}
}
