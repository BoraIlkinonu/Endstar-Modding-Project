using System;
using UnityEngine;

namespace Endless.Gameplay;

public class MoveToCommandDestinationStrategy : IActionStrategy
{
	private readonly NpcEntity entity;

	private Quaternion? targetRotation;

	private int pathfindingAttempts;

	public Func<float> GetCost { get; }

	public GoapAction.Status Status { get; private set; }

	public MoveToCommandDestinationStrategy(NpcEntity entity)
	{
		this.entity = entity;
	}

	public void Start()
	{
		if (!entity.NpcBlackboard.TryGet<Vector3>(NpcBlackboard.Key.CommandDestination, out var value))
		{
			Status = GoapAction.Status.Failed;
			return;
		}
		Status = GoapAction.Status.InProgress;
		targetRotation = null;
		entity.Components.Pathing.RequestPath(value, PathfindingResponseHandler);
		pathfindingAttempts = 0;
	}

	private void PathfindingResponseHandler(Pathfinding.Response response)
	{
		if (response.PathfindingResult == NpcEnum.PathfindingResult.Failure)
		{
			Status = GoapAction.Status.Failed;
			return;
		}
		entity.Components.PathFollower.SetPath(response.Path);
		entity.Components.PathFollower.OnPathFinished -= PathFollowerOnOnPathFinished;
		entity.Components.PathFollower.OnPathFinished += PathFollowerOnOnPathFinished;
	}

	private void PathFollowerOnOnPathFinished(bool result)
	{
		if (result)
		{
			if (entity.NpcBlackboard.TryGet<Quaternion>(NpcBlackboard.Key.Rotation, out var value))
			{
				targetRotation = value;
			}
			else
			{
				Status = GoapAction.Status.Complete;
				entity.NpcBlackboard.Clear<Vector3>(NpcBlackboard.Key.CommandDestination);
			}
		}
		else
		{
			pathfindingAttempts++;
			if (pathfindingAttempts > 10)
			{
				Status = GoapAction.Status.Failed;
			}
			else
			{
				if (!entity.NpcBlackboard.TryGet<Vector3>(NpcBlackboard.Key.CommandDestination, out var value2))
				{
					Status = GoapAction.Status.Failed;
					return;
				}
				entity.Components.Pathing.RequestPath(value2, PathfindingResponseHandler);
			}
		}
		entity.Components.PathFollower.OnPathFinished -= PathFollowerOnOnPathFinished;
	}

	public void Update(float deltaTime)
	{
		if (entity.Components.PathFollower.Path != null && Vector3.Distance(entity.Components.PathFollower.Path.Destination, entity.FootPosition) < entity.NpcBlackboard.GetValueOrDefault(NpcBlackboard.Key.DestinationTolerance, 0.5f))
		{
			entity.Components.PathFollower.OnPathFinished -= PathFollowerOnOnPathFinished;
			if (entity.NpcBlackboard.TryGet<Quaternion>(NpcBlackboard.Key.Rotation, out var value))
			{
				targetRotation = value;
			}
			else
			{
				Status = GoapAction.Status.Complete;
				entity.NpcBlackboard.Clear<Vector3>(NpcBlackboard.Key.CommandDestination);
			}
		}
		Quaternion? quaternion = targetRotation;
		if (quaternion.HasValue)
		{
			if (Quaternion.Angle(entity.transform.rotation, targetRotation.Value) > 5f)
			{
				entity.transform.rotation = Quaternion.RotateTowards(entity.transform.rotation, targetRotation.Value, entity.Settings.RotationSpeed * Time.deltaTime);
				return;
			}
			Status = GoapAction.Status.Complete;
			entity.NpcBlackboard.Clear<Vector3>(NpcBlackboard.Key.CommandDestination);
		}
	}

	public void Stop()
	{
		entity.NpcBlackboard.Clear<Quaternion>(NpcBlackboard.Key.Rotation);
		entity.Components.PathFollower.StopPath();
	}
}
