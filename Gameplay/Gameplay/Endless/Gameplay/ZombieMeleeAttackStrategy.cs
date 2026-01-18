using System;
using System.Collections.Generic;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x0200018A RID: 394
	public class ZombieMeleeAttackStrategy : IActionStrategy
	{
		// Token: 0x06000906 RID: 2310 RVA: 0x0002A486 File Offset: 0x00028686
		public ZombieMeleeAttackStrategy(NpcEntity entity)
		{
			this.entity = entity;
			this.attackComponent = entity.Components.Attack as MeleeAttackComponent;
		}

		// Token: 0x170001B8 RID: 440
		// (get) Token: 0x06000907 RID: 2311 RVA: 0x0002A4AB File Offset: 0x000286AB
		public Func<float> GetCost { get; }

		// Token: 0x170001B9 RID: 441
		// (get) Token: 0x06000908 RID: 2312 RVA: 0x0002A4B3 File Offset: 0x000286B3
		// (set) Token: 0x06000909 RID: 2313 RVA: 0x0002A4BB File Offset: 0x000286BB
		public GoapAction.Status Status { get; private set; }

		// Token: 0x0600090A RID: 2314 RVA: 0x0002A4C4 File Offset: 0x000286C4
		public void Start()
		{
			if (!this.entity.Target || !this.attackComponent)
			{
				this.Status = GoapAction.Status.Failed;
				return;
			}
			this.Status = GoapAction.Status.InProgress;
			this.waitingForPath = false;
			this.attack = this.attackComponent.ComboAttacks[global::UnityEngine.Random.Range(0, this.attackComponent.ComboAttacks.Count)];
			this.MoveToMelee();
			uint num = NetClock.CurrentFrame + 10U;
			for (int i = 0; i < this.attack.ComboSteps.Count; i++)
			{
				ComboStep comboStep = this.attack.ComboSteps[i];
				this.entity.EnqueueMeleeAttackClientRpc(num, comboStep.MeleeAttackData.TotalAttackFrameCount, comboStep.MeleeAttackIndex);
				num += (uint)(comboStep.MeleeAttackData.TotalAttackFrameCount + (int)comboStep.pauseFramesAfterAttack);
			}
			this.lastComboFrame = num + 5U;
		}

		// Token: 0x0600090B RID: 2315 RVA: 0x0002A5AC File Offset: 0x000287AC
		public void Tick(uint frame)
		{
			if (!this.entity.Target)
			{
				this.Status = GoapAction.Status.Failed;
				return;
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
			if (Vector3.Distance((path != null) ? path.Destination : this.entity.transform.position, this.entity.Components.TargeterComponent.Target.Position) > this.entity.NearDistance)
			{
				this.MoveToMelee();
			}
		}

		// Token: 0x0600090C RID: 2316 RVA: 0x0002A650 File Offset: 0x00028850
		public void Update(float deltaTime)
		{
			if (!this.entity.Components.Agent.hasPath && this.entity.Target)
			{
				Vector3 vector = this.entity.Target.transform.position - this.entity.transform.position;
				vector = new Vector3(vector.x, 0f, vector.z);
				Quaternion quaternion = Quaternion.LookRotation(vector, Vector3.up);
				if (Quaternion.Angle(this.entity.transform.rotation, quaternion) > 5f)
				{
					this.entity.transform.rotation = Quaternion.RotateTowards(this.entity.transform.rotation, quaternion, this.entity.Settings.RotationSpeed * Time.deltaTime);
				}
			}
		}

		// Token: 0x0600090D RID: 2317 RVA: 0x0002A734 File Offset: 0x00028934
		public void Stop()
		{
			this.entity.Components.PathFollower.StopPath(false);
			this.attackComponent.ClearAttackQueue();
		}

		// Token: 0x0600090E RID: 2318 RVA: 0x0002A758 File Offset: 0x00028958
		private void MoveToMelee()
		{
			ValueTuple<float, Vector3> valueTuple = new ValueTuple<float, Vector3>(float.MaxValue, Vector3.zero);
			List<TargetDatum> targetableColliderData = this.entity.Target.GetTargetableColliderData();
			for (int i = 0; i < targetableColliderData.Count; i++)
			{
				TargetDatum targetDatum = targetableColliderData[i];
				float num = Vector3.Distance(this.entity.transform.position, targetDatum.Position);
				if (num < valueTuple.Item1)
				{
					valueTuple = new ValueTuple<float, Vector3>(num, targetDatum.Position);
				}
			}
			Vector3 vector;
			if (valueTuple.Item2 == Vector3.zero || !CombatPositionGenerator.TryGetClosestMeleePosition(this.entity.Position, valueTuple.Item2, 1f, out vector))
			{
				this.Status = GoapAction.Status.Failed;
				return;
			}
			this.waitingForPath = true;
			this.entity.Components.Pathing.RequestPath(vector, new Action<Pathfinding.Response>(this.PathfindingResponseHandler));
		}

		// Token: 0x0600090F RID: 2319 RVA: 0x0002A83C File Offset: 0x00028A3C
		private void PathfindingResponseHandler(Pathfinding.Response response)
		{
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

		// Token: 0x04000766 RID: 1894
		private readonly NpcEntity entity;

		// Token: 0x04000767 RID: 1895
		private bool waitingForPath;

		// Token: 0x04000768 RID: 1896
		private uint lastComboFrame;

		// Token: 0x04000769 RID: 1897
		private ComboAttack attack;

		// Token: 0x0400076A RID: 1898
		private readonly MeleeAttackComponent attackComponent;
	}
}
