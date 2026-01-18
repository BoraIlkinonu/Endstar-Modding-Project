using System;
using System.Collections.Generic;
using Endless.Gameplay.LuaEnums;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x0200023C RID: 572
	public class DefaultBehaviorList : EndlessBehaviour
	{
		// Token: 0x06000BCC RID: 3020 RVA: 0x00040B84 File Offset: 0x0003ED84
		private void InitializeDictionary()
		{
			this.behaviorNodesByBehaviorEnum.Clear();
			foreach (DefaultBehaviorList.IdleBehaviorKvp idleBehaviorKvp in this.behaviorNodes)
			{
				if (!this.behaviorNodesByBehaviorEnum.TryAdd(idleBehaviorKvp.IdleBehavior, idleBehaviorKvp.BehaviorNode))
				{
					Debug.LogWarning(string.Format("Multiple entries in behavior nodes with key {0}, ignoring all but the first", idleBehaviorKvp.IdleBehavior));
				}
			}
		}

		// Token: 0x06000BCD RID: 3021 RVA: 0x00040C10 File Offset: 0x0003EE10
		public bool TryGetBehaviorNode(IdleBehavior idleBehavior, out BehaviorNode behaviorNode)
		{
			if (this.behaviorNodesByBehaviorEnum.Count != this.behaviorNodes.Count)
			{
				this.InitializeDictionary();
			}
			if (this.behaviorNodesByBehaviorEnum.TryGetValue(idleBehavior, out behaviorNode) && behaviorNode)
			{
				return true;
			}
			Debug.LogWarning(string.Format("No Behavior node was found associated with {0}", idleBehavior));
			return false;
		}

		// Token: 0x04000B01 RID: 2817
		[SerializeField]
		private List<DefaultBehaviorList.IdleBehaviorKvp> behaviorNodes;

		// Token: 0x04000B02 RID: 2818
		private readonly Dictionary<IdleBehavior, BehaviorNode> behaviorNodesByBehaviorEnum = new Dictionary<IdleBehavior, BehaviorNode>();

		// Token: 0x0200023D RID: 573
		[Serializable]
		public struct IdleBehaviorKvp
		{
			// Token: 0x04000B03 RID: 2819
			public IdleBehavior IdleBehavior;

			// Token: 0x04000B04 RID: 2820
			public BehaviorNode BehaviorNode;
		}
	}
}
