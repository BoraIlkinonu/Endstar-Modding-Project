using System;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000196 RID: 406
	[CreateAssetMenu(menuName = "ScriptableObject/GoalBuilders/PatrolGoalBuilder", fileName = "PatrolBuilder")]
	public class PatrolGoalBuilderSo : GoalBuilderSo
	{
		// Token: 0x06000938 RID: 2360 RVA: 0x0002ACD1 File Offset: 0x00028ED1
		protected override void Activate(NpcEntity entity)
		{
			entity.NpcBlackboard.Set<float>(NpcBlackboard.Key.LastPatrol, Time.time);
		}

		// Token: 0x06000939 RID: 2361 RVA: 0x0002ACE4 File Offset: 0x00028EE4
		protected override float Priority(NpcEntity entity)
		{
			float num;
			float num2;
			if (entity.NpcBlackboard.TryGet<float>(NpcBlackboard.Key.LastPatrol, out num))
			{
				num2 = Time.time - num;
			}
			else
			{
				num2 = Time.time;
			}
			return this.priorityCurve.Evaluate(num2 / this.timeToMaxPriority);
		}

		// Token: 0x0400078A RID: 1930
		[SerializeField]
		private AnimationCurve priorityCurve;

		// Token: 0x0400078B RID: 1931
		[SerializeField]
		private float timeToMaxPriority;
	}
}
