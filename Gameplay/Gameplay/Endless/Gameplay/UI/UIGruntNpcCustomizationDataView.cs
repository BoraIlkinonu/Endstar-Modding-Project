using System;
using Endless.Shared.Debugging;

namespace Endless.Gameplay.UI
{
	// Token: 0x020003E6 RID: 998
	public class UIGruntNpcCustomizationDataView : UINpcClassCustomizationDataView<GruntNpcCustomizationData>
	{
		// Token: 0x06001910 RID: 6416 RVA: 0x00074086 File Offset: 0x00072286
		public override void Clear()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Clear", Array.Empty<object>());
			}
		}
	}
}
