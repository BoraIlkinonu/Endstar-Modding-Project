using System;
using Endless.Shared.Debugging;

namespace Endless.Shared
{
	// Token: 0x02000070 RID: 112
	public class OnEnableMessageEvent : BaseMessageEvent
	{
		// Token: 0x0600037E RID: 894 RVA: 0x000102F0 File Offset: 0x0000E4F0
		private void OnEnable()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEnable", Array.Empty<object>());
			}
			this.OnMessageTriggered.Invoke();
		}
	}
}
