using System;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x0200017F RID: 383
	public class MoveNearTargetStrategy : IActionStrategy
	{
		// Token: 0x060008AC RID: 2220 RVA: 0x00028A4D File Offset: 0x00026C4D
		public MoveNearTargetStrategy(NpcEntity entity)
		{
			this.entity = entity;
		}

		// Token: 0x170001A5 RID: 421
		// (get) Token: 0x060008AD RID: 2221 RVA: 0x00028A5C File Offset: 0x00026C5C
		public Func<float> GetCost { get; }

		// Token: 0x170001A6 RID: 422
		// (get) Token: 0x060008AE RID: 2222 RVA: 0x00028A64 File Offset: 0x00026C64
		// (set) Token: 0x060008AF RID: 2223 RVA: 0x00028A6C File Offset: 0x00026C6C
		public GoapAction.Status Status { get; private set; }

		// Token: 0x060008B0 RID: 2224 RVA: 0x00028A75 File Offset: 0x00026C75
		public void Start()
		{
			if (!this.entity.Target)
			{
				this.Status = GoapAction.Status.Failed;
				return;
			}
			this.Status = GoapAction.Status.InProgress;
			this.SetDestinationToTarget();
		}

		// Token: 0x060008B1 RID: 2225 RVA: 0x00028AA0 File Offset: 0x00026CA0
		public void Tick(uint frame)
		{
			if (!this.entity.Target)
			{
				this.Status = GoapAction.Status.Failed;
				return;
			}
			if (Vector3.Distance(this.entity.FootPosition, this.entity.Components.TargeterComponent.Target.NavPosition) < this.entity.NearDistance)
			{
				this.Status = GoapAction.Status.Complete;
				return;
			}
			if (this.entity.Components.PathFollower.Path != null && Vector3.Distance(this.entity.Components.PathFollower.Path.Destination, this.entity.Target.NavPosition) > 0.5f)
			{
				this.SetDestinationToTarget();
			}
		}

		// Token: 0x060008B2 RID: 2226 RVA: 0x00028B59 File Offset: 0x00026D59
		public void Stop()
		{
			this.entity.Components.PathFollower.StopPath(false);
			this.entity.Components.PathFollower.OnPathFinished -= this.HandleOnPathFinished;
		}

		// Token: 0x060008B3 RID: 2227 RVA: 0x00028B94 File Offset: 0x00026D94
		private void SetDestinationToTarget()
		{
			Vector3 vector;
			if (!this.entity.Target.PositionPrediction.TryGetPredictedNavigationPosition(this.entity.Settings.PredictionTime, out vector))
			{
				this.Status = GoapAction.Status.Failed;
				return;
			}
			this.entity.Components.Pathing.RequestPath(vector, new Action<Pathfinding.Response>(this.PathfindingResponseHandler));
		}

		// Token: 0x060008B4 RID: 2228 RVA: 0x00028BF8 File Offset: 0x00026DF8
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
			if (response.Path.GetLength() < 10f)
			{
				this.entity.Components.PathFollower.LookRotationOverride = this.entity.Target.transform;
			}
			this.entity.Components.PathFollower.OnPathFinished -= this.HandleOnPathFinished;
			this.entity.Components.PathFollower.OnPathFinished += this.HandleOnPathFinished;
		}

		// Token: 0x060008B5 RID: 2229 RVA: 0x00028CBC File Offset: 0x00026EBC
		private void HandleOnPathFinished(bool result)
		{
			this.Status = (result ? GoapAction.Status.Complete : GoapAction.Status.Failed);
		}

		// Token: 0x0400072D RID: 1837
		private readonly NpcEntity entity;
	}
}
