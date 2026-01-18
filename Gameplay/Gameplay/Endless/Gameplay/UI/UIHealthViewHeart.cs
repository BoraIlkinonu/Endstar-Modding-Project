using System;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Endless.Gameplay.UI
{
	// Token: 0x020003C2 RID: 962
	public class UIHealthViewHeart : UIGameObject, IPoolableT
	{
		// Token: 0x17000508 RID: 1288
		// (get) Token: 0x0600187A RID: 6266 RVA: 0x00071C04 File Offset: 0x0006FE04
		// (set) Token: 0x0600187B RID: 6267 RVA: 0x00071C0C File Offset: 0x0006FE0C
		public MonoBehaviour Prefab { get; set; }

		// Token: 0x17000509 RID: 1289
		// (get) Token: 0x0600187C RID: 6268 RVA: 0x00017586 File Offset: 0x00015786
		public bool IsUi
		{
			get
			{
				return true;
			}
		}

		// Token: 0x0600187D RID: 6269 RVA: 0x00071C18 File Offset: 0x0006FE18
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.beforeDespawnTweens.OnAllTweenCompleted.AddListener(new UnityAction(this.Despawn));
			this.defaultColor = this.heartImage.color;
		}

		// Token: 0x0600187E RID: 6270 RVA: 0x00071C6A File Offset: 0x0006FE6A
		public void OnSpawn()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnSpawn", Array.Empty<object>());
			}
		}

		// Token: 0x0600187F RID: 6271 RVA: 0x00071C84 File Offset: 0x0006FE84
		public void OnDespawn()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDespawn", Array.Empty<object>());
			}
			this.ForceDoneAnyTweensInProgress();
			this.heartImage.color = this.defaultColor;
		}

		// Token: 0x06001880 RID: 6272 RVA: 0x00071CB8 File Offset: 0x0006FEB8
		public void Initialize(int healthPoint, int delayIndex)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Initialize", new object[] { healthPoint, delayIndex });
			}
			this.isLeft = healthPoint == 0 || healthPoint % 2 == 0;
			this.heartImage.rectTransform.SetPivot(this.isLeft ? PivotPresets.MiddleRight : PivotPresets.MiddleLeft);
			BaseTween[] tweens = this.spawnTweens.Tweens;
			for (int i = 0; i < tweens.Length; i++)
			{
				tweens[i].Delay = (float)delayIndex * this.spawnTweenDelay;
			}
			this.spawnTweens.Tween();
		}

		// Token: 0x06001881 RID: 6273 RVA: 0x00071D54 File Offset: 0x0006FF54
		public void Toggle(bool state, int delayIndex)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Toggle", new object[] { state, delayIndex });
			}
			this.ForceDoneAnyTweensInProgress();
			BaseTween[] array;
			if (state)
			{
				array = this.toggledOnTweens.Tweens;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].Delay = (float)delayIndex * this.toggledOnTweenDelay;
				}
				this.toggledOnTweens.Tween();
				return;
			}
			this.toggledOffAnchoredPositionShaker.Delay = (float)delayIndex * this.toggledOffTweenDelay;
			this.toggledOffAnchoredPositionShaker.Shake();
			array = this.toggledOffTweens.Tweens;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Delay = (float)delayIndex * this.toggledOffTweenDelay;
			}
			this.toggledOffTweens.Tween();
		}

		// Token: 0x06001882 RID: 6274 RVA: 0x00071E24 File Offset: 0x00070024
		public void TweenAwayAndDespawn(int delayIndex)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "TweenAwayAndDespawn", new object[] { delayIndex });
			}
			this.ForceDoneAnyTweensInProgress();
			BaseTween[] tweens = this.beforeDespawnTweens.Tweens;
			for (int i = 0; i < tweens.Length; i++)
			{
				tweens[i].Delay = (float)delayIndex * this.beforeDespawnTweenDelay;
			}
			this.beforeDespawnTweens.Tween();
		}

		// Token: 0x06001883 RID: 6275 RVA: 0x00071E90 File Offset: 0x00070090
		private void ForceDoneAnyTweensInProgress()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ForceDoneAnyTweensInProgress", Array.Empty<object>());
			}
			if (this.spawnTweens.IsAnyTweening())
			{
				this.spawnTweens.ForceDone(true);
			}
			if (this.toggledOnTweens.IsAnyTweening())
			{
				this.toggledOnTweens.ForceDone(true);
			}
			if (this.toggledOffTweens.IsAnyTweening())
			{
				this.toggledOffTweens.ForceDone(true);
				if (this.toggledOffAnchoredPositionShaker.IsShaking)
				{
					this.toggledOffAnchoredPositionShaker.Stop();
				}
			}
			if (this.beforeDespawnTweens.IsAnyTweening())
			{
				this.beforeDespawnTweens.ForceDone(false);
			}
		}

		// Token: 0x06001884 RID: 6276 RVA: 0x00071F31 File Offset: 0x00070131
		private void Despawn()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Despawn", Array.Empty<object>());
			}
			MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn<UIHealthViewHeart>(this);
		}

		// Token: 0x040013AB RID: 5035
		[Header("Heart Visuals")]
		[SerializeField]
		private Image heartImage;

		// Token: 0x040013AC RID: 5036
		[Header("Spawn Tweens")]
		[SerializeField]
		private TweenCollection spawnTweens;

		// Token: 0x040013AD RID: 5037
		[SerializeField]
		[Tooltip("In seconds")]
		private float spawnTweenDelay = 0.125f;

		// Token: 0x040013AE RID: 5038
		[Header("Toggle On Tweens")]
		[SerializeField]
		private TweenCollection toggledOnTweens;

		// Token: 0x040013AF RID: 5039
		[SerializeField]
		[Tooltip("In seconds")]
		private float toggledOnTweenDelay = 0.125f;

		// Token: 0x040013B0 RID: 5040
		[Header("Toggle Off Tweens")]
		[SerializeField]
		private TweenCollection toggledOffTweens;

		// Token: 0x040013B1 RID: 5041
		[SerializeField]
		[Tooltip("In seconds")]
		private float toggledOffTweenDelay = 0.125f;

		// Token: 0x040013B2 RID: 5042
		[SerializeField]
		private UIAnchoredPositionShaker toggledOffAnchoredPositionShaker;

		// Token: 0x040013B3 RID: 5043
		[Header("Before Despawn Tweens")]
		[SerializeField]
		private TweenCollection beforeDespawnTweens;

		// Token: 0x040013B4 RID: 5044
		[SerializeField]
		[Tooltip("In seconds")]
		private float beforeDespawnTweenDelay = 0.125f;

		// Token: 0x040013B5 RID: 5045
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x040013B6 RID: 5046
		private Color defaultColor = Color.white;

		// Token: 0x040013B7 RID: 5047
		private bool isLeft;
	}
}
