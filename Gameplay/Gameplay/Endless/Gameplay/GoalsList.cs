using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000199 RID: 409
	[CreateAssetMenu(menuName = "ScriptableObject/GoalsList", fileName = "GoalsList")]
	public class GoalsList : ScriptableObject
	{
		// Token: 0x0600093E RID: 2366 RVA: 0x0002AD54 File Offset: 0x00028F54
		public IEnumerable<Goal> GetGoals(NpcEntity entity)
		{
			return this.goalBuilders.Select((GoalBuilderSo goalBuilder) => goalBuilder.GetGoal(entity));
		}

		// Token: 0x0400078D RID: 1933
		[SerializeField]
		private List<GoalBuilderSo> goalBuilders;
	}
}
