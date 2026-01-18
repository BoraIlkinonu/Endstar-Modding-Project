using System.Collections.Generic;
using Endless.Assets;
using UnityEngine;

namespace Endless.Gameplay.LevelEditing.Tilesets;

public class GeneratedTerrainTracker
{
	private Dictionary<AssetReference, List<GameObject>> assetIdToGeneratedTerrainMap;

	public GeneratedTerrainTracker()
	{
		assetIdToGeneratedTerrainMap = new Dictionary<AssetReference, List<GameObject>>();
	}

	public void Add(AssetReference assetReference, GameObject generatedTerrain)
	{
		if (!assetIdToGeneratedTerrainMap.TryGetValue(assetReference, out var value))
		{
			value = new List<GameObject>();
			assetIdToGeneratedTerrainMap.Add(assetReference, value);
		}
		value.Add(generatedTerrain);
	}

	public void ClearGeneratedTerrainForId(AssetReference assetReference)
	{
		if (assetIdToGeneratedTerrainMap.TryGetValue(assetReference, out var value))
		{
			for (int num = value.Count - 1; num >= 0; num--)
			{
				Object.Destroy(value[num]);
			}
		}
	}
}
