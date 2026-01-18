using System;
using UnityEngine;

namespace Endless.Gameplay.LevelEditing.Level;

[Serializable]
public class SerializedTerrainCell
{
	public Vector3Int Coordinate;

	public int TilesetIndex;
}
