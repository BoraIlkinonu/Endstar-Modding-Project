using System;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000181 RID: 385
	public class MoveToCommandDestinationStrategy : IActionStrategy
	{
		// Token: 0x060008BF RID: 2239 RVA: 0x00028F42 File Offset: 0x00027142
		public MoveToCommandDestinationStrategy(NpcEntity entity)
		{
			this.entity = entity;
		}

		// Token: 0x170001A9 RID: 425
		// (get) Token: 0x060008C0 RID: 2240 RVA: 0x00028F51 File Offset: 0x00027151
		public Func<float> GetCost { get; }

		// Token: 0x170001AA RID: 426
		// (get) Token: 0x060008C1 RID: 2241 RVA: 0x00028F59 File Offset: 0x00027159
		// (set) Token: 0x060008C2 RID: 2242 RVA: 0x00028F61 File Offset: 0x00027161
		public GoapAction.Status Status { get; private set; }

		// Token: 0x060008C3 RID: 2243 RVA: 0x00028F6C File Offset: 0x0002716C
		public void Start()
		{
			Vector3 vector;
			if (!this.entity.NpcBlackboard.TryGet<Vector3>(NpcBlackboard.Key.CommandDestination, out vector))
			{
				this.Status = GoapAction.Status.Failed;
				return;
			}
			this.Status = GoapAction.Status.InProgress;
			this.targetRotation = null;
			this.entity.Components.Pathing.RequestPath(vector, new Action<Pathfinding.Response>(this.PathfindingResponseHandler));
			this.pathfindingAttempts = 0;
		}

		// Token: 0x060008C4 RID: 2244 RVA: 0x00028FD4 File Offset: 0x000271D4
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

		// Token: 0x060008C5 RID: 2245 RVA: 0x00029050 File Offset: 0x00027250
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
					this.entity.NpcBlackboard.Clear<Vector3>(NpcBlackboard.Key.CommandDestination);
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
					if (!this.entity.NpcBlackboard.TryGet<Vector3>(NpcBlackboard.Key.CommandDestination, out vector))
					{
						this.Status = GoapAction.Status.Failed;
						return;
					}
					this.entity.Components.Pathing.RequestPath(vector, new Action<Pathfinding.Response>(this.PathfindingResponseHandler));
				}
			}
			this.entity.Components.PathFollower.OnPathFinished -= this.PathFollowerOnOnPathFinished;
		}

		// Token: 0x060008C6 RID: 2246 RVA: 0x00029124 File Offset: 0x00027324
		public void Update(float deltaTime)
		{
			if (this.entity.Components.PathFollower.Path != null && Vector3.Distance(this.entity.Components.PathFollower.Path.Destination, this.entity.FootPosition) < this.entity.NpcBlackboard.GetValueOrDefault<float>(NpcBlackboard.Key.DestinationTolerance, 0.5f))
			{
				this.entity.Components.PathFollower.OnPathFinished -= this.PathFollowerOnOnPathFinished;
				Quaternion quaternion;
				if (this.entity.NpcBlackboard.TryGet<Quaternion>(NpcBlackboard.Key.Rotation, out quaternion))
				{
					this.targetRotation = new Quaternion?(quaternion);
				}
				else
				{
					this.Status = GoapAction.Status.Complete;
					this.entity.NpcBlackboard.Clear<Vector3>(NpcBlackboard.Key.CommandDestination);
				}
			}
			Quaternion? quaternion2 = this.targetRotation;
			if (quaternion2 != null)
			{
				if (Quaternion.Angle(this.entity.transform.rotation, this.targetRotation.Value) > 5f)
				{
					this.entity.transform.rotation = Quaternion.RotateTowards(this.entity.transform.rotation, this.targetRotation.Value, this.entity.Settings.RotationSpeed * Time.deltaTime);
					return;
				}
				this.Status = GoapAction.Status.Complete;
				this.entity.NpcBlackboard.Clear<Vector3>(NpcBlackboard.Key.CommandDestination);
			}
		}

		// Token: 0x060008C7 RID: 2247 RVA: 0x00029286 File Offset: 0x00027486
		public void Stop()
		{
			this.entity.NpcBlackboard.Clear<Quaternion>(NpcBlackboard.Key.Rotation);
			this.entity.Components.PathFollower.StopPath(false);
		}

		// Token: 0x04000735 RID: 1845
		private readonly NpcEntity entity;

		// Token: 0x04000736 RID: 1846
		private Quaternion? targetRotation;

		// Token: 0x04000737 RID: 1847
		private int pathfindingAttempts;
	}
}
