using System;

namespace MatchmakingClientSDK.Events
{
	// Token: 0x0200006A RID: 106
	public class ActionTreeNode<T> : TreeNode<T>
	{
		// Token: 0x0600040A RID: 1034 RVA: 0x00012300 File Offset: 0x00010500
		public override bool Evaluate(T data)
		{
			return this.Action(data);
		}

		// Token: 0x0400029B RID: 667
		public Func<T, bool> Action;
	}
}
