using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace Endless.Gameplay;

public readonly struct SectionSurface
{
	private readonly float3 p1;

	private readonly float3 p2;

	private readonly float3 p3;

	private readonly float3 p4;

	public float3 Center { get; }

	private float3 this[int i] => i switch
	{
		0 => p1, 
		1 => p2, 
		2 => p3, 
		3 => p4, 
		_ => throw new ArgumentOutOfRangeException("i", i, null), 
	};

	public SectionSurface(float3 p1, float3 p2, float3 p3, float3 p4)
	{
		this.p1 = p1;
		this.p2 = p2;
		this.p3 = p3;
		this.p4 = p4;
		Center = (p1 + p2 + p3 + p4) / 4f;
	}

	public SectionEdge GetSectionEdge(int edgeIndex)
	{
		return edgeIndex switch
		{
			0 => new SectionEdge(p1, p2), 
			1 => new SectionEdge(p2, p3), 
			2 => new SectionEdge(p3, p4), 
			3 => new SectionEdge(p4, p1), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	[BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Low)]
	public SectionEdge GetClosestEdgeToPoint(float3 point)
	{
		NativeArray<float3> nativeArray = new NativeArray<float3>(4, Allocator.Temp);
		nativeArray[0] = p1;
		nativeArray[1] = p2;
		nativeArray[2] = p3;
		nativeArray[3] = p4;
		float num = float.MaxValue;
		float num2 = float.MaxValue;
		int num3 = -1;
		int index = -1;
		for (int i = 0; i < nativeArray.Length; i++)
		{
			float num4 = math.distancesq(nativeArray[i], point);
			if (num4 < num)
			{
				num2 = num;
				index = num3;
				num = num4;
				num3 = i;
			}
			else if (num4 < num2)
			{
				num2 = num4;
				index = i;
			}
		}
		return new SectionEdge(nativeArray[num3], nativeArray[index]);
	}

	private float3 GetPlaneNormal()
	{
		return math.normalize(math.cross(p2 - p1, p4 - p1));
	}

	public float3 TranslateSurfacePoint(float3 point, float3 direction, float distance)
	{
		if (distance == 0f || direction.Equals(float3.zero))
		{
			return point;
		}
		float3 planeNormal = GetPlaneNormal();
		float3 @float = math.normalize(math.cross(math.cross(direction, planeNormal), planeNormal));
		@float = math.select(-@float, @float, math.dot(direction, @float) > 0f);
		point += @float * distance;
		if (IsPointInQuad(point))
		{
			return point;
		}
		float3 result = point;
		float num = float.MaxValue;
		for (int i = 0; i < 4; i++)
		{
			float3 float2 = this[i];
			float3 float3 = this[(i + 1) % 4];
			float3 closestPointOnEdge = new SectionEdge(float2, float3).GetClosestPointOnEdge(point, 0.1f);
			float num2 = math.distancesq(point, closestPointOnEdge);
			if (num2 < num)
			{
				num = num2;
				result = closestPointOnEdge;
			}
		}
		return result;
	}

	public NativeList<float3> GetSurfaceSamplePoints()
	{
		float x = math.distance(p1, p2);
		float num = math.distance(p2, p3);
		float3 @float = math.normalize(p3 - p2);
		int num2 = math.clamp((int)math.round(x) + 1, 2, 5);
		int num3 = math.clamp((int)math.round(num) + 1, 2, 5);
		NativeList<float3> result = new NativeList<float3>(num2 * num3, AllocatorManager.Temp);
		float num4 = 1f / (float)(num2 - 1);
		float num5 = 1f / (float)(num3 - 1);
		for (int i = 0; i < num2; i++)
		{
			float3 float2 = math.lerp(p1, p2, (float)i * num4);
			for (int j = 0; j < num3; j++)
			{
				float num6 = math.lerp(0f, num, (float)j * num5);
				result.Add(float2 + @float * num6);
			}
		}
		return result;
	}

	public NativeList<float3> GetSurfaceEdgeSamplePoints()
	{
		NativeList<float3> result = new NativeList<float3>(16, AllocatorManager.Temp);
		for (int i = 0; i < 4; i++)
		{
			SectionEdge sectionEdge = GetSectionEdge(i);
			int valueToClamp = (int)math.ceil(math.distance(sectionEdge.p1, sectionEdge.p2) / 0.5f);
			valueToClamp = math.clamp(valueToClamp, 3, 5);
			for (int j = 0; j < valueToClamp; j++)
			{
				result.Add(math.lerp(sectionEdge.p1, sectionEdge.p2, (float)j / ((float)valueToClamp - 1f)));
			}
		}
		return result;
	}

	private bool IsPointInQuad(float3 pt)
	{
		float3 planeNormal = GetPlaneNormal();
		for (int i = 0; i < 4; i++)
		{
			float3 @float = this[i];
			float3 x = this[(i + 1) % 4] - @float;
			float3 y = pt - @float;
			if (math.dot(math.cross(x, y), planeNormal) < 0f)
			{
				return false;
			}
		}
		return true;
	}
}
