using System;
using System.Collections;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Gameplay.UI
{
	// Token: 0x02000399 RID: 921
	public class UIAnchoredPositionShaker : MonoBehaviour
	{
		// Token: 0x170004D5 RID: 1237
		// (get) Token: 0x06001775 RID: 6005 RVA: 0x0006D441 File Offset: 0x0006B641
		public bool IsShaking
		{
			get
			{
				return this.shakeCoroutineCache != null || this.shakeEndCoroutineCache != null;
			}
		}

		// Token: 0x06001776 RID: 6006 RVA: 0x0006D456 File Offset: 0x0006B656
		private void OnDestroy()
		{
			if (this.IsShaking)
			{
				MonoBehaviourSingleton<UICoroutineManager>.Instance.StopThisCoroutine(this.shakeCoroutineCache);
				MonoBehaviourSingleton<UICoroutineManager>.Instance.StopThisCoroutine(this.shakeEndCoroutineCache);
				this.shakeCoroutineCache = null;
				this.shakeEndCoroutineCache = null;
			}
		}

		// Token: 0x06001777 RID: 6007 RVA: 0x0006D490 File Offset: 0x0006B690
		public void Shake()
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log("Shake", this);
			}
			if (this.IsShaking)
			{
				this.Stop();
			}
			this.anchoredPositionCache = this.Target.anchoredPosition;
			this.shakeCoroutineCache = MonoBehaviourSingleton<UICoroutineManager>.Instance.StartThisCoroutine(this.ShakeCoroutine());
			this.shakeEndCoroutineCache = MonoBehaviourSingleton<UICoroutineManager>.Instance.StartThisCoroutine(this.ShakeEndCoroutine());
		}

		// Token: 0x06001778 RID: 6008 RVA: 0x0006D4FC File Offset: 0x0006B6FC
		public void Stop()
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log("Stop", this);
			}
			MonoBehaviourSingleton<UICoroutineManager>.Instance.StopThisCoroutine(this.shakeCoroutineCache);
			MonoBehaviourSingleton<UICoroutineManager>.Instance.StopThisCoroutine(this.shakeEndCoroutineCache);
			this.shakeCoroutineCache = null;
			this.shakeEndCoroutineCache = null;
			this.Target.anchoredPosition = this.anchoredPositionCache;
		}

		// Token: 0x06001779 RID: 6009 RVA: 0x0006D55B File Offset: 0x0006B75B
		private IEnumerator ShakeCoroutine()
		{
			yield return new WaitForSeconds(this.Delay);
			for (;;)
			{
				this.Target.anchoredPosition = new Vector2(this.anchoredPositionCache.x + global::UnityEngine.Random.Range(this.ShakeRangeX.x, this.ShakeRangeX.y), this.anchoredPositionCache.y + global::UnityEngine.Random.Range(this.ShakeRangeY.x, this.ShakeRangeY.y));
				yield return new WaitForSeconds(this.ShakeIterations);
			}
			yield break;
		}

		// Token: 0x0600177A RID: 6010 RVA: 0x0006D56A File Offset: 0x0006B76A
		private IEnumerator ShakeEndCoroutine()
		{
			yield return new WaitForSeconds(this.Delay);
			yield return new WaitForSeconds(this.ShakeDuration);
			this.Stop();
			yield break;
		}

		// Token: 0x040012DF RID: 4831
		public RectTransform Target;

		// Token: 0x040012E0 RID: 4832
		public Vector2 ShakeRangeX = new Vector2(-5f, 5f);

		// Token: 0x040012E1 RID: 4833
		public Vector2 ShakeRangeY = new Vector2(-5f, 5f);

		// Token: 0x040012E2 RID: 4834
		[Tooltip("In seconds")]
		public float Delay;

		// Token: 0x040012E3 RID: 4835
		[Tooltip("In seconds")]
		public float ShakeIterations = 0.1f;

		// Token: 0x040012E4 RID: 4836
		[Tooltip("In seconds")]
		public float ShakeDuration = 0.5f;

		// Token: 0x040012E5 RID: 4837
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x040012E6 RID: 4838
		private Vector2 anchoredPositionCache = Vector2.zero;

		// Token: 0x040012E7 RID: 4839
		private Coroutine shakeCoroutineCache;

		// Token: 0x040012E8 RID: 4840
		private Coroutine shakeEndCoroutineCache;
	}
}
