using UnityEngine;

namespace Endless.Gameplay;

public class RoveGoalBuilderComponent : GoalBuilderComponent
{
	[SerializeField]
	private float roveDistance;

	[SerializeField]
	private AnimationCurve priorityCurve;

	[SerializeField]
	private float maxWanderJitter;

	private float wanderJitter;

	private Vector3 roveCenter;

	private void Awake()
	{
		wanderJitter = Random.Range(0f, maxWanderJitter);
		roveCenter = base.transform.position;
		priorityCurve.preWrapMode = WrapMode.ClampForever;
		priorityCurve.postWrapMode = WrapMode.ClampForever;
	}

	protected override void Activate(NpcEntity entity)
	{
		Vector3? rovePosition = Pathfinding.GetRovePosition(roveCenter, entity.Position, roveDistance, roveDistance);
		if (rovePosition.HasValue)
		{
			entity.NpcBlackboard.Set(NpcBlackboard.Key.BehaviorDestination, rovePosition.Value);
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
