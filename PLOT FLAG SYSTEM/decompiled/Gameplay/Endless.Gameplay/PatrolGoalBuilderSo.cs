using UnityEngine;

namespace Endless.Gameplay;

[CreateAssetMenu(menuName = "ScriptableObject/GoalBuilders/PatrolGoalBuilder", fileName = "PatrolBuilder")]
public class PatrolGoalBuilderSo : GoalBuilderSo
{
	[SerializeField]
	private AnimationCurve priorityCurve;

	[SerializeField]
	private float timeToMaxPriority;

	protected override void Activate(NpcEntity entity)
	{
		entity.NpcBlackboard.Set(NpcBlackboard.Key.LastPatrol, Time.time);
	}

	protected override float Priority(NpcEntity entity)
	{
		float value;
		float num = ((!entity.NpcBlackboard.TryGet<float>(NpcBlackboard.Key.LastPatrol, out value)) ? Time.time : (Time.time - value));
		return priorityCurve.Evaluate(num / timeToMaxPriority);
	}
}
