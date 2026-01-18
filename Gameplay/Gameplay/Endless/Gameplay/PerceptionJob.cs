using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x0200025E RID: 606
	[BurstCompile(FloatPrecision = FloatPrecision.Low, FloatMode = FloatMode.Fast)]
	public struct PerceptionJob : IJobParallelFor
	{
		// Token: 0x06000C8B RID: 3211 RVA: 0x00043A44 File Offset: 0x00041C44
		public void Execute(int index)
		{
			TargeterDatum targeterDatum = this.TargeterData[index];
			int length = this.TargetData.Length;
			NativeArray<TargetDatum> subArray = this.Targets.GetSubArray(index * length, this.TargetData.Length);
			NativeArray<RaycastHit> subArray2 = this.CharacterRaycastHits.GetSubArray(index * length * this.MaxHits, length * this.MaxHits);
			float3 @float = math.normalize(new float3(targeterDatum.LookVector.x, 0f, targeterDatum.LookVector.z));
			int num = 0;
			for (int i = 0; i < length; i++)
			{
				TargetDatum targetDatum = this.TargetData[i];
				RaycastHit raycastHit = this.ObstacleRaycastHits[index * subArray.Length + i];
				foreach (RaycastHit raycastHit2 in subArray2.GetSubArray(i * this.MaxHits, this.MaxHits))
				{
					if (raycastHit2.colliderInstanceID == targetDatum.ColliderId && (targeterDatum.UseXray || raycastHit.colliderInstanceID == 0 || raycastHit.distance >= raycastHit2.distance))
					{
						float num2 = math.distance(targeterDatum.Position, targetDatum.Position);
						float num3 = 100f * math.clamp(1f - (num2 - targeterDatum.ProximityDistance) / (targeterDatum.MaxDistance - targeterDatum.ProximityDistance), 0f, 1f);
						float3 float2 = math.normalize(targetDatum.Position - targeterDatum.Position);
						float3 float3 = math.normalize(new float3(float2.x, 0f, float2.z));
						float num4 = math.degrees(math.acos(math.clamp(math.dot(@float, float3), -1f, 1f)));
						float num5 = math.degrees(math.acos(math.clamp(math.dot(float3, float2), -1f, 1f)));
						float num6 = math.select(math.clamp((targeterDatum.HorizontalViewAngle - num4) / targeterDatum.HorizontalViewAngle, 0f, 1f), 1f, math.abs(targeterDatum.HorizontalViewAngle - 180f) < 0.1f);
						float num7 = math.clamp((targeterDatum.VerticalViewAngle - num5) / targeterDatum.VerticalViewAngle, 0f, 1f);
						num7 = math.select(num7, 1f, math.abs(targeterDatum.VerticalViewAngle - 180f) < 0.1f);
						float num8 = num6 * num7;
						num3 *= num8;
						float num9 = math.select(100f, num3, num2 >= targeterDatum.ProximityDistance);
						targetDatum.Awareness = num9;
						if (num9 > 0f)
						{
							subArray[num] = targetDatum;
							num++;
							break;
						}
					}
				}
			}
			this.ValidTargets[index] = num;
		}

		// Token: 0x04000B84 RID: 2948
		public int MaxHits;

		// Token: 0x04000B85 RID: 2949
		[ReadOnly]
		public NativeArray<TargeterDatum> TargeterData;

		// Token: 0x04000B86 RID: 2950
		[ReadOnly]
		public NativeArray<TargetDatum> TargetData;

		// Token: 0x04000B87 RID: 2951
		[ReadOnly]
		public NativeArray<RaycastHit> CharacterRaycastHits;

		// Token: 0x04000B88 RID: 2952
		[ReadOnly]
		public NativeArray<RaycastHit> ObstacleRaycastHits;

		// Token: 0x04000B89 RID: 2953
		[NativeDisableParallelForRestriction]
		public NativeArray<TargetDatum> Targets;

		// Token: 0x04000B8A RID: 2954
		[NativeDisableParallelForRestriction]
		public NativeArray<int> ValidTargets;
	}
}
