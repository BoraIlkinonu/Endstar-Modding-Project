using System;
using Endless.Shared.Debugging;

namespace Endless.Gameplay.UI
{
	// Token: 0x020003E4 RID: 996
	public class UIBlankNpcCustomizationDataView : UINpcClassCustomizationDataView<BlankNpcCustomizationData>
	{
		// Token: 0x0600190C RID: 6412 RVA: 0x0007405C File Offset: 0x0007225C
		public override void Clear()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Clear", Array.Empty<object>());
			}
		}
	}
}
