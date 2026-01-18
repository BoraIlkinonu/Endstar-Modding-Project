using System;
using Endless.Shared.Debugging;
using Endless.Shared.Validation;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared.Tweens
{
	// Token: 0x020000B9 RID: 185
	public class TweenCollection : MonoBehaviour, IValidatable
	{
		// Token: 0x170000D4 RID: 212
		// (get) Token: 0x0600051D RID: 1309 RVA: 0x000167A0 File Offset: 0x000149A0
		public BaseTween[] Tweens
		{
			get
			{
				return this.tweens;
			}
		}

		// Token: 0x170000D5 RID: 213
		// (get) Token: 0x0600051E RID: 1310 RVA: 0x000167A8 File Offset: 0x000149A8
		public bool IsTweening
		{
			get
			{
				return this.IsAnyTweening();
			}
		}

		// Token: 0x0600051F RID: 1311 RVA: 0x000167B0 File Offset: 0x000149B0
		private void OnDestroy()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDestroy", Array.Empty<object>());
			}
			this.CleanupTweenListeners();
			this.pendingOnCompleteCallback = null;
		}

		// Token: 0x06000520 RID: 1312 RVA: 0x000167D8 File Offset: 0x000149D8
		[ContextMenu("Validate")]
		public void Validate()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Validate", Array.Empty<object>());
			}
			BaseTween[] array = this.tweens;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] == null)
				{
					Debug.LogError("There is a null entry on " + base.gameObject.name + "!", this);
				}
			}
		}

		// Token: 0x06000521 RID: 1313 RVA: 0x00016840 File Offset: 0x00014A40
		public void ValidateForNumberOfTweens(int numberOfTweens = 1)
		{
			if (this.tweens.Length < numberOfTweens)
			{
				Debug.LogException(new Exception(string.Format("{0} requires at least {1} {2}!", "TweenCollection", numberOfTweens, "tweens")), this);
				return;
			}
			int num = 0;
			BaseTween[] array = this.tweens;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] != null)
				{
					num++;
				}
			}
			if (num < numberOfTweens)
			{
				Debug.LogException(new Exception(string.Format("{0} requires at least {1} {2}!", "TweenCollection", numberOfTweens, "tweens")), this);
			}
		}

		// Token: 0x06000522 RID: 1314 RVA: 0x000168CD File Offset: 0x00014ACD
		public void Tween()
		{
			if (this.verboseLogging)
			{
				Debug.Log("Tween | " + base.gameObject.name, this);
			}
			if (ExitManager.IsQuitting)
			{
				return;
			}
			this.Tween(null);
		}

		// Token: 0x06000523 RID: 1315 RVA: 0x00016904 File Offset: 0x00014B04
		public void Tween(Action onTweenComplete = null)
		{
			if (this.verboseLogging)
			{
				Debug.Log("Tween ( onTweenComplete: " + onTweenComplete.DebugIsNull() + " ) | " + base.gameObject.name, this);
			}
			if (ExitManager.IsQuitting)
			{
				return;
			}
			if (this.IsTweening)
			{
				this.Cancel();
			}
			this.tweensCompletedCount = 0;
			this.activeTweensCount = 0;
			this.pendingOnCompleteCallback = onTweenComplete;
			foreach (BaseTween baseTween in this.tweens)
			{
				if (baseTween)
				{
					this.activeTweensCount++;
					baseTween.OnTweenComplete.AddListener(new UnityAction(this.OnChildTweenCompleted));
					baseTween.Tween();
				}
			}
			if (this.activeTweensCount == 0)
			{
				if (this.verboseLogging)
				{
					Debug.Log("Tween | No active tweens, invoking callbacks immediately | " + base.gameObject.name, this);
				}
				this.OnAllTweenCompleted.Invoke();
				Action action = this.pendingOnCompleteCallback;
				if (action != null)
				{
					action();
				}
				this.pendingOnCompleteCallback = null;
			}
		}

		// Token: 0x06000524 RID: 1316 RVA: 0x00016A04 File Offset: 0x00014C04
		private void OnChildTweenCompleted()
		{
			this.tweensCompletedCount++;
			if (this.verboseLogging)
			{
				Debug.Log(string.Format("{0} | {1}/{2} | {3}", new object[]
				{
					"OnChildTweenCompleted",
					this.tweensCompletedCount,
					this.activeTweensCount,
					base.gameObject.name
				}), this);
			}
			if (this.tweensCompletedCount >= this.activeTweensCount)
			{
				this.CleanupTweenListeners();
				this.OnAllTweenCompleted.Invoke();
				Action action = this.pendingOnCompleteCallback;
				if (action != null)
				{
					action();
				}
				this.pendingOnCompleteCallback = null;
			}
		}

		// Token: 0x06000525 RID: 1317 RVA: 0x00016AA8 File Offset: 0x00014CA8
		private void CleanupTweenListeners()
		{
			if (this.verboseLogging)
			{
				Debug.Log("CleanupTweenListeners | " + base.gameObject.name, this);
			}
			foreach (BaseTween baseTween in this.tweens)
			{
				if (baseTween)
				{
					baseTween.OnTweenComplete.RemoveListener(new UnityAction(this.OnChildTweenCompleted));
				}
			}
		}

		// Token: 0x06000526 RID: 1318 RVA: 0x00016B10 File Offset: 0x00014D10
		public void Cancel()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Cancel", Array.Empty<object>());
			}
			if (ExitManager.IsQuitting)
			{
				return;
			}
			this.CleanupTweenListeners();
			this.pendingOnCompleteCallback = null;
			foreach (BaseTween baseTween in this.tweens)
			{
				if (baseTween != null && MonoBehaviourSingleton<TweenManager>.Instance.IsTweening(baseTween))
				{
					MonoBehaviourSingleton<TweenManager>.Instance.CancelTween(baseTween);
				}
			}
		}

		// Token: 0x06000527 RID: 1319 RVA: 0x00016B84 File Offset: 0x00014D84
		public void ForceDone(bool triggerOnDoneEvents = true)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ForceDone", new object[] { triggerOnDoneEvents });
			}
			if (ExitManager.IsQuitting)
			{
				return;
			}
			this.CleanupTweenListeners();
			foreach (BaseTween baseTween in this.tweens)
			{
				if (baseTween != null)
				{
					baseTween.ForceDone(triggerOnDoneEvents);
				}
			}
			if (triggerOnDoneEvents)
			{
				this.OnAllTweenCompleted.Invoke();
				Action action = this.pendingOnCompleteCallback;
				if (action != null)
				{
					action();
				}
			}
			this.pendingOnCompleteCallback = null;
		}

		// Token: 0x06000528 RID: 1320 RVA: 0x00016C10 File Offset: 0x00014E10
		public bool IsAnyTweening()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "IsAnyTweening", Array.Empty<object>());
			}
			if (ExitManager.IsQuitting)
			{
				return false;
			}
			foreach (BaseTween baseTween in this.tweens)
			{
				if (baseTween && MonoBehaviourSingleton<TweenManager>.Instance.IsTweening(baseTween))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000529 RID: 1321 RVA: 0x00016C70 File Offset: 0x00014E70
		public void OnTween(float interpolation)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnTween", new object[] { interpolation });
			}
			foreach (BaseTween baseTween in this.tweens)
			{
				if (baseTween)
				{
					baseTween.OnTween(interpolation);
				}
			}
		}

		// Token: 0x0600052A RID: 1322 RVA: 0x00016CC8 File Offset: 0x00014EC8
		public void SetInSeconds(float inSeconds)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetInSeconds", new object[] { inSeconds });
			}
			foreach (BaseTween baseTween in this.tweens)
			{
				if (baseTween != null)
				{
					baseTween.InSeconds = inSeconds;
				}
			}
		}

		// Token: 0x0600052B RID: 1323 RVA: 0x00016D20 File Offset: 0x00014F20
		public void SetDelay(float delay)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetDelay", new object[] { delay });
			}
			foreach (BaseTween baseTween in this.tweens)
			{
				if (baseTween)
				{
					baseTween.Delay = delay;
				}
			}
		}

		// Token: 0x0600052C RID: 1324 RVA: 0x00016D77 File Offset: 0x00014F77
		public void SetToStart()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetToStart", Array.Empty<object>());
			}
			if (ExitManager.IsQuitting)
			{
				return;
			}
			this.OnTween(0f);
		}

		// Token: 0x0600052D RID: 1325 RVA: 0x00016DA4 File Offset: 0x00014FA4
		public void SetToEnd()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetToEnd", Array.Empty<object>());
			}
			if (ExitManager.IsQuitting)
			{
				return;
			}
			this.OnTween(1f);
		}

		// Token: 0x0400029A RID: 666
		public UnityEvent OnAllTweenCompleted = new UnityEvent();

		// Token: 0x0400029B RID: 667
		[SerializeField]
		private BaseTween[] tweens = Array.Empty<BaseTween>();

		// Token: 0x0400029C RID: 668
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x0400029D RID: 669
		private int tweensCompletedCount;

		// Token: 0x0400029E RID: 670
		private int activeTweensCount;

		// Token: 0x0400029F RID: 671
		private Action pendingOnCompleteCallback;
	}
}
