using System;
using Endless.Shared.Debugging;

namespace Endless.Gameplay.UI
{
	// Token: 0x020003EB RID: 1003
	public class UIRiflemanNpcCustomizationDataView : UINpcClassCustomizationDataView<RiflemanNpcCustomizationData>
	{
		// Token: 0x06001921 RID: 6433 RVA: 0x00074538 File Offset: 0x00072738
		public override void Clear()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Clear", Array.Empty<object>());
			}
		}
	}
}
