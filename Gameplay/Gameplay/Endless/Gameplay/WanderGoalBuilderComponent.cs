using System;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000192 RID: 402
	public class WanderGoalBuilderComponent : GoalBuilderComponent
	{
		// Token: 0x0600092B RID: 2347 RVA: 0x0002AB90 File Offset: 0x00028D90
		private void Awake()
		{
			this.wanderJitter = global::UnityEngine.Random.Range(0f, this.maxWanderJitter);
			this.priorityCurve.preWrapMode = WrapMode.ClampForever;
			this.priorityCurve.postWrapMode = WrapMode.ClampForever;
		}

		// Token: 0x0600092C RID: 2348 RVA: 0x0002ABC0 File Offset: 0x00028DC0
		protected override void Activate(NpcEntity entity)
		{
			Vector3? wanderPosition = Pathfinding.GetWanderPosition(entity.Position, this.wanderDistance);
			if (wanderPosition != null)
			{
				entity.NpcBlackboard.Set<Vector3>(NpcBlackboard.Key.BehaviorDestination, wanderPosition.Value);
			}
		}

		// Token: 0x0600092D RID: 2349 RVA: 0x0002ABFC File Offset: 0x00028DFC
		protected override void Deactivate(NpcEntity entity)
		{
			entity.NpcBlackboard.Set<float>(NpcBlackboard.Key.LastWander, Time.time);
			this.wanderJitter = global::UnityEngine.Random.Range(0f, this.maxWanderJitter);
			entity.NpcBlackboard.Clear<Vector3>(NpcBlackboard.Key.BehaviorDestination);
		}

		// Token: 0x0600092E RID: 2350 RVA: 0x0002AC34 File Offset: 0x00028E34
		protected override float Priority(NpcEntity entity)
		{
			float num;
			float num2;
			if (entity.NpcBlackboard.TryGet<float>(NpcBlackboard.Key.LastWander, out num))
			{
				num2 = Time.time - num;
			}
			else
			{
				num2 = Time.time;
			}
			return this.priorityCurve.Evaluate(num2 - this.wanderJitter);
		}

		// Token: 0x04000783 RID: 1923
		[SerializeField]
		private float wanderDistance;

		// Token: 0x04000784 RID: 1924
		[SerializeField]
		private AnimationCurve priorityCurve;

		// Token: 0x04000785 RID: 1925
		[SerializeField]
		private float maxWanderJitter;

		// Token: 0x04000786 RID: 1926
		private float wanderJitter;
	}
}
