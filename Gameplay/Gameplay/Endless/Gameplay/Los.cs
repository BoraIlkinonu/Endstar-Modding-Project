using System;
using System.Collections.Generic;
using Endless.Shared;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000249 RID: 585
	public class Los : MonoBehaviourSingleton<Los>
	{
		// Token: 0x06000C1D RID: 3101 RVA: 0x00041EEC File Offset: 0x000400EC
		public void RequestLosPositions(NpcEntity aiEntity, Action<List<Vector3>> callback)
		{
			List<Vector3> list = new List<Vector3>();
			if (!aiEntity.Target)
			{
				callback(list);
				return;
			}
			List<Vector3> list2 = MonoBehaviourSingleton<Pathfinding>.Instance.FindNavigationPositionsInRange(aiEntity.FootPosition, this.losSampleDistance);
			List<TargetDatum> targetableColliderData = aiEntity.Target.GetTargetableColliderData();
			int count = targetableColliderData.Count;
			int num = list2.Count * count;
			if (num == 0)
			{
				callback(list);
				return;
			}
			NativeArray<RaycastCommand> nativeArray = new NativeArray<RaycastCommand>(num, Allocator.TempJob, NativeArrayOptions.ClearMemory);
			QueryParameters queryParameters = new QueryParameters(this.raycastMask, false, QueryTriggerInteraction.UseGlobal, false);
			for (int i = 0; i < list2.Count; i++)
			{
				Vector3 vector = list2[i] + Vector3.up;
				for (int j = 0; j < count; j++)
				{
					Vector3 vector2 = targetableColliderData[j].Position - vector;
					Vector3 normalized = vector2.normalized;
					int num2 = i * count + j;
					nativeArray[num2] = new RaycastCommand(vector, normalized, queryParameters, vector2.magnitude);
				}
			}
			NativeArray<RaycastHit> nativeArray2 = new NativeArray<RaycastHit>(num, Allocator.TempJob, NativeArrayOptions.ClearMemory);
			RaycastCommand.ScheduleBatch(nativeArray, nativeArray2, this.raycastBatchSize, default(JobHandle)).Complete();
			for (int k = 0; k < list2.Count; k++)
			{
				for (int l = 0; l < count; l++)
				{
					TargetDatum targetDatum = targetableColliderData[l];
					int num3 = k * count + l;
					if (nativeArray2[num3].colliderInstanceID == targetDatum.ColliderId)
					{
						list.Add(list2[k]);
						break;
					}
				}
			}
			nativeArray2.Dispose(default(JobHandle));
			nativeArray.Dispose(default(JobHandle));
			Vector3 vector3;
			if (list.Count == 0 && aiEntity.Target.CombatPositionGenerator.TryGetClosestAroundPosition(aiEntity.FootPosition, out vector3))
			{
				list.Add(vector3);
			}
			list.Sort((Vector3 a, Vector3 b) => math.distance(a, aiEntity.FootPosition).CompareTo(math.distance(b, aiEntity.FootPosition)));
			callback(list);
		}

		// Token: 0x04000B2C RID: 2860
		[Tooltip("Maximum distance from the Ai that cells will be sampled for being a potential los position.")]
		[SerializeField]
		private float losSampleDistance;

		// Token: 0x04000B2D RID: 2861
		[Tooltip("Value for optimizing batched raycasts")]
		[SerializeField]
		[Range(1f, 1000f)]
		private int raycastBatchSize;

		// Token: 0x04000B2E RID: 2862
		[SerializeField]
		private LayerMask raycastMask;
	}
}
