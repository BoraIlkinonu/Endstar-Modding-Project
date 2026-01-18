using System;

namespace Endless.Gameplay
{
	// Token: 0x020001A0 RID: 416
	public struct PlanNode
	{
		// Token: 0x040007A5 RID: 1957
		public GoapAction.ActionKind Action;

		// Token: 0x040007A6 RID: 1958
		public unsafe PlanNode** Prerequisites;

		// Token: 0x040007A7 RID: 1959
		public int NumPrerequisites;

		// Token: 0x040007A8 RID: 1960
		public float Cost;
	}
}
