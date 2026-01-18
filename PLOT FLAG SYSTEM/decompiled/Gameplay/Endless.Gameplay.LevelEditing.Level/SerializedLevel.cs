using System;
using System.Collections.Generic;
using Endless.Shared.DataTypes;
using UnityEngine;

namespace Endless.Gameplay.LevelEditing.Level;

[Serializable]
public class SerializedLevel
{
	[SerializeField]
	private List<SerializedTerrainCell> serializedTerrainCells;

	[SerializeField]
	private List<SerializedProp> serializedProps;

	public IReadOnlyList<SerializedTerrainCell> SerializedTerrainCells => serializedTerrainCells;

	public IReadOnlyList<SerializedProp> SerializedProps => serializedProps;

	public SerializedLevel()
	{
		serializedTerrainCells = new List<SerializedTerrainCell>();
		serializedProps = new List<SerializedProp>();
	}

	public void AddTerrainCell(Vector3Int coordinate, int tilesetIndex)
	{
		serializedTerrainCells.Add(new SerializedTerrainCell
		{
			Coordinate = coordinate,
			TilesetIndex = tilesetIndex
		});
	}

	public void AddProp(Vector3 position, float rotation, SerializableGuid masterGuid)
	{
		serializedProps.Add(new SerializedProp
		{
			Position = position,
			Rotation = rotation,
			MasterReference = masterGuid
		});
	}
}
