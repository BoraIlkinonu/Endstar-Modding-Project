using System;
using System.Collections;
using Endless.Shared.Debugging;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x02000120 RID: 288
	public class UICoroutineManager : UIMonoBehaviourSingleton<UICoroutineManager>
	{
		// Token: 0x06000724 RID: 1828 RVA: 0x0001E4C1 File Offset: 0x0001C6C1
		public Coroutine StartThisCoroutine(IEnumerator coroutine)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "StartThisCoroutine", new object[] { coroutine });
			}
			return base.StartCoroutine(coroutine);
		}

		// Token: 0x06000725 RID: 1829 RVA: 0x0001E4E7 File Offset: 0x0001C6E7
		public void StopThisCoroutine(Coroutine coroutine)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "StopThisCoroutine", new object[] { coroutine });
			}
			base.StopCoroutine(coroutine);
		}

		// Token: 0x06000726 RID: 1830 RVA: 0x0001E510 File Offset: 0x0001C710
		public void WaitFramesAndInvoke(Action action, int frames = 1)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "WaitFramesAndInvoke", new object[] { action, frames });
			}
			if (ExitManager.IsQuitting)
			{
				return;
			}
			if (frames < 1)
			{
				frames = 1;
			}
			base.StartCoroutine(this.WaitFrameAndInvokeCoroutine(action, frames));
		}

		// Token: 0x06000727 RID: 1831 RVA: 0x0001E561 File Offset: 0x0001C761
		private IEnumerator WaitFrameAndInvokeCoroutine(Action action, int frames)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "WaitFrameAndInvokeCoroutine", new object[] { action, frames });
			}
			while (frames > 0)
			{
				int num = frames;
				frames = num - 1;
				yield return null;
			}
			if (action != null)
			{
				action();
			}
			yield break;
		}

		// Token: 0x0400042B RID: 1067
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
