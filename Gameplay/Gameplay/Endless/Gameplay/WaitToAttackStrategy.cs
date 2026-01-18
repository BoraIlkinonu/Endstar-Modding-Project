using System;
using System.Collections.Generic;
using Endless.Shared;
using Unity.Mathematics;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000188 RID: 392
	public class WaitToAttackStrategy : IActionStrategy
	{
		// Token: 0x060008FB RID: 2299 RVA: 0x00029FF8 File Offset: 0x000281F8
		public WaitToAttackStrategy(NpcEntity entity)
		{
			this.entity = entity;
		}

		// Token: 0x170001B6 RID: 438
		// (get) Token: 0x060008FC RID: 2300 RVA: 0x0002A007 File Offset: 0x00028207
		public Func<float> GetCost { get; }

		// Token: 0x170001B7 RID: 439
		// (get) Token: 0x060008FD RID: 2301 RVA: 0x0002A00F File Offset: 0x0002820F
		// (set) Token: 0x060008FE RID: 2302 RVA: 0x0002A017 File Offset: 0x00028217
		public GoapAction.Status Status { get; private set; }

		// Token: 0x060008FF RID: 2303 RVA: 0x0002A020 File Offset: 0x00028220
		public void Start()
		{
			if (!this.entity.Target)
			{
				this.Status = GoapAction.Status.Failed;
				return;
			}
			this.Status = GoapAction.Status.InProgress;
			this.cachedSpeed = this.entity.Components.Agent.speed;
			this.entity.Components.Agent.speed = this.entity.Settings.StrafingSpeed;
			this.entity.Components.Animator.SetBool(NpcAnimator.ZLock, true);
			this.entity.CurrentRequest = new AttackRequest(this.entity.WorldObject, this.entity.Target, delegate(WorldObject worldObject)
			{
				worldObject.GetUserComponent<NpcEntity>().HasAttackToken = true;
			});
			MonoBehaviourSingleton<CombatManager>.Instance.SubmitAttackRequest(this.entity.CurrentRequest);
		}

		// Token: 0x06000900 RID: 2304 RVA: 0x0002A104 File Offset: 0x00028304
		public void Tick(uint frame)
		{
			if (!this.entity.Target)
			{
				this.Status = GoapAction.Status.Failed;
				return;
			}
			if (this.entity.HasAttackToken)
			{
				this.Status = GoapAction.Status.Complete;
				return;
			}
			float num = math.distance(this.entity.FootPosition, this.entity.Target.NavPosition);
			if (num > this.entity.NearDistance + 0.2f)
			{
				this.Status = GoapAction.Status.Failed;
				return;
			}
			if (this.entity.Components.PathFollower == null || this.waitingForPath)
			{
				return;
			}
			Vector3 vector;
			if (num < this.entity.MeleeDistance && this.entity.Target.CombatPositionGenerator.TryGetClosestNearPosition(this.entity.transform.position, out vector))
			{
				this.waitingForPath = true;
				this.entity.Components.Pathing.RequestPath(vector, new Action<Pathfinding.Response>(this.PathfindingResponseHandler));
				return;
			}
			IEnumerable<NpcEntity> npcs = MonoBehaviourSingleton<NpcManager>.Instance.Npcs;
			float num2 = float.MaxValue;
			NpcEntity npcEntity = null;
			foreach (NpcEntity npcEntity2 in npcs)
			{
				if (!(this.entity == npcEntity2))
				{
					num = math.distance(npcEntity2.Position, this.entity.Position);
					if (num < 1f && num < num2)
					{
						num2 = num;
						npcEntity = npcEntity2;
					}
				}
			}
			if (!npcEntity)
			{
				return;
			}
			Vector3 vector2 = npcEntity.transform.position - this.entity.transform.position;
			Vector3 vector3 = ((Vector3.Dot(this.entity.transform.right, vector2) > 0f) ? (-this.entity.transform.right) : this.entity.transform.right);
			vector3 *= 3f;
			if (this.entity.Components.TargeterComponent.Target.CombatPositionGenerator.TryGetClosestNearPosition(this.entity.FootPosition + vector3, out vector) && Vector3.Distance(vector, this.entity.FootPosition) > 0.5f)
			{
				this.waitingForPath = true;
				this.entity.Components.Pathing.RequestPath(vector, new Action<Pathfinding.Response>(this.PathfindingResponseHandler));
			}
		}

		// Token: 0x06000901 RID: 2305 RVA: 0x0002A38C File Offset: 0x0002858C
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
				this.Status = GoapAction.Status.Failed;
				return;
			}
			this.entity.Components.PathFollower.SetPath(response.Path);
			this.entity.Components.PathFollower.LookRotationOverride = this.entity.Target.transform;
		}

		// Token: 0x06000902 RID: 2306 RVA: 0x0002A40C File Offset: 0x0002860C
		public void Stop()
		{
			this.entity.Components.Agent.speed = this.cachedSpeed;
			this.entity.Components.PathFollower.StopPath(false);
			MonoBehaviourSingleton<CombatManager>.Instance.WithdrawAttackRequest(this.entity.CurrentRequest);
			this.entity.Components.Animator.SetBool(NpcAnimator.ZLock, false);
		}

		// Token: 0x0400075F RID: 1887
		private readonly NpcEntity entity;

		// Token: 0x04000760 RID: 1888
		private float cachedSpeed;

		// Token: 0x04000761 RID: 1889
		private bool waitingForPath;
	}
}
