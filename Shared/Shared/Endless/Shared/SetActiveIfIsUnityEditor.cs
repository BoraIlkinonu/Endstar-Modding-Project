using System;
using Endless.Shared.Debugging;

namespace Endless.Shared
{
	// Token: 0x02000090 RID: 144
	public class SetActiveIfIsUnityEditor : BaseSetActiveIf
	{
		// Token: 0x0600040A RID: 1034 RVA: 0x00011918 File Offset: 0x0000FB18
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
