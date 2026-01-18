using System;
using Endless.Assets;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Tilesets;
using Endless.Props.Assets;
using Endless.Shared.Debugging;
using Endless.TerrainCosmetics;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x020000D8 RID: 216
	public class UIGameAsset : IEquatable<UIGameAsset>
	{
		// Token: 0x0600037A RID: 890 RVA: 0x000170BE File Offset: 0x000152BE
		public UIGameAsset(PropLibrary.RuntimePropInfo propInfo)
		{
			this.Asset = propInfo.PropData;
			this.Type = UIGameAssetTypes.Prop;
			this.IconFileInstance = -1;
			this.Tileset = null;
			this.IsTileset = false;
			this.Icon = propInfo.Icon;
		}

		// Token: 0x0600037B RID: 891 RVA: 0x000170FC File Offset: 0x000152FC
		public UIGameAsset(RuntimeAudioInfo audioInfo)
		{
			this.Asset = audioInfo.AudioAsset;
			switch (audioInfo.AudioAsset.AudioCategory)
			{
			case AudioCategory.Music:
				this.Type = UIGameAssetTypes.Music;
				break;
			case AudioCategory.SFX:
				this.Type = UIGameAssetTypes.SFX;
				break;
			case AudioCategory.Ambient:
				this.Type = UIGameAssetTypes.Ambient;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			this.IconFileInstance = -1;
			this.Tileset = null;
			this.IsTileset = false;
			this.Icon = audioInfo.Icon;
		}

		// Token: 0x0600037C RID: 892 RVA: 0x0001717E File Offset: 0x0001537E
		public UIGameAsset(Prop prop)
		{
			this.Asset = prop;
			this.Type = UIGameAssetTypes.Prop;
			this.IconFileInstance = prop.IconFileInstanceId;
			this.Tileset = null;
			this.IsTileset = false;
			this.Icon = null;
		}

		// Token: 0x0600037D RID: 893 RVA: 0x000171B8 File Offset: 0x000153B8
		public UIGameAsset(TerrainTilesetCosmeticAsset terrainTilesetCosmeticAsset)
		{
			this.Asset = terrainTilesetCosmeticAsset;
			this.Type = UIGameAssetTypes.Terrain;
			if (terrainTilesetCosmeticAsset.DisplayIconFileInstance == null)
			{
				DebugUtility.LogException(new NullReferenceException(string.Concat(new string[] { "terrainTilesetCosmeticAsset.DisplayIconFileInstance is null for ", terrainTilesetCosmeticAsset.Name, " with an asset id of ", terrainTilesetCosmeticAsset.AssetID, "!" })), null);
			}
			else
			{
				this.IconFileInstance = terrainTilesetCosmeticAsset.DisplayIconFileInstance.AssetFileInstanceId;
			}
			this.Tileset = null;
			this.IsTileset = false;
			this.Icon = null;
		}

		// Token: 0x0600037E RID: 894 RVA: 0x00017249 File Offset: 0x00015449
		public UIGameAsset(Tileset tileset)
		{
			this.Asset = null;
			this.Type = UIGameAssetTypes.Terrain;
			this.IconFileInstance = -1;
			this.Tileset = tileset;
			this.IsTileset = true;
			this.Icon = null;
		}

		// Token: 0x0600037F RID: 895 RVA: 0x0001727B File Offset: 0x0001547B
		public UIGameAsset(AudioAsset audioAsset, UIGameAssetTypes type)
		{
			this.Asset = audioAsset;
			this.Type = type;
			this.IconFileInstance = audioAsset.IconFileInstanceId;
			this.Tileset = null;
			this.IsTileset = false;
			this.Icon = null;
		}

		// Token: 0x17000049 RID: 73
		// (get) Token: 0x06000380 RID: 896 RVA: 0x000172B2 File Offset: 0x000154B2
		// (set) Token: 0x06000381 RID: 897 RVA: 0x000172BA File Offset: 0x000154BA
		public Sprite Icon { get; private set; }

		// Token: 0x1700004A RID: 74
		// (get) Token: 0x06000382 RID: 898 RVA: 0x000172C3 File Offset: 0x000154C3
		// (set) Token: 0x06000383 RID: 899 RVA: 0x000172CB File Offset: 0x000154CB
		public Asset Asset { get; private set; }

		// Token: 0x1700004B RID: 75
		// (get) Token: 0x06000384 RID: 900 RVA: 0x000172D4 File Offset: 0x000154D4
		// (set) Token: 0x06000385 RID: 901 RVA: 0x000172DC File Offset: 0x000154DC
		public UIGameAssetTypes Type { get; private set; }

		// Token: 0x1700004C RID: 76
		// (get) Token: 0x06000386 RID: 902 RVA: 0x000172E5 File Offset: 0x000154E5
		// (set) Token: 0x06000387 RID: 903 RVA: 0x000172ED File Offset: 0x000154ED
		public int IconFileInstance { get; private set; }

		// Token: 0x1700004D RID: 77
		// (get) Token: 0x06000388 RID: 904 RVA: 0x000172F6 File Offset: 0x000154F6
		// (set) Token: 0x06000389 RID: 905 RVA: 0x000172FE File Offset: 0x000154FE
		public Tileset Tileset { get; private set; }

		// Token: 0x1700004E RID: 78
		// (get) Token: 0x0600038A RID: 906 RVA: 0x00017307 File Offset: 0x00015507
		// (set) Token: 0x0600038B RID: 907 RVA: 0x0001730F File Offset: 0x0001550F
		public bool IsTileset { get; private set; }

		// Token: 0x1700004F RID: 79
		// (get) Token: 0x0600038C RID: 908 RVA: 0x00017318 File Offset: 0x00015518
		public string AssetID
		{
			get
			{
				if (!this.IsTileset)
				{
					return this.Asset.AssetID;
				}
				return this.Tileset.Asset.AssetID;
			}
		}

		// Token: 0x17000050 RID: 80
		// (get) Token: 0x0600038D RID: 909 RVA: 0x0001733E File Offset: 0x0001553E
		public string Name
		{
			get
			{
				if (!this.IsTileset)
				{
					return this.Asset.Name;
				}
				return this.Tileset.DisplayName;
			}
		}

		// Token: 0x17000051 RID: 81
		// (get) Token: 0x0600038E RID: 910 RVA: 0x0001735F File Offset: 0x0001555F
		public string Description
		{
			get
			{
				if (!this.IsTileset)
				{
					return this.Asset.Description;
				}
				return this.Tileset.Description;
			}
		}

		// Token: 0x17000052 RID: 82
		// (get) Token: 0x0600038F RID: 911 RVA: 0x00017380 File Offset: 0x00015580
		public string AssetVersion
		{
			get
			{
				if (!this.IsTileset)
				{
					return this.Asset.AssetVersion;
				}
				return this.Tileset.Asset.AssetVersion;
			}
		}

		// Token: 0x17000053 RID: 83
		// (get) Token: 0x06000390 RID: 912 RVA: 0x000173A6 File Offset: 0x000155A6
		public bool IsNull
		{
			get
			{
				return this.Tileset == null && this.Asset == null;
			}
		}

		// Token: 0x06000391 RID: 913 RVA: 0x000173BB File Offset: 0x000155BB
		public void SetAsset(Asset asset)
		{
			this.Asset = asset;
		}

		// Token: 0x06000392 RID: 914 RVA: 0x000173C4 File Offset: 0x000155C4
		public void SetType(UIGameAssetTypes type)
		{
			this.Type = type;
		}

		// Token: 0x06000393 RID: 915 RVA: 0x000173CD File Offset: 0x000155CD
		public static bool operator ==(UIGameAsset left, UIGameAsset right)
		{
			return left == right || (left != null && right != null && left.Equals(right));
		}

		// Token: 0x06000394 RID: 916 RVA: 0x000173E4 File Offset: 0x000155E4
		public static bool operator !=(UIGameAsset left, UIGameAsset right)
		{
			return !(left == right);
		}

		// Token: 0x06000395 RID: 917 RVA: 0x000173F0 File Offset: 0x000155F0
		public bool Equals(UIGameAsset other)
		{
			return other != null && (this == other || (object.Equals(this.Icon, other.Icon) && object.Equals(this.Asset, other.Asset) && this.Type == other.Type && this.IconFileInstance == other.IconFileInstance && object.Equals(this.Tileset, other.Tileset) && this.IsTileset == other.IsTileset));
		}

		// Token: 0x06000396 RID: 918 RVA: 0x0001746D File Offset: 0x0001566D
		public override bool Equals(object obj)
		{
			return this.Equals(obj as UIGameAsset);
		}

		// Token: 0x06000397 RID: 919 RVA: 0x0001747B File Offset: 0x0001567B
		public static bool IsNullOrEmpty(UIGameAsset asset)
		{
			return asset == null || asset.IsNull;
		}

		// Token: 0x06000398 RID: 920 RVA: 0x00017488 File Offset: 0x00015688
		public override int GetHashCode()
		{
			return (((((17 * 23 + ((this.Icon != null) ? this.Icon.GetHashCode() : 0)) * 23 + ((this.Asset != null) ? this.Asset.GetHashCode() : 0)) * 23 + this.Type.GetHashCode()) * 23 + this.IconFileInstance.GetHashCode()) * 23 + ((this.Tileset != null) ? this.Tileset.GetHashCode() : 0)) * 23 + this.IsTileset.GetHashCode();
		}

		// Token: 0x06000399 RID: 921 RVA: 0x00017528 File Offset: 0x00015728
		public override string ToString()
		{
			return string.Concat(new string[]
			{
				string.Format("{0}: {1}\n", "Type", this.Type),
				string.Format("{0}: {1}\n", "IconFileInstance", this.IconFileInstance),
				"Asset: ",
				this.Asset.DebugSafeJson(),
				"\nTileset: ",
				this.Tileset.DebugSafeJson()
			});
		}
	}
}
