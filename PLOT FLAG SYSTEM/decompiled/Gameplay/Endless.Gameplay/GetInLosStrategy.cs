using System;
using System.Collections.Generic;
using Endless.Gameplay.LuaEnums;
using Endless.Shared;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

namespace Endless.Gameplay;

public class GetInLosStrategy : IActionStrategy
{
	private readonly NpcEntity entity;

	private Vector3 targetPositionAtRequest;

	public Func<float> GetCost { get; }

	public GoapAction.Status Status { get; private set; }

	public GetInLosStrategy(NpcEntity entity)
	{
		this.entity = entity;
	}

	public void Start()
	{
		if (!entity.Target)
		{
			Status = GoapAction.Status.Failed;
			return;
		}
		Status = GoapAction.Status.InProgress;
		MonoBehaviourSingleton<Los>.Instance.RequestLosPositions(entity, LosPositionCallback);
		targetPositionAtRequest = entity.Target.NavPosition;
	}

	public void Tick(uint frame)
	{
		if (!entity.Target || math.distance(targetPositionAtRequest, entity.Target.NavPosition) > 1f)
		{
			Status = GoapAction.Status.Failed;
		}
		else if (MonoBehaviourSingleton<WorldStateDictionary>.Instance[WorldState.HasLos].Evaluate(entity))
		{
			Status = GoapAction.Status.Complete;
		}
	}

	public void Stop()
	{
		entity.Components.PathFollower.StopPath();
		entity.Components.PathFollower.OnPathFinished -= HandleOnPathFinished;
	}

	private void LosPositionCallback(List<Vector3> losPositions)
	{
		if (losPositions.Count > 0)
		{
			foreach (Vector3 losPosition in losPositions)
			{
				if (Vector3.Distance(entity.Components.TargeterComponent.Target.Position, losPosition) < 60f && MonoBehaviourSingleton<Pathfinding>.Instance.IsValidDestination(entity.FootPosition, losPosition, entity.PathfindingRange, entity.CanDoubleJump) && NavMesh.SamplePosition(losPosition, out var hit, 1f, -1))
				{
					entity.Components.Pathing.RequestPath(hit.position, PathfindingResponseHandler);
					return;
				}
			}
		}
		Status = GoapAction.Status.Failed;
	}

	private void PathfindingResponseHandler(Pathfinding.Response response)
	{
		if (response.PathfindingResult == NpcEnum.PathfindingResult.Failure)
		{
			Status = GoapAction.Status.Failed;
		}
		else if ((bool)entity.Target)
		{
			entity.Components.PathFollower.SetPath(response.Path);
			entity.Components.PathFollower.LookRotationOverride = entity.Target.transform;
			entity.Components.PathFollower.OnPathFinished -= HandleOnPathFinished;
			entity.Components.PathFollower.OnPathFinished += HandleOnPathFinished;
		}
	}

	private void HandleOnPathFinished(bool _)
	{
		Status = (MonoBehaviourSingleton<WorldStateDictionary>.Instance[WorldState.HasLos].Evaluate(entity) ? GoapAction.Status.Complete : GoapAction.Status.Failed);
	}
}
