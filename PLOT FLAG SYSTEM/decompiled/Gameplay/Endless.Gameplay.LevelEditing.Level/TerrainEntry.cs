using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Endless.Gameplay.LevelEditing.Level;

[Serializable]
public class TerrainEntry
{
	[JsonProperty("Pos")]
	public Vector3Int Position;

	public int TilesetId;

	public TerrainEntry Copy()
	{
		return new TerrainEntry
		{
			Position = Position,
			TilesetId = TilesetId
		};
	}
}
