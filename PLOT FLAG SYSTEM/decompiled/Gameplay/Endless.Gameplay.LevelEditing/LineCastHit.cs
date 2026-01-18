using UnityEngine;

namespace Endless.Gameplay.LevelEditing;

public struct LineCastHit
{
	public bool IntersectionOccured { get; set; }

	public float Distance { get; set; }

	public Vector3Int IntersectedObjectPosition { get; set; }

	public Vector3Int NearestPositionToObject { get; set; }
}
