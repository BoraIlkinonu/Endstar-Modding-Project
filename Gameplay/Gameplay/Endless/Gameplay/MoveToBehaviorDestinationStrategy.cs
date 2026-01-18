using System;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000180 RID: 384
	public class MoveToBehaviorDestinationStrategy : IActionStrategy
	{
		// Token: 0x060008B6 RID: 2230 RVA: 0x00028CCB File Offset: 0x00026ECB
		public MoveToBehaviorDestinationStrategy(NpcEntity entity)
		{
			this.entity = entity;
		}

		// Token: 0x170001A7 RID: 423
		// (get) Token: 0x060008B7 RID: 2231 RVA: 0x00028CDA File Offset: 0x00026EDA
		public Func<float> GetCost { get; }

		// Token: 0x170001A8 RID: 424
		// (get) Token: 0x060008B8 RID: 2232 RVA: 0x00028CE2 File Offset: 0x00026EE2
		// (set) Token: 0x060008B9 RID: 2233 RVA: 0x00028CEA File Offset: 0x00026EEA
		public GoapAction.Status Status { get; private set; }

		// Token: 0x060008BA RID: 2234 RVA: 0x00028CF4 File Offset: 0x00026EF4
		public void Start()
		{
			Vector3 vector;
			if (!this.entity.NpcBlackboard.TryGet<Vector3>(NpcBlackboard.Key.BehaviorDestination, out vector))
			{
				this.Status = GoapAction.Status.Failed;
				return;
			}
			this.Status = GoapAction.Status.InProgress;
			this.targetRotation = null;
			this.entity.Components.Pathing.RequestPath(vector, new Action<Pathfinding.Response>(this.PathfindingResponseHandler));
			this.pathfindingAttempts = 0;
		}

		// Token: 0x060008BB RID: 2235 RVA: 0x00028D5C File Offset: 0x00026F5C
		private void PathfindingResponseHandler(Pathfinding.Response response)
		{
			if (response.PathfindingResult == NpcEnum.PathfindingResult.Failure)
			{
				this.Status = GoapAction.Status.Failed;
				return;
			}
			this.entity.Components.PathFollower.SetPath(response.Path);
			this.entity.Components.PathFollower.OnPathFinished -= this.PathFollowerOnOnPathFinished;
			this.entity.Components.PathFollower.OnPathFinished += this.PathFollowerOnOnPathFinished;
		}

		// Token: 0x060008BC RID: 2236 RVA: 0x00028DD8 File Offset: 0x00026FD8
		private void PathFollowerOnOnPathFinished(bool result)
		{
			if (result)
			{
				Quaternion quaternion;
				if (this.entity.NpcBlackboard.TryGet<Quaternion>(NpcBlackboard.Key.Rotation, out quaternion))
				{
					this.targetRotation = new Quaternion?(quaternion);
				}
				else
				{
					this.Status = GoapAction.Status.Complete;
				}
			}
			else
			{
				this.pathfindingAttempts++;
				if (this.pathfindingAttempts > 10)
				{
					this.Status = GoapAction.Status.Failed;
				}
				else
				{
					Vector3 vector;
					if (!this.entity.NpcBlackboard.TryGet<Vector3>(NpcBlackboard.Key.BehaviorDestination, out vector))
					{
						this.Status = GoapAction.Status.Failed;
						return;
					}
					this.entity.Components.Pathing.RequestPath(vector, new Action<Pathfinding.Response>(this.PathfindingResponseHandler));
				}
			}
			this.entity.Components.PathFollower.OnPathFinished -= this.PathFollowerOnOnPathFinished;
		}

		// Token: 0x060008BD RID: 2237 RVA: 0x00028E98 File Offset: 0x00027098
		public void Update(float deltaTime)
		{
			Quaternion? quaternion = this.targetRotation;
			if (quaternion != null)
			{
				if (Quaternion.Angle(this.entity.transform.rotation, this.targetRotation.Value) > 5f)
				{
					this.entity.transform.rotation = Quaternion.RotateTowards(this.entity.transform.rotation, this.targetRotation.Value, this.entity.Settings.RotationSpeed * Time.deltaTime);
					return;
				}
				this.Status = GoapAction.Status.Complete;
			}
		}

		// Token: 0x060008BE RID: 2238 RVA: 0x00028F2A File Offset: 0x0002712A
		public void Stop()
		{
			this.entity.Components.PathFollower.StopPath(false);
		}

		// Token: 0x04000730 RID: 1840
		private readonly NpcEntity entity;

		// Token: 0x04000731 RID: 1841
		private Quaternion? targetRotation;

		// Token: 0x04000732 RID: 1842
		private int pathfindingAttempts;
	}
}
