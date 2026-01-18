using System;
using Endless.Gameplay.LuaEnums;
using Unity.Mathematics;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000174 RID: 372
	public class FallBackStrategy : IActionStrategy
	{
		// Token: 0x06000856 RID: 2134 RVA: 0x000278E3 File Offset: 0x00025AE3
		public FallBackStrategy(NpcEntity entity)
		{
			this.entity = entity;
		}

		// Token: 0x17000191 RID: 401
		// (get) Token: 0x06000857 RID: 2135 RVA: 0x000278F2 File Offset: 0x00025AF2
		public Func<float> GetCost { get; }

		// Token: 0x17000192 RID: 402
		// (get) Token: 0x06000858 RID: 2136 RVA: 0x000278FA File Offset: 0x00025AFA
		// (set) Token: 0x06000859 RID: 2137 RVA: 0x00027902 File Offset: 0x00025B02
		public GoapAction.Status Status { get; private set; }

		// Token: 0x0600085A RID: 2138 RVA: 0x0002790C File Offset: 0x00025B0C
		public void Start()
		{
			Vector3 vector;
			if (!this.entity.Target || !this.entity.Target.CombatPositionGenerator.TryGetClosestAroundPosition(this.entity.transform.position, out vector))
			{
				this.Status = GoapAction.Status.Failed;
				return;
			}
			this.Status = GoapAction.Status.InProgress;
			this.entity.Components.Animator.SetBool(NpcAnimator.ZLock, true);
			this.cachedSpeed = this.entity.Components.Agent.speed;
			this.entity.Components.Agent.speed = this.entity.Settings.StrafingSpeed;
			if (this.entity.NpcClass.NpcClass == NpcClass.Rifleman)
			{
				this.entity.NpcBlackboard.Set<float>(NpcBlackboard.Key.OverrideSpeed, this.cachedSpeed);
			}
			this.entity.Components.Pathing.RequestPath(vector, new Action<Pathfinding.Response>(this.PathfindingResponseHandler));
		}

		// Token: 0x0600085B RID: 2139 RVA: 0x00027A0C File Offset: 0x00025C0C
		public void Tick(uint frame)
		{
			if (!this.entity.Target || math.distance(this.targetPositionAtRequest, this.entity.Target.Position) > 1f)
			{
				this.Status = GoapAction.Status.Failed;
			}
		}

		// Token: 0x0600085C RID: 2140 RVA: 0x00027A60 File Offset: 0x00025C60
		public void Stop()
		{
			this.entity.Components.Animator.SetBool(NpcAnimator.ZLock, true);
			this.entity.Components.PathFollower.StopPath(false);
			this.entity.NpcBlackboard.Set<float>(NpcBlackboard.Key.BoredomTime, 0f);
			this.entity.Components.Agent.speed = this.cachedSpeed;
			this.entity.NpcBlackboard.Clear<float>(NpcBlackboard.Key.OverrideSpeed);
			this.entity.Components.PathFollower.OnPathFinished -= this.HandleOnPathFinished;
		}

		// Token: 0x0600085D RID: 2141 RVA: 0x00027B04 File Offset: 0x00025D04
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
			this.entity.Components.PathFollower.LookRotationOverride = this.entity.Target.transform;
			this.entity.Components.PathFollower.OnPathFinished -= this.HandleOnPathFinished;
			this.entity.Components.PathFollower.OnPathFinished += this.HandleOnPathFinished;
		}

		// Token: 0x0600085E RID: 2142 RVA: 0x00027BBD File Offset: 0x00025DBD
		private void HandleOnPathFinished(bool complete)
		{
			this.Status = (complete ? GoapAction.Status.Complete : GoapAction.Status.Failed);
		}

		// Token: 0x040006F1 RID: 1777
		private readonly NpcEntity entity;

		// Token: 0x040006F2 RID: 1778
		private float cachedSpeed;

		// Token: 0x040006F3 RID: 1779
		private Vector3 targetPositionAtRequest;
	}
}
