using System;
using System.Collections.Generic;
using Endless.Shared.DataTypes;

namespace Endless.Gameplay.LevelEditing.Level;

[Serializable]
public class LevelData
{
	public string Name = string.Empty;

	public string Description = string.Empty;

	public bool Archived;

	public List<(SerializableGuid InstanceId, string Label)> SpawnPoints = new List<(SerializableGuid, string)>();

	public LevelData()
	{
	}

	public LevelData(LevelData reference)
	{
		Name = reference.Name;
		Description = reference.Description;
		Archived = reference.Archived;
		SpawnPoints = reference.SpawnPoints;
	}

	public LevelData(LevelState levelState)
	{
		Name = levelState.Name;
		Description = levelState.Description;
		HashSet<SerializableGuid> hashSet = new HashSet<SerializableGuid>();
		foreach (SerializableGuid spawnPointId in levelState.SpawnPointIds)
		{
			hashSet.Add(spawnPointId);
		}
		foreach (PropEntry propEntry in levelState.PropEntries)
		{
			if (hashSet.Contains(propEntry.InstanceId))
			{
				SpawnPoints.Add((propEntry.InstanceId, propEntry.Label));
			}
		}
	}
}
