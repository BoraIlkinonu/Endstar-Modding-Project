using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Assets;
using Endless.Gameplay.LevelEditing.Level.UpgradeVersions;
using Endless.Shared.DataTypes;
using Newtonsoft.Json;
using UnityEngine;

namespace Runtime.Gameplay.LevelEditing;

[Serializable]
public class GameLibrary
{
	public struct RemoveTerrainEntryResult
	{
		public bool Success;

		public int IndexOfTerrainRedirectTo;

		public RemoveTerrainEntryResult(bool success, int indexOfTerrainRedirectTo)
		{
			Success = success;
			IndexOfTerrainRedirectTo = indexOfTerrainRedirectTo;
		}
	}

	[SerializeField]
	[JsonProperty]
	private List<TerrainUsage> terrainEntries = new List<TerrainUsage>();

	[SerializeField]
	[JsonProperty("props")]
	private List<AssetReference> propReferences = new List<AssetReference>();

	[SerializeField]
	[JsonProperty("audio")]
	private List<AssetReference> audioReferences = new List<AssetReference>();

	[JsonIgnore]
	public List<TerrainUsage> TerrainEntries => terrainEntries;

	[JsonIgnore]
	public IReadOnlyList<AssetReference> PropReferences => propReferences;

	[JsonIgnore]
	public IReadOnlyList<AssetReference> AudioReferences => audioReferences;

	public RemoveTerrainEntryResult RemoveTerrainEntry(TerrainUsage terrainUsage, TerrainUsage redirectEntry)
	{
		return RemoveTerrainEntry(terrainUsage.TilesetId, redirectEntry.TilesetId);
	}

	public RemoveTerrainEntryResult RemoveTerrainEntry(SerializableGuid id, SerializableGuid redirectId)
	{
		TerrainUsage terrainUsage = terrainEntries.FirstOrDefault((TerrainUsage entry) => entry.TilesetId == id);
		if ((object)terrainUsage == null)
		{
			return new RemoveTerrainEntryResult(success: false, -1);
		}
		TerrainUsage terrainUsage2 = terrainEntries.FirstOrDefault((TerrainUsage replacement) => replacement.TilesetId == redirectId);
		if ((object)terrainUsage2 == null)
		{
			return new RemoveTerrainEntryResult(success: false, -1);
		}
		terrainUsage.TerrainAssetReference = null;
		terrainUsage.RedirectIndex = terrainEntries.IndexOf(terrainUsage2);
		return new RemoveTerrainEntryResult(success: true, terrainEntries.IndexOf(terrainUsage));
	}

	public bool AddTerrainUsage(SerializableGuid tilesetId, string version)
	{
		TerrainUsage terrainUsage = new TerrainUsage(new AssetReference
		{
			AssetID = tilesetId,
			AssetVersion = version,
			AssetType = "terrain-tileset-cosmetic"
		});
		terrainUsage.RedirectIndex = terrainEntries.Count;
		if (terrainEntries.Any((TerrainUsage entry) => entry.TilesetId == tilesetId))
		{
			return false;
		}
		terrainEntries.Add(terrainUsage);
		return true;
	}

	public bool AddTerrainUsages(IEnumerable<AssetReference> newUsages)
	{
		int num = 0;
		foreach (AssetReference newAssetReference in newUsages)
		{
			TerrainUsage item = new TerrainUsage(newAssetReference)
			{
				RedirectIndex = terrainEntries.Count
			};
			if (!terrainEntries.Any((TerrainUsage entry) => entry.TilesetId == newAssetReference.AssetID))
			{
				terrainEntries.Add(item);
				num++;
			}
		}
		return num > 0;
	}

	public int InsertTerrainUsage(SerializableGuid tilesetId, string version, int index)
	{
		TerrainUsage terrainUsage = new TerrainUsage(new AssetReference
		{
			AssetID = tilesetId,
			AssetVersion = version,
			AssetType = "terrain-tileset-cosmetic"
		});
		terrainUsage.RedirectIndex = index;
		_ = terrainEntries.Count;
		terrainEntries[index] = terrainUsage;
		return terrainEntries.Count - 1;
	}

	public bool SetTerrainUsageVersion(SerializableGuid id, string newTerrainVersion)
	{
		TerrainUsage terrainUsage = terrainEntries.FirstOrDefault((TerrainUsage entry) => entry.TilesetId == id);
		if ((object)terrainUsage == null)
		{
			Debug.LogException(new Exception(string.Format("Could not find {0} in {1} with an {2} of {3}!", "TerrainUsage", "terrainEntries", "id", id)));
			return false;
		}
		int index = terrainEntries.IndexOf(terrainUsage);
		if (terrainEntries[index].TerrainAssetReference.AssetVersion != newTerrainVersion)
		{
			terrainEntries[index].TerrainAssetReference.AssetVersion = newTerrainVersion;
			return true;
		}
		return false;
	}

	public void OverrideTerrainUsages(List<TerrainUsage> overrides)
	{
		terrainEntries = overrides;
	}

	public void ClearProps()
	{
		propReferences.Clear();
	}

	public bool AddProp(AssetReference propReference)
	{
		for (int i = 0; i < propReferences.Count; i++)
		{
			if (propReferences[i].AssetID == propReference.AssetID)
			{
				return false;
			}
		}
		propReferences.Add(propReference);
		return true;
	}

	public bool AddProps(IEnumerable<AssetReference> newPropReferences)
	{
		List<AssetReference> list = new List<AssetReference>();
		foreach (AssetReference newPropReference in newPropReferences)
		{
			bool flag = false;
			for (int i = 0; i < propReferences.Count; i++)
			{
				if (propReferences[i].AssetID == newPropReference.AssetID)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				list.Add(newPropReference);
			}
		}
		if (list.Count > 0)
		{
			propReferences.AddRange(list);
			return true;
		}
		return false;
	}

	public bool SetPropVersion(SerializableGuid id, string newAssetVersion)
	{
		AssetReference assetReference = propReferences.FirstOrDefault((AssetReference entry) => (SerializableGuid)entry.AssetID == id);
		if ((object)assetReference == null)
		{
			Debug.LogException(new Exception(string.Format("Could not find {0} in {1} with an {2} of {3}!", "AssetReference", "propReferences", "id", id)));
			return false;
		}
		int index = propReferences.IndexOf(assetReference);
		if (propReferences[index].AssetVersion != newAssetVersion)
		{
			propReferences[index].AssetVersion = newAssetVersion;
			return true;
		}
		return false;
	}

	public bool RemoveProp(SerializableGuid assetID)
	{
		AssetReference assetReference = propReferences.FirstOrDefault((AssetReference propReference) => (SerializableGuid)propReference.AssetID == assetID);
		if ((object)assetReference == null)
		{
			return false;
		}
		propReferences.Remove(assetReference);
		return true;
	}

	public static GameLibrary Upgrade(Game_0_0.GameLibrary_0_0 oldLibrary)
	{
		GameLibrary gameLibrary = new GameLibrary();
		gameLibrary.propReferences = new List<AssetReference>();
		gameLibrary.terrainEntries = new List<TerrainUsage>();
		foreach (Game_0_0.TerrainUsage_0_0 terrainEntry in oldLibrary.terrainEntries)
		{
			gameLibrary.terrainEntries.Add(TerrainUsage.Upgrade(terrainEntry));
		}
		return gameLibrary;
	}

	public IReadOnlyList<AssetReference> GetPropAssetReferencesForLoad(HashSet<SerializableGuid> propIds)
	{
		return PropReferences.Where((AssetReference reference) => propIds.Contains(reference.AssetID)).ToArray();
	}

	public IReadOnlyList<TerrainUsage> GetTerrainUsagesInLevel(HashSet<int> tilesetIds)
	{
		List<TerrainUsage> list = new List<TerrainUsage>();
		foreach (int tilesetId in tilesetIds)
		{
			if (TerrainEntries.Count > tilesetId)
			{
				TerrainUsage item = TerrainEntries[tilesetId];
				list.Add(item);
			}
		}
		return list;
	}

	public int GetTerrainIndex(SerializableGuid assetId)
	{
		string text = assetId;
		for (int i = 0; i < terrainEntries.Count; i++)
		{
			if (terrainEntries[i].IsActive && terrainEntries[i].TerrainAssetReference.AssetID == text)
			{
				return i;
			}
		}
		return -1;
	}

	public bool AddAudio(AssetReference audioReference)
	{
		for (int i = 0; i < audioReferences.Count; i++)
		{
			if (audioReferences[i].AssetID == audioReference.AssetID)
			{
				return false;
			}
		}
		audioReferences.Add(audioReference);
		return true;
	}

	public bool SetAudioVersion(SerializableGuid id, string newAssetVersion)
	{
		AssetReference assetReference = audioReferences.FirstOrDefault((AssetReference entry) => (SerializableGuid)entry.AssetID == id);
		if ((object)assetReference == null)
		{
			Debug.LogException(new Exception(string.Format("Could not find {0} in {1} with an {2} of {3}!", "AssetReference", "audioReferences", "id", id)));
			return false;
		}
		int index = audioReferences.IndexOf(assetReference);
		if (audioReferences[index].AssetVersion != newAssetVersion)
		{
			audioReferences[index].AssetVersion = newAssetVersion;
			return true;
		}
		return false;
	}

	public bool RemoveAudio(SerializableGuid assetID)
	{
		AssetReference assetReference = audioReferences.FirstOrDefault((AssetReference propReference) => (SerializableGuid)propReference.AssetID == assetID);
		if ((object)assetReference == null)
		{
			return false;
		}
		audioReferences.Remove(assetReference);
		return true;
	}

	public TerrainUsage GetActiveTerrainUsageFromIndex(int index)
	{
		HashSet<int> hashSet = new HashSet<int>();
		TerrainUsage terrainUsage = TerrainEntries[index];
		while (!terrainUsage.IsActive)
		{
			int redirectIndex = terrainUsage.RedirectIndex;
			if (redirectIndex > TerrainEntries.Count)
			{
				Debug.LogException(new Exception("Had improper index in terrain redirects"));
				return null;
			}
			if (hashSet.Contains(redirectIndex))
			{
				Debug.LogException(new Exception($"Redirect Indexes looped. Can't validate terrain usage at index: {index}"));
				return null;
			}
			hashSet.Add(redirectIndex);
			terrainUsage = TerrainEntries[redirectIndex];
		}
		return terrainUsage;
	}
}
