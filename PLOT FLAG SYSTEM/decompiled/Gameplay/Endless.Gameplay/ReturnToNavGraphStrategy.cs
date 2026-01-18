using System;
using System.Collections.Generic;
using Endless.Shared;
using UnityEngine;

namespace Endless.Gameplay;

public class ReturnToNavGraphStrategy : IActionStrategy
{
	private readonly NpcEntity entity;

	private Vector3 offMeshPosition;

	private Vector3 returnPosition;

	private readonly float lerpTime = 1f;

	private float startTime;

	public Func<float> GetCost { get; }

	public GoapAction.Status Status { get; private set; }

	public ReturnToNavGraphStrategy(NpcEntity entity)
	{
		this.entity = entity;
	}

	public void Start()
	{
		Status = GoapAction.Status.InProgress;
		List<Vector3> list = MonoBehaviourSingleton<Pathfinding>.Instance.FindNavigationPositionsInRange(entity.FootPosition, 1f);
		if (list.Count == 0)
		{
			Status = GoapAction.Status.Failed;
			return;
		}
		offMeshPosition = entity.FootPosition;
		returnPosition = list[0];
		startTime = Time.time;
	}

	public void Update(float deltaTime)
	{
		float num = (Time.time - startTime) / lerpTime;
		entity.Position = Vector3.Lerp(offMeshPosition, returnPosition, num) + Vector3.up * 0.5f;
		if (num > 1f)
		{
			Status = GoapAction.Status.Complete;
		}
	}
}
