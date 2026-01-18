using System;

namespace Endless.Gameplay;

public class FindTargetStrategy : IActionStrategy
{
	private readonly NpcEntity entity;

	public Func<float> GetCost { get; }

	public GoapAction.Status Status { get; private set; }

	public FindTargetStrategy(NpcEntity entity)
	{
		this.entity = entity;
	}

	public void Start()
	{
		Status = GoapAction.Status.InProgress;
		entity.Components.Pathing.RequestPath(entity.LastKnownTargetLocation, PathfindingResponseHandler);
	}

	private void PathfindingResponseHandler(Pathfinding.Response response)
	{
		if (response.PathfindingResult == NpcEnum.PathfindingResult.Failure)
		{
			Status = GoapAction.Status.Failed;
			return;
		}
		entity.Components.PathFollower.SetPath(response.Path);
		entity.Components.PathFollower.OnPathFinished -= HandleOnPathFinished;
		entity.Components.PathFollower.OnPathFinished += HandleOnPathFinished;
	}

	private void HandleOnPathFinished(bool obj)
	{
		entity.Components.PathFollower.OnPathFinished -= HandleOnPathFinished;
	}

	public void Tick(uint frame)
	{
		if ((bool)entity.Target)
		{
			Status = GoapAction.Status.Complete;
		}
	}

	public void Stop()
	{
		entity.Components.PathFollower.OnPathFinished -= HandleOnPathFinished;
		entity.LostTarget = false;
	}
}
