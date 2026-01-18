using System;
using UnityEngine;

namespace Endless.Gameplay;

public class RangedAttackStrategy : IActionStrategy
{
	private readonly NpcEntity entity;

	private readonly CountdownTimer timer;

	private bool movingRight;

	private uint lastAttackFrame;

	private float cachedSpeed;

	public Func<float> GetCost { get; }

	public GoapAction.Status Status { get; private set; }

	public RangedAttackStrategy(NpcEntity entity)
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
		entity.Components.PathFollower.StopPath();
		entity.Components.Animator.SetBool(NpcAnimator.ZLock, value: true);
		uint num = NetClock.CurrentFrame + 10 + 5;
		((RangedAttackComponent)entity.Components.Attack).EnqueueRangedAttack(num, entity.RangedAttackFrames);
		lastAttackFrame = num + entity.RangedAttackFrames + 15;
	}

	public void Tick(uint frame)
	{
		if (!entity.Target)
		{
			Status = GoapAction.Status.Failed;
		}
		else if (frame >= lastAttackFrame)
		{
			Status = GoapAction.Status.Complete;
		}
	}

	public void Update(float deltaTime)
	{
		if (!entity.Target)
		{
			Status = GoapAction.Status.Failed;
			return;
		}
		Vector3 vector = entity.Target.transform.position - entity.transform.position;
		vector = new Vector3(vector.x, 0f, vector.z);
		Quaternion quaternion = Quaternion.LookRotation(vector, Vector3.up);
		if (!(Quaternion.Angle(entity.transform.rotation, quaternion) < 5f))
		{
			entity.transform.rotation = Quaternion.RotateTowards(entity.transform.rotation, quaternion, entity.Settings.RotationSpeed * deltaTime);
		}
	}

	public void Stop()
	{
		entity.Components.Animator.SetBool(NpcAnimator.ZLock, value: false);
		entity.HasAttackToken = false;
	}
}
