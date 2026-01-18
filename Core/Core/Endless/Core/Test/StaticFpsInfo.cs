using System;

namespace Endless.Core.Test
{
	// Token: 0x020000E1 RID: 225
	public class StaticFpsInfo : LoadFpsInfo
	{
		// Token: 0x170000A1 RID: 161
		// (get) Token: 0x06000515 RID: 1301 RVA: 0x000027B9 File Offset: 0x000009B9
		protected override FpsTestType TestType
		{
			get
			{
				return FpsTestType.Static;
			}
		}
	}
}
