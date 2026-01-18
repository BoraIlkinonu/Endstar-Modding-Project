using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Endless.Gameplay;

[BurstCompile(FloatPrecision = FloatPrecision.Low, FloatMode = FloatMode.Fast)]
public struct PerceptionJob : IJobParallelFor
{
	public int MaxHits;

	[ReadOnly]
	public NativeArray<TargeterDatum> TargeterData;

	[ReadOnly]
	public NativeArray<TargetDatum> TargetData;

	[ReadOnly]
	public NativeArray<RaycastHit> CharacterRaycastHits;

	[ReadOnly]
	public NativeArray<RaycastHit> ObstacleRaycastHits;

	[NativeDisableParallelForRestriction]
	public NativeArray<TargetDatum> Targets;

	[NativeDisableParallelForRestriction]
	public NativeArray<int> ValidTargets;

	public void Execute(int index)
	{
		TargeterDatum targeterDatum = TargeterData[index];
		int length = TargetData.Length;
		NativeArray<TargetDatum> subArray = Targets.GetSubArray(index * length, TargetData.Length);
		NativeArray<RaycastHit> subArray2 = CharacterRaycastHits.GetSubArray(index * length * MaxHits, length * MaxHits);
		float3 x = math.normalize(new float3(targeterDatum.LookVector.x, 0f, targeterDatum.LookVector.z));
		int num = 0;
		for (int i = 0; i < length; i++)
		{
			TargetDatum value = TargetData[i];
			RaycastHit raycastHit = ObstacleRaycastHits[index * subArray.Length + i];
			foreach (RaycastHit item in subArray2.GetSubArray(i * MaxHits, MaxHits))
			{
				if (item.colliderInstanceID == value.ColliderId && (targeterDatum.UseXray || raycastHit.colliderInstanceID == 0 || !(raycastHit.distance < item.distance)))
				{
					float num2 = math.distance(targeterDatum.Position, value.Position);
					float num3 = 100f * math.clamp(1f - (num2 - targeterDatum.ProximityDistance) / (targeterDatum.MaxDistance - targeterDatum.ProximityDistance), 0f, 1f);
					float3 y = math.normalize(value.Position - targeterDatum.Position);
					float3 @float = math.normalize(new float3(y.x, 0f, y.z));
					float num4 = math.degrees(math.acos(math.clamp(math.dot(x, @float), -1f, 1f)));
					float num5 = math.degrees(math.acos(math.clamp(math.dot(@float, y), -1f, 1f)));
					float num6 = math.select(math.clamp((targeterDatum.HorizontalViewAngle - num4) / targeterDatum.HorizontalViewAngle, 0f, 1f), 1f, math.abs(targeterDatum.HorizontalViewAngle - 180f) < 0.1f);
					float falseValue = math.clamp((targeterDatum.VerticalViewAngle - num5) / targeterDatum.VerticalViewAngle, 0f, 1f);
					falseValue = math.select(falseValue, 1f, math.abs(targeterDatum.VerticalViewAngle - 180f) < 0.1f);
					float num7 = num6 * falseValue;
					num3 *= num7;
					if ((value.Awareness = math.select(100f, num3, num2 >= targeterDatum.ProximityDistance)) > 0f)
					{
						subArray[num] = value;
						num++;
						break;
					}
				}
			}
		}
		ValidTargets[index] = num;
	}
}
