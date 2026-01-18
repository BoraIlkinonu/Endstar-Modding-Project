using System;
using UnityEngine;

namespace Endless.Gameplay.LevelEditing.Level;

[Serializable]
public class TerrainChange
{
	public Vector3Int[] Coordinates = new Vector3Int[0];

	public int TilesetIndex;

	public bool Erased;
}
