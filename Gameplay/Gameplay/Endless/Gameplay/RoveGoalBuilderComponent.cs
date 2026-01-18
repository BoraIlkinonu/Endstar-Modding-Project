using System;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000191 RID: 401
	public class RoveGoalBuilderComponent : GoalBuilderComponent
	{
		// Token: 0x06000926 RID: 2342 RVA: 0x0002AA84 File Offset: 0x00028C84
		private void Awake()
		{
			this.wanderJitter = global::UnityEngine.Random.Range(0f, this.maxWanderJitter);
			this.roveCenter = base.transform.position;
			this.priorityCurve.preWrapMode = WrapMode.ClampForever;
			this.priorityCurve.postWrapMode = WrapMode.ClampForever;
		}

		// Token: 0x06000927 RID: 2343 RVA: 0x0002AAD0 File Offset: 0x00028CD0
		protected override void Activate(NpcEntity entity)
		{
			Vector3? rovePosition = Pathfinding.GetRovePosition(this.roveCenter, entity.Position, this.roveDistance, this.roveDistance);
			if (rovePosition != null)
			{
				entity.NpcBlackboard.Set<Vector3>(NpcBlackboard.Key.BehaviorDestination, rovePosition.Value);
			}
		}

		// Token: 0x06000928 RID: 2344 RVA: 0x0002AB18 File Offset: 0x00028D18
		protected override void Deactivate(NpcEntity entity)
		{
			entity.NpcBlackboard.Set<float>(NpcBlackboard.Key.LastWander, Time.time);
			this.wanderJitter = global::UnityEngine.Random.Range(0f, this.maxWanderJitter);
			entity.NpcBlackboard.Clear<Vector3>(NpcBlackboard.Key.BehaviorDestination);
		}

		// Token: 0x06000929 RID: 2345 RVA: 0x0002AB50 File Offset: 0x00028D50
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

		// Token: 0x0400077E RID: 1918
		[SerializeField]
		private float roveDistance;

		// Token: 0x0400077F RID: 1919
		[SerializeField]
		private AnimationCurve priorityCurve;

		// Token: 0x04000780 RID: 1920
		[SerializeField]
		private float maxWanderJitter;

		// Token: 0x04000781 RID: 1921
		private float wanderJitter;

		// Token: 0x04000782 RID: 1922
		private Vector3 roveCenter;
	}
}
