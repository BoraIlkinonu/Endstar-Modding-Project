using System;
using System.Collections.Generic;
using Endless.Shared;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

namespace Endless.Gameplay
{
	// Token: 0x02000183 RID: 387
	public class RangedWaitToAttackStrategy : IActionStrategy
	{
		// Token: 0x060008D0 RID: 2256 RVA: 0x000294A4 File Offset: 0x000276A4
		public RangedWaitToAttackStrategy(NpcEntity entity)
		{
			this.entity = entity;
			this.timer = new CountdownTimer(global::UnityEngine.Random.Range(1f, 3f));
			CountdownTimer countdownTimer = this.timer;
			countdownTimer.OnTimerStop = (Action)Delegate.Combine(countdownTimer.OnTimerStop, new Action(delegate
			{
				this.movingRight = !this.movingRight;
				this.timer.Time = global::UnityEngine.Random.Range(1f, 3f);
				this.timer.Reset();
				this.timer.Start();
			}));
		}

		// Token: 0x170001AD RID: 429
		// (get) Token: 0x060008D1 RID: 2257 RVA: 0x000294FF File Offset: 0x000276FF
		public Func<float> GetCost { get; }

		// Token: 0x170001AE RID: 430
		// (get) Token: 0x060008D2 RID: 2258 RVA: 0x00029507 File Offset: 0x00027707
		// (set) Token: 0x060008D3 RID: 2259 RVA: 0x0002950F File Offset: 0x0002770F
		public GoapAction.Status Status { get; private set; }

		// Token: 0x060008D4 RID: 2260 RVA: 0x00029518 File Offset: 0x00027718
		public void Start()
		{
			if (!this.entity.Target)
			{
				this.Status = GoapAction.Status.Failed;
				return;
			}
			this.timer.Start();
			this.Status = GoapAction.Status.InProgress;
			this.cachedSpeed = this.entity.Components.Agent.speed;
			this.entity.Components.Agent.speed = this.entity.Settings.StrafingSpeed;
			this.entity.Components.Animator.SetBool(NpcAnimator.ZLock, true);
			this.entity.CurrentRequest = new AttackRequest(this.entity.WorldObject, this.entity.Target, delegate(WorldObject worldObject)
			{
				worldObject.GetUserComponent<NpcEntity>().HasAttackToken = true;
			});
			MonoBehaviourSingleton<CombatManager>.Instance.SubmitAttackRequest(this.entity.CurrentRequest);
			this.entity.NpcBlackboard.Set<float>(NpcBlackboard.Key.OverrideSpeed, this.cachedSpeed);
		}

		// Token: 0x060008D5 RID: 2261 RVA: 0x00029620 File Offset: 0x00027820
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
			if (num > this.entity.MaxRangedAttackDistance + 0.2f)
			{
				this.Status = GoapAction.Status.Failed;
				return;
			}
			if (this.entity.Components.PathFollower == null || this.waitingForPath)
			{
				return;
			}
			Vector3 vector;
			if (num < this.entity.NearDistance && this.entity.Target.CombatPositionGenerator.TryGetClosestAroundPosition(this.entity.transform.position, out vector))
			{
				this.waitingForPath = true;
				this.entity.Components.Pathing.RequestPath(vector, new Action<Pathfinding.Response>(this.PathfindingResponseHandler));
				return;
			}
			Transform transform = this.entity.transform;
			Vector3 position = transform.position;
			NavMeshHit navMeshHit;
			if (NavMesh.SamplePosition(this.movingRight ? (position + transform.right) : (position - transform.right), out navMeshHit, 1f, -1))
			{
				Queue<NavPath.Segment> queue = new Queue<NavPath.Segment>();
				queue.Enqueue(new NavPath.Segment
				{
					ConnectionKind = ConnectionKind.Walk,
					EndPosition = navMeshHit.position
				});
				NavPath navPath = new NavPath(navMeshHit.position, queue);
				this.entity.Components.PathFollower.SetPath(navPath);
				this.entity.Components.PathFollower.LookRotationOverride = this.entity.Target.transform;
				return;
			}
			this.movingRight = !this.movingRight;
		}

		// Token: 0x060008D6 RID: 2262 RVA: 0x000297F7 File Offset: 0x000279F7
		public void Update(float deltaTime)
		{
			this.timer.Tick(deltaTime);
		}

		// Token: 0x060008D7 RID: 2263 RVA: 0x00029808 File Offset: 0x00027A08
		private void PathfindingResponseHandler(Pathfinding.Response response)
		{
			this.waitingForPath = false;
			if (response.PathfindingResult == NpcEnum.PathfindingResult.Success)
			{
				if (!this.entity.Target)
				{
					return;
				}
				this.entity.Components.PathFollower.SetPath(response.Path);
				this.entity.Components.PathFollower.LookRotationOverride = this.entity.Target.transform;
			}
		}

		// Token: 0x060008D8 RID: 2264 RVA: 0x00029878 File Offset: 0x00027A78
		public void Stop()
		{
			this.entity.Components.Agent.speed = this.cachedSpeed;
			this.entity.Components.PathFollower.StopPath(false);
			MonoBehaviourSingleton<CombatManager>.Instance.WithdrawAttackRequest(this.entity.CurrentRequest);
			this.entity.Components.Animator.SetBool(NpcAnimator.ZLock, false);
			this.entity.NpcBlackboard.Clear<float>(NpcBlackboard.Key.OverrideSpeed);
		}

		// Token: 0x04000741 RID: 1857
		private readonly NpcEntity entity;

		// Token: 0x04000742 RID: 1858
		private float cachedSpeed;

		// Token: 0x04000743 RID: 1859
		private bool waitingForPath;

		// Token: 0x04000744 RID: 1860
		private bool movingRight;

		// Token: 0x04000745 RID: 1861
		private readonly CountdownTimer timer;
	}
}
