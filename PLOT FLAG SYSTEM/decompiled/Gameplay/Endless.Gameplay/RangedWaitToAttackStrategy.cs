using System;
using System.Collections.Generic;
using Endless.Shared;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

namespace Endless.Gameplay;

public class RangedWaitToAttackStrategy : IActionStrategy
{
	private readonly NpcEntity entity;

	private float cachedSpeed;

	private bool waitingForPath;

	private bool movingRight;

	private readonly CountdownTimer timer;

	public Func<float> GetCost { get; }

	public GoapAction.Status Status { get; private set; }

	public RangedWaitToAttackStrategy(NpcEntity entity)
	{
		this.entity = entity;
		timer = new CountdownTimer(UnityEngine.Random.Range(1f, 3f));
		CountdownTimer countdownTimer = timer;
		countdownTimer.OnTimerStop = (Action)Delegate.Combine(countdownTimer.OnTimerStop, (Action)delegate
		{
			movingRight = !movingRight;
			timer.Time = UnityEngine.Random.Range(1f, 3f);
			timer.Reset();
			timer.Start();
		});
	}

	public void Start()
	{
		if (!entity.Target)
		{
			Status = GoapAction.Status.Failed;
			return;
		}
		timer.Start();
		Status = GoapAction.Status.InProgress;
		cachedSpeed = entity.Components.Agent.speed;
		entity.Components.Agent.speed = entity.Settings.StrafingSpeed;
		entity.Components.Animator.SetBool(NpcAnimator.ZLock, value: true);
		entity.CurrentRequest = new AttackRequest(entity.WorldObject, entity.Target, delegate(WorldObject worldObject)
		{
			worldObject.GetUserComponent<NpcEntity>().HasAttackToken = true;
		});
		MonoBehaviourSingleton<CombatManager>.Instance.SubmitAttackRequest(entity.CurrentRequest);
		entity.NpcBlackboard.Set(NpcBlackboard.Key.OverrideSpeed, cachedSpeed);
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
		if (num > entity.MaxRangedAttackDistance + 0.2f)
		{
			Status = GoapAction.Status.Failed;
		}
		else
		{
			if (entity.Components.PathFollower == null || waitingForPath)
			{
				return;
			}
			if (num < entity.NearDistance && entity.Target.CombatPositionGenerator.TryGetClosestAroundPosition(entity.transform.position, out var aroundPosition))
			{
				waitingForPath = true;
				entity.Components.Pathing.RequestPath(aroundPosition, PathfindingResponseHandler);
				return;
			}
			Transform transform = entity.transform;
			Vector3 position = transform.position;
			if (NavMesh.SamplePosition(movingRight ? (position + transform.right) : (position - transform.right), out var hit, 1f, -1))
			{
				Queue<NavPath.Segment> queue = new Queue<NavPath.Segment>();
				queue.Enqueue(new NavPath.Segment
				{
					ConnectionKind = ConnectionKind.Walk,
					EndPosition = hit.position
				});
				NavPath path = new NavPath(hit.position, queue);
				entity.Components.PathFollower.SetPath(path);
				entity.Components.PathFollower.LookRotationOverride = entity.Target.transform;
			}
			else
			{
				movingRight = !movingRight;
			}
		}
	}

	public void Update(float deltaTime)
	{
		timer.Tick(deltaTime);
	}

	private void PathfindingResponseHandler(Pathfinding.Response response)
	{
		waitingForPath = false;
		if (response.PathfindingResult == NpcEnum.PathfindingResult.Success && (bool)entity.Target)
		{
			entity.Components.PathFollower.SetPath(response.Path);
			entity.Components.PathFollower.LookRotationOverride = entity.Target.transform;
		}
	}

	public void Stop()
	{
		entity.Components.Agent.speed = cachedSpeed;
		entity.Components.PathFollower.StopPath();
		MonoBehaviourSingleton<CombatManager>.Instance.WithdrawAttackRequest(entity.CurrentRequest);
		entity.Components.Animator.SetBool(NpcAnimator.ZLock, value: false);
		entity.NpcBlackboard.Clear<float>(NpcBlackboard.Key.OverrideSpeed);
	}
}
