using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Assets;
using Endless.Data.WeightTables;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared.DataTypes;
using Endless.TerrainCosmetics;
using UnityEngine;

namespace Endless.Gameplay.LevelEditing.Tilesets
{
	// Token: 0x02000534 RID: 1332
	public abstract class Tileset
	{
		// Token: 0x17000614 RID: 1556
		// (get) Token: 0x06001FF7 RID: 8183 RVA: 0x000908C9 File Offset: 0x0008EAC9
		public Asset Asset
		{
			get
			{
				return this.asset;
			}
		}

		// Token: 0x17000615 RID: 1557
		// (get) Token: 0x06001FF8 RID: 8184 RVA: 0x000908D1 File Offset: 0x0008EAD1
		public virtual string DisplayName
		{
			get
			{
				return this.asset.Name;
			}
		}

		// Token: 0x17000616 RID: 1558
		// (get) Token: 0x06001FF9 RID: 8185 RVA: 0x000908DE File Offset: 0x0008EADE
		public virtual string Description
		{
			get
			{
				return this.asset.Description;
			}
		}

		// Token: 0x17000617 RID: 1559
		// (get) Token: 0x06001FFA RID: 8186 RVA: 0x000908EB File Offset: 0x0008EAEB
		public virtual Sprite DisplayIcon
		{
			get
			{
				return this.displayIcon;
			}
		}

		// Token: 0x17000618 RID: 1560
		// (get) Token: 0x06001FFB RID: 8187 RVA: 0x000908F3 File Offset: 0x0008EAF3
		public TilesetType TilesetType { get; }

		// Token: 0x17000619 RID: 1561
		// (get) Token: 0x06001FFC RID: 8188 RVA: 0x000908FB File Offset: 0x0008EAFB
		public bool HasFringe
		{
			get
			{
				return this.fringeSets.Length != 0;
			}
		}

		// Token: 0x1700061A RID: 1562
		// (get) Token: 0x06001FFD RID: 8189 RVA: 0x00090907 File Offset: 0x0008EB07
		protected TilesetCosmeticProfile CosmeticProfile
		{
			get
			{
				return this.cosmeticProfile;
			}
		}

		// Token: 0x1700061B RID: 1563
		// (get) Token: 0x06001FFE RID: 8190 RVA: 0x0009090F File Offset: 0x0008EB0F
		// (set) Token: 0x06001FFF RID: 8191 RVA: 0x00090917 File Offset: 0x0008EB17
		public int Index { get; set; }

		// Token: 0x06002000 RID: 8192 RVA: 0x00090920 File Offset: 0x0008EB20
		public Tileset(TilesetCosmeticProfile cosmeticProfile, Asset asset, Sprite displayIcon, int index)
		{
			this.Index = index;
			this.cosmeticProfile = cosmeticProfile;
			this.asset = asset;
			this.displayIcon = displayIcon;
			List<FringeSet> list = new List<FringeSet>();
			if (!cosmeticProfile)
			{
				return;
			}
			this.TilesetType = cosmeticProfile.TilesetType;
			this.consideredTilesets = this.cosmeticProfile.ConsideredTilesetIds ?? Array.Empty<SerializableGuid>();
			this.materialVariantWeightTables = this.cosmeticProfile.MaterialVariantWeightTables;
			foreach (BaseFringeCosmeticProfile baseFringeCosmeticProfile in this.cosmeticProfile.BaseFringeSets)
			{
				list.Add(new BaseFringeSet(baseFringeCosmeticProfile));
			}
			foreach (SlopeFringeCosmeticProfile slopeFringeCosmeticProfile in this.cosmeticProfile.SlopeFringeSets)
			{
				list.Add(new SlopeFringeSet(slopeFringeCosmeticProfile));
			}
			this.fringeSets = list.ToArray();
		}

		// Token: 0x06002001 RID: 8193
		public abstract TileSpawnContext GetValidVisualForCellPosition(Stage stage, Vector3Int coordinates);

		// Token: 0x06002002 RID: 8194 RVA: 0x00090A27 File Offset: 0x0008EC27
		public bool ConsidersTileset(string tilesetId)
		{
			return !string.IsNullOrEmpty(tilesetId) && this.consideredTilesets.Contains(tilesetId);
		}

		// Token: 0x06002003 RID: 8195 RVA: 0x00090A44 File Offset: 0x0008EC44
		public bool ConsidersTileset(Tileset tilset)
		{
			return this.consideredTilesets.Contains(tilset.Asset.AssetID);
		}

		// Token: 0x06002004 RID: 8196 RVA: 0x00090A64 File Offset: 0x0008EC64
		public void ApplyMaterialVariants(Transform chosenVisual, global::System.Random random)
		{
			if (this.materialNameToBaseNameMap == null)
			{
				this.materialNameToBaseNameMap = new Dictionary<string, string>();
			}
			MeshRenderer component = chosenVisual.GetComponent<MeshRenderer>();
			if (!component)
			{
				return;
			}
			Material[] sharedMaterials = component.sharedMaterials;
			for (int i = 0; i < sharedMaterials.Length; i++)
			{
				if (sharedMaterials[i] != null)
				{
					string name = sharedMaterials[i].name;
					string name2;
					if (!this.materialNameToBaseNameMap.TryGetValue(name, out name2))
					{
						name2 = sharedMaterials[i].name;
						this.materialNameToBaseNameMap.Add(name, name2);
					}
					if (this.materialVariantWeightTables != null && this.materialVariantWeightTables.Length != 0)
					{
						MaterialVariantWeightTable materialVariantWeightTable = null;
						foreach (MaterialWeightTableAsset materialWeightTableAsset in this.materialVariantWeightTables)
						{
							if (!(materialWeightTableAsset.MaterialId != name2))
							{
								materialVariantWeightTable = materialWeightTableAsset.Table;
								break;
							}
						}
						if (materialVariantWeightTable != null)
						{
							Material randomWeightedEntry = materialVariantWeightTable.GetRandomWeightedEntry(random);
							sharedMaterials[i] = randomWeightedEntry;
						}
					}
				}
			}
			component.sharedMaterials = sharedMaterials;
		}

		// Token: 0x06002005 RID: 8197 RVA: 0x00090B54 File Offset: 0x0008ED54
		public void ApplyFringe(Transform chosenVisual, Vector3Int cellCoordinate, TileSpawnContext context, Stage stage)
		{
			FringeSet[] array = this.fringeSets;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].AddFringe(chosenVisual, cellCoordinate, context, stage);
			}
		}

		// Token: 0x06002006 RID: 8198 RVA: 0x00090B84 File Offset: 0x0008ED84
		public void ApplyDecorations(TerrainCell cell, TileSpawnContext context, Action<GameObject> destroy, global::System.Random random, Stage stage)
		{
			for (DecorationIndex decorationIndex = DecorationIndex.Top; decorationIndex <= DecorationIndex.Right; decorationIndex++)
			{
				this.HandleDecorationIndex(decorationIndex, cell, context, destroy, random, stage);
			}
		}

		// Token: 0x06002007 RID: 8199 RVA: 0x00090BAC File Offset: 0x0008EDAC
		private void HandleDecorationIndex(DecorationIndex index, TerrainCell cell, TileSpawnContext context, Action<GameObject> destroy, global::System.Random random, Stage stage)
		{
			if (!this.cosmeticProfile)
			{
				return;
			}
			bool flag = this.cosmeticProfile.TopDecorationSet != null;
			DecorationSet decorationSet = null;
			int num = 0;
			Vector3Int vector3Int = cell.Coordinate;
			switch (index)
			{
			case DecorationIndex.Top:
			{
				flag = flag && context.AllowTopDecoration;
				decorationSet = this.cosmeticProfile.TopDecorationSet;
				num = random.Next(0, 4);
				vector3Int = cell.Coordinate + Vector3Int.up;
				Cell cellFromCoordinate = stage.GetCellFromCoordinate(vector3Int);
				if (cellFromCoordinate != null && cellFromCoordinate.BlocksDecorations())
				{
					flag = false;
				}
				break;
			}
			case DecorationIndex.Bottom:
				flag = !context.BottomFilled && this.cosmeticProfile.BottomDecorationSet != null;
				decorationSet = this.cosmeticProfile.BottomDecorationSet;
				vector3Int = cell.Coordinate + Vector3Int.down;
				break;
			case DecorationIndex.Front:
				num = 0;
				flag = !context.FrontFilled && this.cosmeticProfile.SideDecorationSet != null;
				decorationSet = this.cosmeticProfile.SideDecorationSet;
				vector3Int = cell.Coordinate + Vector3Int.forward;
				break;
			case DecorationIndex.Back:
				num = 2;
				flag = !context.BackFilled && this.cosmeticProfile.SideDecorationSet != null;
				decorationSet = this.cosmeticProfile.SideDecorationSet;
				vector3Int = cell.Coordinate + Vector3Int.back;
				break;
			case DecorationIndex.Left:
				num = 3;
				flag = !context.LeftFilled && this.cosmeticProfile.SideDecorationSet != null;
				decorationSet = this.cosmeticProfile.SideDecorationSet;
				vector3Int = cell.Coordinate + Vector3Int.left;
				break;
			case DecorationIndex.Right:
				num = 1;
				flag = !context.RightFilled && this.cosmeticProfile.SideDecorationSet != null;
				decorationSet = this.cosmeticProfile.SideDecorationSet;
				vector3Int = cell.Coordinate + Vector3Int.right;
				break;
			}
			if (decorationSet != null && flag && cell.DecorationAtIndex(index) == null)
			{
				this.AttemptSpawnDecoration(cell, decorationSet, new global::System.Random(this.Hash(vector3Int)), index, (float)num * 90f);
				return;
			}
			if (!flag)
			{
				this.DestroyDecorationIfExists(cell, destroy, index);
			}
		}

		// Token: 0x06002008 RID: 8200 RVA: 0x00090DDC File Offset: 0x0008EFDC
		private void DestroyDecorationIfExists(TerrainCell cell, Action<GameObject> destroy, DecorationIndex index)
		{
			if (cell.HasDecorationAtIndex(index))
			{
				destroy(cell.DecorationAtIndex(index).gameObject);
			}
		}

		// Token: 0x06002009 RID: 8201 RVA: 0x00090DFC File Offset: 0x0008EFFC
		private void AttemptSpawnDecoration(TerrainCell cell, DecorationSet set, global::System.Random random, DecorationIndex decorationIndex, float rotationOffset = 0f)
		{
			Transform transform = set.GenerateDecoration(random);
			if (transform != null)
			{
				cell.AddDecoration(decorationIndex, transform);
				transform.SetParent(cell.CellBase, false);
				transform.localRotation = Quaternion.Euler(0f, rotationOffset, 0f);
			}
		}

		// Token: 0x0600200A RID: 8202 RVA: 0x00090E47 File Offset: 0x0008F047
		public int Hash(Vector3Int coordinate)
		{
			return this.Hash(this.Hash(coordinate.x) * 229 + this.Hash(coordinate.z) * 193) + this.Hash(coordinate.y);
		}

		// Token: 0x0600200B RID: 8203 RVA: 0x00090E84 File Offset: 0x0008F084
		private int Hash(int input)
		{
			input ^= input << 13;
			input ^= input >> 17;
			input ^= input << 5;
			return input;
		}

		// Token: 0x0600200C RID: 8204 RVA: 0x00090E9E File Offset: 0x0008F09E
		private int Hash(int a, int b)
		{
			return Mathf.RoundToInt(0.5f * (float)(a + b) * (float)(a + b + 1) + (float)b);
		}

		// Token: 0x0600200D RID: 8205 RVA: 0x00090EB9 File Offset: 0x0008F0B9
		public void UpdateToAsset(TerrainTilesetCosmeticAsset profile)
		{
			if (profile.AssetID == this.asset.AssetID)
			{
				return;
			}
			this.asset = profile;
		}

		// Token: 0x0400199C RID: 6556
		private TilesetCosmeticProfile cosmeticProfile;

		// Token: 0x0400199D RID: 6557
		private FringeSet[] fringeSets = Array.Empty<FringeSet>();

		// Token: 0x0400199E RID: 6558
		private Asset asset;

		// Token: 0x0400199F RID: 6559
		private Sprite displayIcon;

		// Token: 0x040019A2 RID: 6562
		private SerializableGuid[] consideredTilesets = Array.Empty<SerializableGuid>();

		// Token: 0x040019A3 RID: 6563
		private MaterialWeightTableAsset[] materialVariantWeightTables = Array.Empty<MaterialWeightTableAsset>();

		// Token: 0x040019A4 RID: 6564
		private Dictionary<string, string> materialNameToBaseNameMap = new Dictionary<string, string>();
	}
}
