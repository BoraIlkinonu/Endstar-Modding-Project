using UnityEngine;

namespace Endless.Gameplay;

public class MoveToPositionGoalBuilderComponent : GoalBuilderComponent
{
	protected override void Activate(NpcEntity entity)
	{
		entity.NpcBlackboard.Set(NpcBlackboard.Key.CommandDestination, base.transform.position - Vector3.up * 0.5f);
	}

	protected override void Deactivate(NpcEntity entity)
	{
		entity.NpcBlackboard.Clear<Vector3>(NpcBlackboard.Key.CommandDestination);
	}
}
