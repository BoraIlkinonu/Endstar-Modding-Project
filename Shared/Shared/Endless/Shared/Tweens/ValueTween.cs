using System;
using UnityEngine;

namespace Endless.Shared.Tweens
{
	// Token: 0x020000CE RID: 206
	public class ValueTween
	{
		// Token: 0x0600056A RID: 1386 RVA: 0x00017EF3 File Offset: 0x000160F3
		internal ValueTween(Coroutine coroutine)
		{
			this.coroutine = coroutine;
			this.isComplete = false;
		}

		// Token: 0x0600056B RID: 1387 RVA: 0x00017F09 File Offset: 0x00016109
		public void Cancel()
		{
			if (this.isComplete || this.coroutine == null)
			{
				return;
			}
			MonoBehaviourSingleton<TweenManager>.Instance.StopCoroutine(this.coroutine);
			MonoBehaviourSingleton<TweenManager>.Instance.RemoveValueTween(this);
			this.isComplete = true;
		}

		// Token: 0x0600056C RID: 1388 RVA: 0x00017F3E File Offset: 0x0001613E
		public bool IsActive()
		{
			return !this.isComplete;
		}

		// Token: 0x0600056D RID: 1389 RVA: 0x00017F49 File Offset: 0x00016149
		public static void CancelAndNull(ref ValueTween tween)
		{
			if (tween == null)
			{
				return;
			}
			tween.Cancel();
			tween = null;
		}

		// Token: 0x040002DE RID: 734
		internal Coroutine coroutine;

		// Token: 0x040002DF RID: 735
		internal bool isComplete;
	}
}
