using System;
using Endless.Shared.Debugging;
using Endless.Shared.Validation;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Endless.Shared.Tweens
{
	// Token: 0x020000B1 RID: 177
	public abstract class BaseTween : MonoBehaviour, IValidatable
	{
		// Token: 0x170000CC RID: 204
		// (get) Token: 0x060004CF RID: 1231 RVA: 0x0001556D File Offset: 0x0001376D
		// (set) Token: 0x060004D0 RID: 1232 RVA: 0x00015575 File Offset: 0x00013775
		protected bool VerboseLogging { get; set; }

		// Token: 0x170000CD RID: 205
		// (get) Token: 0x060004D1 RID: 1233 RVA: 0x0001557E File Offset: 0x0001377E
		// (set) Token: 0x060004D2 RID: 1234 RVA: 0x00015586 File Offset: 0x00013786
		public float CurrentProgress { get; private set; }

		// Token: 0x170000CE RID: 206
		// (get) Token: 0x060004D3 RID: 1235 RVA: 0x0001558F File Offset: 0x0001378F
		// (set) Token: 0x060004D4 RID: 1236 RVA: 0x00015597 File Offset: 0x00013797
		public float CurrentElapsedTime { get; internal set; }

		// Token: 0x170000CF RID: 207
		// (get) Token: 0x060004D5 RID: 1237 RVA: 0x000155A0 File Offset: 0x000137A0
		// (set) Token: 0x060004D6 RID: 1238 RVA: 0x000155A8 File Offset: 0x000137A8
		public bool IsDelaying { get; internal set; }

		// Token: 0x170000D0 RID: 208
		// (get) Token: 0x060004D7 RID: 1239 RVA: 0x000155B1 File Offset: 0x000137B1
		// (set) Token: 0x060004D8 RID: 1240 RVA: 0x000155D2 File Offset: 0x000137D2
		public GameObject Target
		{
			get
			{
				if (!this.target)
				{
					this.target = base.gameObject;
				}
				return this.target;
			}
			set
			{
				this.target = value;
			}
		}

		// Token: 0x170000D1 RID: 209
		// (get) Token: 0x060004D9 RID: 1241 RVA: 0x000155DB File Offset: 0x000137DB
		public bool IsTweening
		{
			get
			{
				return MonoBehaviourSingleton<TweenManager>.Instance.IsTweening(this);
			}
		}

		// Token: 0x170000D2 RID: 210
		// (get) Token: 0x060004DA RID: 1242 RVA: 0x000155E8 File Offset: 0x000137E8
		public float TotalDuration
		{
			get
			{
				return this.Delay + this.InSeconds;
			}
		}

		// Token: 0x170000D3 RID: 211
		// (get) Token: 0x060004DB RID: 1243 RVA: 0x000155F7 File Offset: 0x000137F7
		public float ScaledTotalDuration
		{
			get
			{
				if (Time.timeScale != 0f)
				{
					return this.TotalDuration / Time.timeScale;
				}
				return float.PositiveInfinity;
			}
		}

		// Token: 0x14000030 RID: 48
		// (add) Token: 0x060004DC RID: 1244 RVA: 0x00015618 File Offset: 0x00013818
		// (remove) Token: 0x060004DD RID: 1245 RVA: 0x00015650 File Offset: 0x00013850
		public event Action OnTweenInProgress;

		// Token: 0x060004DE RID: 1246 RVA: 0x00015685 File Offset: 0x00013885
		private void OnDestroy()
		{
			if (this.VerboseLogging)
			{
				Debug.Log("OnDestroy", this);
			}
			this.CleanupPendingHandler();
		}

		// Token: 0x060004DF RID: 1247 RVA: 0x000156A0 File Offset: 0x000138A0
		[ContextMenu("Validate")]
		public void Validate()
		{
			if (this.VerboseLogging)
			{
				Debug.Log("Validate", this);
			}
			DebugUtility.DebugIsNull("Target", this.Target, this);
		}

		// Token: 0x060004E0 RID: 1248
		public abstract void Tween();

		// Token: 0x060004E1 RID: 1249 RVA: 0x000156C8 File Offset: 0x000138C8
		public void Tween(Action onComplete)
		{
			BaseTween.<>c__DisplayClass42_0 CS$<>8__locals1 = new BaseTween.<>c__DisplayClass42_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.onComplete = onComplete;
			if (this.VerboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2} )", "Tween", "onComplete", CS$<>8__locals1.onComplete), this);
			}
			this.CleanupPendingHandler();
			this.pendingOnCompleteHandler = new UnityAction(CS$<>8__locals1.<Tween>g__OnCompleteHandler|0);
			this.OnTweenComplete.AddListener(this.pendingOnCompleteHandler);
			this.Tween();
		}

		// Token: 0x060004E2 RID: 1250 RVA: 0x00015740 File Offset: 0x00013940
		public virtual void OnTween(float interpolation)
		{
			if (this.VerboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2} )", "OnTween", "interpolation", interpolation), this);
			}
			Action onTweenInProgress = this.OnTweenInProgress;
			if (onTweenInProgress == null)
			{
				return;
			}
			onTweenInProgress();
		}

		// Token: 0x060004E3 RID: 1251 RVA: 0x0001577A File Offset: 0x0001397A
		public void OnDone()
		{
			if (this.VerboseLogging)
			{
				Debug.Log("OnDone", this);
			}
			this.OnTweenComplete.Invoke();
		}

		// Token: 0x060004E4 RID: 1252 RVA: 0x0001579A File Offset: 0x0001399A
		public void SetToStart()
		{
			if (this.VerboseLogging)
			{
				Debug.Log("SetToStart", this);
			}
			this.OnTween(0f);
		}

		// Token: 0x060004E5 RID: 1253 RVA: 0x000157BA File Offset: 0x000139BA
		public void SetToEnd()
		{
			if (this.VerboseLogging)
			{
				Debug.Log("SetToEnd", this);
			}
			this.OnTween(1f);
		}

		// Token: 0x060004E6 RID: 1254 RVA: 0x000157DC File Offset: 0x000139DC
		public void ForceDone(bool triggerOnDoneEvents = true)
		{
			if (this.VerboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2} )", "ForceDone", "triggerOnDoneEvents", triggerOnDoneEvents), this);
			}
			MonoBehaviourSingleton<TweenManager>.Instance.CancelTween(this);
			this.OnTween(1f);
			this.CurrentProgress = 1f;
			this.IsDelaying = false;
			this.CurrentElapsedTime = this.TotalDuration;
			if (triggerOnDoneEvents)
			{
				this.OnDone();
				return;
			}
			this.CleanupPendingHandler();
		}

		// Token: 0x060004E7 RID: 1255 RVA: 0x00015855 File Offset: 0x00013A55
		public void Cancel()
		{
			if (this.VerboseLogging)
			{
				Debug.Log("Cancel", this);
			}
			MonoBehaviourSingleton<TweenManager>.Instance.CancelTween(this);
			this.CleanupPendingHandler();
		}

		// Token: 0x060004E8 RID: 1256 RVA: 0x0001587C File Offset: 0x00013A7C
		protected float GetEasedValue(float interpolation)
		{
			if (this.VerboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2} )", "GetEasedValue", "interpolation", interpolation), this);
			}
			if (this.easeType != TweenEase.Custom)
			{
				return TweenEaseUtility.Interpolate(this.easeType, 0f, 1f, interpolation);
			}
			return this.customEase.Evaluate(interpolation);
		}

		// Token: 0x060004E9 RID: 1257 RVA: 0x000158DE File Offset: 0x00013ADE
		internal void InternalOnTween(float normalizedTime)
		{
			if (this.VerboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2} )", "InternalOnTween", "normalizedTime", normalizedTime), this);
			}
			this.OnTween(normalizedTime);
		}

		// Token: 0x060004EA RID: 1258 RVA: 0x0001590F File Offset: 0x00013B0F
		internal void InternalOnDone()
		{
			if (this.VerboseLogging)
			{
				Debug.Log("InternalOnDone", this);
			}
			this.CurrentProgress = 1f;
			this.IsDelaying = false;
			this.CurrentElapsedTime = this.TotalDuration;
			this.OnDone();
		}

		// Token: 0x060004EB RID: 1259 RVA: 0x00015948 File Offset: 0x00013B48
		private void CleanupPendingHandler()
		{
			if (this.VerboseLogging)
			{
				Debug.Log("CleanupPendingHandler", this);
			}
			if (this.pendingOnCompleteHandler != null)
			{
				this.OnTweenComplete.RemoveListener(this.pendingOnCompleteHandler);
				this.pendingOnCompleteHandler = null;
			}
		}

		// Token: 0x060004EC RID: 1260 RVA: 0x00015980 File Offset: 0x00013B80
		internal void InternalUpdateProgress(float progress, bool isDelaying, float elapsedTime)
		{
			if (this.VerboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2}, {3}: {4}, {5}: {6} )", new object[] { "InternalUpdateProgress", "progress", progress, "isDelaying", isDelaying, "elapsedTime", elapsedTime }), this);
			}
			this.CurrentProgress = progress;
			this.IsDelaying = isDelaying;
			this.CurrentElapsedTime = elapsedTime;
		}

		// Token: 0x0400025D RID: 605
		private const float INTERPOLATION_TO = 1f;

		// Token: 0x0400025E RID: 606
		[Tooltip("The duration of the tween in seconds")]
		public float InSeconds = 0.25f;

		// Token: 0x0400025F RID: 607
		[Tooltip("Delay before tween starts, in seconds")]
		public float Delay;

		// Token: 0x04000260 RID: 608
		[Tooltip("Whether the tween should loop continuously")]
		public bool Loop;

		// Token: 0x04000261 RID: 609
		[Tooltip("Time mode for this tween")]
		public TweenTimeMode TimeMode = TweenTimeMode.Unscaled;

		// Token: 0x04000262 RID: 610
		[Tooltip("Replaces the From value with the current value when tween starts")]
		public bool FromIsCurrentValue = true;

		// Token: 0x04000263 RID: 611
		[Tooltip("Triggered when the tween completes")]
		public UnityEvent OnTweenComplete = new UnityEvent();

		// Token: 0x04000264 RID: 612
		[Tooltip("Defaults to self if null")]
		[SerializeField]
		protected GameObject target;

		// Token: 0x04000265 RID: 613
		[SerializeField]
		private TweenEase easeType = TweenEase.Custom;

		// Token: 0x04000266 RID: 614
		[FormerlySerializedAs("Ease")]
		[SerializeField]
		private AnimationCurve customEase = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

		// Token: 0x0400026B RID: 619
		private UnityAction pendingOnCompleteHandler;
	}
}
