using System;
using Endless.Assets;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Tilesets;
using Endless.Props.Assets;
using Endless.Shared.Debugging;
using Endless.TerrainCosmetics;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIGameAsset : IEquatable<UIGameAsset>
{
	public Sprite Icon { get; private set; }

	public Asset Asset { get; private set; }

	public UIGameAssetTypes Type { get; private set; }

	public int IconFileInstance { get; private set; }

	public Tileset Tileset { get; private set; }

	public bool IsTileset { get; private set; }

	public string AssetID
	{
		get
		{
			if (!IsTileset)
			{
				return Asset.AssetID;
			}
			return Tileset.Asset.AssetID;
		}
	}

	public string Name
	{
		get
		{
			if (!IsTileset)
			{
				return Asset.Name;
			}
			return Tileset.DisplayName;
		}
	}

	public string Description
	{
		get
		{
			if (!IsTileset)
			{
				return Asset.Description;
			}
			return Tileset.Description;
		}
	}

	public string AssetVersion
	{
		get
		{
			if (!IsTileset)
			{
				return Asset.AssetVersion;
			}
			return Tileset.Asset.AssetVersion;
		}
	}

	public bool IsNull
	{
		get
		{
			if (Tileset == null)
			{
				return Asset == null;
			}
			return false;
		}
	}

	public UIGameAsset(PropLibrary.RuntimePropInfo propInfo)
	{
		Asset = propInfo.PropData;
		Type = UIGameAssetTypes.Prop;
		IconFileInstance = -1;
		Tileset = null;
		IsTileset = false;
		Icon = propInfo.Icon;
	}

	public UIGameAsset(RuntimeAudioInfo audioInfo)
	{
		Asset = audioInfo.AudioAsset;
		switch (audioInfo.AudioAsset.AudioCategory)
		{
		case AudioCategory.Music:
			Type = UIGameAssetTypes.Music;
			break;
		case AudioCategory.SFX:
			Type = UIGameAssetTypes.SFX;
			break;
		case AudioCategory.Ambient:
			Type = UIGameAssetTypes.Ambient;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		IconFileInstance = -1;
		Tileset = null;
		IsTileset = false;
		Icon = audioInfo.Icon;
	}

	public UIGameAsset(Prop prop)
	{
		Asset = prop;
		Type = UIGameAssetTypes.Prop;
		IconFileInstance = prop.IconFileInstanceId;
		Tileset = null;
		IsTileset = false;
		Icon = null;
	}

	public UIGameAsset(TerrainTilesetCosmeticAsset terrainTilesetCosmeticAsset)
	{
		Asset = terrainTilesetCosmeticAsset;
		Type = UIGameAssetTypes.Terrain;
		if (terrainTilesetCosmeticAsset.DisplayIconFileInstance == null)
		{
			DebugUtility.LogException(new NullReferenceException("terrainTilesetCosmeticAsset.DisplayIconFileInstance is null for " + terrainTilesetCosmeticAsset.Name + " with an asset id of " + terrainTilesetCosmeticAsset.AssetID + "!"));
		}
		else
		{
			IconFileInstance = terrainTilesetCosmeticAsset.DisplayIconFileInstance.AssetFileInstanceId;
		}
		Tileset = null;
		IsTileset = false;
		Icon = null;
	}

	public UIGameAsset(Tileset tileset)
	{
		Asset = null;
		Type = UIGameAssetTypes.Terrain;
		IconFileInstance = -1;
		Tileset = tileset;
		IsTileset = true;
		Icon = null;
	}

	public UIGameAsset(AudioAsset audioAsset, UIGameAssetTypes type)
	{
		Asset = audioAsset;
		Type = type;
		IconFileInstance = audioAsset.IconFileInstanceId;
		Tileset = null;
		IsTileset = false;
		Icon = null;
	}

	public void SetAsset(Asset asset)
	{
		Asset = asset;
	}

	public void SetType(UIGameAssetTypes type)
	{
		Type = type;
	}

	public static bool operator ==(UIGameAsset left, UIGameAsset right)
	{
		if ((object)left == right)
		{
			return true;
		}
		if ((object)left == null || (object)right == null)
		{
			return false;
		}
		return left.Equals(right);
	}

	public static bool operator !=(UIGameAsset left, UIGameAsset right)
	{
		return !(left == right);
	}

	public bool Equals(UIGameAsset other)
	{
		if ((object)other == null)
		{
			return false;
		}
		if ((object)this == other)
		{
			return true;
		}
		if (object.Equals(Icon, other.Icon) && object.Equals(Asset, other.Asset) && Type == other.Type && IconFileInstance == other.IconFileInstance && object.Equals(Tileset, other.Tileset))
		{
			return IsTileset == other.IsTileset;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as UIGameAsset);
	}

	public static bool IsNullOrEmpty(UIGameAsset asset)
	{
		return asset?.IsNull ?? true;
	}

	public override int GetHashCode()
	{
		return (((((17 * 23 + ((Icon != null) ? Icon.GetHashCode() : 0)) * 23 + ((Asset != null) ? Asset.GetHashCode() : 0)) * 23 + Type.GetHashCode()) * 23 + IconFileInstance.GetHashCode()) * 23 + ((Tileset != null) ? Tileset.GetHashCode() : 0)) * 23 + IsTileset.GetHashCode();
	}

	public override string ToString()
	{
		return string.Format("{0}: {1}\n", "Type", Type) + string.Format("{0}: {1}\n", "IconFileInstance", IconFileInstance) + "Asset: " + Asset.DebugSafeJson() + "\nTileset: " + Tileset.DebugSafeJson();
	}
}
