using System;
using System.Collections;
using System.Collections.Generic;
using Endless.Data;
using Endless.Gameplay;
using Endless.Gameplay.Screenshotting;
using Endless.Matchmaking;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using Endless.Shared.UI;
using Runtime.Shared.Matchmaking;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Endless.Creator.UI;

public class UIPlayBar : UIGameObject
{
	private const int USER_CAP_BEFORE_GRID = 5;

	private const float READY_STATUS_AUDIO_COOLDOWN = 0.5f;

	[SerializeField]
	private bool usePortraits = true;

	[SerializeField]
	private Canvas rootCanvas;

	[SerializeField]
	private UIButton playButton;

	[SerializeField]
	private UIButton playOptionButton;

	[SerializeField]
	private TweenCollection playButtonPressedTweenCollection;

	[SerializeField]
	private TweenCollection playOptionButtonPressedTweenCollection;

	[SerializeField]
	private UITooltip stopButtonTooltip;

	[SerializeField]
	private UITooltip playOptionTooltip;

	[SerializeField]
	private TextMeshProUGUI timerText;

	[SerializeField]
	private TweenCollection timerTextStartedTweenCollection;

	[SerializeField]
	private TweenCollection timerTextUpdatedTweenCollection;

	[SerializeField]
	private TextMeshProUGUI readyCountText;

	[SerializeField]
	private TextMeshProUGUI notReadyCountText;

	[SerializeField]
	private Image leftEndcapImage;

	[SerializeField]
	private Image rightEndcapImage;

	[SerializeField]
	private Sprite playSprite;

	[SerializeField]
	private Sprite creatorSprite;

	[SerializeField]
	private UIPoolableGameObject targetSource;

	[SerializeField]
	private UIPlayBarUserPortrait portraitPrefab;

	[SerializeField]
	private RectTransform playBarUserPortraitsContainer;

	[SerializeField]
	private RectTransform notReadyRowContainer;

	[SerializeField]
	private RectTransform notReadyGridContainer;

	[SerializeField]
	private RectTransform readyRowContainer;

	[SerializeField]
	private RectTransform readyGridContainer;

	[SerializeField]
	private Color notReadyColor = Color.blue;

	[SerializeField]
	private Color readyColor = Color.red;

	[SerializeField]
	private Graphic[] colorSwaps;

	[SerializeField]
	private GameObject playObject;

	[SerializeField]
	private GameObject stopObject;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private readonly Dictionary<int, UIPlayBarUserPortrait> playBarUserPortraits = new Dictionary<int, UIPlayBarUserPortrait>();

	private readonly Dictionary<int, UIPoolableGameObject> rowTargets = new Dictionary<int, UIPoolableGameObject>();

	private readonly Dictionary<int, UIPoolableGameObject> gridTargets = new Dictionary<int, UIPoolableGameObject>();

	private readonly HashSet<Transform> notReadyContainerHashSet = new HashSet<Transform>();

	private readonly HashSet<Transform> readyContainerHashSet = new HashSet<Transform>();

	private readonly HashSet<UnityEngine.Object> menuBlockingObjects = new HashSet<UnityEngine.Object>();

	private readonly HashSet<RectTransform> dirtyContainers = new HashSet<RectTransform>();

	private readonly Dictionary<int, Action> pendingTweens = new Dictionary<int, Action>();

	private readonly HashSet<int> usersWithParentChanges = new HashSet<int>();

	private Coroutine layoutUpdateCoroutine;

	private int readyCount;

	private int notReadyCount;

	private PlayerInputActions playerInputActions;

	private int lastDisplayedTime = -1;

	private bool previousIsTimerStarted;

	private float lastReadyStatusAudioTime = -999f;

	private bool UseGrid => playBarUserPortraits.Count > 5;

	private void Awake()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Awake");
		}
		playerInputActions = new PlayerInputActions();
		stopButtonTooltip.ShouldShow = false;
		readyCountText.enabled = !usePortraits;
		notReadyCountText.enabled = !usePortraits;
		readyRowContainer.gameObject.SetActive(usePortraits);
		notReadyRowContainer.gameObject.SetActive(usePortraits);
		notReadyContainerHashSet.Add(notReadyRowContainer);
		notReadyContainerHashSet.Add(notReadyGridContainer);
		readyContainerHashSet.Add(readyRowContainer);
		readyContainerHashSet.Add(readyGridContainer);
	}

	private void OnEnable()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnEnable");
		}
		playerInputActions.Player.FlipGameState.started += HandlePlayHotkey;
		playerInputActions.Player.FlipGameState.Enable();
		playButton.onClick.AddListener(HandleButtonClicked);
		playOptionButton.onClick.AddListener(HandlePlayOptionButtonClicked);
		UIScreenManager.OnScreenSystemOpen = (Action)Delegate.Combine(UIScreenManager.OnScreenSystemOpen, new Action(MenuOpened));
		UIScreenManager.OnScreenSystemClose = (Action)Delegate.Combine(UIScreenManager.OnScreenSystemClose, new Action(MenuClosed));
		UIScreenObserver.OnSizeChange = (Action)Delegate.Combine(UIScreenObserver.OnSizeChange, new Action(HandleScreenSizeChanged));
	}

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		HandleTimerUpdated();
		UpdateVisibility();
		NetworkBehaviourSingleton<PlayBarManager>.Instance.OnPlayerReadyChanged.AddListener(HandlePlayerReadyChanged);
		NetworkBehaviourSingleton<PlayBarManager>.Instance.OnMatchTimerUpdated.AddListener(HandleTimerUpdated);
		MonoBehaviourSingleton<GameplayManager>.Instance.OnGameplayStarted.AddListener(EnterGameplay);
		NetworkBehaviourSingleton<CreatorManager>.Instance.OnCreatorStarted.AddListener(EnterCreator);
		MonoBehaviourSingleton<ScreenshotAPI>.Instance.OnBeforeScreenshot.AddListener(ScreenshotStarted);
		MonoBehaviourSingleton<ScreenshotAPI>.Instance.OnAfterScreenshot.AddListener(ScreenshotStopped);
	}

	private void OnDisable()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnDisable");
		}
		if (layoutUpdateCoroutine != null)
		{
			StopCoroutine(layoutUpdateCoroutine);
			layoutUpdateCoroutine = null;
		}
		playerInputActions.Player.FlipGameState.started -= HandlePlayHotkey;
		playerInputActions.Player.FlipGameState.Disable();
		playButton.onClick.RemoveListener(HandleButtonClicked);
		playOptionButton.onClick.RemoveListener(HandlePlayOptionButtonClicked);
		UIScreenManager.OnScreenSystemOpen = (Action)Delegate.Remove(UIScreenManager.OnScreenSystemOpen, new Action(MenuOpened));
		UIScreenManager.OnScreenSystemClose = (Action)Delegate.Remove(UIScreenManager.OnScreenSystemClose, new Action(MenuClosed));
		UIScreenObserver.OnSizeChange = (Action)Delegate.Remove(UIScreenObserver.OnSizeChange, new Action(HandleScreenSizeChanged));
	}

	private void OnDestroy()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnDestroy");
		}
		if (!ExitManager.IsQuitting)
		{
			NetworkBehaviourSingleton<PlayBarManager>.Instance.OnPlayerReadyChanged.RemoveListener(HandlePlayerReadyChanged);
			NetworkBehaviourSingleton<PlayBarManager>.Instance.OnMatchTimerUpdated.RemoveListener(HandleTimerUpdated);
			MonoBehaviourSingleton<GameplayManager>.Instance.OnGameplayStarted.RemoveListener(EnterGameplay);
			NetworkBehaviourSingleton<CreatorManager>.Instance.OnCreatorStarted.RemoveListener(EnterCreator);
			MonoBehaviourSingleton<ScreenshotAPI>.Instance.OnBeforeScreenshot.RemoveListener(ScreenshotStarted);
			MonoBehaviourSingleton<ScreenshotAPI>.Instance.OnAfterScreenshot.RemoveListener(ScreenshotStopped);
			dirtyContainers.Clear();
			pendingTweens.Clear();
			usersWithParentChanges.Clear();
		}
	}

	private void Update()
	{
		if (!NetworkBehaviourSingleton<PlayBarManager>.Instance.IsTimerStarted)
		{
			return;
		}
		float f = NetworkBehaviourSingleton<PlayBarManager>.Instance.MatchTransitionTime - NetworkManager.Singleton.ServerTime.TimeAsFloat;
		int num = Mathf.Max(0, Mathf.CeilToInt(f));
		if (num != lastDisplayedTime)
		{
			lastDisplayedTime = num;
			if (num <= 5)
			{
				MonoBehaviourSingleton<UiAudioManager>.Instance.PlayUiAudio(NormalUiSoundType.PlayTimerCompletionWarning);
			}
			timerText.SetText("{0}", num);
			if (!timerTextStartedTweenCollection.IsAnyTweening())
			{
				timerTextUpdatedTweenCollection.Tween();
			}
		}
	}

	private void HandlePlayerReadyChanged(int userId, PlayBarManager.PlayerReadyStatus previousStatus, PlayBarManager.PlayerReadyStatus newStatus)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "HandlePlayerReadyChanged", userId, previousStatus, newStatus);
			DebugUtility.Log($"PlayerReady changed for {userId}: {previousStatus} -> {newStatus}", this);
		}
		UpdatePlayerReadyCounts(previousStatus, newStatus);
		bool useGrid = UseGrid;
		bool flag = previousStatus == PlayBarManager.PlayerReadyStatus.None && newStatus != PlayBarManager.PlayerReadyStatus.None;
		bool flag2 = newStatus == PlayBarManager.PlayerReadyStatus.None && previousStatus != PlayBarManager.PlayerReadyStatus.None;
		bool flag3 = !flag && !flag2;
		if (verboseLogging)
		{
			DebugUtility.Log(string.Format("{0}: {1}", "isNewPlayer", flag), this);
			DebugUtility.Log(string.Format("{0}: {1}", "isPlayerLeaving", flag2), this);
			DebugUtility.Log(string.Format("{0}: {1}", "isStatusFlip", flag3), this);
		}
		if (flag)
		{
			SpawnPlayBarUserPortrait(userId);
		}
		else if (flag2)
		{
			DespawnPlayBarUserPortrait(userId);
		}
		else
		{
			UpdatePlayBarUserPortraitsTarget(userId, newStatus, tweenPositionAndSizeDeltaComplete: true);
			float unscaledTime = Time.unscaledTime;
			if (unscaledTime - lastReadyStatusAudioTime >= 0.5f)
			{
				MonoBehaviourSingleton<UiAudioManager>.Instance.PlayUiAudio((newStatus == PlayBarManager.PlayerReadyStatus.NotReady) ? NormalUiSoundType.PlayerNotReady : NormalUiSoundType.PlayerReady);
				lastReadyStatusAudioTime = unscaledTime;
			}
		}
		if (UseGrid != useGrid)
		{
			UpdateArrangement();
		}
		else if (flag2 || flag3)
		{
			CompressPlayBarUserPortraits(previousStatus);
		}
		HandleColors(userId, newStatus);
		HandlePlayButtonVisuals(userId, newStatus);
		UpdateText();
	}

	private void UpdateArrangement()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "UpdateArrangement");
		}
		Dictionary<int, UIPoolableGameObject> dictionary = (UseGrid ? gridTargets : rowTargets);
		RectTransform container = (UseGrid ? readyGridContainer : readyRowContainer);
		RectTransform container2 = (UseGrid ? notReadyGridContainer : notReadyRowContainer);
		MarkContainerForLayoutUpdate(container);
		MarkContainerForLayoutUpdate(container2);
		foreach (KeyValuePair<int, UIPlayBarUserPortrait> playBarUserPortrait in playBarUserPortraits)
		{
			RectTransform target = dictionary[playBarUserPortrait.Key].RectTransform;
			int portraitUserId = playBarUserPortrait.Key;
			usersWithParentChanges.Add(portraitUserId);
			pendingTweens[portraitUserId] = delegate
			{
				playBarUserPortraits[portraitUserId].ChangeArrangement(target);
			};
		}
	}

	private void UpdatePlayerReadyCounts(PlayBarManager.PlayerReadyStatus previousStatus, PlayBarManager.PlayerReadyStatus newStatus)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "UpdatePlayerReadyCounts", previousStatus, newStatus);
		}
		switch (previousStatus)
		{
		case PlayBarManager.PlayerReadyStatus.NotReady:
			notReadyCount--;
			break;
		case PlayBarManager.PlayerReadyStatus.Ready:
			readyCount--;
			break;
		}
		switch (newStatus)
		{
		case PlayBarManager.PlayerReadyStatus.Ready:
			readyCount++;
			break;
		case PlayBarManager.PlayerReadyStatus.NotReady:
			notReadyCount++;
			break;
		}
		notReadyCount = Mathf.Max(0, notReadyCount);
		readyCount = Mathf.Max(0, readyCount);
	}

	private void HandleColors(int userId, PlayBarManager.PlayerReadyStatus newStatus)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "HandleColors", userId, newStatus);
		}
		if (userId == EndlessServices.Instance.CloudService.ActiveUserId)
		{
			bool flag = newStatus == PlayBarManager.PlayerReadyStatus.Ready;
			Graphic[] array = colorSwaps;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].color = (flag ? readyColor : notReadyColor);
			}
		}
	}

	private void HandlePlayButtonVisuals(int userId, PlayBarManager.PlayerReadyStatus newStatus)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "HandlePlayButtonVisuals", userId, newStatus);
		}
		if (NetworkBehaviourSingleton<UserIdManager>.Instance.TryGetClientId(userId, out var clientId) && clientId == 0L)
		{
			if (MonoBehaviourSingleton<GameplayManager>.Instance.IsPlaying && !NetworkManager.Singleton.IsHost)
			{
				playButton.interactable = newStatus != PlayBarManager.PlayerReadyStatus.Ready;
				stopButtonTooltip.ShouldShow = newStatus == PlayBarManager.PlayerReadyStatus.Ready;
			}
			else
			{
				playButton.interactable = true;
				stopButtonTooltip.ShouldShow = false;
			}
		}
	}

	private void CompressPlayBarUserPortraits(PlayBarManager.PlayerReadyStatus statusToCompress)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "CompressPlayBarUserPortraits", statusToCompress);
		}
		switch (statusToCompress)
		{
		case PlayBarManager.PlayerReadyStatus.NotReady:
		{
			foreach (KeyValuePair<int, UIPlayBarUserPortrait> playBarUserPortrait in playBarUserPortraits)
			{
				if (notReadyContainerHashSet.Contains(playBarUserPortrait.Value.Target.parent))
				{
					UpdatePlayBarUserPortraitsTarget(playBarUserPortrait.Key, PlayBarManager.PlayerReadyStatus.NotReady, tweenPositionAndSizeDeltaComplete: false);
				}
			}
			break;
		}
		case PlayBarManager.PlayerReadyStatus.Ready:
		{
			foreach (KeyValuePair<int, UIPlayBarUserPortrait> playBarUserPortrait2 in playBarUserPortraits)
			{
				if (readyContainerHashSet.Contains(playBarUserPortrait2.Value.Target.parent))
				{
					UpdatePlayBarUserPortraitsTarget(playBarUserPortrait2.Key, PlayBarManager.PlayerReadyStatus.Ready, tweenPositionAndSizeDeltaComplete: false);
				}
			}
			break;
		}
		}
	}

	private void UpdatePlayBarUserPortraitsTarget(int userId, PlayBarManager.PlayerReadyStatus status, bool tweenPositionAndSizeDeltaComplete)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "UpdatePlayBarUserPortraitsTarget", userId, status, tweenPositionAndSizeDeltaComplete);
		}
		RectTransform parent = ((status == PlayBarManager.PlayerReadyStatus.Ready) ? readyRowContainer : notReadyRowContainer);
		RectTransform parent2 = ((status == PlayBarManager.PlayerReadyStatus.Ready) ? readyGridContainer : notReadyGridContainer);
		if (!rowTargets.TryGetValue(userId, out var value) || !gridTargets.TryGetValue(userId, out var value2))
		{
			DebugUtility.LogWarning(string.Format("Could not find {0} or {1} for {2}", "rowTargetGameObject", "gridTargetGameObject", userId), this);
			return;
		}
		RectTransform rectTransform = value.RectTransform;
		RectTransform rectTransform2 = value2.RectTransform;
		rectTransform.SetParent(parent);
		rectTransform2.SetParent(parent2);
		switch (status)
		{
		case PlayBarManager.PlayerReadyStatus.NotReady:
			rectTransform.SetAsLastSibling();
			rectTransform2.SetAsLastSibling();
			break;
		case PlayBarManager.PlayerReadyStatus.Ready:
			rectTransform.SetAsFirstSibling();
			rectTransform2.SetAsFirstSibling();
			break;
		}
		RectTransform target = (UseGrid ? rectTransform2 : rectTransform);
		RectTransform container = target.parent as RectTransform;
		MarkContainerForLayoutUpdate(container);
		usersWithParentChanges.Add(userId);
		pendingTweens[userId] = delegate
		{
			if (!playBarUserPortraits.TryGetValue(userId, out var value3) || !value3 || !target)
			{
				DebugUtility.LogWarning(string.Format("Could not find {0} or {1} for {2}", "portrait", "target", userId), this);
			}
			else
			{
				value3.SetTargetAndTweenPositionAndSizeDelta(target, tweenPositionAndSizeDeltaComplete);
			}
		};
	}

	private void SpawnPlayBarUserPortrait(int userId)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "SpawnPlayBarUserPortrait", userId);
		}
		PoolManagerT instance = MonoBehaviourSingleton<PoolManagerT>.Instance;
		UIPoolableGameObject prefab = targetSource;
		Transform parent = notReadyRowContainer;
		UIPoolableGameObject uIPoolableGameObject = instance.Spawn(prefab, default(Vector3), default(Quaternion), parent);
		PoolManagerT instance2 = MonoBehaviourSingleton<PoolManagerT>.Instance;
		UIPoolableGameObject prefab2 = targetSource;
		parent = notReadyGridContainer;
		UIPoolableGameObject uIPoolableGameObject2 = instance2.Spawn(prefab2, default(Vector3), default(Quaternion), parent);
		PoolManagerT instance3 = MonoBehaviourSingleton<PoolManagerT>.Instance;
		UIPlayBarUserPortrait prefab3 = portraitPrefab;
		parent = playBarUserPortraitsContainer;
		UIPlayBarUserPortrait uIPlayBarUserPortrait = instance3.Spawn(prefab3, default(Vector3), default(Quaternion), parent);
		rowTargets.Add(userId, uIPoolableGameObject);
		gridTargets.Add(userId, uIPoolableGameObject2);
		playBarUserPortraits.Add(userId, uIPlayBarUserPortrait);
		uIPlayBarUserPortrait.Initialize(userId, UseGrid ? uIPoolableGameObject2.RectTransform : uIPoolableGameObject.RectTransform);
		if (playBarUserPortraits.Count > 1)
		{
			MonoBehaviourSingleton<UiAudioManager>.Instance.PlayUiAudio(NormalUiSoundType.PlayBarUserPortraitJoin);
		}
	}

	private void DespawnPlayBarUserPortrait(int userId)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "DespawnPlayBarUserPortrait", userId);
		}
		playBarUserPortraits[userId].ShrinkAwayAndDespawn();
		playBarUserPortraits.Remove(userId);
		MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn(rowTargets[userId]);
		MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn(gridTargets[userId]);
		rowTargets.Remove(userId);
		gridTargets.Remove(userId);
		pendingTweens.Remove(userId);
		if (playBarUserPortraits.Count >= 1)
		{
			MonoBehaviourSingleton<UiAudioManager>.Instance.PlayUiAudio(NormalUiSoundType.PlayBarUserPortraitLeft);
		}
	}

	private void MarkContainerForLayoutUpdate(RectTransform container)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "MarkContainerForLayoutUpdate", container.DebugSafeName());
		}
		if (!(container == null))
		{
			dirtyContainers.Add(container);
			if (layoutUpdateCoroutine == null)
			{
				layoutUpdateCoroutine = StartCoroutine(ProcessLayoutUpdates());
			}
		}
	}

	private IEnumerator ProcessLayoutUpdates()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "ProcessLayoutUpdates");
		}
		yield return null;
		foreach (RectTransform dirtyContainer in dirtyContainers)
		{
			if ((bool)dirtyContainer)
			{
				LayoutRebuilder.ForceRebuildLayoutImmediate(dirtyContainer);
			}
		}
		foreach (KeyValuePair<int, Action> pendingTween in pendingTweens)
		{
			pendingTween.Value?.Invoke();
		}
		if (usersWithParentChanges.Count > 0)
		{
			TweenPortraitsWithoutParentChanges();
		}
		dirtyContainers.Clear();
		pendingTweens.Clear();
		usersWithParentChanges.Clear();
		layoutUpdateCoroutine = null;
	}

	private void TweenPortraitsWithoutParentChanges()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "TweenPortraitsWithoutParentChanges");
		}
		foreach (var (num2, uIPlayBarUserPortrait2) in playBarUserPortraits)
		{
			if (!usersWithParentChanges.Contains(num2) && (bool)uIPlayBarUserPortrait2 && !uIPlayBarUserPortrait2.IsAtTargetPositionAndSize())
			{
				Dictionary<int, UIPoolableGameObject> dictionary = (UseGrid ? gridTargets : rowTargets);
				if (dictionary.TryGetValue(num2, out var _))
				{
					RectTransform target = dictionary[num2].RectTransform;
					uIPlayBarUserPortrait2.SetTargetAndTweenPositionAndSizeDelta(target, tweenPositionAndSizeDeltaComplete: false);
				}
			}
		}
	}

	private void ScreenshotStopped()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "ScreenshotStopped");
		}
		menuBlockingObjects.Remove(MonoBehaviourSingleton<ScreenshotAPI>.Instance);
		UpdateVisibility();
	}

	private void ScreenshotStarted()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "ScreenshotStarted");
		}
		menuBlockingObjects.Add(MonoBehaviourSingleton<ScreenshotAPI>.Instance);
		UpdateVisibility();
	}

	private void MenuOpened()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "MenuOpened");
		}
		menuBlockingObjects.Add(MonoBehaviourSingleton<UIScreenManager>.Instance);
		UpdateVisibility();
	}

	private void MenuClosed()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "MenuClosed");
		}
		menuBlockingObjects.Remove(MonoBehaviourSingleton<UIScreenManager>.Instance);
		UpdateVisibility();
	}

	private void UpdateVisibility()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "UpdateVisibility");
		}
		if (menuBlockingObjects.Count > 0)
		{
			rootCanvas.enabled = false;
			return;
		}
		bool flag = false;
		bool flag2 = false;
		if ((bool)MatchmakingClientController.Instance && MatchmakingClientController.Instance.LocalMatch != null)
		{
			MatchData matchData = MatchmakingClientController.Instance.LocalMatch.GetMatchData();
			flag2 = matchData.IsEditSession;
			flag = matchData.IsPlaytest();
		}
		rootCanvas.enabled = flag2 || flag;
	}

	private void EnterCreator()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "EnterCreator");
		}
		playObject.SetActive(value: true);
		stopObject.SetActive(value: false);
		UpdateLocalOption();
		UpdateSprites();
		HandleTimerUpdated();
	}

	private void EnterGameplay()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "EnterGameplay");
		}
		playObject.SetActive(value: false);
		stopObject.SetActive(value: true);
		UpdateLocalOption();
		UpdateSprites();
		HandleTimerUpdated();
	}

	private void UpdateSprites()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "UpdateSprites");
		}
		leftEndcapImage.sprite = (MonoBehaviourSingleton<GameplayManager>.Instance.IsPlaying ? playSprite : creatorSprite);
		rightEndcapImage.sprite = (MonoBehaviourSingleton<GameplayManager>.Instance.IsPlaying ? creatorSprite : playSprite);
	}

	private void UpdateLocalOption()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "UpdateLocalOption");
		}
		PlayBarManager.PlayOption localPlayOption = NetworkBehaviourSingleton<PlayBarManager>.Instance.GetLocalPlayOption();
		playOptionButton.gameObject.SetActive(localPlayOption != PlayBarManager.PlayOption.None);
		switch (localPlayOption)
		{
		case PlayBarManager.PlayOption.PlayWithParty:
			playOptionTooltip.SetTooltip("Play now (Party)");
			break;
		case PlayBarManager.PlayOption.PlaySolo:
			playOptionTooltip.SetTooltip("Play now (Solo)");
			break;
		case PlayBarManager.PlayOption.StopSolo:
			playOptionTooltip.SetTooltip("Return now (Solo)");
			break;
		case PlayBarManager.PlayOption.StopWithParty:
			playOptionTooltip.SetTooltip("Return now (Party)");
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case PlayBarManager.PlayOption.None:
			break;
		}
	}

	private void HandleTimerUpdated()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "HandleTimerUpdated");
		}
		bool isTimerStarted = NetworkBehaviourSingleton<PlayBarManager>.Instance.IsTimerStarted;
		if (previousIsTimerStarted != isTimerStarted)
		{
			MonoBehaviourSingleton<UiAudioManager>.Instance.PlayUiAudio(isTimerStarted ? NormalUiSoundType.TimerStarted : NormalUiSoundType.TimerEnded);
		}
		timerText.enabled = isTimerStarted;
		previousIsTimerStarted = isTimerStarted;
		if (isTimerStarted)
		{
			timerTextStartedTweenCollection.Tween();
		}
	}

	private void UpdateText()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "UpdateText");
		}
		readyCountText.SetText("{0}", readyCount);
		notReadyCountText.SetText("{0}", notReadyCount);
	}

	private void HandleButtonClicked()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "HandleButtonClicked");
		}
		NetworkBehaviourSingleton<PlayBarManager>.Instance.PlayHit();
		playButtonPressedTweenCollection.Tween();
	}

	private void HandlePlayOptionButtonClicked()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "HandlePlayOptionButtonClicked");
		}
		NetworkBehaviourSingleton<PlayBarManager>.Instance.ExecuteLocalPlayOption();
		playOptionButtonPressedTweenCollection.Tween();
	}

	private void HandlePlayHotkey(InputAction.CallbackContext callbackContext)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "HandlePlayHotkey", callbackContext);
		}
		if (playButton.IsInteractable())
		{
			bool flag = false;
			bool flag2 = false;
			if ((bool)MatchmakingClientController.Instance && MatchmakingClientController.Instance.LocalMatch != null)
			{
				MatchData matchData = MatchmakingClientController.Instance.LocalMatch.GetMatchData();
				flag2 = matchData.IsEditSession;
				flag = matchData.IsPlaytest();
			}
			if (flag2 || flag)
			{
				HandleButtonClicked();
			}
		}
	}

	private void HandleScreenSizeChanged()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "HandleScreenSizeChanged");
		}
		foreach (KeyValuePair<int, UIPlayBarUserPortrait> playBarUserPortrait in playBarUserPortraits)
		{
			playBarUserPortrait.Value.CancelPositionAndSizeTweens();
		}
		QueueAllPortraitsForTween();
	}

	private void QueueAllPortraitsForTween()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "QueueAllPortraitsForTween");
		}
		RectTransform container = (UseGrid ? readyGridContainer : readyRowContainer);
		RectTransform container2 = (UseGrid ? notReadyGridContainer : notReadyRowContainer);
		MarkContainerForLayoutUpdate(container);
		MarkContainerForLayoutUpdate(container2);
		foreach (KeyValuePair<int, UIPlayBarUserPortrait> playBarUserPortrait in playBarUserPortraits)
		{
			int portraitUserId = playBarUserPortrait.Key;
			if (!(UseGrid ? gridTargets : rowTargets).TryGetValue(portraitUserId, out var value))
			{
				continue;
			}
			RectTransform target = value.RectTransform;
			pendingTweens[portraitUserId] = delegate
			{
				if (playBarUserPortraits.TryGetValue(portraitUserId, out var value2) && (bool)value2 && (bool)target)
				{
					value2.SetTargetAndTweenPositionAndSizeDelta(target, tweenPositionAndSizeDeltaComplete: false);
				}
			};
		}
	}
}
