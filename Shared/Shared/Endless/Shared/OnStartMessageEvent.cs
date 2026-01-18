using System;
using Endless.Shared.Debugging;

namespace Endless.Shared
{
	// Token: 0x02000073 RID: 115
	public class OnStartMessageEvent : BaseMessageEvent
	{
		// Token: 0x06000388 RID: 904 RVA: 0x000103E5 File Offset: 0x0000E5E5
		private void Start()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.OnMessageTriggered.Invoke();
		}
	}
}
