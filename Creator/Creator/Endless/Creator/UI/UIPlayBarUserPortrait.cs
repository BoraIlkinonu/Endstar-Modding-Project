using System;
using Endless.Gameplay.UI;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Endless.Creator.UI
{
	// Token: 0x02000267 RID: 615
	public class UIPlayBarUserPortrait : UIGameObject, IPoolableT
	{
		// Token: 0x17000142 RID: 322
		// (get) Token: 0x06000A18 RID: 2584 RVA: 0x0002F22C File Offset: 0x0002D42C
		// (set) Token: 0x06000A19 RID: 2585 RVA: 0x0002F234 File Offset: 0x0002D434
		public MonoBehaviour Prefab { get; set; }

		// Token: 0x17000143 RID: 323
		// (get) Token: 0x06000A1A RID: 2586 RVA: 0x0002ABC6 File Offset: 0x00028DC6
		public bool IsUi
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000144 RID: 324
		// (get) Token: 0x06000A1B RID: 2587 RVA: 0x0002F23D File Offset: 0x0002D43D
		// (set) Token: 0x06000A1C RID: 2588 RVA: 0x0002F245 File Offset: 0x0002D445
		public RectTransform Target { get; private set; }

		// Token: 0x06000A1D RID: 2589 RVA: 0x0002F250 File Offset: 0x0002D450
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.arrangementChangeHideTweenCollection.OnAllTweenCompleted.AddListener(new UnityAction(this.arrangementChangeDisplayTweenCollection.Tween));
			this.arrangementChangeHideTweenCollection.OnAllTweenCompleted.AddListener(new UnityAction(this.TweenPositionAndSizeDelta));
			this.despawnTweenCollection.OnAllTweenCompleted.AddListener(new UnityAction(this.Despawn));
		}

		// Token: 0x06000A1E RID: 2590 RVA: 0x0002F2CE File Offset: 0x0002D4CE
		public void OnSpawn()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnSpawn", Array.Empty<object>());
			}
		}

		// Token: 0x06000A1F RID: 2591 RVA: 0x0002F2E8 File Offset: 0x0002D4E8
		public void OnDespawn()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDespawn", Array.Empty<object>());
			}
			this.CancelAllTweens();
			this.tweenPositionAndSizeDeltaComplete = true;
		}

		// Token: 0x06000A20 RID: 2592 RVA: 0x0002F310 File Offset: 0x0002D510
		public void Initialize(int userId, RectTransform target, bool showHostAndParty = true)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Initialize", new object[]
				{
					userId,
					target.DebugSafeName(true),
					showHostAndParty
				});
			}
			this.userPortrait.Initialize(userId, showHostAndParty);
			this.Target = target;
			LayoutRebuilder.ForceRebuildLayoutImmediate(target.parent as RectTransform);
			base.RectTransform.position = target.position;
			base.RectTransform.sizeDelta = target.sizeDelta;
			this.initializeTweenCollection.Tween();
		}

		// Token: 0x06000A21 RID: 2593 RVA: 0x0002F3A4 File Offset: 0x0002D5A4
		public void SetTargetAndTweenPositionAndSizeDelta(RectTransform target, bool tweenPositionAndSizeDeltaComplete)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetTargetAndTweenPositionAndSizeDelta", new object[]
				{
					target.DebugSafeName(true),
					tweenPositionAndSizeDeltaComplete
				});
				DebugUtility.Log(target.DebugSafeName(true), target);
			}
			this.Target = target;
			base.transform.SetSiblingIndex(this.Target.GetSiblingIndex());
			this.tweenPositionAndSizeDeltaComplete = tweenPositionAndSizeDeltaComplete;
			this.TweenPositionAndSizeDelta();
		}

		// Token: 0x06000A22 RID: 2594 RVA: 0x0002F414 File Offset: 0x0002D614
		public void ChangeArrangement(RectTransform target)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ChangeArrangement", new object[] { target.DebugSafeName(true) });
			}
			this.Target = target;
			this.tweenPositionAndSizeDeltaComplete = false;
			float num = 0.25f * (float)base.transform.GetSiblingIndex();
			this.arrangementChangeDisplayTweenCollection.SetDelay(num);
			this.arrangementChangeHideTweenCollection.Tween();
		}

		// Token: 0x06000A23 RID: 2595 RVA: 0x0002F47C File Offset: 0x0002D67C
		public void ShrinkAwayAndDespawn()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ShrinkAwayAndDespawn", Array.Empty<object>());
			}
			this.despawnTweenCollection.Tween();
		}

		// Token: 0x06000A24 RID: 2596 RVA: 0x0002F4A4 File Offset: 0x0002D6A4
		public bool IsAtTargetPositionAndSize()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "IsAtTargetPositionAndSize", Array.Empty<object>());
			}
			if (!this.Target)
			{
				return true;
			}
			bool flag = Vector3.Distance(base.RectTransform.position, this.Target.position) < 1f;
			bool flag2 = Vector2.Distance(base.RectTransform.sizeDelta, this.Target.sizeDelta) < 1f;
			return flag && flag2;
		}

		// Token: 0x06000A25 RID: 2597 RVA: 0x0002F51F File Offset: 0x0002D71F
		public void CancelPositionAndSizeTweens()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "CancelPositionAndSizeTweens", Array.Empty<object>());
			}
			this.tweenPosition.Cancel();
			this.tweenSizeDelta.Cancel();
		}

		// Token: 0x06000A26 RID: 2598 RVA: 0x0002F550 File Offset: 0x0002D750
		private void TweenPositionAndSizeDelta()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "TweenPositionAndSizeDelta", Array.Empty<object>());
			}
			this.tweenPosition.To = this.Target.position;
			this.tweenSizeDelta.To = this.Target.sizeDelta;
			this.tweenPosition.Tween(this.tweenPositionAndSizeDeltaComplete ? new Action(this.onTweenPositionAndSizeDeltaCompleteTweenCollection.Tween) : null);
			this.tweenSizeDelta.Tween();
			this.tweenPositionAndSizeDeltaComplete = false;
		}

		// Token: 0x06000A27 RID: 2599 RVA: 0x0002F5DC File Offset: 0x0002D7DC
		private void CancelAllTweens()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "CancelAllTweens", Array.Empty<object>());
			}
			this.tweenPosition.Cancel();
			this.tweenSizeDelta.Cancel();
			this.initializeTweenCollection.Cancel();
			this.onTweenPositionAndSizeDeltaCompleteTweenCollection.Cancel();
			this.arrangementChangeHideTweenCollection.Cancel();
			this.arrangementChangeDisplayTweenCollection.Cancel();
			this.despawnTweenCollection.Cancel();
		}

		// Token: 0x06000A28 RID: 2600 RVA: 0x0002F64E File Offset: 0x0002D84E
		private void Despawn()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Despawn", Array.Empty<object>());
			}
			MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn<UIPlayBarUserPortrait>(this);
		}

		// Token: 0x0400085E RID: 2142
		[SerializeField]
		private UserPortrait userPortrait;

		// Token: 0x0400085F RID: 2143
		[SerializeField]
		private TweenPosition tweenPosition;

		// Token: 0x04000860 RID: 2144
		[SerializeField]
		private TweenSizeDelta tweenSizeDelta;

		// Token: 0x04000861 RID: 2145
		[SerializeField]
		private TweenCollection initializeTweenCollection;

		// Token: 0x04000862 RID: 2146
		[SerializeField]
		private TweenCollection onTweenPositionAndSizeDeltaCompleteTweenCollection;

		// Token: 0x04000863 RID: 2147
		[SerializeField]
		private TweenCollection arrangementChangeHideTweenCollection;

		// Token: 0x04000864 RID: 2148
		[SerializeField]
		private TweenCollection arrangementChangeDisplayTweenCollection;

		// Token: 0x04000865 RID: 2149
		[SerializeField]
		private TweenCollection despawnTweenCollection;

		// Token: 0x04000866 RID: 2150
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x04000867 RID: 2151
		private bool tweenPositionAndSizeDeltaComplete = true;
	}
}
