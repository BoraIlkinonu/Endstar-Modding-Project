using System;

namespace MatchmakingClientSDK.Events
{
	// Token: 0x02000068 RID: 104
	public abstract class TreeNode<T>
	{
		// Token: 0x06000404 RID: 1028
		public abstract bool Evaluate(T data);

		// Token: 0x06000405 RID: 1029 RVA: 0x0001229D File Offset: 0x0001049D
		public static TreeNode<T> New(Func<T, bool> condition, TreeNode<T> @true, TreeNode<T> @false)
		{
			return new ConditionTreeNode<T>
			{
				Condition = condition,
				True = @true,
				False = @false
			};
		}

		// Token: 0x06000406 RID: 1030 RVA: 0x000122B9 File Offset: 0x000104B9
		public static TreeNode<T> New(Func<T, bool> action)
		{
			return new ActionTreeNode<T>
			{
				Action = action
			};
		}
	}
}
