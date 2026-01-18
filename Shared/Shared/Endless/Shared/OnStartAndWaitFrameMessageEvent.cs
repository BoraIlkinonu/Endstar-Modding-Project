using System;
using System.Collections;
using Endless.Shared.Debugging;
using UnityEngine;

namespace Endless.Shared
{
	// Token: 0x02000071 RID: 113
	public class OnStartAndWaitFrameMessageEvent : BaseMessageEvent
	{
		// Token: 0x06000380 RID: 896 RVA: 0x0001031D File Offset: 0x0000E51D
		private IEnumerator Start()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			int framesToWaitCount = this.framesToWait;
			while (framesToWaitCount > 0)
			{
				int num = framesToWaitCount;
				framesToWaitCount = num - 1;
				yield return new WaitForEndOfFrame();
			}
			this.OnMessageTriggered.Invoke();
			yield break;
		}

		// Token: 0x040001B0 RID: 432
		[Min(1f)]
		[SerializeField]
		private int framesToWait = 1;
	}
}
