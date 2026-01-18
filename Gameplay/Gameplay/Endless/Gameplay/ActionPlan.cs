using System;
using System.Collections.Generic;

namespace Endless.Gameplay
{
	// Token: 0x0200016F RID: 367
	public class ActionPlan
	{
		// Token: 0x17000186 RID: 390
		// (get) Token: 0x0600082B RID: 2091 RVA: 0x00027084 File Offset: 0x00025284
		public Goal Goal { get; }

		// Token: 0x17000187 RID: 391
		// (get) Token: 0x0600082C RID: 2092 RVA: 0x0002708C File Offset: 0x0002528C
		public Stack<GoapAction> Actions { get; }

		// Token: 0x17000188 RID: 392
		// (get) Token: 0x0600082D RID: 2093 RVA: 0x00027094 File Offset: 0x00025294
		// (set) Token: 0x0600082E RID: 2094 RVA: 0x0002709C File Offset: 0x0002529C
		public float TotalCost { get; set; }

		// Token: 0x0600082F RID: 2095 RVA: 0x000270A5 File Offset: 0x000252A5
		public ActionPlan(Goal goal, Stack<GoapAction> actions, float totalCost)
		{
			this.Goal = goal;
			this.Actions = actions;
			this.TotalCost = totalCost;
		}
	}
}
