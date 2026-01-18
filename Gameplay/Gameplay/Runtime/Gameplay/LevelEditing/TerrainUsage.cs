using System;
using Endless.Assets;
using Endless.Gameplay.LevelEditing.Level.UpgradeVersions;
using Endless.Shared.DataTypes;
using Newtonsoft.Json;
using UnityEngine;

namespace Runtime.Gameplay.LevelEditing
{
	// Token: 0x02000025 RID: 37
	[Serializable]
	public class TerrainUsage
	{
		// Token: 0x1700001D RID: 29
		// (get) Token: 0x06000089 RID: 137 RVA: 0x00003AC5 File Offset: 0x00001CC5
		[JsonIgnore]
		public SerializableGuid TilesetId
		{
			get
			{
				if (!this.IsActive)
				{
					return SerializableGuid.Empty;
				}
				return this.TerrainAssetReference.AssetID;
			}
		}

		// Token: 0x1700001E RID: 30
		// (get) Token: 0x0600008A RID: 138 RVA: 0x00003AE5 File Offset: 0x00001CE5
		[JsonIgnore]
		public bool IsActive
		{
			get
			{
				return this.TerrainAssetReference != null;
			}
		}

		// Token: 0x0600008B RID: 139 RVA: 0x00003AF3 File Offset: 0x00001CF3
		public TerrainUsage(AssetReference terrainReference)
		{
			this.TerrainAssetReference = terrainReference;
		}

		// Token: 0x0600008C RID: 140 RVA: 0x00003B02 File Offset: 0x00001D02
		public static bool operator ==(TerrainUsage a, TerrainUsage b)
		{
			return a == b || (a != null && b != null && a.Equals(b));
		}

		// Token: 0x0600008D RID: 141 RVA: 0x00003B1B File Offset: 0x00001D1B
		public static bool operator !=(TerrainUsage a, TerrainUsage b)
		{
			return !(a == b);
		}

		// Token: 0x0600008E RID: 142 RVA: 0x00003B28 File Offset: 0x00001D28
		public override bool Equals(object obj)
		{
			TerrainUsage terrainUsage = obj as TerrainUsage;
			return terrainUsage != null && this.TerrainAssetReference == terrainUsage.TerrainAssetReference && this.TilesetId.Equals(terrainUsage.TilesetId) && this.RedirectIndex == terrainUsage.RedirectIndex;
		}

		// Token: 0x0600008F RID: 143 RVA: 0x00003B78 File Offset: 0x00001D78
		public override int GetHashCode()
		{
			return HashCode.Combine<AssetReference, SerializableGuid, bool, int>(this.TerrainAssetReference, this.TilesetId, this.IsActive, this.RedirectIndex);
		}

		// Token: 0x06000090 RID: 144 RVA: 0x00003B98 File Offset: 0x00001D98
		internal static TerrainUsage Upgrade(Game_0_0.TerrainUsage_0_0 oldUsage)
		{
			TerrainUsage terrainUsage = new TerrainUsage(null);
			Debug.Log(string.Format("upgrading terrain IsActive: {0} {1}", oldUsage.IsActive, oldUsage.terrainId));
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

		// Token: 0x04000055 RID: 85
		[JsonProperty("terrain_asset_ref")]
		public AssetReference TerrainAssetReference;

		// Token: 0x04000056 RID: 86
		public int RedirectIndex;
	}
}
