using System;
using System.Collections.Generic;

namespace MatchmakingClientSDK.Events
{
	// Token: 0x02000067 RID: 103
	public class SequentialDecisionTree<T>
	{
		// Token: 0x06000402 RID: 1026 RVA: 0x00012264 File Offset: 0x00010464
		public bool TriggerRoot(string rootId, T data)
		{
			TreeNode<T> treeNode;
			return this.RootNodes.TryGetValue(rootId, out treeNode) && treeNode.Evaluate(data);
		}

		// Token: 0x04000297 RID: 663
		public readonly Dictionary<string, TreeNode<T>> RootNodes = new Dictionary<string, TreeNode<T>>();
	}
}
