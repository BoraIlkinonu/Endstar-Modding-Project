using System;
using System.Collections.Generic;
using Endless.Gameplay.LuaEnums;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

namespace Endless.Gameplay
{
	// Token: 0x02000186 RID: 390
	public class StrafeStrategy : IActionStrategy
	{
		// Token: 0x060008E3 RID: 2275 RVA: 0x00029A60 File Offset: 0x00027C60
		public StrafeStrategy(NpcEntity entity)
		{
			this.entity = entity;
			this.GetCost = new Func<float>(this.GetCost_Imp);
			this.timer = new CountdownTimer(2f);
			CountdownTimer countdownTimer = this.timer;
			countdownTimer.OnTimerStart = (Action)Delegate.Combine(countdownTimer.OnTimerStart, new Action(delegate
			{
				this.Status = GoapAction.Status.InProgress;
			}));
			CountdownTimer countdownTimer2 = this.timer;
			countdownTimer2.OnTimerStop = (Action)Delegate.Combine(countdownTimer2.OnTimerStop, new Action(delegate
			{
				this.Status = GoapAction.Status.Complete;
			}));
		}

		// Token: 0x170001B1 RID: 433
		// (get) Token: 0x060008E4 RID: 2276 RVA: 0x00029AEA File Offset: 0x00027CEA
		public Func<float> GetCost { get; }

		// Token: 0x170001B2 RID: 434
		// (get) Token: 0x060008E5 RID: 2277 RVA: 0x00029AF2 File Offset: 0x00027CF2
		// (set) Token: 0x060008E6 RID: 2278 RVA: 0x00029AFA File Offset: 0x00027CFA
		public GoapAction.Status Status { get; private set; }

		// Token: 0x060008E7 RID: 2279 RVA: 0x00029B04 File Offset: 0x00027D04
		public void Start()
		{
			this.timer.Reset(math.lerp(2f, 5f, global::UnityEngine.Random.value));
			this.timer.Start();
			this.movingRight = global::UnityEngine.Random.value > 0.5f;
			this.entity.Components.PathFollower.StopPath(false);
			this.entity.Components.Animator.SetBool(NpcAnimator.ZLock, true);
			this.cachedSpeed = this.entity.Components.Agent.speed;
			this.entity.Components.Agent.speed = this.entity.Settings.StrafingSpeed;
			this.SetNextStrafeTime();
			if (this.entity.NpcClass.NpcClass == NpcClass.Rifleman)
			{
				this.entity.NpcBlackboard.Set<float>(NpcBlackboard.Key.OverrideSpeed, this.cachedSpeed);
			}
		}

		// Token: 0x060008E8 RID: 2280 RVA: 0x00029BF0 File Offset: 0x00027DF0
		public void Tick(uint frame)
		{
			if (!this.entity.Target || this.entity.CombatState != NpcEnum.CombatState.Engaged)
			{
				this.Status = GoapAction.Status.Failed;
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

		// Token: 0x060008E9 RID: 2281 RVA: 0x00029CF5 File Offset: 0x00027EF5
		public void Update(float deltaTime)
		{
			this.timer.Tick(deltaTime);
		}

		// Token: 0x060008EA RID: 2282 RVA: 0x00029D04 File Offset: 0x00027F04
		public void Stop()
		{
			this.SetNextStrafeTime();
			this.entity.Components.PathFollower.StopPath(false);
			this.entity.Components.Animator.SetBool(NpcAnimator.ZLock, false);
			this.entity.Components.Agent.speed = this.cachedSpeed;
			this.entity.NpcBlackboard.Clear<float>(NpcBlackboard.Key.OverrideSpeed);
		}

		// Token: 0x060008EB RID: 2283 RVA: 0x00029D75 File Offset: 0x00027F75
		private float GetCost_Imp()
		{
			if (Time.time <= this.nextStrafeTime)
			{
				return float.MaxValue;
			}
			return 0f;
		}

		// Token: 0x060008EC RID: 2284 RVA: 0x00029D8F File Offset: 0x00027F8F
		private void SetNextStrafeTime()
		{
			this.nextStrafeTime = Time.time + 3f + math.lerp(0f, 7f, global::UnityEngine.Random.value);
		}

		// Token: 0x04000751 RID: 1873
		private const float strafeTimeFloor = 2f;

		// Token: 0x04000752 RID: 1874
		private const float strafeTimeCeiling = 5f;

		// Token: 0x04000753 RID: 1875
		private float nextStrafeTime;

		// Token: 0x04000754 RID: 1876
		private readonly NpcEntity entity;

		// Token: 0x04000755 RID: 1877
		private readonly CountdownTimer timer;

		// Token: 0x04000756 RID: 1878
		private bool movingRight;

		// Token: 0x04000757 RID: 1879
		private float cachedSpeed;
	}
}
