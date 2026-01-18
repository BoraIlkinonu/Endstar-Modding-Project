using System;
using System.Collections;
using System.Collections.Generic;
using Endless.Shared.Debugging;
using UnityEngine;

namespace Endless.Shared.Tweens
{
	// Token: 0x020000AE RID: 174
	[DefaultExecutionOrder(-2147483648)]
	public class TweenManager : MonoBehaviourSingleton<TweenManager>
	{
		// Token: 0x170000C8 RID: 200
		// (get) Token: 0x060004B8 RID: 1208 RVA: 0x00014F6A File Offset: 0x0001316A
		public int ActiveTweenCount
		{
			get
			{
				return this.activeTweens.Count;
			}
		}

		// Token: 0x170000C9 RID: 201
		// (get) Token: 0x060004B9 RID: 1209 RVA: 0x00014F77 File Offset: 0x00013177
		public IReadOnlyList<TweenManager.ActiveTweenData> ActiveTweens
		{
			get
			{
				return this.activeTweens;
			}
		}

		// Token: 0x060004BA RID: 1210 RVA: 0x00014F80 File Offset: 0x00013180
		private void Update()
		{
			if (ExitManager.IsQuitting)
			{
				return;
			}
			for (int i = this.activeTweens.Count - 1; i >= 0; i--)
			{
				TweenManager.ActiveTweenData activeTweenData = this.activeTweens[i];
				if (!activeTweenData.Tween)
				{
					this.activeTweens.RemoveAt(i);
				}
				else
				{
					float deltaTime = TweenManager.GetDeltaTime(activeTweenData.Tween.TimeMode);
					this.UpdateTween(activeTweenData, deltaTime, i);
				}
			}
		}

		// Token: 0x060004BB RID: 1211 RVA: 0x00014FF0 File Offset: 0x000131F0
		public void RegisterTween(BaseTween tween)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "RegisterTween", new object[] { tween.DebugSafeName(true) });
			}
			if (!tween)
			{
				return;
			}
			if (this.IsTweening(tween))
			{
				this.CancelTween(tween);
			}
			TweenManager.ActiveTweenData activeTweenData = new TweenManager.ActiveTweenData
			{
				Tween = tween,
				ElapsedTime = 0f,
				IsDelaying = (tween.Delay > 0f)
			};
			this.activeTweens.Add(activeTweenData);
		}

		// Token: 0x060004BC RID: 1212 RVA: 0x00015070 File Offset: 0x00013270
		public void CancelTween(BaseTween tween)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "CancelTween", new object[] { tween.DebugSafeName(true) });
			}
			if (!tween)
			{
				return;
			}
			for (int i = this.activeTweens.Count - 1; i >= 0; i--)
			{
				if (this.activeTweens[i].Tween == tween)
				{
					this.activeTweens.RemoveAt(i);
					return;
				}
			}
		}

		// Token: 0x060004BD RID: 1213 RVA: 0x000150E8 File Offset: 0x000132E8
		public bool IsTweening(BaseTween tween)
		{
			if (!tween)
			{
				return false;
			}
			for (int i = 0; i < this.activeTweens.Count; i++)
			{
				if (this.activeTweens[i].Tween == tween)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060004BE RID: 1214 RVA: 0x00015134 File Offset: 0x00013334
		public void CancelAllTweens(GameObject target)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "CancelAllTweens", new object[] { target.DebugSafeName(true) });
			}
			if (target == null)
			{
				return;
			}
			for (int i = this.activeTweens.Count - 1; i >= 0; i--)
			{
				if (this.activeTweens[i].Tween != null && this.activeTweens[i].Tween.gameObject == target)
				{
					this.activeTweens.RemoveAt(i);
				}
			}
		}

		// Token: 0x060004BF RID: 1215 RVA: 0x000151C9 File Offset: 0x000133C9
		public ValueTween TweenValue(float duration, Action<float> onUpdate, Action onComplete = null, TweenTimeMode timeMode = TweenTimeMode.Scaled, AnimationCurve ease = null)
		{
			return this.TweenValue(0f, 1f, duration, onUpdate, onComplete, timeMode, ease);
		}

		// Token: 0x060004C0 RID: 1216 RVA: 0x000151E4 File Offset: 0x000133E4
		public ValueTween TweenValue(float from, float to, float duration, Action<float> onUpdate, Action onComplete = null, TweenTimeMode timeMode = TweenTimeMode.Scaled, AnimationCurve ease = null)
		{
			ValueTween valueTween = new ValueTween(null);
			Coroutine coroutine = base.StartCoroutine(this.TweenValueCoroutine(from, to, duration, onUpdate, onComplete, timeMode, ease, valueTween));
			valueTween.coroutine = coroutine;
			this.activeValueTweens.Add(valueTween);
			return valueTween;
		}

		// Token: 0x060004C1 RID: 1217 RVA: 0x00015225 File Offset: 0x00013425
		internal void RemoveValueTween(ValueTween valueTween)
		{
			this.activeValueTweens.Remove(valueTween);
		}

		// Token: 0x060004C2 RID: 1218 RVA: 0x00015234 File Offset: 0x00013434
		private void UpdateTween(TweenManager.ActiveTweenData tweenData, float deltaTime, int index)
		{
			tweenData.ElapsedTime += deltaTime;
			if (tweenData.ElapsedTime < tweenData.Tween.Delay)
			{
				tweenData.IsDelaying = true;
				tweenData.Tween.InternalUpdateProgress(0f, true, tweenData.ElapsedTime);
				return;
			}
			tweenData.IsDelaying = false;
			float num = tweenData.ElapsedTime - tweenData.Tween.Delay;
			float inSeconds = tweenData.Tween.InSeconds;
			if (num >= inSeconds)
			{
				tweenData.Tween.InternalUpdateProgress(1f, false, tweenData.ElapsedTime);
				tweenData.Tween.InternalOnTween(1f);
				this.CompleteTween(tweenData, index);
				return;
			}
			float num2 = num / inSeconds;
			tweenData.Tween.InternalUpdateProgress(num2, false, tweenData.ElapsedTime);
			tweenData.Tween.InternalOnTween(num2);
		}

		// Token: 0x060004C3 RID: 1219 RVA: 0x00015300 File Offset: 0x00013500
		private void CompleteTween(TweenManager.ActiveTweenData activeTweenData, int index)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "CompleteTween", new object[] { activeTweenData });
			}
			this.activeTweens.RemoveAt(index);
			BaseTween tween = activeTweenData.Tween;
			tween.InternalOnDone();
			if (tween.Loop)
			{
				tween.Tween();
			}
		}

		// Token: 0x060004C4 RID: 1220 RVA: 0x00015354 File Offset: 0x00013554
		private IEnumerator TweenValueCoroutine(float from, float to, float duration, Action<float> onUpdate, Action onComplete, TweenTimeMode timeMode, AnimationCurve ease, ValueTween valueTween)
		{
			float elapsed = 0f;
			while (elapsed < duration)
			{
				float deltaTime = TweenManager.GetDeltaTime(timeMode);
				elapsed += deltaTime;
				float num = Mathf.Clamp01(elapsed / duration);
				float num2 = ((ease != null) ? ease.Evaluate(num) : num);
				float num3 = Mathf.LerpUnclamped(from, to, num2);
				if (onUpdate != null)
				{
					onUpdate(num3);
				}
				yield return null;
			}
			if (onUpdate != null)
			{
				onUpdate(to);
			}
			if (onComplete != null)
			{
				onComplete();
			}
			valueTween.isComplete = true;
			this.RemoveValueTween(valueTween);
			yield break;
		}

		// Token: 0x060004C5 RID: 1221 RVA: 0x000153AB File Offset: 0x000135AB
		private static float GetDeltaTime(TweenTimeMode timeMode)
		{
			if (timeMode != TweenTimeMode.Scaled)
			{
				return Time.unscaledDeltaTime;
			}
			return Time.deltaTime;
		}

		// Token: 0x0400024B RID: 587
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x0400024C RID: 588
		private readonly List<TweenManager.ActiveTweenData> activeTweens = new List<TweenManager.ActiveTweenData>();

		// Token: 0x0400024D RID: 589
		private readonly List<ValueTween> activeValueTweens = new List<ValueTween>();

		// Token: 0x020000AF RID: 175
		public class ActiveTweenData
		{
			// Token: 0x060004C7 RID: 1223 RVA: 0x000153DC File Offset: 0x000135DC
			public override string ToString()
			{
				return string.Format("{{ {0}: {1}, {2}: {3}, {4}: {5} }}", new object[]
				{
					"Tween",
					this.Tween.GetType().Name,
					"ElapsedTime",
					this.ElapsedTime,
					"IsDelaying",
					this.IsDelaying
				});
			}

			// Token: 0x0400024E RID: 590
			public BaseTween Tween;

			// Token: 0x0400024F RID: 591
			public float ElapsedTime;

			// Token: 0x04000250 RID: 592
			public bool IsDelaying;
		}
	}
}
