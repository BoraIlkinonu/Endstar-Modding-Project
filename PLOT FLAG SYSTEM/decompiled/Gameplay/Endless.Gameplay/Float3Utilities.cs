using Unity.Mathematics;

namespace Endless.Gameplay;

public static class Float3Utilities
{
	public static float3 WithY(this float3 a, float y)
	{
		return new float3(a.x, y, a.z);
	}
}
