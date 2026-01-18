using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.DataTypes;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x020000E3 RID: 227
	public static class UILevelUtility
	{
		// Token: 0x060003CC RID: 972 RVA: 0x000184FC File Offset: 0x000166FC
		public static LevelReference GetLevelData(SerializableGuid levelId)
		{
			if (UILevelUtility.verboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2} )", "GetLevelData", "levelId", levelId));
			}
			if (levelId.IsEmpty)
			{
				if (UILevelUtility.verboseLogging)
				{
					Debug.Log(string.Format("{0} ( {1}: {2} ) | Could not find! || It's empty!", "GetLevelData", "levelId", levelId));
				}
				return null;
			}
			foreach (LevelReference levelReference in MonoBehaviourSingleton<GameEditor>.Instance.GetLevelReferences())
			{
				if (levelReference.AssetID == levelId)
				{
					return levelReference;
				}
			}
			if (UILevelUtility.verboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2} ) | Could not find!", "GetLevelData", "levelId", levelId));
			}
			return null;
		}

		// Token: 0x060003CD RID: 973 RVA: 0x000185E4 File Offset: 0x000167E4
		[return: TupleElementNames(new string[] { "InstanceId", "Label" })]
		public static List<ValueTuple<SerializableGuid, string>> GetSpawnPointsList(SerializableGuid levelId)
		{
			if (UILevelUtility.verboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2} )", "GetSpawnPointsList", "levelId", levelId));
			}
			Debug.LogWarning("We need a way to get Spawn Point Ids and Labels from LevelData");
			List<ValueTuple<SerializableGuid, string>> list = new List<ValueTuple<SerializableGuid, string>>();
			LevelState levelStateFromLocalLevel = UILevelUtility.GetLevelStateFromLocalLevel(levelId);
			if (levelStateFromLocalLevel != null)
			{
				foreach (PropEntry propEntry in levelStateFromLocalLevel.PropEntries)
				{
					if (levelStateFromLocalLevel.SpawnPointIds.Contains(propEntry.InstanceId))
					{
						list.Add(new ValueTuple<SerializableGuid, string>(propEntry.InstanceId, propEntry.Label));
					}
				}
			}
			if (UILevelUtility.verboseLogging)
			{
				Debug.Log(string.Format("{0}: {1}", "spawnPoints", list.Count));
			}
			return list;
		}

		// Token: 0x060003CE RID: 974 RVA: 0x000186BC File Offset: 0x000168BC
		public static IReadOnlyDictionary<SerializableGuid, string> GetSpawnPointsLookup(SerializableGuid levelId)
		{
			if (UILevelUtility.verboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2} )", "GetSpawnPointsLookup", "levelId", levelId));
			}
			Dictionary<SerializableGuid, string> dictionary = new Dictionary<SerializableGuid, string>();
			List<ValueTuple<SerializableGuid, string>> spawnPointsList = UILevelUtility.GetSpawnPointsList(levelId);
			for (int i = 0; i < spawnPointsList.Count; i++)
			{
				dictionary.Add(spawnPointsList[i].Item1, spawnPointsList[i].Item2);
			}
			return dictionary;
		}

		// Token: 0x060003CF RID: 975 RVA: 0x0001872C File Offset: 0x0001692C
		public static string[] GetEveryLocalLevelPath()
		{
			if (UILevelUtility.verboseLogging)
			{
				Debug.Log("GetEveryLocalLevelPath");
			}
			string[] files = Directory.GetFiles(Application.persistentDataPath, "*.json", SearchOption.AllDirectories);
			if (UILevelUtility.verboseLogging)
			{
				Debug.Log(string.Format("{0}: {1}", "localLevelPaths", files.Length));
			}
			return files;
		}

		// Token: 0x060003D0 RID: 976 RVA: 0x00018780 File Offset: 0x00016980
		public static LevelState GetLevelStateFromLocalLevel(SerializableGuid levelId)
		{
			if (UILevelUtility.verboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2} )", "GetLevelStateFromLocalLevel", "levelId", levelId));
			}
			string[] everyLocalLevelPath = UILevelUtility.GetEveryLocalLevelPath();
			for (int i = 0; i < everyLocalLevelPath.Length; i++)
			{
				LevelState levelState = LevelStateLoader.Load(File.ReadAllText(everyLocalLevelPath[i]));
				if (levelState.AssetID == levelId)
				{
					return levelState;
				}
			}
			if (UILevelUtility.verboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2} ) | Could not find!", "GetLevelStateFromLocalLevel", "levelId", levelId));
			}
			return null;
		}

		// Token: 0x040003E9 RID: 1001
		[Header("Debugging")]
		private static readonly bool verboseLogging;
	}
}
