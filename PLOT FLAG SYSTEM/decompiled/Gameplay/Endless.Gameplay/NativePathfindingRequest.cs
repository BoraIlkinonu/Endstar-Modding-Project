using Endless.Gameplay.LuaEnums;
using UnityEngine;

namespace Endless.Gameplay;

public struct NativePathfindingRequest
{
	public Vector3 StartPosition;

	public Vector3 EndPosition;

	public int StartNodeKey;

	public int EndNodeKey;

	public PathfindingRange PathfindingRange;

	public float OnMeshPathDistance;
}
