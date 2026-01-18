using System;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000190 RID: 400
	public class MoveToPositionGoalBuilderComponent : GoalBuilderComponent
	{
		// Token: 0x06000923 RID: 2339 RVA: 0x0002AA45 File Offset: 0x00028C45
		protected override void Activate(NpcEntity entity)
		{
			entity.NpcBlackboard.Set<Vector3>(NpcBlackboard.Key.CommandDestination, base.transform.position - Vector3.up * 0.5f);
		}

		// Token: 0x06000924 RID: 2340 RVA: 0x0002AA73 File Offset: 0x00028C73
		protected override void Deactivate(NpcEntity entity)
		{
			entity.NpcBlackboard.Clear<Vector3>(NpcBlackboard.Key.CommandDestination);
		}
	}
}
