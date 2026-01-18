using Unity.Mathematics;
using UnityEngine;

namespace Endless.Gameplay;

public static class Int3Extensions
{
	public static readonly int3 Up = new int3(0, 1, 0);

	public static readonly int3 Down = new int3(0, -1, 0);

	public static readonly int3 Forward = new int3(0, 0, 1);

	public static readonly int3 Back = new int3(0, 0, -1);

	public static readonly int3 Right = new int3(1, 0, 0);

	public static readonly int3 Left = new int3(-1, 0, 0);

	public static readonly int3 Invalid = new int3(-1000, -1000, -1000);

	public static readonly int3[] CardinalDirections = new int3[4] { Forward, Right, Back, Left };

	public static int3 zero = new int3(0, 0, 0);

	public static Vector3 ToVector3(this int3 int3)
	{
		return new Vector3(int3.x, int3.y, int3.z);
	}

	public static Vector3 ToVector3(this float3 float3)
	{
		return new Vector3(float3.x, float3.y, float3.z);
	}

	public static Vector3Int ToVector3Int(this int3 int3)
	{
		return new Vector3Int(int3.x, int3.y, int3.z);
	}

	public static int3 ToInt3(this Vector3Int vector3Int)
	{
		return new int3(vector3Int.x, vector3Int.y, vector3Int.z);
	}

	public static float3 ToFloat3(this Vector3 vector3)
	{
		return new float3(vector3.x, vector3.y, vector3.z);
	}

	public static int GetManhattanDistance(int3 a, int3 b)
	{
		return math.abs(b.x - a.x) + math.abs(b.y - a.y) + math.abs(b.z - a.z);
	}
}
