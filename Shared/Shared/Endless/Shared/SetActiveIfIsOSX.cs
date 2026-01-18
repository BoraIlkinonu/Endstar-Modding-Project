using System;
using Endless.Shared.Debugging;

namespace Endless.Shared
{
	// Token: 0x0200008F RID: 143
	public class SetActiveIfIsOSX : BaseSetActiveIf
	{
		// Token: 0x06000408 RID: 1032 RVA: 0x000118E8 File Offset: 0x0000FAE8
		private void Start()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			bool flag = false;
			base.SetActive(flag);
		}
	}
}
