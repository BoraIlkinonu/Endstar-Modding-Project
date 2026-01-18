using System;
using System.Collections.Generic;
using Endless.Data.WeightTables;
using Endless.Gameplay.LevelEditing.Level;
using Endless.TerrainCosmetics;
using UnityEngine;

namespace Endless.Gameplay.LevelEditing.Tilesets
{
	// Token: 0x02000533 RID: 1331
	[Serializable]
	public class Tile
	{
		// Token: 0x17000611 RID: 1553
		// (get) Token: 0x06001FEB RID: 8171 RVA: 0x00090570 File Offset: 0x0008E770
		// (set) Token: 0x06001FEC RID: 8172 RVA: 0x00090578 File Offset: 0x0008E778
		public int Index
		{
			get
			{
				return this.index;
			}
			set
			{
				this.index = value;
			}
		}

		// Token: 0x17000612 RID: 1554
		// (get) Token: 0x06001FED RID: 8173 RVA: 0x00090581 File Offset: 0x0008E781
		// (set) Token: 0x06001FEE RID: 8174 RVA: 0x00090589 File Offset: 0x0008E789
		public List<string> VariantMaterialBaseNames
		{
			get
			{
				return this.variantMaterialBaseNames;
			}
			set
			{
				this.variantMaterialBaseNames = value;
			}
		}

		// Token: 0x17000613 RID: 1555
		// (get) Token: 0x06001FEF RID: 8175 RVA: 0x00090592 File Offset: 0x0008E792
		public int VisualCount
		{
			get
			{
				return this.visuals.Count;
			}
		}

		// Token: 0x06001FF0 RID: 8176 RVA: 0x0009059F File Offset: 0x0008E79F
		public Tile(int index)
		{
			this.index = index;
			this.visuals = new List<Transform>();
			this.materialNameToBaseNameMap = new Dictionary<string, string>();
		}

		// Token: 0x06001FF1 RID: 8177 RVA: 0x000905CF File Offset: 0x0008E7CF
		public Tile(TileCosmetic cosmetic)
		{
			this.visuals = cosmetic.Visuals;
			this.index = cosmetic.Index;
			this.variantMaterialBaseNames = cosmetic.VariantMaterialBaseNames;
		}

		// Token: 0x06001FF2 RID: 8178 RVA: 0x00090606 File Offset: 0x0008E806
		public void AddVisual(Transform asset)
		{
			this.visuals.Add(asset);
		}

		// Token: 0x06001FF3 RID: 8179 RVA: 0x00090614 File Offset: 0x0008E814
		public Transform GetVisual(Vector3Int coordinates)
		{
			if (this.visuals.Count == 0)
			{
				return null;
			}
			int num = new global::System.Random(coordinates.x.GetHashCode() + coordinates.y.GetHashCode() + coordinates.z.GetHashCode()).Next(0, this.visuals.Count);
			return this.visuals[num];
		}

		// Token: 0x06001FF4 RID: 8180 RVA: 0x00090684 File Offset: 0x0008E884
		public void Spawn(TerrainCell cell, TileSpawnContext context, Stage stage, Action<GameObject> destroy)
		{
			if (cell != null)
			{
				cell.Destroy();
			}
			if (context.Tile.visuals.Count == 0 && context.Tile.Index == 63)
			{
				GameObject gameObject = new GameObject("Default Empty Tile");
				gameObject.transform.position = cell.Coordinate;
				gameObject.transform.SetParent(cell.CellBase);
				cell.SetCellVisual(gameObject.transform);
				return;
			}
			global::System.Random random = new global::System.Random(context.Tileset.Hash(cell.Coordinate));
			int num = random.Next(0, this.visuals.Count);
			Transform transform = global::UnityEngine.Object.Instantiate<GameObject>(this.visuals[num].gameObject, cell.CellBase).transform;
			transform.gameObject.SetActive(true);
			transform.transform.position = cell.Coordinate;
			context.Tileset.ApplyMaterialVariants(transform, random);
			context.Tileset.ApplyDecorations(cell, context, destroy, random, stage);
			context.Tileset.ApplyFringe(transform, cell.Coordinate, context, stage);
			cell.SetCellVisual(transform);
			cell.TileIndex = this.index;
		}

		// Token: 0x06001FF5 RID: 8181 RVA: 0x000907B0 File Offset: 0x0008E9B0
		private void ApplyMaterialVariants(TilesetCosmeticProfile tilesetCosmeticProfile, Transform visual, global::System.Random random)
		{
			if (this.materialNameToBaseNameMap == null)
			{
				this.materialNameToBaseNameMap = new Dictionary<string, string>();
			}
			MeshRenderer component = visual.GetComponent<MeshRenderer>();
			if (!component)
			{
				return;
			}
			Material[] sharedMaterials = component.sharedMaterials;
			for (int i = 0; i < sharedMaterials.Length; i++)
			{
				string name = sharedMaterials[i].name;
				string text;
				if (!this.materialNameToBaseNameMap.TryGetValue(name, out text))
				{
					text = sharedMaterials[i].name.Substring(0, sharedMaterials[i].name.LastIndexOf("_mat", StringComparison.Ordinal) + 4);
					this.materialNameToBaseNameMap.Add(name, text);
				}
				MaterialWeightTableAsset[] materialVariantWeightTables = tilesetCosmeticProfile.MaterialVariantWeightTables;
				if (materialVariantWeightTables == null || materialVariantWeightTables.Length != 0)
				{
					MaterialVariantWeightTable materialVariantWeightTable = null;
					if (tilesetCosmeticProfile.MaterialVariantWeightTables != null)
					{
						foreach (MaterialWeightTableAsset materialWeightTableAsset in tilesetCosmeticProfile.MaterialVariantWeightTables)
						{
							if (!(materialWeightTableAsset.MaterialId != text))
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

		// Token: 0x06001FF6 RID: 8182 RVA: 0x000908BC File Offset: 0x0008EABC
		public void ClearVisuals()
		{
			this.visuals.Clear();
		}

		// Token: 0x04001998 RID: 6552
		[SerializeField]
		private List<Transform> visuals;

		// Token: 0x04001999 RID: 6553
		[HideInInspector]
		[SerializeField]
		private int index;

		// Token: 0x0400199A RID: 6554
		[SerializeField]
		private List<string> variantMaterialBaseNames;

		// Token: 0x0400199B RID: 6555
		private Dictionary<string, string> materialNameToBaseNameMap = new Dictionary<string, string>();
	}
}
