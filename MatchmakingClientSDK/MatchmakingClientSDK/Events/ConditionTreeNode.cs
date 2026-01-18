using System;

namespace MatchmakingClientSDK.Events
{
	// Token: 0x02000069 RID: 105
	public class ConditionTreeNode<T> : TreeNode<T>
	{
		// Token: 0x06000408 RID: 1032 RVA: 0x000122CF File Offset: 0x000104CF
		public override bool Evaluate(T data)
		{
			if (this.Condition(data))
			{
				return this.True.Evaluate(data);
			}
			return this.False.Evaluate(data);
		}

		// Token: 0x04000298 RID: 664
		public TreeNode<T> True;

		// Token: 0x04000299 RID: 665
		public TreeNode<T> False;

		// Token: 0x0400029A RID: 666
		public Func<T, bool> Condition;
	}
}
