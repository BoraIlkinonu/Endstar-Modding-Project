using System;
using Endless.Gameplay.LuaEnums;
using Unity.Mathematics;
using UnityEngine;

namespace Endless.Gameplay;

public class FallBackStrategy : IActionStrategy
{
	private readonly NpcEntity entity;

	private float cachedSpeed;

	private Vector3 targetPositionAtRequest;

	public Func<float> GetCost { get; }

	public GoapAction.Status Status { get; private set; }

	public FallBackStrategy(NpcEntity entity)
	{
		this.entity = entity;
	}

	public void Start()
	{
		if (!entity.Target || !entity.Target.CombatPositionGenerator.TryGetClosestAroundPosition(entity.transform.position, out var aroundPosition))
		{
			Status = GoapAction.Status.Failed;
			return;
		}
		Status = GoapAction.Status.InProgress;
		entity.Components.Animator.SetBool(NpcAnimator.ZLock, value: true);
		cachedSpeed = entity.Components.Agent.speed;
		entity.Components.Agent.speed = entity.Settings.StrafingSpeed;
		if (entity.NpcClass.NpcClass == NpcClass.Rifleman)
		{
			entity.NpcBlackboard.Set(NpcBlackboard.Key.OverrideSpeed, cachedSpeed);
		}
		entity.Components.Pathing.RequestPath(aroundPosition, PathfindingResponseHandler);
	}

	public void Tick(uint frame)
	{
		if (!entity.Target || math.distance(targetPositionAtRequest, entity.Target.Position) > 1f)
		{
			Status = GoapAction.Status.Failed;
		}
	}

	public void Stop()
	{
		entity.Components.Animator.SetBool(NpcAnimator.ZLock, value: true);
		entity.Components.PathFollower.StopPath();
		entity.NpcBlackboard.Set(NpcBlackboard.Key.BoredomTime, 0f);
		entity.Components.Agent.speed = cachedSpeed;
		entity.NpcBlackboard.Clear<float>(NpcBlackboard.Key.OverrideSpeed);
		entity.Components.PathFollower.OnPathFinished -= HandleOnPathFinished;
	}

	private void PathfindingResponseHandler(Pathfinding.Response response)
	{
		if (response.PathfindingResult == NpcEnum.PathfindingResult.Failure)
		{
			Status = GoapAction.Status.Failed;
			return;
		}
		if (!entity.Target)
		{
			Status = GoapAction.Status.Failed;
			return;
		}
		entity.Components.PathFollower.SetPath(response.Path);
		entity.Components.PathFollower.LookRotationOverride = entity.Target.transform;
		entity.Components.PathFollower.OnPathFinished -= HandleOnPathFinished;
		entity.Components.PathFollower.OnPathFinished += HandleOnPathFinished;
	}

	private void HandleOnPathFinished(bool complete)
	{
		Status = (complete ? GoapAction.Status.Complete : GoapAction.Status.Failed);
	}
}
