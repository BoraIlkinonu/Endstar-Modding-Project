using System;
using System.Collections.Generic;
using Endless.Gameplay.Stats;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Endless.Shared.UI.Windows;
using Endless.Shared.Validation;
using UnityEngine;

namespace Endless.Gameplay.UI;

public class UIGameOverWindow : UIGameObject, IUIWindow, IPoolableT, IValidatable
{
	private const float STAT_ENTRY_STAGGER_DELAY = 0.05f;

	[Header("UIGameOverWindow")]
	[SerializeField]
	private UIText titleText;

	[SerializeField]
	private UIText descriptionText;

	[SerializeField]
	private UIButton replayButton;

	[SerializeField]
	private UIButton endMatchButton;

	[SerializeField]
	private UIButton nextLevelButton;

	[SerializeField]
	private RectTransform statParent;

	[SerializeField]
	private InterfaceReference<IUIChildLayoutable>[] statParentLayoutables = Array.Empty<InterfaceReference<IUIChildLayoutable>>();

	[SerializeField]
	private BasicStatEntry basicStatPrefab;

	[SerializeField]
	private UIPerPlayerStatGrid perPlayerStatGridPrefab;

	[SerializeField]
	private GameObject replayVoteDisplay;

	[SerializeField]
	private GameObject endMatchVoteDisplay;

	[SerializeField]
	private GameObject nextLevelVoteDisplay;

	[SerializeField]
	private UIText replayVoteText;

	[SerializeField]
	private UIText endMatchVoteText;

	[SerializeField]
	private UIText nextLevelVoteText;

	[SerializeField]
	private UIWindowTweenHandler tweenHandler;

	[SerializeField]
	private InterfaceReference<IUILayoutable>[] layoutables = Array.Empty<InterfaceReference<IUILayoutable>>();

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private UIPerPlayerStatGrid statGrid;

	private readonly List<BasicStatEntry> basicStatEntryList = new List<BasicStatEntry>();

	private readonly List<UIPerPlayerStatGrid> perPlayerStatGridList = new List<UIPerPlayerStatGrid>();

	private int statEntryCount;

	public MonoBehaviour Prefab { get; set; }

	public static UIGameOverWindow Open(UIGameOverWindow prefab, UIGameOverWindowModel model, RectTransform container)
	{
		UIGameOverWindow uIGameOverWindow = MonoBehaviourSingleton<PoolManagerT>.Instance.Spawn(prefab, default(Vector3), default(Quaternion), container);
		uIGameOverWindow.RectTransform.SetAnchor(AnchorPresets.StretchAll);
		MonoBehaviourSingleton<UINewWindowManager>.Instance.RegisterWindow(uIGameOverWindow);
		uIGameOverWindow.Initialize(model);
		return uIGameOverWindow;
	}

	public void Validate()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Validate");
		}
		DebugUtility.DebugIsNull("titleText", titleText, this);
		DebugUtility.DebugIsNull("descriptionText", descriptionText, this);
		DebugUtility.DebugIsNull("replayButton", replayButton, this);
		DebugUtility.DebugIsNull("endMatchButton", endMatchButton, this);
		DebugUtility.DebugIsNull("nextLevelButton", nextLevelButton, this);
		DebugUtility.DebugIsNull("statParent", statParent, this);
		DebugUtility.DebugIsNull("basicStatPrefab", basicStatPrefab, this);
		DebugUtility.DebugIsNull("perPlayerStatGridPrefab", perPlayerStatGridPrefab, this);
		DebugUtility.DebugIsNull("replayVoteDisplay", replayVoteDisplay, this);
		DebugUtility.DebugIsNull("endMatchVoteDisplay", endMatchVoteDisplay, this);
		DebugUtility.DebugIsNull("nextLevelVoteDisplay", nextLevelVoteDisplay, this);
		DebugUtility.DebugIsNull("replayVoteText", replayVoteText, this);
		DebugUtility.DebugIsNull("endMatchVoteText", endMatchVoteText, this);
		DebugUtility.DebugIsNull("nextLevelVoteText", nextLevelVoteText, this);
		DebugUtility.DebugIsNull("tweenHandler", tweenHandler, this);
	}

	private void Initialize(UIGameOverWindowModel model)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Initialize", model);
		}
		NetworkBehaviourSingleton<GameEndManager>.Instance.OnVoteMoved.AddListener(HandleVoteMoved);
		replayButton.onClick.AddListener(NetworkBehaviourSingleton<GameEndManager>.Instance.ReplayVote);
		endMatchButton.onClick.AddListener(NetworkBehaviourSingleton<GameEndManager>.Instance.EndGameVote);
		nextLevelButton.onClick.AddListener(NetworkBehaviourSingleton<GameEndManager>.Instance.NextLevelVote);
		tweenHandler.PlayOpenTween();
		replayVoteDisplay.SetActive(value: false);
		endMatchVoteDisplay.SetActive(value: false);
		nextLevelVoteDisplay.SetActive(value: false);
		titleText.Value = model.Title;
		descriptionText.Value = model.Description;
		replayButton.gameObject.SetActive(model.ShowReplay);
		endMatchButton.gameObject.SetActive(model.ShowEndMatch);
		nextLevelButton.gameObject.SetActive(model.ShowNextLevel);
		statEntryCount = 0;
		bool flag = model.PerPlayerStats.Length != 0;
		for (int i = 0; i < model.BasicStats.Length; i++)
		{
			BasicStat basicStat = model.BasicStats[i];
			if (flag && basicStat.Order > 100)
			{
				SpawnPerPlayerGrid(model);
				flag = false;
			}
			BasicStatEntry basicStatEntry = MonoBehaviourSingleton<PoolManagerT>.Instance.Spawn(basicStatPrefab);
			statParentLayoutables.AddChildLayoutItem(basicStatEntry.RectTransform);
			basicStatEntry.Initialize(basicStat);
			basicStatEntry.PlayDisplayTween((float)statEntryCount * 0.05f);
			basicStatEntryList.Add(basicStatEntry);
			statEntryCount++;
		}
		if (flag)
		{
			SpawnPerPlayerGrid(model);
		}
		statParentLayoutables.RequestLayout();
		layoutables.RequestLayout();
	}

	public void Close()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Close");
		}
		if (tweenHandler.PlayCloseTween(Despawn))
		{
			MonoBehaviourSingleton<UINewWindowManager>.Instance.UnregisterWindow(this);
		}
	}

	private void Despawn()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Despawn");
		}
		MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn(this);
		NetworkBehaviourSingleton<GameEndManager>.Instance.OnVoteMoved.RemoveListener(HandleVoteMoved);
		replayButton.onClick.RemoveListener(NetworkBehaviourSingleton<GameEndManager>.Instance.ReplayVote);
		endMatchButton.onClick.RemoveListener(NetworkBehaviourSingleton<GameEndManager>.Instance.EndGameVote);
		nextLevelButton.onClick.RemoveListener(NetworkBehaviourSingleton<GameEndManager>.Instance.NextLevelVote);
		for (int i = 0; i < basicStatEntryList.Count; i++)
		{
			statParentLayoutables.RemoveChildLayoutItem(basicStatEntryList[i].RectTransform);
		}
		basicStatEntryList.DespawnAllItemsAndClear();
		for (int j = 0; j < perPlayerStatGridList.Count; j++)
		{
			statParentLayoutables.RemoveChildLayoutItem(perPlayerStatGridList[j].RectTransform);
		}
		perPlayerStatGridList.DespawnAllItemsAndClear();
		statGrid = null;
		statEntryCount = 0;
	}

	private void HandleVoteMoved(int userId, GameEndManager.VoteType oldVoteType, GameEndManager.VoteType newVoteType)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "HandleVoteMoved", userId, oldVoteType, newVoteType);
		}
		int count = NetworkBehaviourSingleton<GameEndManager>.Instance.EndMatchVotes.Count;
		int count2 = NetworkBehaviourSingleton<GameEndManager>.Instance.ReplayVotes.Count;
		int count3 = NetworkBehaviourSingleton<GameEndManager>.Instance.NextLevelVotes.Count;
		replayVoteDisplay.SetActive(count2 > 0);
		replayVoteText.Value = count2.ToString();
		endMatchVoteDisplay.SetActive(count > 0);
		endMatchVoteText.Value = count.ToString();
		nextLevelVoteDisplay.SetActive(count3 > 0);
		nextLevelVoteText.Value = count3.ToString();
	}

	private void SpawnPerPlayerGrid(UIGameOverWindowModel model)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "SpawnPerPlayerGrid", model);
		}
		statGrid = MonoBehaviourSingleton<PoolManagerT>.Instance.Spawn(perPlayerStatGridPrefab);
		statGrid.transform.SetParent(statParent, worldPositionStays: false);
		statGrid.Initialize(model.PerPlayerStats);
		statGrid.PlayDisplayTween((float)statEntryCount * 0.05f);
		perPlayerStatGridList.Add(statGrid);
		statParentLayoutables.AddChildLayoutItem(statGrid.RectTransform);
		statEntryCount++;
	}
}
