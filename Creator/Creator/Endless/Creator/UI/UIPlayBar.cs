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
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Endless.Creator.UI
{
	// Token: 0x02000262 RID: 610
	public class UIPlayBar : UIGameObject
	{
		// Token: 0x1700013F RID: 319
		// (get) Token: 0x060009E8 RID: 2536 RVA: 0x0002D9DB File Offset: 0x0002BBDB
		private bool UseGrid
		{
			get
			{
				return this.playBarUserPortraits.Count > 5;
			}
		}

		// Token: 0x060009E9 RID: 2537 RVA: 0x0002D9EC File Offset: 0x0002BBEC
		private void Awake()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Awake", Array.Empty<object>());
			}
			this.playerInputActions = new PlayerInputActions();
			this.stopButtonTooltip.ShouldShow = false;
			this.readyCountText.enabled = !this.usePortraits;
			this.notReadyCountText.enabled = !this.usePortraits;
			this.readyRowContainer.gameObject.SetActive(this.usePortraits);
			this.notReadyRowContainer.gameObject.SetActive(this.usePortraits);
			this.notReadyContainerHashSet.Add(this.notReadyRowContainer);
			this.notReadyContainerHashSet.Add(this.notReadyGridContainer);
			this.readyContainerHashSet.Add(this.readyRowContainer);
			this.readyContainerHashSet.Add(this.readyGridContainer);
		}

		// Token: 0x060009EA RID: 2538 RVA: 0x0002DAC4 File Offset: 0x0002BCC4
		private void OnEnable()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEnable", Array.Empty<object>());
			}
			this.playerInputActions.Player.FlipGameState.started += this.HandlePlayHotkey;
			this.playerInputActions.Player.FlipGameState.Enable();
			this.playButton.onClick.AddListener(new UnityAction(this.HandleButtonClicked));
			this.playOptionButton.onClick.AddListener(new UnityAction(this.HandlePlayOptionButtonClicked));
			UIScreenManager.OnScreenSystemOpen = (Action)Delegate.Combine(UIScreenManager.OnScreenSystemOpen, new Action(this.MenuOpened));
			UIScreenManager.OnScreenSystemClose = (Action)Delegate.Combine(UIScreenManager.OnScreenSystemClose, new Action(this.MenuClosed));
			UIScreenObserver.OnSizeChange = (Action)Delegate.Combine(UIScreenObserver.OnSizeChange, new Action(this.HandleScreenSizeChanged));
		}

		// Token: 0x060009EB RID: 2539 RVA: 0x0002DBC0 File Offset: 0x0002BDC0
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.HandleTimerUpdated();
			this.UpdateVisibility();
			NetworkBehaviourSingleton<PlayBarManager>.Instance.OnPlayerReadyChanged.AddListener(new UnityAction<int, PlayBarManager.PlayerReadyStatus, PlayBarManager.PlayerReadyStatus>(this.HandlePlayerReadyChanged));
			NetworkBehaviourSingleton<PlayBarManager>.Instance.OnMatchTimerUpdated.AddListener(new UnityAction(this.HandleTimerUpdated));
			MonoBehaviourSingleton<GameplayManager>.Instance.OnGameplayStarted.AddListener(new UnityAction(this.EnterGameplay));
			NetworkBehaviourSingleton<CreatorManager>.Instance.OnCreatorStarted.AddListener(new UnityAction(this.EnterCreator));
			MonoBehaviourSingleton<ScreenshotAPI>.Instance.OnBeforeScreenshot.AddListener(new UnityAction(this.ScreenshotStarted));
			MonoBehaviourSingleton<ScreenshotAPI>.Instance.OnAfterScreenshot.AddListener(new UnityAction(this.ScreenshotStopped));
		}

		// Token: 0x060009EC RID: 2540 RVA: 0x0002DC94 File Offset: 0x0002BE94
		private void OnDisable()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDisable", Array.Empty<object>());
			}
			if (this.layoutUpdateCoroutine != null)
			{
				base.StopCoroutine(this.layoutUpdateCoroutine);
				this.layoutUpdateCoroutine = null;
			}
			this.playerInputActions.Player.FlipGameState.started -= this.HandlePlayHotkey;
			this.playerInputActions.Player.FlipGameState.Disable();
			this.playButton.onClick.RemoveListener(new UnityAction(this.HandleButtonClicked));
			this.playOptionButton.onClick.RemoveListener(new UnityAction(this.HandlePlayOptionButtonClicked));
			UIScreenManager.OnScreenSystemOpen = (Action)Delegate.Remove(UIScreenManager.OnScreenSystemOpen, new Action(this.MenuOpened));
			UIScreenManager.OnScreenSystemClose = (Action)Delegate.Remove(UIScreenManager.OnScreenSystemClose, new Action(this.MenuClosed));
			UIScreenObserver.OnSizeChange = (Action)Delegate.Remove(UIScreenObserver.OnSizeChange, new Action(this.HandleScreenSizeChanged));
		}

		// Token: 0x060009ED RID: 2541 RVA: 0x0002DDA8 File Offset: 0x0002BFA8
		private void OnDestroy()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDestroy", Array.Empty<object>());
			}
			if (ExitManager.IsQuitting)
			{
				return;
			}
			NetworkBehaviourSingleton<PlayBarManager>.Instance.OnPlayerReadyChanged.RemoveListener(new UnityAction<int, PlayBarManager.PlayerReadyStatus, PlayBarManager.PlayerReadyStatus>(this.HandlePlayerReadyChanged));
			NetworkBehaviourSingleton<PlayBarManager>.Instance.OnMatchTimerUpdated.RemoveListener(new UnityAction(this.HandleTimerUpdated));
			MonoBehaviourSingleton<GameplayManager>.Instance.OnGameplayStarted.RemoveListener(new UnityAction(this.EnterGameplay));
			NetworkBehaviourSingleton<CreatorManager>.Instance.OnCreatorStarted.RemoveListener(new UnityAction(this.EnterCreator));
			MonoBehaviourSingleton<ScreenshotAPI>.Instance.OnBeforeScreenshot.RemoveListener(new UnityAction(this.ScreenshotStarted));
			MonoBehaviourSingleton<ScreenshotAPI>.Instance.OnAfterScreenshot.RemoveListener(new UnityAction(this.ScreenshotStopped));
			this.dirtyContainers.Clear();
			this.pendingTweens.Clear();
			this.usersWithParentChanges.Clear();
		}

		// Token: 0x060009EE RID: 2542 RVA: 0x0002DE98 File Offset: 0x0002C098
		private void Update()
		{
			if (!NetworkBehaviourSingleton<PlayBarManager>.Instance.IsTimerStarted)
			{
				return;
			}
			float num = NetworkBehaviourSingleton<PlayBarManager>.Instance.MatchTransitionTime - NetworkManager.Singleton.ServerTime.TimeAsFloat;
			int num2 = Mathf.Max(0, Mathf.CeilToInt(num));
			if (num2 == this.lastDisplayedTime)
			{
				return;
			}
			this.lastDisplayedTime = num2;
			if (num2 <= 5)
			{
				MonoBehaviourSingleton<UiAudioManager>.Instance.PlayUiAudio(NormalUiSoundType.PlayTimerCompletionWarning);
			}
			this.timerText.SetText("{0}", (float)num2);
			if (!this.timerTextStartedTweenCollection.IsAnyTweening())
			{
				this.timerTextUpdatedTweenCollection.Tween();
			}
		}

		// Token: 0x060009EF RID: 2543 RVA: 0x0002DF28 File Offset: 0x0002C128
		private void HandlePlayerReadyChanged(int userId, PlayBarManager.PlayerReadyStatus previousStatus, PlayBarManager.PlayerReadyStatus newStatus)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "HandlePlayerReadyChanged", new object[] { userId, previousStatus, newStatus });
				DebugUtility.Log(string.Format("PlayerReady changed for {0}: {1} -> {2}", userId, previousStatus, newStatus), this);
			}
			this.UpdatePlayerReadyCounts(previousStatus, newStatus);
			bool useGrid = this.UseGrid;
			bool flag = previousStatus == PlayBarManager.PlayerReadyStatus.None && newStatus > PlayBarManager.PlayerReadyStatus.None;
			bool flag2 = newStatus == PlayBarManager.PlayerReadyStatus.None && previousStatus > PlayBarManager.PlayerReadyStatus.None;
			bool flag3 = !flag && !flag2;
			if (this.verboseLogging)
			{
				DebugUtility.Log(string.Format("{0}: {1}", "isNewPlayer", flag), this);
				DebugUtility.Log(string.Format("{0}: {1}", "isPlayerLeaving", flag2), this);
				DebugUtility.Log(string.Format("{0}: {1}", "isStatusFlip", flag3), this);
			}
			if (flag)
			{
				this.SpawnPlayBarUserPortrait(userId);
			}
			else if (flag2)
			{
				this.DespawnPlayBarUserPortrait(userId);
			}
			else
			{
				this.UpdatePlayBarUserPortraitsTarget(userId, newStatus, true);
				float unscaledTime = Time.unscaledTime;
				if (unscaledTime - this.lastReadyStatusAudioTime >= 0.5f)
				{
					MonoBehaviourSingleton<UiAudioManager>.Instance.PlayUiAudio((newStatus == PlayBarManager.PlayerReadyStatus.NotReady) ? NormalUiSoundType.PlayerNotReady : NormalUiSoundType.PlayerReady);
					this.lastReadyStatusAudioTime = unscaledTime;
				}
			}
			if (this.UseGrid != useGrid)
			{
				this.UpdateArrangement();
			}
			else if (flag2 || flag3)
			{
				this.CompressPlayBarUserPortraits(previousStatus);
			}
			this.HandleColors(userId, newStatus);
			this.HandlePlayButtonVisuals(userId, newStatus);
			this.UpdateText();
		}

		// Token: 0x060009F0 RID: 2544 RVA: 0x0002E0A0 File Offset: 0x0002C2A0
		private void UpdateArrangement()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "UpdateArrangement", Array.Empty<object>());
			}
			Dictionary<int, UIPoolableGameObject> dictionary = (this.UseGrid ? this.gridTargets : this.rowTargets);
			RectTransform rectTransform = (this.UseGrid ? this.readyGridContainer : this.readyRowContainer);
			RectTransform rectTransform2 = (this.UseGrid ? this.notReadyGridContainer : this.notReadyRowContainer);
			this.MarkContainerForLayoutUpdate(rectTransform);
			this.MarkContainerForLayoutUpdate(rectTransform2);
			foreach (KeyValuePair<int, UIPlayBarUserPortrait> keyValuePair in this.playBarUserPortraits)
			{
				RectTransform target = dictionary[keyValuePair.Key].RectTransform;
				int portraitUserId = keyValuePair.Key;
				this.usersWithParentChanges.Add(portraitUserId);
				this.pendingTweens[portraitUserId] = delegate
				{
					this.playBarUserPortraits[portraitUserId].ChangeArrangement(target);
				};
			}
		}

		// Token: 0x060009F1 RID: 2545 RVA: 0x0002E1C0 File Offset: 0x0002C3C0
		private void UpdatePlayerReadyCounts(PlayBarManager.PlayerReadyStatus previousStatus, PlayBarManager.PlayerReadyStatus newStatus)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "UpdatePlayerReadyCounts", new object[] { previousStatus, newStatus });
			}
			if (previousStatus == PlayBarManager.PlayerReadyStatus.NotReady)
			{
				this.notReadyCount--;
			}
			else if (previousStatus == PlayBarManager.PlayerReadyStatus.Ready)
			{
				this.readyCount--;
			}
			if (newStatus == PlayBarManager.PlayerReadyStatus.Ready)
			{
				this.readyCount++;
			}
			else if (newStatus == PlayBarManager.PlayerReadyStatus.NotReady)
			{
				this.notReadyCount++;
			}
			this.notReadyCount = Mathf.Max(0, this.notReadyCount);
			this.readyCount = Mathf.Max(0, this.readyCount);
		}

		// Token: 0x060009F2 RID: 2546 RVA: 0x0002E268 File Offset: 0x0002C468
		private void HandleColors(int userId, PlayBarManager.PlayerReadyStatus newStatus)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "HandleColors", new object[] { userId, newStatus });
			}
			if (userId != EndlessServices.Instance.CloudService.ActiveUserId)
			{
				return;
			}
			bool flag = newStatus == PlayBarManager.PlayerReadyStatus.Ready;
			Graphic[] array = this.colorSwaps;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].color = (flag ? this.readyColor : this.notReadyColor);
			}
		}

		// Token: 0x060009F3 RID: 2547 RVA: 0x0002E2E8 File Offset: 0x0002C4E8
		private void HandlePlayButtonVisuals(int userId, PlayBarManager.PlayerReadyStatus newStatus)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "HandlePlayButtonVisuals", new object[] { userId, newStatus });
			}
			ulong num;
			if (NetworkBehaviourSingleton<UserIdManager>.Instance.TryGetClientId(userId, out num) && num == 0UL)
			{
				if (MonoBehaviourSingleton<GameplayManager>.Instance.IsPlaying && !NetworkManager.Singleton.IsHost)
				{
					this.playButton.interactable = newStatus != PlayBarManager.PlayerReadyStatus.Ready;
					this.stopButtonTooltip.ShouldShow = newStatus == PlayBarManager.PlayerReadyStatus.Ready;
					return;
				}
				this.playButton.interactable = true;
				this.stopButtonTooltip.ShouldShow = false;
			}
		}

		// Token: 0x060009F4 RID: 2548 RVA: 0x0002E384 File Offset: 0x0002C584
		private void CompressPlayBarUserPortraits(PlayBarManager.PlayerReadyStatus statusToCompress)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "CompressPlayBarUserPortraits", new object[] { statusToCompress });
			}
			if (statusToCompress == PlayBarManager.PlayerReadyStatus.NotReady)
			{
				using (Dictionary<int, UIPlayBarUserPortrait>.Enumerator enumerator = this.playBarUserPortraits.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						KeyValuePair<int, UIPlayBarUserPortrait> keyValuePair = enumerator.Current;
						if (this.notReadyContainerHashSet.Contains(keyValuePair.Value.Target.parent))
						{
							this.UpdatePlayBarUserPortraitsTarget(keyValuePair.Key, PlayBarManager.PlayerReadyStatus.NotReady, false);
						}
					}
					return;
				}
			}
			if (statusToCompress == PlayBarManager.PlayerReadyStatus.Ready)
			{
				foreach (KeyValuePair<int, UIPlayBarUserPortrait> keyValuePair2 in this.playBarUserPortraits)
				{
					if (this.readyContainerHashSet.Contains(keyValuePair2.Value.Target.parent))
					{
						this.UpdatePlayBarUserPortraitsTarget(keyValuePair2.Key, PlayBarManager.PlayerReadyStatus.Ready, false);
					}
				}
			}
		}

		// Token: 0x060009F5 RID: 2549 RVA: 0x0002E490 File Offset: 0x0002C690
		private void UpdatePlayBarUserPortraitsTarget(int userId, PlayBarManager.PlayerReadyStatus status, bool tweenPositionAndSizeDeltaComplete)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "UpdatePlayBarUserPortraitsTarget", new object[] { userId, status, tweenPositionAndSizeDeltaComplete });
			}
			RectTransform rectTransform = ((status == PlayBarManager.PlayerReadyStatus.Ready) ? this.readyRowContainer : this.notReadyRowContainer);
			RectTransform rectTransform2 = ((status == PlayBarManager.PlayerReadyStatus.Ready) ? this.readyGridContainer : this.notReadyGridContainer);
			UIPoolableGameObject uipoolableGameObject;
			UIPoolableGameObject uipoolableGameObject2;
			if (!this.rowTargets.TryGetValue(userId, out uipoolableGameObject) || !this.gridTargets.TryGetValue(userId, out uipoolableGameObject2))
			{
				DebugUtility.LogWarning(string.Format("Could not find {0} or {1} for {2}", "rowTargetGameObject", "gridTargetGameObject", userId), this);
				return;
			}
			RectTransform rectTransform3 = uipoolableGameObject.RectTransform;
			RectTransform rectTransform4 = uipoolableGameObject2.RectTransform;
			rectTransform3.SetParent(rectTransform);
			rectTransform4.SetParent(rectTransform2);
			if (status == PlayBarManager.PlayerReadyStatus.NotReady)
			{
				rectTransform3.SetAsLastSibling();
				rectTransform4.SetAsLastSibling();
			}
			else if (status == PlayBarManager.PlayerReadyStatus.Ready)
			{
				rectTransform3.SetAsFirstSibling();
				rectTransform4.SetAsFirstSibling();
			}
			RectTransform target = (this.UseGrid ? rectTransform4 : rectTransform3);
			RectTransform rectTransform5 = target.parent as RectTransform;
			this.MarkContainerForLayoutUpdate(rectTransform5);
			this.usersWithParentChanges.Add(userId);
			this.pendingTweens[userId] = delegate
			{
				UIPlayBarUserPortrait uiplayBarUserPortrait;
				if (!this.playBarUserPortraits.TryGetValue(userId, out uiplayBarUserPortrait) || !uiplayBarUserPortrait || !target)
				{
					DebugUtility.LogWarning(string.Format("Could not find {0} or {1} for {2}", "portrait", "target", userId), this);
					return;
				}
				uiplayBarUserPortrait.SetTargetAndTweenPositionAndSizeDelta(target, tweenPositionAndSizeDeltaComplete);
			};
		}

		// Token: 0x060009F6 RID: 2550 RVA: 0x0002E610 File Offset: 0x0002C810
		private void SpawnPlayBarUserPortrait(int userId)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SpawnPlayBarUserPortrait", new object[] { userId });
			}
			PoolManagerT instance = MonoBehaviourSingleton<PoolManagerT>.Instance;
			UIPoolableGameObject uipoolableGameObject = this.targetSource;
			Transform transform = this.notReadyRowContainer;
			UIPoolableGameObject uipoolableGameObject2 = instance.Spawn<UIPoolableGameObject>(uipoolableGameObject, default(Vector3), default(Quaternion), transform);
			PoolManagerT instance2 = MonoBehaviourSingleton<PoolManagerT>.Instance;
			UIPoolableGameObject uipoolableGameObject3 = this.targetSource;
			transform = this.notReadyGridContainer;
			UIPoolableGameObject uipoolableGameObject4 = instance2.Spawn<UIPoolableGameObject>(uipoolableGameObject3, default(Vector3), default(Quaternion), transform);
			PoolManagerT instance3 = MonoBehaviourSingleton<PoolManagerT>.Instance;
			UIPlayBarUserPortrait uiplayBarUserPortrait = this.portraitPrefab;
			transform = this.playBarUserPortraitsContainer;
			UIPlayBarUserPortrait uiplayBarUserPortrait2 = instance3.Spawn<UIPlayBarUserPortrait>(uiplayBarUserPortrait, default(Vector3), default(Quaternion), transform);
			this.rowTargets.Add(userId, uipoolableGameObject2);
			this.gridTargets.Add(userId, uipoolableGameObject4);
			this.playBarUserPortraits.Add(userId, uiplayBarUserPortrait2);
			uiplayBarUserPortrait2.Initialize(userId, this.UseGrid ? uipoolableGameObject4.RectTransform : uipoolableGameObject2.RectTransform, true);
			if (this.playBarUserPortraits.Count > 1)
			{
				MonoBehaviourSingleton<UiAudioManager>.Instance.PlayUiAudio(NormalUiSoundType.PlayBarUserPortraitJoin);
			}
		}

		// Token: 0x060009F7 RID: 2551 RVA: 0x0002E724 File Offset: 0x0002C924
		private void DespawnPlayBarUserPortrait(int userId)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "DespawnPlayBarUserPortrait", new object[] { userId });
			}
			this.playBarUserPortraits[userId].ShrinkAwayAndDespawn();
			this.playBarUserPortraits.Remove(userId);
			MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn<UIPoolableGameObject>(this.rowTargets[userId]);
			MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn<UIPoolableGameObject>(this.gridTargets[userId]);
			this.rowTargets.Remove(userId);
			this.gridTargets.Remove(userId);
			this.pendingTweens.Remove(userId);
			if (this.playBarUserPortraits.Count >= 1)
			{
				MonoBehaviourSingleton<UiAudioManager>.Instance.PlayUiAudio(NormalUiSoundType.PlayBarUserPortraitLeft);
			}
		}

		// Token: 0x060009F8 RID: 2552 RVA: 0x0002E7E0 File Offset: 0x0002C9E0
		private void MarkContainerForLayoutUpdate(RectTransform container)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "MarkContainerForLayoutUpdate", new object[] { container.DebugSafeName(true) });
			}
			if (container == null)
			{
				return;
			}
			this.dirtyContainers.Add(container);
			if (this.layoutUpdateCoroutine == null)
			{
				this.layoutUpdateCoroutine = base.StartCoroutine(this.ProcessLayoutUpdates());
			}
		}

		// Token: 0x060009F9 RID: 2553 RVA: 0x0002E841 File Offset: 0x0002CA41
		private IEnumerator ProcessLayoutUpdates()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ProcessLayoutUpdates", Array.Empty<object>());
			}
			yield return null;
			foreach (RectTransform rectTransform in this.dirtyContainers)
			{
				if (rectTransform)
				{
					LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
				}
			}
			foreach (KeyValuePair<int, Action> keyValuePair in this.pendingTweens)
			{
				Action value = keyValuePair.Value;
				if (value != null)
				{
					value();
				}
			}
			if (this.usersWithParentChanges.Count > 0)
			{
				this.TweenPortraitsWithoutParentChanges();
			}
			this.dirtyContainers.Clear();
			this.pendingTweens.Clear();
			this.usersWithParentChanges.Clear();
			this.layoutUpdateCoroutine = null;
			yield break;
		}

		// Token: 0x060009FA RID: 2554 RVA: 0x0002E850 File Offset: 0x0002CA50
		private void TweenPortraitsWithoutParentChanges()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "TweenPortraitsWithoutParentChanges", Array.Empty<object>());
			}
			foreach (KeyValuePair<int, UIPlayBarUserPortrait> keyValuePair in this.playBarUserPortraits)
			{
				int num;
				UIPlayBarUserPortrait uiplayBarUserPortrait;
				keyValuePair.Deconstruct(out num, out uiplayBarUserPortrait);
				int num2 = num;
				UIPlayBarUserPortrait uiplayBarUserPortrait2 = uiplayBarUserPortrait;
				if (!this.usersWithParentChanges.Contains(num2) && uiplayBarUserPortrait2 && !uiplayBarUserPortrait2.IsAtTargetPositionAndSize())
				{
					Dictionary<int, UIPoolableGameObject> dictionary = (this.UseGrid ? this.gridTargets : this.rowTargets);
					UIPoolableGameObject uipoolableGameObject;
					if (dictionary.TryGetValue(num2, out uipoolableGameObject))
					{
						RectTransform rectTransform = dictionary[num2].RectTransform;
						uiplayBarUserPortrait2.SetTargetAndTweenPositionAndSizeDelta(rectTransform, false);
					}
				}
			}
		}

		// Token: 0x060009FB RID: 2555 RVA: 0x0002E920 File Offset: 0x0002CB20
		private void ScreenshotStopped()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ScreenshotStopped", Array.Empty<object>());
			}
			this.menuBlockingObjects.Remove(MonoBehaviourSingleton<ScreenshotAPI>.Instance);
			this.UpdateVisibility();
		}

		// Token: 0x060009FC RID: 2556 RVA: 0x0002E951 File Offset: 0x0002CB51
		private void ScreenshotStarted()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ScreenshotStarted", Array.Empty<object>());
			}
			this.menuBlockingObjects.Add(MonoBehaviourSingleton<ScreenshotAPI>.Instance);
			this.UpdateVisibility();
		}

		// Token: 0x060009FD RID: 2557 RVA: 0x0002E982 File Offset: 0x0002CB82
		private void MenuOpened()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "MenuOpened", Array.Empty<object>());
			}
			this.menuBlockingObjects.Add(MonoBehaviourSingleton<UIScreenManager>.Instance);
			this.UpdateVisibility();
		}

		// Token: 0x060009FE RID: 2558 RVA: 0x0002E9B3 File Offset: 0x0002CBB3
		private void MenuClosed()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "MenuClosed", Array.Empty<object>());
			}
			this.menuBlockingObjects.Remove(MonoBehaviourSingleton<UIScreenManager>.Instance);
			this.UpdateVisibility();
		}

		// Token: 0x060009FF RID: 2559 RVA: 0x0002E9E4 File Offset: 0x0002CBE4
		private void UpdateVisibility()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "UpdateVisibility", Array.Empty<object>());
			}
			if (this.menuBlockingObjects.Count > 0)
			{
				this.rootCanvas.enabled = false;
				return;
			}
			bool flag = false;
			bool flag2 = false;
			if (MatchmakingClientController.Instance && MatchmakingClientController.Instance.LocalMatch != null)
			{
				MatchData matchData = MatchmakingClientController.Instance.LocalMatch.GetMatchData();
				flag2 = matchData.IsEditSession;
				flag = matchData.IsPlaytest();
			}
			this.rootCanvas.enabled = flag2 || flag;
		}

		// Token: 0x06000A00 RID: 2560 RVA: 0x0002EA6C File Offset: 0x0002CC6C
		private void EnterCreator()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "EnterCreator", Array.Empty<object>());
			}
			this.playObject.SetActive(true);
			this.stopObject.SetActive(false);
			this.UpdateLocalOption();
			this.UpdateSprites();
			this.HandleTimerUpdated();
		}

		// Token: 0x06000A01 RID: 2561 RVA: 0x0002EABC File Offset: 0x0002CCBC
		private void EnterGameplay()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "EnterGameplay", Array.Empty<object>());
			}
			this.playObject.SetActive(false);
			this.stopObject.SetActive(true);
			this.UpdateLocalOption();
			this.UpdateSprites();
			this.HandleTimerUpdated();
		}

		// Token: 0x06000A02 RID: 2562 RVA: 0x0002EB0C File Offset: 0x0002CD0C
		private void UpdateSprites()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "UpdateSprites", Array.Empty<object>());
			}
			this.leftEndcapImage.sprite = (MonoBehaviourSingleton<GameplayManager>.Instance.IsPlaying ? this.playSprite : this.creatorSprite);
			this.rightEndcapImage.sprite = (MonoBehaviourSingleton<GameplayManager>.Instance.IsPlaying ? this.creatorSprite : this.playSprite);
		}

		// Token: 0x06000A03 RID: 2563 RVA: 0x0002EB7C File Offset: 0x0002CD7C
		private void UpdateLocalOption()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "UpdateLocalOption", Array.Empty<object>());
			}
			PlayBarManager.PlayOption localPlayOption = NetworkBehaviourSingleton<PlayBarManager>.Instance.GetLocalPlayOption();
			this.playOptionButton.gameObject.SetActive(localPlayOption > PlayBarManager.PlayOption.None);
			switch (localPlayOption)
			{
			case PlayBarManager.PlayOption.None:
				return;
			case PlayBarManager.PlayOption.PlayWithParty:
				this.playOptionTooltip.SetTooltip("Play now (Party)");
				return;
			case PlayBarManager.PlayOption.PlaySolo:
				this.playOptionTooltip.SetTooltip("Play now (Solo)");
				return;
			case PlayBarManager.PlayOption.StopSolo:
				this.playOptionTooltip.SetTooltip("Return now (Solo)");
				return;
			case PlayBarManager.PlayOption.StopWithParty:
				this.playOptionTooltip.SetTooltip("Return now (Party)");
				return;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}

		// Token: 0x06000A04 RID: 2564 RVA: 0x0002EC28 File Offset: 0x0002CE28
		private void HandleTimerUpdated()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "HandleTimerUpdated", Array.Empty<object>());
			}
			bool isTimerStarted = NetworkBehaviourSingleton<PlayBarManager>.Instance.IsTimerStarted;
			if (this.previousIsTimerStarted != isTimerStarted)
			{
				MonoBehaviourSingleton<UiAudioManager>.Instance.PlayUiAudio(isTimerStarted ? NormalUiSoundType.TimerStarted : NormalUiSoundType.TimerEnded);
			}
			this.timerText.enabled = isTimerStarted;
			this.previousIsTimerStarted = isTimerStarted;
			if (isTimerStarted)
			{
				this.timerTextStartedTweenCollection.Tween();
			}
		}

		// Token: 0x06000A05 RID: 2565 RVA: 0x0002EC94 File Offset: 0x0002CE94
		private void UpdateText()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "UpdateText", Array.Empty<object>());
			}
			this.readyCountText.SetText("{0}", (float)this.readyCount);
			this.notReadyCountText.SetText("{0}", (float)this.notReadyCount);
		}

		// Token: 0x06000A06 RID: 2566 RVA: 0x0002ECE7 File Offset: 0x0002CEE7
		private void HandleButtonClicked()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "HandleButtonClicked", Array.Empty<object>());
			}
			NetworkBehaviourSingleton<PlayBarManager>.Instance.PlayHit();
			this.playButtonPressedTweenCollection.Tween();
		}

		// Token: 0x06000A07 RID: 2567 RVA: 0x0002ED16 File Offset: 0x0002CF16
		private void HandlePlayOptionButtonClicked()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "HandlePlayOptionButtonClicked", Array.Empty<object>());
			}
			NetworkBehaviourSingleton<PlayBarManager>.Instance.ExecuteLocalPlayOption();
			this.playOptionButtonPressedTweenCollection.Tween();
		}

		// Token: 0x06000A08 RID: 2568 RVA: 0x0002ED48 File Offset: 0x0002CF48
		private void HandlePlayHotkey(InputAction.CallbackContext callbackContext)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "HandlePlayHotkey", new object[] { callbackContext });
			}
			if (!this.playButton.IsInteractable())
			{
				return;
			}
			bool flag = false;
			bool flag2 = false;
			if (MatchmakingClientController.Instance && MatchmakingClientController.Instance.LocalMatch != null)
			{
				MatchData matchData = MatchmakingClientController.Instance.LocalMatch.GetMatchData();
				flag2 = matchData.IsEditSession;
				flag = matchData.IsPlaytest();
			}
			if (flag2 || flag)
			{
				this.HandleButtonClicked();
			}
		}

		// Token: 0x06000A09 RID: 2569 RVA: 0x0002EDC8 File Offset: 0x0002CFC8
		private void HandleScreenSizeChanged()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "HandleScreenSizeChanged", Array.Empty<object>());
			}
			foreach (KeyValuePair<int, UIPlayBarUserPortrait> keyValuePair in this.playBarUserPortraits)
			{
				keyValuePair.Value.CancelPositionAndSizeTweens();
			}
			this.QueueAllPortraitsForTween();
		}

		// Token: 0x06000A0A RID: 2570 RVA: 0x0002EE40 File Offset: 0x0002D040
		private void QueueAllPortraitsForTween()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "QueueAllPortraitsForTween", Array.Empty<object>());
			}
			RectTransform rectTransform = (this.UseGrid ? this.readyGridContainer : this.readyRowContainer);
			RectTransform rectTransform2 = (this.UseGrid ? this.notReadyGridContainer : this.notReadyRowContainer);
			this.MarkContainerForLayoutUpdate(rectTransform);
			this.MarkContainerForLayoutUpdate(rectTransform2);
			foreach (KeyValuePair<int, UIPlayBarUserPortrait> keyValuePair in this.playBarUserPortraits)
			{
				int portraitUserId = keyValuePair.Key;
				UIPoolableGameObject uipoolableGameObject;
				if ((this.UseGrid ? this.gridTargets : this.rowTargets).TryGetValue(portraitUserId, out uipoolableGameObject))
				{
					RectTransform target = uipoolableGameObject.RectTransform;
					this.pendingTweens[portraitUserId] = delegate
					{
						UIPlayBarUserPortrait uiplayBarUserPortrait;
						if (this.playBarUserPortraits.TryGetValue(portraitUserId, out uiplayBarUserPortrait) && uiplayBarUserPortrait && target)
						{
							uiplayBarUserPortrait.SetTargetAndTweenPositionAndSizeDelta(target, false);
						}
					};
				}
			}
		}

		// Token: 0x04000821 RID: 2081
		private const int USER_CAP_BEFORE_GRID = 5;

		// Token: 0x04000822 RID: 2082
		private const float READY_STATUS_AUDIO_COOLDOWN = 0.5f;

		// Token: 0x04000823 RID: 2083
		[SerializeField]
		private bool usePortraits = true;

		// Token: 0x04000824 RID: 2084
		[SerializeField]
		private Canvas rootCanvas;

		// Token: 0x04000825 RID: 2085
		[SerializeField]
		private UIButton playButton;

		// Token: 0x04000826 RID: 2086
		[SerializeField]
		private UIButton playOptionButton;

		// Token: 0x04000827 RID: 2087
		[SerializeField]
		private TweenCollection playButtonPressedTweenCollection;

		// Token: 0x04000828 RID: 2088
		[SerializeField]
		private TweenCollection playOptionButtonPressedTweenCollection;

		// Token: 0x04000829 RID: 2089
		[SerializeField]
		private UITooltip stopButtonTooltip;

		// Token: 0x0400082A RID: 2090
		[SerializeField]
		private UITooltip playOptionTooltip;

		// Token: 0x0400082B RID: 2091
		[SerializeField]
		private TextMeshProUGUI timerText;

		// Token: 0x0400082C RID: 2092
		[SerializeField]
		private TweenCollection timerTextStartedTweenCollection;

		// Token: 0x0400082D RID: 2093
		[SerializeField]
		private TweenCollection timerTextUpdatedTweenCollection;

		// Token: 0x0400082E RID: 2094
		[SerializeField]
		private TextMeshProUGUI readyCountText;

		// Token: 0x0400082F RID: 2095
		[SerializeField]
		private TextMeshProUGUI notReadyCountText;

		// Token: 0x04000830 RID: 2096
		[SerializeField]
		private Image leftEndcapImage;

		// Token: 0x04000831 RID: 2097
		[SerializeField]
		private Image rightEndcapImage;

		// Token: 0x04000832 RID: 2098
		[SerializeField]
		private Sprite playSprite;

		// Token: 0x04000833 RID: 2099
		[SerializeField]
		private Sprite creatorSprite;

		// Token: 0x04000834 RID: 2100
		[SerializeField]
		private UIPoolableGameObject targetSource;

		// Token: 0x04000835 RID: 2101
		[SerializeField]
		private UIPlayBarUserPortrait portraitPrefab;

		// Token: 0x04000836 RID: 2102
		[SerializeField]
		private RectTransform playBarUserPortraitsContainer;

		// Token: 0x04000837 RID: 2103
		[SerializeField]
		private RectTransform notReadyRowContainer;

		// Token: 0x04000838 RID: 2104
		[SerializeField]
		private RectTransform notReadyGridContainer;

		// Token: 0x04000839 RID: 2105
		[SerializeField]
		private RectTransform readyRowContainer;

		// Token: 0x0400083A RID: 2106
		[SerializeField]
		private RectTransform readyGridContainer;

		// Token: 0x0400083B RID: 2107
		[SerializeField]
		private Color notReadyColor = Color.blue;

		// Token: 0x0400083C RID: 2108
		[SerializeField]
		private Color readyColor = Color.red;

		// Token: 0x0400083D RID: 2109
		[SerializeField]
		private Graphic[] colorSwaps;

		// Token: 0x0400083E RID: 2110
		[SerializeField]
		private GameObject playObject;

		// Token: 0x0400083F RID: 2111
		[SerializeField]
		private GameObject stopObject;

		// Token: 0x04000840 RID: 2112
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x04000841 RID: 2113
		private readonly Dictionary<int, UIPlayBarUserPortrait> playBarUserPortraits = new Dictionary<int, UIPlayBarUserPortrait>();

		// Token: 0x04000842 RID: 2114
		private readonly Dictionary<int, UIPoolableGameObject> rowTargets = new Dictionary<int, UIPoolableGameObject>();

		// Token: 0x04000843 RID: 2115
		private readonly Dictionary<int, UIPoolableGameObject> gridTargets = new Dictionary<int, UIPoolableGameObject>();

		// Token: 0x04000844 RID: 2116
		private readonly HashSet<Transform> notReadyContainerHashSet = new HashSet<Transform>();

		// Token: 0x04000845 RID: 2117
		private readonly HashSet<Transform> readyContainerHashSet = new HashSet<Transform>();

		// Token: 0x04000846 RID: 2118
		private readonly HashSet<global::UnityEngine.Object> menuBlockingObjects = new HashSet<global::UnityEngine.Object>();

		// Token: 0x04000847 RID: 2119
		private readonly HashSet<RectTransform> dirtyContainers = new HashSet<RectTransform>();

		// Token: 0x04000848 RID: 2120
		private readonly Dictionary<int, Action> pendingTweens = new Dictionary<int, Action>();

		// Token: 0x04000849 RID: 2121
		private readonly HashSet<int> usersWithParentChanges = new HashSet<int>();

		// Token: 0x0400084A RID: 2122
		private Coroutine layoutUpdateCoroutine;

		// Token: 0x0400084B RID: 2123
		private int readyCount;

		// Token: 0x0400084C RID: 2124
		private int notReadyCount;

		// Token: 0x0400084D RID: 2125
		private PlayerInputActions playerInputActions;

		// Token: 0x0400084E RID: 2126
		private int lastDisplayedTime = -1;

		// Token: 0x0400084F RID: 2127
		private bool previousIsTimerStarted;

		// Token: 0x04000850 RID: 2128
		private float lastReadyStatusAudioTime = -999f;
	}
}
