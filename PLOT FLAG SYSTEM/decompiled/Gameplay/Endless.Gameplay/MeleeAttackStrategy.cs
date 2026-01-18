using System;
using System.Collections.Generic;
using UnityEngine;

namespace Endless.Gameplay;

public class MeleeAttackStrategy : IActionStrategy
{
	private readonly NpcEntity entity;

	private bool waitingForPath;

	private uint lastComboFrame;

	private ComboAttack attack;

	private readonly MeleeAttackComponent attackComponent;

	private bool attackIsLocked;

	public Func<float> GetCost { get; }

	public GoapAction.Status Status { get; private set; }

	public MeleeAttackStrategy(NpcEntity entity)
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
		attackIsLocked = false;
		lastComboFrame = uint.MaxValue;
		MoveToMelee();
	}

	public void Tick(uint frame)
	{
		if (!entity.Target)
		{
			Status = GoapAction.Status.Failed;
			return;
		}
		if (entity.CombatState != NpcEnum.CombatState.Attacking && !attackIsLocked)
		{
			Status = GoapAction.Status.Failed;
		}
		if (frame >= lastComboFrame)
		{
			Status = GoapAction.Status.Complete;
		}
		else if (!waitingForPath && Vector3.Distance(entity.Components.PathFollower.Path?.Destination ?? entity.FootPosition, entity.Components.TargeterComponent.Target.NavPosition) > 0.5f)
		{
			MoveToMelee();
		}
	}

	public void Stop()
	{
		entity.Components.PathFollower.StopPath();
		entity.HasAttackToken = false;
		attackComponent.ClearAttackQueue();
	}

	private void MoveToMelee()
	{
		(float, Collider) tuple = (float.MaxValue, null);
		List<Collider> targetableColliders = entity.Target.GetTargetableColliders();
		for (int i = 0; i < targetableColliders.Count; i++)
		{
			Collider collider = targetableColliders[i];
			float num = Vector3.Distance(entity.transform.position, collider.ClosestPointOnBounds(entity.transform.position));
			if (num < tuple.Item1)
			{
				tuple = (num, collider);
			}
		}
		if ((object)tuple.Item2 == null)
		{
			Status = GoapAction.Status.Failed;
			return;
		}
		Vector3 predictedPosition = entity.Target.PositionPredictions[tuple.Item2].GetPredictedPosition(0.25f);
		if (predictedPosition == Vector3.zero || !Physics.Raycast(predictedPosition + Vector3.up, Vector3.down, out var hitInfo, 10f, LayerMask.GetMask("Default")))
		{
			Status = GoapAction.Status.Failed;
			return;
		}
		if (!CombatPositionGenerator.TryGetClosestMeleePosition(entity.FootPosition, hitInfo.point, 0.75f, out var meleePosition))
		{
			Status = GoapAction.Status.Failed;
			return;
		}
		waitingForPath = true;
		if (!entity.Components.Pathing.RequestPath(meleePosition, PathfindingResponseHandler))
		{
			Status = GoapAction.Status.Failed;
		}
	}

	public void Update(float deltaTime)
	{
		if (!entity.Target)
		{
			return;
		}
		if (!entity.Components.Agent.hasPath)
		{
			Vector3 vector = entity.Target.transform.position - entity.transform.position;
			vector = new Vector3(vector.x, 0f, vector.z);
			Quaternion quaternion = Quaternion.LookRotation(vector, Vector3.up);
			if (Quaternion.Angle(entity.transform.rotation, quaternion) > 5f)
			{
				entity.transform.rotation = Quaternion.RotateTowards(entity.transform.rotation, quaternion, entity.Settings.RotationSpeed * Time.deltaTime);
			}
		}
		if (Vector3.Distance(entity.FootPosition, entity.Target.NavPosition) < 1.2f && !attackIsLocked)
		{
			LockInAttack();
		}
	}

	private void PathfindingResponseHandler(Pathfinding.Response response)
	{
		waitingForPath = false;
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

	private void LockInAttack()
	{
		attackIsLocked = true;
		attack = attackComponent.ComboAttacks[UnityEngine.Random.Range(0, attackComponent.ComboAttacks.Count)];
		entity.Components.AttackAlert.ImminentlyAttacking();
		uint num = NetClock.CurrentFrame + 10;
		for (int i = 0; i < attack.ComboSteps.Count; i++)
		{
			ComboStep comboStep = attack.ComboSteps[i];
			entity.EnqueueMeleeAttackClientRpc(num, comboStep.MeleeAttackData.TotalAttackFrameCount, comboStep.MeleeAttackIndex);
			num += (uint)(comboStep.MeleeAttackData.TotalAttackFrameCount + (int)comboStep.pauseFramesAfterAttack);
		}
		lastComboFrame = num + 5;
	}
}
