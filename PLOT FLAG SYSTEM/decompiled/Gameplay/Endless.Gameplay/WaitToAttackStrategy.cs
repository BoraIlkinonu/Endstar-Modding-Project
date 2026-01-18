using System;
using System.Collections.Generic;
using Endless.Shared;
using Unity.Mathematics;
using UnityEngine;

namespace Endless.Gameplay;

public class WaitToAttackStrategy : IActionStrategy
{
	private readonly NpcEntity entity;

	private float cachedSpeed;

	private bool waitingForPath;

	public Func<float> GetCost { get; }

	public GoapAction.Status Status { get; private set; }

	public WaitToAttackStrategy(NpcEntity entity)
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
		cachedSpeed = entity.Components.Agent.speed;
		entity.Components.Agent.speed = entity.Settings.StrafingSpeed;
		entity.Components.Animator.SetBool(NpcAnimator.ZLock, value: true);
		entity.CurrentRequest = new AttackRequest(entity.WorldObject, entity.Target, delegate(WorldObject worldObject)
		{
			worldObject.GetUserComponent<NpcEntity>().HasAttackToken = true;
		});
		MonoBehaviourSingleton<CombatManager>.Instance.SubmitAttackRequest(entity.CurrentRequest);
	}

	public void Tick(uint frame)
	{
		if (!entity.Target)
		{
			Status = GoapAction.Status.Failed;
			return;
		}
		if (entity.HasAttackToken)
		{
			Status = GoapAction.Status.Complete;
			return;
		}
		float num = math.distance(entity.FootPosition, entity.Target.NavPosition);
		if (num > entity.NearDistance + 0.2f)
		{
			Status = GoapAction.Status.Failed;
		}
		else
		{
			if (entity.Components.PathFollower == null || waitingForPath)
			{
				return;
			}
			if (num < entity.MeleeDistance && entity.Target.CombatPositionGenerator.TryGetClosestNearPosition(entity.transform.position, out var nearPosition))
			{
				waitingForPath = true;
				entity.Components.Pathing.RequestPath(nearPosition, PathfindingResponseHandler);
				return;
			}
			IReadOnlyList<NpcEntity> npcs = MonoBehaviourSingleton<NpcManager>.Instance.Npcs;
			float num2 = float.MaxValue;
			NpcEntity npcEntity = null;
			foreach (NpcEntity item in npcs)
			{
				if (!(entity == item))
				{
					num = math.distance(item.Position, entity.Position);
					if (num < 1f && num < num2)
					{
						num2 = num;
						npcEntity = item;
					}
				}
			}
			if ((bool)npcEntity)
			{
				Vector3 rhs = npcEntity.transform.position - entity.transform.position;
				Vector3 vector = ((Vector3.Dot(entity.transform.right, rhs) > 0f) ? (-entity.transform.right) : entity.transform.right);
				vector *= 3f;
				if (entity.Components.TargeterComponent.Target.CombatPositionGenerator.TryGetClosestNearPosition(entity.FootPosition + vector, out nearPosition) && Vector3.Distance(nearPosition, entity.FootPosition) > 0.5f)
				{
					waitingForPath = true;
					entity.Components.Pathing.RequestPath(nearPosition, PathfindingResponseHandler);
				}
			}
		}
	}

	private void PathfindingResponseHandler(Pathfinding.Response response)
	{
		waitingForPath = false;
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
	}

	public void Stop()
	{
		entity.Components.Agent.speed = cachedSpeed;
		entity.Components.PathFollower.StopPath();
		MonoBehaviourSingleton<CombatManager>.Instance.WithdrawAttackRequest(entity.CurrentRequest);
		entity.Components.Animator.SetBool(NpcAnimator.ZLock, value: false);
	}
}
