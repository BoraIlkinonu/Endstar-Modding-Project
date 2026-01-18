using System;
using System.Collections.Generic;
using UnityEngine;

namespace Endless.Gameplay;

public class ZombieMeleeAttackStrategy : IActionStrategy
{
	private readonly NpcEntity entity;

	private bool waitingForPath;

	private uint lastComboFrame;

	private ComboAttack attack;

	private readonly MeleeAttackComponent attackComponent;

	public Func<float> GetCost { get; }

	public GoapAction.Status Status { get; private set; }

	public ZombieMeleeAttackStrategy(NpcEntity entity)
	{
		this.entity = entity;
		attackComponent = entity.Components.Attack as MeleeAttackComponent;
	}

	public void Start()
	{
		if (!entity.Target || !attackComponent)
		{
			Status = GoapAction.Status.Failed;
			return;
		}
		Status = GoapAction.Status.InProgress;
		waitingForPath = false;
		attack = attackComponent.ComboAttacks[UnityEngine.Random.Range(0, attackComponent.ComboAttacks.Count)];
		MoveToMelee();
		uint num = NetClock.CurrentFrame + 10;
		for (int i = 0; i < attack.ComboSteps.Count; i++)
		{
			ComboStep comboStep = attack.ComboSteps[i];
			entity.EnqueueMeleeAttackClientRpc(num, comboStep.MeleeAttackData.TotalAttackFrameCount, comboStep.MeleeAttackIndex);
			num += (uint)(comboStep.MeleeAttackData.TotalAttackFrameCount + (int)comboStep.pauseFramesAfterAttack);
		}
		lastComboFrame = num + 5;
	}

	public void Tick(uint frame)
	{
		if (!entity.Target)
		{
			Status = GoapAction.Status.Failed;
		}
		else if (frame >= lastComboFrame)
		{
			Status = GoapAction.Status.Complete;
		}
		else if (!waitingForPath && Vector3.Distance(entity.Components.PathFollower.Path?.Destination ?? entity.transform.position, entity.Components.TargeterComponent.Target.Position) > entity.NearDistance)
		{
			MoveToMelee();
		}
	}

	public void Update(float deltaTime)
	{
		if (!entity.Components.Agent.hasPath && (bool)entity.Target)
		{
			Vector3 vector = entity.Target.transform.position - entity.transform.position;
			vector = new Vector3(vector.x, 0f, vector.z);
			Quaternion quaternion = Quaternion.LookRotation(vector, Vector3.up);
			if (Quaternion.Angle(entity.transform.rotation, quaternion) > 5f)
			{
				entity.transform.rotation = Quaternion.RotateTowards(entity.transform.rotation, quaternion, entity.Settings.RotationSpeed * Time.deltaTime);
			}
		}
	}

	public void Stop()
	{
		entity.Components.PathFollower.StopPath();
		attackComponent.ClearAttackQueue();
	}

	private void MoveToMelee()
	{
		(float, Vector3) tuple = (float.MaxValue, Vector3.zero);
		List<TargetDatum> targetableColliderData = entity.Target.GetTargetableColliderData();
		for (int i = 0; i < targetableColliderData.Count; i++)
		{
			TargetDatum targetDatum = targetableColliderData[i];
			float num = Vector3.Distance(entity.transform.position, targetDatum.Position);
			if (num < tuple.Item1)
			{
				tuple = (num, targetDatum.Position);
			}
		}
		if (tuple.Item2 == Vector3.zero || !CombatPositionGenerator.TryGetClosestMeleePosition(entity.Position, tuple.Item2, 1f, out var meleePosition))
		{
			Status = GoapAction.Status.Failed;
			return;
		}
		waitingForPath = true;
		entity.Components.Pathing.RequestPath(meleePosition, PathfindingResponseHandler);
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
		}
	}
}
