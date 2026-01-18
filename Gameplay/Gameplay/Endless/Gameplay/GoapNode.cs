using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020001A4 RID: 420
	public abstract class GoapNode : AttributeModifierNode, IGoapNode, IInstructionNode, INpcAttributeModifier
	{
		// Token: 0x06000975 RID: 2421 RVA: 0x0002BC58 File Offset: 0x00029E58
		protected void OnValidate()
		{
			foreach (global::UnityEngine.Object @object in this.InstructionGoals.Where((global::UnityEngine.Object obj) => obj && !(obj is IGoalBuilder)).ToList<global::UnityEngine.Object>())
			{
				this.InstructionGoals.Remove(@object);
				Debug.LogWarning("objects in instructionGoals must reference an IGoalBuilder");
			}
		}

		// Token: 0x06000976 RID: 2422
		public abstract void AddNodeGoals(NpcEntity entity);

		// Token: 0x06000977 RID: 2423
		public abstract void RemoveNodeGoals(NpcEntity entity);

		// Token: 0x040007B2 RID: 1970
		[SerializeField]
		protected List<global::UnityEngine.Object> InstructionGoals = new List<global::UnityEngine.Object>();

		// Token: 0x040007B3 RID: 1971
		protected readonly Dictionary<NpcEntity, List<Goal>> AddedGoalsByNpcEntity = new Dictionary<NpcEntity, List<Goal>>();
	}
}
