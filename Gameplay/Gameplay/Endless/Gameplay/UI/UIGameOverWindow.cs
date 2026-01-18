using System;
using System.Collections.Generic;
using Endless.Gameplay.Stats;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Endless.Shared.UI.Windows;
using Endless.Shared.Validation;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay.UI
{
	// Token: 0x02000418 RID: 1048
	public class UIGameOverWindow : UIGameObject, IUIWindow, IPoolableT, IValidatable
	{
		// Token: 0x17000542 RID: 1346
		// (get) Token: 0x06001A0A RID: 6666 RVA: 0x00077842 File Offset: 0x00075A42
		// (set) Token: 0x06001A0B RID: 6667 RVA: 0x0007784A File Offset: 0x00075A4A
		public MonoBehaviour Prefab { get; set; }

		// Token: 0x06001A0C RID: 6668 RVA: 0x00077854 File Offset: 0x00075A54
		public static UIGameOverWindow Open(UIGameOverWindow prefab, UIGameOverWindowModel model, RectTransform container)
		{
			UIGameOverWindow uigameOverWindow = MonoBehaviourSingleton<PoolManagerT>.Instance.Spawn<UIGameOverWindow>(prefab, default(Vector3), default(Quaternion), container);
			uigameOverWindow.RectTransform.SetAnchor(AnchorPresets.StretchAll, 0f, 0f, 0f, 0f);
			MonoBehaviourSingleton<UINewWindowManager>.Instance.RegisterWindow(uigameOverWindow);
			uigameOverWindow.Initialize(model);
			return uigameOverWindow;
		}

		// Token: 0x06001A0D RID: 6669 RVA: 0x000778B8 File Offset: 0x00075AB8
		public void Validate()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Validate", Array.Empty<object>());
			}
			DebugUtility.DebugIsNull("titleText", this.titleText, this);
			DebugUtility.DebugIsNull("descriptionText", this.descriptionText, this);
			DebugUtility.DebugIsNull("replayButton", this.replayButton, this);
			DebugUtility.DebugIsNull("endMatchButton", this.endMatchButton, this);
			DebugUtility.DebugIsNull("nextLevelButton", this.nextLevelButton, this);
			DebugUtility.DebugIsNull("statParent", this.statParent, this);
			DebugUtility.DebugIsNull("basicStatPrefab", this.basicStatPrefab, this);
			DebugUtility.DebugIsNull("perPlayerStatGridPrefab", this.perPlayerStatGridPrefab, this);
			DebugUtility.DebugIsNull("replayVoteDisplay", this.replayVoteDisplay, this);
			DebugUtility.DebugIsNull("endMatchVoteDisplay", this.endMatchVoteDisplay, this);
			DebugUtility.DebugIsNull("nextLevelVoteDisplay", this.nextLevelVoteDisplay, this);
			DebugUtility.DebugIsNull("replayVoteText", this.replayVoteText, this);
			DebugUtility.DebugIsNull("endMatchVoteText", this.endMatchVoteText, this);
			DebugUtility.DebugIsNull("nextLevelVoteText", this.nextLevelVoteText, this);
			DebugUtility.DebugIsNull("tweenHandler", this.tweenHandler, this);
		}

		// Token: 0x06001A0E RID: 6670 RVA: 0x000779EC File Offset: 0x00075BEC
		private void Initialize(UIGameOverWindowModel model)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Initialize", new object[] { model });
			}
			NetworkBehaviourSingleton<GameEndManager>.Instance.OnVoteMoved.AddListener(new UnityAction<int, GameEndManager.VoteType, GameEndManager.VoteType>(this.HandleVoteMoved));
			this.replayButton.onClick.AddListener(new UnityAction(NetworkBehaviourSingleton<GameEndManager>.Instance.ReplayVote));
			this.endMatchButton.onClick.AddListener(new UnityAction(NetworkBehaviourSingleton<GameEndManager>.Instance.EndGameVote));
			this.nextLevelButton.onClick.AddListener(new UnityAction(NetworkBehaviourSingleton<GameEndManager>.Instance.NextLevelVote));
			this.tweenHandler.PlayOpenTween(null);
			this.replayVoteDisplay.SetActive(false);
			this.endMatchVoteDisplay.SetActive(false);
			this.nextLevelVoteDisplay.SetActive(false);
			this.titleText.Value = model.Title;
			this.descriptionText.Value = model.Description;
			this.replayButton.gameObject.SetActive(model.ShowReplay);
			this.endMatchButton.gameObject.SetActive(model.ShowEndMatch);
			this.nextLevelButton.gameObject.SetActive(model.ShowNextLevel);
			this.statEntryCount = 0;
			bool flag = model.PerPlayerStats.Length != 0;
			for (int i = 0; i < model.BasicStats.Length; i++)
			{
				BasicStat basicStat = model.BasicStats[i];
				if (flag && basicStat.Order > 100)
				{
					this.SpawnPerPlayerGrid(model);
					flag = false;
				}
				BasicStatEntry basicStatEntry = MonoBehaviourSingleton<PoolManagerT>.Instance.Spawn<BasicStatEntry>(this.basicStatPrefab, default(Vector3), default(Quaternion), null);
				this.statParentLayoutables.AddChildLayoutItem(basicStatEntry.RectTransform, null);
				basicStatEntry.Initialize(basicStat);
				basicStatEntry.PlayDisplayTween((float)this.statEntryCount * 0.05f, null);
				this.basicStatEntryList.Add(basicStatEntry);
				this.statEntryCount++;
			}
			if (flag)
			{
				this.SpawnPerPlayerGrid(model);
			}
			this.statParentLayoutables.RequestLayout();
			this.layoutables.RequestLayout();
		}

		// Token: 0x06001A0F RID: 6671 RVA: 0x00077C05 File Offset: 0x00075E05
		public void Close()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Close", Array.Empty<object>());
			}
			if (!this.tweenHandler.PlayCloseTween(new Action(this.Despawn)))
			{
				return;
			}
			MonoBehaviourSingleton<UINewWindowManager>.Instance.UnregisterWindow(this);
		}

		// Token: 0x06001A10 RID: 6672 RVA: 0x00077C44 File Offset: 0x00075E44
		private void Despawn()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Despawn", Array.Empty<object>());
			}
			MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn<UIGameOverWindow>(this);
			NetworkBehaviourSingleton<GameEndManager>.Instance.OnVoteMoved.RemoveListener(new UnityAction<int, GameEndManager.VoteType, GameEndManager.VoteType>(this.HandleVoteMoved));
			this.replayButton.onClick.RemoveListener(new UnityAction(NetworkBehaviourSingleton<GameEndManager>.Instance.ReplayVote));
			this.endMatchButton.onClick.RemoveListener(new UnityAction(NetworkBehaviourSingleton<GameEndManager>.Instance.EndGameVote));
			this.nextLevelButton.onClick.RemoveListener(new UnityAction(NetworkBehaviourSingleton<GameEndManager>.Instance.NextLevelVote));
			for (int i = 0; i < this.basicStatEntryList.Count; i++)
			{
				this.statParentLayoutables.RemoveChildLayoutItem(this.basicStatEntryList[i].RectTransform);
			}
			this.basicStatEntryList.DespawnAllItemsAndClear<BasicStatEntry>();
			for (int j = 0; j < this.perPlayerStatGridList.Count; j++)
			{
				this.statParentLayoutables.RemoveChildLayoutItem(this.perPlayerStatGridList[j].RectTransform);
			}
			this.perPlayerStatGridList.DespawnAllItemsAndClear<UIPerPlayerStatGrid>();
			this.statGrid = null;
			this.statEntryCount = 0;
		}

		// Token: 0x06001A11 RID: 6673 RVA: 0x00077D78 File Offset: 0x00075F78
		private void HandleVoteMoved(int userId, GameEndManager.VoteType oldVoteType, GameEndManager.VoteType newVoteType)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "HandleVoteMoved", new object[] { userId, oldVoteType, newVoteType });
			}
			int count = NetworkBehaviourSingleton<GameEndManager>.Instance.EndMatchVotes.Count;
			int count2 = NetworkBehaviourSingleton<GameEndManager>.Instance.ReplayVotes.Count;
			int count3 = NetworkBehaviourSingleton<GameEndManager>.Instance.NextLevelVotes.Count;
			this.replayVoteDisplay.SetActive(count2 > 0);
			this.replayVoteText.Value = count2.ToString();
			this.endMatchVoteDisplay.SetActive(count > 0);
			this.endMatchVoteText.Value = count.ToString();
			this.nextLevelVoteDisplay.SetActive(count3 > 0);
			this.nextLevelVoteText.Value = count3.ToString();
		}

		// Token: 0x06001A12 RID: 6674 RVA: 0x00077E4C File Offset: 0x0007604C
		private void SpawnPerPlayerGrid(UIGameOverWindowModel model)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SpawnPerPlayerGrid", new object[] { model });
			}
			this.statGrid = MonoBehaviourSingleton<PoolManagerT>.Instance.Spawn<UIPerPlayerStatGrid>(this.perPlayerStatGridPrefab, default(Vector3), default(Quaternion), null);
			this.statGrid.transform.SetParent(this.statParent, false);
			this.statGrid.Initialize(model.PerPlayerStats);
			this.statGrid.PlayDisplayTween((float)this.statEntryCount * 0.05f, null);
			this.perPlayerStatGridList.Add(this.statGrid);
			this.statParentLayoutables.AddChildLayoutItem(this.statGrid.RectTransform, null);
			this.statEntryCount++;
		}

		// Token: 0x040014B6 RID: 5302
		private const float STAT_ENTRY_STAGGER_DELAY = 0.05f;

		// Token: 0x040014B7 RID: 5303
		[Header("UIGameOverWindow")]
		[SerializeField]
		private UIText titleText;

		// Token: 0x040014B8 RID: 5304
		[SerializeField]
		private UIText descriptionText;

		// Token: 0x040014B9 RID: 5305
		[SerializeField]
		private UIButton replayButton;

		// Token: 0x040014BA RID: 5306
		[SerializeField]
		private UIButton endMatchButton;

		// Token: 0x040014BB RID: 5307
		[SerializeField]
		private UIButton nextLevelButton;

		// Token: 0x040014BC RID: 5308
		[SerializeField]
		private RectTransform statParent;

		// Token: 0x040014BD RID: 5309
		[SerializeField]
		private InterfaceReference<IUIChildLayoutable>[] statParentLayoutables = Array.Empty<InterfaceReference<IUIChildLayoutable>>();

		// Token: 0x040014BE RID: 5310
		[SerializeField]
		private BasicStatEntry basicStatPrefab;

		// Token: 0x040014BF RID: 5311
		[SerializeField]
		private UIPerPlayerStatGrid perPlayerStatGridPrefab;

		// Token: 0x040014C0 RID: 5312
		[SerializeField]
		private GameObject replayVoteDisplay;

		// Token: 0x040014C1 RID: 5313
		[SerializeField]
		private GameObject endMatchVoteDisplay;

		// Token: 0x040014C2 RID: 5314
		[SerializeField]
		private GameObject nextLevelVoteDisplay;

		// Token: 0x040014C3 RID: 5315
		[SerializeField]
		private UIText replayVoteText;

		// Token: 0x040014C4 RID: 5316
		[SerializeField]
		private UIText endMatchVoteText;

		// Token: 0x040014C5 RID: 5317
		[SerializeField]
		private UIText nextLevelVoteText;

		// Token: 0x040014C6 RID: 5318
		[SerializeField]
		private UIWindowTweenHandler tweenHandler;

		// Token: 0x040014C7 RID: 5319
		[SerializeField]
		private InterfaceReference<IUILayoutable>[] layoutables = Array.Empty<InterfaceReference<IUILayoutable>>();

		// Token: 0x040014C8 RID: 5320
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x040014C9 RID: 5321
		private UIPerPlayerStatGrid statGrid;

		// Token: 0x040014CA RID: 5322
		private readonly List<BasicStatEntry> basicStatEntryList = new List<BasicStatEntry>();

		// Token: 0x040014CB RID: 5323
		private readonly List<UIPerPlayerStatGrid> perPlayerStatGridList = new List<UIPerPlayerStatGrid>();

		// Token: 0x040014CC RID: 5324
		private int statEntryCount;
	}
}
