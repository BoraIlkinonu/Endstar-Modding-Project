using System.Collections.Generic;
using System.IO;
using System.Linq;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.DataTypes;
using UnityEngine;

namespace Endless.Creator.UI;

public static class UILevelUtility
{
	[Header("Debugging")]
	private static readonly bool verboseLogging;

	public static LevelReference GetLevelData(SerializableGuid levelId)
	{
		if (verboseLogging)
		{
			Debug.Log(string.Format("{0} ( {1}: {2} )", "GetLevelData", "levelId", levelId));
		}
		if (levelId.IsEmpty)
		{
			if (verboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2} ) | Could not find! || It's empty!", "GetLevelData", "levelId", levelId));
			}
			return null;
		}
		foreach (LevelReference levelReference in MonoBehaviourSingleton<GameEditor>.Instance.GetLevelReferences())
		{
			if ((SerializableGuid)levelReference.AssetID == levelId)
			{
				return levelReference;
			}
		}
		if (verboseLogging)
		{
			Debug.Log(string.Format("{0} ( {1}: {2} ) | Could not find!", "GetLevelData", "levelId", levelId));
		}
		return null;
	}

	public static List<(SerializableGuid InstanceId, string Label)> GetSpawnPointsList(SerializableGuid levelId)
	{
		if (verboseLogging)
		{
			Debug.Log(string.Format("{0} ( {1}: {2} )", "GetSpawnPointsList", "levelId", levelId));
		}
		Debug.LogWarning("We need a way to get Spawn Point Ids and Labels from LevelData");
		List<(SerializableGuid, string)> list = new List<(SerializableGuid, string)>();
		LevelState levelStateFromLocalLevel = GetLevelStateFromLocalLevel(levelId);
		if (levelStateFromLocalLevel != null)
		{
			foreach (PropEntry propEntry in levelStateFromLocalLevel.PropEntries)
			{
				if (levelStateFromLocalLevel.SpawnPointIds.Contains(propEntry.InstanceId))
				{
					list.Add((propEntry.InstanceId, propEntry.Label));
				}
			}
		}
		if (verboseLogging)
		{
			Debug.Log(string.Format("{0}: {1}", "spawnPoints", list.Count));
		}
		return list;
	}

	public static IReadOnlyDictionary<SerializableGuid, string> GetSpawnPointsLookup(SerializableGuid levelId)
	{
		if (verboseLogging)
		{
			Debug.Log(string.Format("{0} ( {1}: {2} )", "GetSpawnPointsLookup", "levelId", levelId));
		}
		Dictionary<SerializableGuid, string> dictionary = new Dictionary<SerializableGuid, string>();
		List<(SerializableGuid, string)> spawnPointsList = GetSpawnPointsList(levelId);
		for (int i = 0; i < spawnPointsList.Count; i++)
		{
			dictionary.Add(spawnPointsList[i].Item1, spawnPointsList[i].Item2);
		}
		return dictionary;
	}

	public static string[] GetEveryLocalLevelPath()
	{
		if (verboseLogging)
		{
			Debug.Log("GetEveryLocalLevelPath");
		}
		string[] files = Directory.GetFiles(Application.persistentDataPath, "*.json", SearchOption.AllDirectories);
		if (verboseLogging)
		{
			Debug.Log(string.Format("{0}: {1}", "localLevelPaths", files.Length));
		}
		return files;
	}

	public static LevelState GetLevelStateFromLocalLevel(SerializableGuid levelId)
	{
		if (verboseLogging)
		{
			Debug.Log(string.Format("{0} ( {1}: {2} )", "GetLevelStateFromLocalLevel", "levelId", levelId));
		}
		string[] everyLocalLevelPath = GetEveryLocalLevelPath();
		for (int i = 0; i < everyLocalLevelPath.Length; i++)
		{
			LevelState levelState = LevelStateLoader.Load(File.ReadAllText(everyLocalLevelPath[i]));
			if ((SerializableGuid)levelState.AssetID == levelId)
			{
				return levelState;
			}
		}
		if (verboseLogging)
		{
			Debug.Log(string.Format("{0} ( {1}: {2} ) | Could not find!", "GetLevelStateFromLocalLevel", "levelId", levelId));
		}
		return null;
	}
}
