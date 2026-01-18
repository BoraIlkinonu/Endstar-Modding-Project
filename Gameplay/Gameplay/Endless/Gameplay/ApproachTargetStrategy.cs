using System;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000170 RID: 368
	public class ApproachTargetStrategy : IActionStrategy
	{
		// Token: 0x06000830 RID: 2096 RVA: 0x000270C2 File Offset: 0x000252C2
		public ApproachTargetStrategy(NpcEntity entity)
		{
			this.entity = entity;
		}

		// Token: 0x17000189 RID: 393
		// (get) Token: 0x06000831 RID: 2097 RVA: 0x000270D1 File Offset: 0x000252D1
		public Func<float> GetCost { get; }

		// Token: 0x1700018A RID: 394
		// (get) Token: 0x06000832 RID: 2098 RVA: 0x000270D9 File Offset: 0x000252D9
		// (set) Token: 0x06000833 RID: 2099 RVA: 0x000270E1 File Offset: 0x000252E1
		public GoapAction.Status Status { get; private set; }

		// Token: 0x06000834 RID: 2100 RVA: 0x000270EC File Offset: 0x000252EC
		public void Start()
		{
			Vector3 vector;
			if (!this.entity.Target || !this.entity.Target.CombatPositionGenerator.TryGetClosestAroundPosition(this.entity.Position, out vector))
			{
				this.Status = GoapAction.Status.Failed;
				return;
			}
			this.Status = GoapAction.Status.InProgress;
			this.targetPositionAtRequest = this.entity.Target.Position;
			this.entity.Components.Pathing.RequestPath(vector, new Action<Pathfinding.Response>(this.PathfindingResponseHandler));
		}

		// Token: 0x06000835 RID: 2101 RVA: 0x00027177 File Offset: 0x00025377
		public void Tick(uint frame)
		{
			if (this.entity.Target && Vector3.Distance(this.targetPositionAtRequest, this.entity.Target.Position) > 1f)
			{
				this.Status = GoapAction.Status.Failed;
			}
		}

		// Token: 0x06000836 RID: 2102 RVA: 0x000271B4 File Offset: 0x000253B4
		public void Stop()
		{
			this.entity.Components.PathFollower.StopPath(false);
			this.entity.Components.PathFollower.OnPathFinished -= this.HandlePathFinished;
			this.entity.NpcBlackboard.Set<float>(NpcBlackboard.Key.BoredomTime, 0f);
		}

		// Token: 0x06000837 RID: 2103 RVA: 0x00027210 File Offset: 0x00025410
		private void PathfindingResponseHandler(Pathfinding.Response response)
		{
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
			this.entity.Components.PathFollower.OnPathFinished -= this.HandlePathFinished;
			this.entity.Components.PathFollower.OnPathFinished += this.HandlePathFinished;
		}

		// Token: 0x06000838 RID: 2104 RVA: 0x000272A4 File Offset: 0x000254A4
		private void HandlePathFinished(bool completed)
		{
			this.Status = (completed ? GoapAction.Status.Complete : GoapAction.Status.Failed);
		}

		// Token: 0x040006E1 RID: 1761
		private readonly NpcEntity entity;

		// Token: 0x040006E2 RID: 1762
		private Vector3 targetPositionAtRequest;
	}
}
