using System.Collections.Generic;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared.DataTypes;

namespace Endless.Creator.UI;

public static class UISpawnPointUtility
{
	private const bool VERBOSE_LOGGING = false;

	public static List<UISpawnPoint> GetSpawnPointsFrom(IReadOnlyList<SerializableGuid> spawnPointIds, IReadOnlyList<PropEntry> propEntries)
	{
		List<UISpawnPoint> list = new List<UISpawnPoint>();
		for (int i = 0; i < spawnPointIds.Count; i++)
		{
			SerializableGuid serializableGuid = spawnPointIds[i];
			string spawnPointLabel = GetSpawnPointLabel(serializableGuid, propEntries);
			UISpawnPoint item = new UISpawnPoint(serializableGuid, spawnPointLabel);
			list.Add(item);
		}
		return list;
	}

	public static string GetSpawnPointLabel(SerializableGuid spawnPointId, IReadOnlyList<PropEntry> propEntries)
	{
		foreach (PropEntry propEntry in propEntries)
		{
			if (propEntry.InstanceId == spawnPointId)
			{
				return propEntry.Label;
			}
		}
		return "Spawn Point Removed";
	}
}
