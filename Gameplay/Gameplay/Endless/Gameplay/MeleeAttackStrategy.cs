using System;
using System.Collections.Generic;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x0200017E RID: 382
	public class MeleeAttackStrategy : IActionStrategy
	{
		// Token: 0x060008A1 RID: 2209 RVA: 0x00028513 File Offset: 0x00026713
		public MeleeAttackStrategy(NpcEntity entity)
		{
			this.entity = entity;
			this.attackComponent = entity.Components.Attack as MeleeAttackComponent;
		}

		// Token: 0x170001A3 RID: 419
		// (get) Token: 0x060008A2 RID: 2210 RVA: 0x00028538 File Offset: 0x00026738
		public Func<float> GetCost { get; }

		// Token: 0x170001A4 RID: 420
		// (get) Token: 0x060008A3 RID: 2211 RVA: 0x00028540 File Offset: 0x00026740
		// (set) Token: 0x060008A4 RID: 2212 RVA: 0x00028548 File Offset: 0x00026748
		public GoapAction.Status Status { get; private set; }

		// Token: 0x060008A5 RID: 2213 RVA: 0x00028554 File Offset: 0x00026754
		public void Start()
		{
			if (!this.entity.Target || !this.attackComponent)
			{
				this.Status = GoapAction.Status.Failed;
				return;
			}
			this.Status = GoapAction.Status.InProgress;
			this.waitingForPath = false;
			this.attackIsLocked = false;
			this.lastComboFrame = uint.MaxValue;
			this.MoveToMelee();
		}

		// Token: 0x060008A6 RID: 2214 RVA: 0x000285AC File Offset: 0x000267AC
		public void Tick(uint frame)
		{
			if (!this.entity.Target)
			{
				this.Status = GoapAction.Status.Failed;
				return;
			}
			if (this.entity.CombatState != NpcEnum.CombatState.Attacking && !this.attackIsLocked)
			{
				this.Status = GoapAction.Status.Failed;
			}
			if (frame >= this.lastComboFrame)
			{
				this.Status = GoapAction.Status.Complete;
				return;
			}
			if (this.waitingForPath)
			{
				return;
			}
			NavPath path = this.entity.Components.PathFollower.Path;
			if (Vector3.Distance((path != null) ? path.Destination : this.entity.FootPosition, this.entity.Components.TargeterComponent.Target.NavPosition) > 0.5f)
			{
				this.MoveToMelee();
			}
		}

		// Token: 0x060008A7 RID: 2215 RVA: 0x00028661 File Offset: 0x00026861
		public void Stop()
		{
			this.entity.Components.PathFollower.StopPath(false);
			this.entity.HasAttackToken = false;
			this.attackComponent.ClearAttackQueue();
		}

		// Token: 0x060008A8 RID: 2216 RVA: 0x00028690 File Offset: 0x00026890
		private void MoveToMelee()
		{
			ValueTuple<float, Collider> valueTuple = new ValueTuple<float, Collider>(float.MaxValue, null);
			List<Collider> targetableColliders = this.entity.Target.GetTargetableColliders();
			for (int i = 0; i < targetableColliders.Count; i++)
			{
				Collider collider = targetableColliders[i];
				float num = Vector3.Distance(this.entity.transform.position, collider.ClosestPointOnBounds(this.entity.transform.position));
				if (num < valueTuple.Item1)
				{
					valueTuple = new ValueTuple<float, Collider>(num, collider);
				}
			}
			if (valueTuple.Item2 == null)
			{
				this.Status = GoapAction.Status.Failed;
				return;
			}
			Vector3 predictedPosition = this.entity.Target.PositionPredictions[valueTuple.Item2].GetPredictedPosition(0.25f);
			RaycastHit raycastHit;
			if (predictedPosition == Vector3.zero || !Physics.Raycast(predictedPosition + Vector3.up, Vector3.down, out raycastHit, 10f, LayerMask.GetMask(new string[] { "Default" })))
			{
				this.Status = GoapAction.Status.Failed;
				return;
			}
			Vector3 vector;
			if (!CombatPositionGenerator.TryGetClosestMeleePosition(this.entity.FootPosition, raycastHit.point, 0.75f, out vector))
			{
				this.Status = GoapAction.Status.Failed;
				return;
			}
			this.waitingForPath = true;
			if (!this.entity.Components.Pathing.RequestPath(vector, new Action<Pathfinding.Response>(this.PathfindingResponseHandler)))
			{
				this.Status = GoapAction.Status.Failed;
			}
		}

		// Token: 0x060008A9 RID: 2217 RVA: 0x000287F4 File Offset: 0x000269F4
		public void Update(float deltaTime)
		{
			if (!this.entity.Target)
			{
				return;
			}
			if (!this.entity.Components.Agent.hasPath)
			{
				Vector3 vector = this.entity.Target.transform.position - this.entity.transform.position;
				vector = new Vector3(vector.x, 0f, vector.z);
				Quaternion quaternion = Quaternion.LookRotation(vector, Vector3.up);
				if (Quaternion.Angle(this.entity.transform.rotation, quaternion) > 5f)
				{
					this.entity.transform.rotation = Quaternion.RotateTowards(this.entity.transform.rotation, quaternion, this.entity.Settings.RotationSpeed * Time.deltaTime);
				}
			}
			if (Vector3.Distance(this.entity.FootPosition, this.entity.Target.NavPosition) < 1.2f && !this.attackIsLocked)
			{
				this.LockInAttack();
			}
		}

		// Token: 0x060008AA RID: 2218 RVA: 0x0002890C File Offset: 0x00026B0C
		private void PathfindingResponseHandler(Pathfinding.Response response)
		{
			this.waitingForPath = false;
			if (response.PathfindingResult == NpcEnum.PathfindingResult.Failure)
			{
				this.Status = GoapAction.Status.Failed;
				return;
			}
			if (!this.entity.Target)
			{
				return;
			}
			this.entity.Components.PathFollower.SetPath(response.Path);
			this.entity.Components.PathFollower.LookRotationOverride = this.entity.Target.transform;
		}

		// Token: 0x060008AB RID: 2219 RVA: 0x00028984 File Offset: 0x00026B84
		private void LockInAttack()
		{
			this.attackIsLocked = true;
			this.attack = this.attackComponent.ComboAttacks[global::UnityEngine.Random.Range(0, this.attackComponent.ComboAttacks.Count)];
			this.entity.Components.AttackAlert.ImminentlyAttacking(false);
			uint num = NetClock.CurrentFrame + 10U;
			for (int i = 0; i < this.attack.ComboSteps.Count; i++)
			{
				ComboStep comboStep = this.attack.ComboSteps[i];
				this.entity.EnqueueMeleeAttackClientRpc(num, comboStep.MeleeAttackData.TotalAttackFrameCount, comboStep.MeleeAttackIndex);
				num += (uint)(comboStep.MeleeAttackData.TotalAttackFrameCount + (int)comboStep.pauseFramesAfterAttack);
			}
			this.lastComboFrame = num + 5U;
		}

		// Token: 0x04000725 RID: 1829
		private readonly NpcEntity entity;

		// Token: 0x04000726 RID: 1830
		private bool waitingForPath;

		// Token: 0x04000727 RID: 1831
		private uint lastComboFrame;

		// Token: 0x04000728 RID: 1832
		private ComboAttack attack;

		// Token: 0x04000729 RID: 1833
		private readonly MeleeAttackComponent attackComponent;

		// Token: 0x0400072A RID: 1834
		private bool attackIsLocked;
	}
}
