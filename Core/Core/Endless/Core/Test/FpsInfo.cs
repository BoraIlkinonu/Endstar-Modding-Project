using System;
using System.Collections.Generic;

namespace Endless.Core.Test
{
	// Token: 0x020000D7 RID: 215
	[Serializable]
	public struct FpsInfo
	{
		// Token: 0x0400033D RID: 829
		public string SectionName;

		// Token: 0x0400033E RID: 830
		public List<float> Frames;

		// Token: 0x0400033F RID: 831
		public FpsTestType TestType;
	}
}
