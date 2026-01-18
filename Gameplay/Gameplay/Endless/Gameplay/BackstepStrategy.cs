using System;
using UnityEngine;
using UnityEngine.AI;

namespace Endless.Gameplay
{
	// Token: 0x02000171 RID: 369
	public class BackstepStrategy : IActionStrategy
	{
		// Token: 0x06000839 RID: 2105 RVA: 0x000272B3 File Offset: 0x000254B3
		public BackstepStrategy(NpcEntity entity)
		{
			this.entity = entity;
		}

		// Token: 0x1700018B RID: 395
		// (get) Token: 0x0600083A RID: 2106 RVA: 0x000272C2 File Offset: 0x000254C2
		public Func<float> GetCost { get; }

		// Token: 0x1700018C RID: 396
		// (get) Token: 0x0600083B RID: 2107 RVA: 0x000272CA File Offset: 0x000254CA
		// (set) Token: 0x0600083C RID: 2108 RVA: 0x000272D2 File Offset: 0x000254D2
		public GoapAction.Status Status { get; private set; }

		// Token: 0x0600083D RID: 2109 RVA: 0x000272DC File Offset: 0x000254DC
		public void Start()
		{
			this.Status = GoapAction.Status.InProgress;
			this.entity.Components.Animator.SetBool(NpcAnimator.ZLock, true);
			Vector3 navPosition = this.entity.Target.NavPosition;
			NavMeshHit navMeshHit;
			if (NavMesh.SamplePosition(navPosition + (this.entity.FootPosition - navPosition).normalized * 1.25f, out navMeshHit, 0.5f, -1))
			{
				this.entity.Components.Pathing.RequestPath(navMeshHit.position, new Action<Pathfinding.Response>(this.PathfindingResponseHandler));
				return;
			}
			this.Status = GoapAction.Status.Failed;
		}

		// Token: 0x0600083E RID: 2110 RVA: 0x00027385 File Offset: 0x00025585
		public void Tick(uint frame)
		{
			if (!this.entity.Target)
			{
				this.Status = GoapAction.Status.Failed;
			}
		}

		// Token: 0x0600083F RID: 2111 RVA: 0x000273A0 File Offset: 0x000255A0
		public void Stop()
		{
			this.entity.Components.Animator.SetBool(NpcAnimator.ZLock, false);
			this.entity.Components.PathFollower.StopPath(false);
			this.entity.Components.PathFollower.OnPathFinished -= this.HandleOnPathFinished;
			this.entity.NpcBlackboard.Set<float>(NpcBlackboard.Key.BoredomTime, 0f);
		}

		// Token: 0x06000840 RID: 2112 RVA: 0x00027418 File Offset: 0x00025618
		private void PathfindingResponseHandler(Pathfinding.Response obj)
		{
			if (obj.PathfindingResult == NpcEnum.PathfindingResult.Failure)
			{
				this.Status = GoapAction.Status.Failed;
				return;
			}
			if (!this.entity.Target)
			{
				this.Status = GoapAction.Status.Failed;
				return;
			}
			this.entity.Components.PathFollower.SetPath(obj.Path);
			this.entity.Components.PathFollower.LookRotationOverride = this.entity.Target.transform;
			this.entity.Components.PathFollower.OnPathFinished -= this.HandleOnPathFinished;
			this.entity.Components.PathFollower.OnPathFinished += this.HandleOnPathFinished;
		}

		// Token: 0x06000841 RID: 2113 RVA: 0x000274D1 File Offset: 0x000256D1
		private void HandleOnPathFinished(bool complete)
		{
			this.Status = (complete ? GoapAction.Status.Complete : GoapAction.Status.Failed);
		}

		// Token: 0x040006E5 RID: 1765
		private readonly NpcEntity entity;
	}
}
