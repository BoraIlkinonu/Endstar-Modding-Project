using UnityEngine;

namespace Endless.Gameplay;

public class WanderGoalBuilderComponent : GoalBuilderComponent
{
	[SerializeField]
	private float wanderDistance;

	[SerializeField]
	private AnimationCurve priorityCurve;

	[SerializeField]
	private float maxWanderJitter;

	private float wanderJitter;

	private void Awake()
	{
		wanderJitter = Random.Range(0f, maxWanderJitter);
		priorityCurve.preWrapMode = WrapMode.ClampForever;
		priorityCurve.postWrapMode = WrapMode.ClampForever;
	}

	protected override void Activate(NpcEntity entity)
	{
		Vector3? wanderPosition = Pathfinding.GetWanderPosition(entity.Position, wanderDistance);
		if (wanderPosition.HasValue)
		{
			entity.NpcBlackboard.Set(NpcBlackboard.Key.BehaviorDestination, wanderPosition.Value);
		}
	}

	protected override void Deactivate(NpcEntity entity)
	{
		entity.NpcBlackboard.Set(NpcBlackboard.Key.LastWander, Time.time);
		wanderJitter = Random.Range(0f, maxWanderJitter);
		entity.NpcBlackboard.Clear<Vector3>(NpcBlackboard.Key.BehaviorDestination);
	}

	protected override float Priority(NpcEntity entity)
	{
		float value;
		float num = ((!entity.NpcBlackboard.TryGet<float>(NpcBlackboard.Key.LastWander, out value)) ? Time.time : (Time.time - value));
		return priorityCurve.Evaluate(num - wanderJitter);
	}
}
