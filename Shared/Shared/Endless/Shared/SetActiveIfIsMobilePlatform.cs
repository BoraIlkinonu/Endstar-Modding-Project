using System;
using Endless.Shared.Debugging;

namespace Endless.Shared
{
	// Token: 0x0200008E RID: 142
	public class SetActiveIfIsMobilePlatform : BaseSetActiveIf
	{
		// Token: 0x06000406 RID: 1030 RVA: 0x000118C1 File Offset: 0x0000FAC1
		private void Start()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			base.SetActive(MobileUtility.IsMobile);
		}
	}
}
