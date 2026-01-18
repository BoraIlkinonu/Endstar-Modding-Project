using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.Experimental.AI;

namespace Endless.Gameplay;

[BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Low)]
public struct BuildSurfacesJob : IJobParallelFor
{
	[ReadOnly]
	public NativeArray<int> SectionKeyArray;

	[ReadOnly]
	public NativeParallelMultiHashMap<int, int> SectionMap;

	[NativeDisableParallelForRestriction]
	public NativeArray<Octant> Octants;

	[NativeDisableParallelForRestriction]
	public NavMeshQuery Query;

	[WriteOnly]
	public NativeParallelHashMap<int, SectionSurface>.ParallelWriter SurfaceMap;

	public void Execute(int index)
	{
		int key = SectionKeyArray[index];
		float num = float.MaxValue;
		float num2 = float.MaxValue;
		float num3 = float.MaxValue;
		float num4 = float.MinValue;
		float num5 = float.MinValue;
		float num6 = float.MinValue;
		float num7 = 1f;
		bool flag = false;
		float num8 = 0f;
		bool flag2 = false;
		foreach (int item2 in SectionMap.GetValuesForKey(key))
		{
			ref Octant reference = ref Octants.GetRef(item2);
			if (!flag)
			{
				num8 = reference.Center.y;
				flag = true;
			}
			else if (!flag2 && math.abs(num8 - reference.Center.y) > 0.01f)
			{
				flag2 = true;
			}
			num7 = reference.Size.x;
			float3 min = reference.Min;
			float3 max = reference.Max;
			num = math.select(num, min.x, min.x < num);
			num2 = math.select(num2, min.y, min.y < num2);
			num3 = math.select(num3, min.z, min.z < num3);
			num4 = math.select(num4, max.x, max.x > num4);
			num5 = math.select(num5, max.y, max.y > num5);
			num6 = math.select(num6, max.z, max.z > num6);
		}
		float y = (num5 + num2) / 2f;
		float num9 = num5 - num2;
		float3 @float = new float3(num, y, num3);
		float3 float2 = new float3(num, y, num6);
		float3 float3 = new float3(num4, y, num6);
		float3 float4 = new float3(num4, y, num3);
		if (num7 >= 1f)
		{
			@float.y = num2;
			float2.y = num2;
			float3.y = num2;
			float4.y = num2;
			SectionSurface item = new SectionSurface(@float, float2, float3, float4);
			SurfaceMap.TryAdd(key, item);
		}
		else
		{
			float3 extents = new float3(0.025f, num9 / 2f + 0.01f, 0.025f);
			@float.y = GetEdgeHeight(float3, @float, extents);
			float2.y = GetEdgeHeight(float4, float2, extents);
			float3.y = GetEdgeHeight(@float, float3, extents);
			float4.y = GetEdgeHeight(float2, float4, extents);
			SectionSurface item = new SectionSurface(@float, float2, float3, float4);
			SurfaceMap.TryAdd(key, item);
		}
	}

	private float GetEdgeHeight(float3 oppositeCorner, float3 cornerPoint, float3 extents)
	{
		for (int i = 0; i < 8; i++)
		{
			float t = (float)i / 8f;
			float3 @float = math.lerp(cornerPoint, oppositeCorner, t);
			NavMeshLocation location = Query.MapLocation(@float, extents, 0);
			if (Query.IsValid(location))
			{
				return location.position.y;
			}
		}
		return cornerPoint.y;
	}
}
