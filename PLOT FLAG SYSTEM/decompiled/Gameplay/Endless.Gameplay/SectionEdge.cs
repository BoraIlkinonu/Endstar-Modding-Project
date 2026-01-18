using Unity.Burst;
using Unity.Mathematics;

namespace Endless.Gameplay;

public readonly struct SectionEdge
{
	public readonly float3 p1;

	public readonly float3 p2;

	public float Length => math.length(p2 - p1);

	public float3 Center => (p1 + p2) / 2f;

	public SectionEdge(float3 p1, float3 p2)
	{
		this.p1 = p1;
		this.p2 = p2;
	}

	[BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Low)]
	public float3 GetClosestPointOnEdge(float3 point, float edgeTolerance = 0f)
	{
		float3 @float = p2 - p1;
		float3 x = point - p1;
		float num = math.dot(@float, @float);
		if (num == 0f)
		{
			return p1;
		}
		float valueToClamp = math.dot(x, @float) / num;
		float num2 = edgeTolerance / math.distance(p1, p2);
		valueToClamp = math.clamp(valueToClamp, num2, 1f - num2);
		return p1 + valueToClamp * @float;
	}
}
