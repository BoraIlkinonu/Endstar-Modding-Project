using System;
using System.Collections.Generic;
using Endless.Gameplay.LuaEnums;
using Endless.Shared;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

namespace Endless.Gameplay
{
	// Token: 0x02000177 RID: 375
	public class GetInLosStrategy : IActionStrategy
	{
		// Token: 0x06000873 RID: 2163 RVA: 0x00027F0F File Offset: 0x0002610F
		public GetInLosStrategy(NpcEntity entity)
		{
			this.entity = entity;
		}

		// Token: 0x17000197 RID: 407
		// (get) Token: 0x06000874 RID: 2164 RVA: 0x00027F1E File Offset: 0x0002611E
		public Func<float> GetCost { get; }

		// Token: 0x17000198 RID: 408
		// (get) Token: 0x06000875 RID: 2165 RVA: 0x00027F26 File Offset: 0x00026126
		// (set) Token: 0x06000876 RID: 2166 RVA: 0x00027F2E File Offset: 0x0002612E
		public GoapAction.Status Status { get; private set; }

		// Token: 0x06000877 RID: 2167 RVA: 0x00027F38 File Offset: 0x00026138
		public void Start()
		{
			if (!this.entity.Target)
			{
				this.Status = GoapAction.Status.Failed;
				return;
			}
			this.Status = GoapAction.Status.InProgress;
			MonoBehaviourSingleton<Los>.Instance.RequestLosPositions(this.entity, new Action<List<Vector3>>(this.LosPositionCallback));
			this.targetPositionAtRequest = this.entity.Target.NavPosition;
		}

		// Token: 0x06000878 RID: 2168 RVA: 0x00027F98 File Offset: 0x00026198
		public void Tick(uint frame)
		{
			if (!this.entity.Target || math.distance(this.targetPositionAtRequest, this.entity.Target.NavPosition) > 1f)
			{
				this.Status = GoapAction.Status.Failed;
				return;
			}
			if (MonoBehaviourSingleton<WorldStateDictionary>.Instance[WorldState.HasLos].Evaluate(this.entity))
			{
				this.Status = GoapAction.Status.Complete;
			}
		}

		// Token: 0x06000879 RID: 2169 RVA: 0x0002800A File Offset: 0x0002620A
		public void Stop()
		{
			this.entity.Components.PathFollower.StopPath(false);
			this.entity.Components.PathFollower.OnPathFinished -= this.HandleOnPathFinished;
		}

		// Token: 0x0600087A RID: 2170 RVA: 0x00028044 File Offset: 0x00026244
		private void LosPositionCallback(List<Vector3> losPositions)
		{
			if (losPositions.Count > 0)
			{
				foreach (Vector3 vector in losPositions)
				{
					NavMeshHit navMeshHit;
					if (Vector3.Distance(this.entity.Components.TargeterComponent.Target.Position, vector) < 60f && MonoBehaviourSingleton<Pathfinding>.Instance.IsValidDestination(this.entity.FootPosition, vector, this.entity.PathfindingRange, this.entity.CanDoubleJump) && NavMesh.SamplePosition(vector, out navMeshHit, 1f, -1))
					{
						this.entity.Components.Pathing.RequestPath(navMeshHit.position, new Action<Pathfinding.Response>(this.PathfindingResponseHandler));
						return;
					}
				}
			}
			this.Status = GoapAction.Status.Failed;
		}

		// Token: 0x0600087B RID: 2171 RVA: 0x00028134 File Offset: 0x00026334
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
			this.entity.Components.PathFollower.OnPathFinished -= this.HandleOnPathFinished;
			this.entity.Components.PathFollower.OnPathFinished += this.HandleOnPathFinished;
		}

		// Token: 0x0600087C RID: 2172 RVA: 0x000281E6 File Offset: 0x000263E6
		private void HandleOnPathFinished(bool _)
		{
			this.Status = (MonoBehaviourSingleton<WorldStateDictionary>.Instance[WorldState.HasLos].Evaluate(this.entity) ? GoapAction.Status.Complete : GoapAction.Status.Failed);
		}

		// Token: 0x04000700 RID: 1792
		private readonly NpcEntity entity;

		// Token: 0x04000701 RID: 1793
		private Vector3 targetPositionAtRequest;
	}
}
