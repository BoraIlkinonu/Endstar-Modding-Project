using System;

namespace Endless.Gameplay
{
	// Token: 0x02000176 RID: 374
	public class FindTargetStrategy : IActionStrategy
	{
		// Token: 0x17000195 RID: 405
		// (get) Token: 0x0600086A RID: 2154 RVA: 0x00027DC9 File Offset: 0x00025FC9
		public Func<float> GetCost { get; }

		// Token: 0x17000196 RID: 406
		// (get) Token: 0x0600086B RID: 2155 RVA: 0x00027DD1 File Offset: 0x00025FD1
		// (set) Token: 0x0600086C RID: 2156 RVA: 0x00027DD9 File Offset: 0x00025FD9
		public GoapAction.Status Status { get; private set; }

		// Token: 0x0600086D RID: 2157 RVA: 0x00027DE2 File Offset: 0x00025FE2
		public FindTargetStrategy(NpcEntity entity)
		{
			this.entity = entity;
		}

		// Token: 0x0600086E RID: 2158 RVA: 0x00027DF1 File Offset: 0x00025FF1
		public void Start()
		{
			this.Status = GoapAction.Status.InProgress;
			this.entity.Components.Pathing.RequestPath(this.entity.LastKnownTargetLocation, new Action<Pathfinding.Response>(this.PathfindingResponseHandler));
		}

		// Token: 0x0600086F RID: 2159 RVA: 0x00027E28 File Offset: 0x00026028
		private void PathfindingResponseHandler(Pathfinding.Response response)
		{
			if (response.PathfindingResult == NpcEnum.PathfindingResult.Failure)
			{
				this.Status = GoapAction.Status.Failed;
				return;
			}
			this.entity.Components.PathFollower.SetPath(response.Path);
			this.entity.Components.PathFollower.OnPathFinished -= this.HandleOnPathFinished;
			this.entity.Components.PathFollower.OnPathFinished += this.HandleOnPathFinished;
		}

		// Token: 0x06000870 RID: 2160 RVA: 0x00027EA2 File Offset: 0x000260A2
		private void HandleOnPathFinished(bool obj)
		{
			this.entity.Components.PathFollower.OnPathFinished -= this.HandleOnPathFinished;
		}

		// Token: 0x06000871 RID: 2161 RVA: 0x00027EC5 File Offset: 0x000260C5
		public void Tick(uint frame)
		{
			if (this.entity.Target)
			{
				this.Status = GoapAction.Status.Complete;
			}
		}

		// Token: 0x06000872 RID: 2162 RVA: 0x00027EE0 File Offset: 0x000260E0
		public void Stop()
		{
			this.entity.Components.PathFollower.OnPathFinished -= this.HandleOnPathFinished;
			this.entity.LostTarget = false;
		}

		// Token: 0x040006FD RID: 1789
		private readonly NpcEntity entity;
	}
}
