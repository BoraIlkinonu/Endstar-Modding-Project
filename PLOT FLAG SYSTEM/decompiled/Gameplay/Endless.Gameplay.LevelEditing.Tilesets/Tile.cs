using System;
using System.Collections.Generic;
using Endless.Data.WeightTables;
using Endless.Gameplay.LevelEditing.Level;
using Endless.TerrainCosmetics;
using UnityEngine;

namespace Endless.Gameplay.LevelEditing.Tilesets;

[Serializable]
public class Tile
{
	[SerializeField]
	private List<Transform> visuals;

	[HideInInspector]
	[SerializeField]
	private int index;

	[SerializeField]
	private List<string> variantMaterialBaseNames;

	private Dictionary<string, string> materialNameToBaseNameMap = new Dictionary<string, string>();

	public int Index
	{
		get
		{
			return index;
		}
		set
		{
			index = value;
		}
	}

	public List<string> VariantMaterialBaseNames
	{
		get
		{
			return variantMaterialBaseNames;
		}
		set
		{
			variantMaterialBaseNames = value;
		}
	}

	public int VisualCount => visuals.Count;

	public Tile(int index)
	{
		this.index = index;
		visuals = new List<Transform>();
		materialNameToBaseNameMap = new Dictionary<string, string>();
	}

	public Tile(TileCosmetic cosmetic)
	{
		visuals = cosmetic.Visuals;
		index = cosmetic.Index;
		variantMaterialBaseNames = cosmetic.VariantMaterialBaseNames;
	}

	public void AddVisual(Transform asset)
	{
		visuals.Add(asset);
	}

	public Transform GetVisual(Vector3Int coordinates)
	{
		if (visuals.Count == 0)
		{
			return null;
		}
		int num = new System.Random(coordinates.x.GetHashCode() + coordinates.y.GetHashCode() + coordinates.z.GetHashCode()).Next(0, visuals.Count);
		return visuals[num];
	}

	public void Spawn(TerrainCell cell, TileSpawnContext context, Stage stage, Action<GameObject> destroy)
	{
		cell?.Destroy();
		if (context.Tile.visuals.Count == 0 && context.Tile.Index == 63)
		{
			GameObject gameObject = new GameObject("Default Empty Tile");
			gameObject.transform.position = cell.Coordinate;
			gameObject.transform.SetParent(cell.CellBase);
			cell.SetCellVisual(gameObject.transform);
			return;
		}
		System.Random random = new System.Random(context.Tileset.Hash(cell.Coordinate));
		int num = random.Next(0, visuals.Count);
		Transform transform = UnityEngine.Object.Instantiate(visuals[num].gameObject, cell.CellBase).transform;
		transform.gameObject.SetActive(value: true);
		transform.transform.position = cell.Coordinate;
		context.Tileset.ApplyMaterialVariants(transform, random);
		context.Tileset.ApplyDecorations(cell, context, destroy, random, stage);
		context.Tileset.ApplyFringe(transform, cell.Coordinate, context, stage);
		cell.SetCellVisual(transform);
		cell.TileIndex = index;
	}

	private void ApplyMaterialVariants(TilesetCosmeticProfile tilesetCosmeticProfile, Transform visual, System.Random random)
	{
		if (materialNameToBaseNameMap == null)
		{
			materialNameToBaseNameMap = new Dictionary<string, string>();
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
			if (!materialNameToBaseNameMap.TryGetValue(name, out var value))
			{
				value = sharedMaterials[i].name.Substring(0, sharedMaterials[i].name.LastIndexOf("_mat", StringComparison.Ordinal) + 4);
				materialNameToBaseNameMap.Add(name, value);
			}
			MaterialWeightTableAsset[] materialVariantWeightTables = tilesetCosmeticProfile.MaterialVariantWeightTables;
			if (materialVariantWeightTables != null && materialVariantWeightTables.Length == 0)
			{
				continue;
			}
			MaterialVariantWeightTable materialVariantWeightTable = null;
			if (tilesetCosmeticProfile.MaterialVariantWeightTables == null)
			{
				continue;
			}
			MaterialWeightTableAsset[] materialVariantWeightTables2 = tilesetCosmeticProfile.MaterialVariantWeightTables;
			foreach (MaterialWeightTableAsset materialWeightTableAsset in materialVariantWeightTables2)
			{
				if (!(materialWeightTableAsset.MaterialId != value))
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
		component.sharedMaterials = sharedMaterials;
	}

	public void ClearVisuals()
	{
		visuals.Clear();
	}
}
