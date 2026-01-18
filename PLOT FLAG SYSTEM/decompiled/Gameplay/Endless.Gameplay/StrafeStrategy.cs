using System;
using System.Collections.Generic;
using Endless.Gameplay.LuaEnums;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

namespace Endless.Gameplay;

public class StrafeStrategy : IActionStrategy
{
	private const float strafeTimeFloor = 2f;

	private const float strafeTimeCeiling = 5f;

	private float nextStrafeTime;

	private readonly NpcEntity entity;

	private readonly CountdownTimer timer;

	private bool movingRight;

	private float cachedSpeed;

	public Func<float> GetCost { get; }

	public GoapAction.Status Status { get; private set; }

	public StrafeStrategy(NpcEntity entity)
	{
		this.entity = entity;
		GetCost = GetCost_Imp;
		timer = new CountdownTimer(2f);
		CountdownTimer countdownTimer = timer;
		countdownTimer.OnTimerStart = (Action)Delegate.Combine(countdownTimer.OnTimerStart, (Action)delegate
		{
			Status = GoapAction.Status.InProgress;
		});
		CountdownTimer countdownTimer2 = timer;
		countdownTimer2.OnTimerStop = (Action)Delegate.Combine(countdownTimer2.OnTimerStop, (Action)delegate
		{
			Status = GoapAction.Status.Complete;
		});
	}

	public void Start()
	{
		timer.Reset(math.lerp(2f, 5f, UnityEngine.Random.value));
		timer.Start();
		movingRight = UnityEngine.Random.value > 0.5f;
		entity.Components.PathFollower.StopPath();
		entity.Components.Animator.SetBool(NpcAnimator.ZLock, value: true);
		cachedSpeed = entity.Components.Agent.speed;
		entity.Components.Agent.speed = entity.Settings.StrafingSpeed;
		SetNextStrafeTime();
		if (entity.NpcClass.NpcClass == NpcClass.Rifleman)
		{
			entity.NpcBlackboard.Set(NpcBlackboard.Key.OverrideSpeed, cachedSpeed);
		}
	}

	public void Tick(uint frame)
	{
		if (!entity.Target || entity.CombatState != NpcEnum.CombatState.Engaged)
		{
			Status = GoapAction.Status.Failed;
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

	public void Update(float deltaTime)
	{
		timer.Tick(deltaTime);
	}

	public void Stop()
	{
		SetNextStrafeTime();
		entity.Components.PathFollower.StopPath();
		entity.Components.Animator.SetBool(NpcAnimator.ZLock, value: false);
		entity.Components.Agent.speed = cachedSpeed;
		entity.NpcBlackboard.Clear<float>(NpcBlackboard.Key.OverrideSpeed);
	}

	private float GetCost_Imp()
	{
		if (!(Time.time > nextStrafeTime))
		{
			return float.MaxValue;
		}
		return 0f;
	}

	private void SetNextStrafeTime()
	{
		nextStrafeTime = Time.time + 3f + math.lerp(0f, 7f, UnityEngine.Random.value);
	}
}
