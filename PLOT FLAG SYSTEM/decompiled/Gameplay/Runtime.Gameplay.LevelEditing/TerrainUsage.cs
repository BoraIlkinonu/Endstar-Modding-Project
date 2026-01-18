using System;
using Endless.Assets;
using Endless.Gameplay.LevelEditing.Level.UpgradeVersions;
using Endless.Shared.DataTypes;
using Newtonsoft.Json;
using UnityEngine;

namespace Runtime.Gameplay.LevelEditing;

[Serializable]
public class TerrainUsage
{
	[JsonProperty("terrain_asset_ref")]
	public AssetReference TerrainAssetReference;

	public int RedirectIndex;

	[JsonIgnore]
	public SerializableGuid TilesetId
	{
		get
		{
			if (!IsActive)
			{
				return SerializableGuid.Empty;
			}
			return TerrainAssetReference.AssetID;
		}
	}

	[JsonIgnore]
	public bool IsActive => TerrainAssetReference != null;

	public TerrainUsage(AssetReference terrainReference)
	{
		TerrainAssetReference = terrainReference;
	}

	public static bool operator ==(TerrainUsage a, TerrainUsage b)
	{
		if ((object)a == b)
		{
			return true;
		}
		if ((object)a == null)
		{
			return false;
		}
		if ((object)b == null)
		{
			return false;
		}
		return a.Equals(b);
	}

	public static bool operator !=(TerrainUsage a, TerrainUsage b)
	{
		return !(a == b);
	}

	public override bool Equals(object obj)
	{
		if (obj is TerrainUsage terrainUsage && TerrainAssetReference == terrainUsage.TerrainAssetReference && TilesetId.Equals(terrainUsage.TilesetId))
		{
			return RedirectIndex == terrainUsage.RedirectIndex;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(TerrainAssetReference, TilesetId, IsActive, RedirectIndex);
	}

	internal static TerrainUsage Upgrade(Game_0_0.TerrainUsage_0_0 oldUsage)
	{
		TerrainUsage terrainUsage = new TerrainUsage(null);
		Debug.Log($"upgrading terrain IsActive: {oldUsage.IsActive} {oldUsage.terrainId}");
		if (oldUsage.IsActive)
		{
			terrainUsage.TerrainAssetReference = new AssetReference
			{
				AssetID = oldUsage.terrainId,
				AssetVersion = oldUsage.TerrainVersion,
				AssetType = "terrain-tileset-cosmetic",
				UpdateParentVersion = false
			};
		}
		terrainUsage.RedirectIndex = oldUsage.RedirectIndex;
		return terrainUsage;
	}
}
