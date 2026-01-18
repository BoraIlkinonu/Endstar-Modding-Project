using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Assets;
using Endless.Data.WeightTables;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared.DataTypes;
using Endless.TerrainCosmetics;
using UnityEngine;

namespace Endless.Gameplay.LevelEditing.Tilesets;

public abstract class Tileset
{
	private TilesetCosmeticProfile cosmeticProfile;

	private FringeSet[] fringeSets = Array.Empty<FringeSet>();

	private Asset asset;

	private Sprite displayIcon;

	private SerializableGuid[] consideredTilesets = Array.Empty<SerializableGuid>();

	private MaterialWeightTableAsset[] materialVariantWeightTables = Array.Empty<MaterialWeightTableAsset>();

	private Dictionary<string, string> materialNameToBaseNameMap = new Dictionary<string, string>();

	public Asset Asset => asset;

	public virtual string DisplayName => asset.Name;

	public virtual string Description => asset.Description;

	public virtual Sprite DisplayIcon => displayIcon;

	public TilesetType TilesetType { get; }

	public bool HasFringe => fringeSets.Length != 0;

	protected TilesetCosmeticProfile CosmeticProfile => cosmeticProfile;

	public int Index { get; set; }

	public Tileset(TilesetCosmeticProfile cosmeticProfile, Asset asset, Sprite displayIcon, int index)
	{
		Index = index;
		this.cosmeticProfile = cosmeticProfile;
		this.asset = asset;
		this.displayIcon = displayIcon;
		List<FringeSet> list = new List<FringeSet>();
		if ((bool)cosmeticProfile)
		{
			TilesetType = cosmeticProfile.TilesetType;
			consideredTilesets = this.cosmeticProfile.ConsideredTilesetIds ?? Array.Empty<SerializableGuid>();
			materialVariantWeightTables = this.cosmeticProfile.MaterialVariantWeightTables;
			BaseFringeCosmeticProfile[] baseFringeSets = this.cosmeticProfile.BaseFringeSets;
			foreach (BaseFringeCosmeticProfile cosmeticSet in baseFringeSets)
			{
				list.Add(new BaseFringeSet(cosmeticSet));
			}
			SlopeFringeCosmeticProfile[] slopeFringeSets = this.cosmeticProfile.SlopeFringeSets;
			foreach (SlopeFringeCosmeticProfile slopeFringeCosmeticSet in slopeFringeSets)
			{
				list.Add(new SlopeFringeSet(slopeFringeCosmeticSet));
			}
			fringeSets = list.ToArray();
		}
	}

	public abstract TileSpawnContext GetValidVisualForCellPosition(Stage stage, Vector3Int coordinates);

	public bool ConsidersTileset(string tilesetId)
	{
		if (string.IsNullOrEmpty(tilesetId))
		{
			return false;
		}
		return consideredTilesets.Contains<SerializableGuid>(tilesetId);
	}

	public bool ConsidersTileset(Tileset tilset)
	{
		return consideredTilesets.Contains<SerializableGuid>(tilset.Asset.AssetID);
	}

	public void ApplyMaterialVariants(Transform chosenVisual, System.Random random)
	{
		if (materialNameToBaseNameMap == null)
		{
			materialNameToBaseNameMap = new Dictionary<string, string>();
		}
		MeshRenderer component = chosenVisual.GetComponent<MeshRenderer>();
		if (!component)
		{
			return;
		}
		Material[] sharedMaterials = component.sharedMaterials;
		for (int i = 0; i < sharedMaterials.Length; i++)
		{
			if ((object)sharedMaterials[i] == null)
			{
				continue;
			}
			string name = sharedMaterials[i].name;
			if (!materialNameToBaseNameMap.TryGetValue(name, out var value))
			{
				value = sharedMaterials[i].name;
				materialNameToBaseNameMap.Add(name, value);
			}
			if (materialVariantWeightTables == null || materialVariantWeightTables.Length == 0)
			{
				continue;
			}
			MaterialVariantWeightTable materialVariantWeightTable = null;
			MaterialWeightTableAsset[] array = materialVariantWeightTables;
			foreach (MaterialWeightTableAsset materialWeightTableAsset in array)
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

	public void ApplyFringe(Transform chosenVisual, Vector3Int cellCoordinate, TileSpawnContext context, Stage stage)
	{
		FringeSet[] array = fringeSets;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].AddFringe(chosenVisual, cellCoordinate, context, stage);
		}
	}

	public void ApplyDecorations(TerrainCell cell, TileSpawnContext context, Action<GameObject> destroy, System.Random random, Stage stage)
	{
		for (DecorationIndex decorationIndex = DecorationIndex.Top; decorationIndex <= DecorationIndex.Right; decorationIndex++)
		{
			HandleDecorationIndex(decorationIndex, cell, context, destroy, random, stage);
		}
	}

	private void HandleDecorationIndex(DecorationIndex index, TerrainCell cell, TileSpawnContext context, Action<GameObject> destroy, System.Random random, Stage stage)
	{
		if (!cosmeticProfile)
		{
			return;
		}
		bool flag = cosmeticProfile.TopDecorationSet != null;
		DecorationSet decorationSet = null;
		int num = 0;
		Vector3Int coordinate = cell.Coordinate;
		switch (index)
		{
		case DecorationIndex.Top:
		{
			flag = flag && context.AllowTopDecoration;
			decorationSet = cosmeticProfile.TopDecorationSet;
			num = random.Next(0, 4);
			coordinate = cell.Coordinate + Vector3Int.up;
			Cell cellFromCoordinate = stage.GetCellFromCoordinate(coordinate);
			if (cellFromCoordinate != null && cellFromCoordinate.BlocksDecorations())
			{
				flag = false;
			}
			break;
		}
		case DecorationIndex.Front:
			num = 0;
			flag = !context.FrontFilled && cosmeticProfile.SideDecorationSet != null;
			decorationSet = cosmeticProfile.SideDecorationSet;
			coordinate = cell.Coordinate + Vector3Int.forward;
			break;
		case DecorationIndex.Back:
			num = 2;
			flag = !context.BackFilled && cosmeticProfile.SideDecorationSet != null;
			decorationSet = cosmeticProfile.SideDecorationSet;
			coordinate = cell.Coordinate + Vector3Int.back;
			break;
		case DecorationIndex.Left:
			num = 3;
			flag = !context.LeftFilled && cosmeticProfile.SideDecorationSet != null;
			decorationSet = cosmeticProfile.SideDecorationSet;
			coordinate = cell.Coordinate + Vector3Int.left;
			break;
		case DecorationIndex.Right:
			num = 1;
			flag = !context.RightFilled && cosmeticProfile.SideDecorationSet != null;
			decorationSet = cosmeticProfile.SideDecorationSet;
			coordinate = cell.Coordinate + Vector3Int.right;
			break;
		case DecorationIndex.Bottom:
			flag = !context.BottomFilled && cosmeticProfile.BottomDecorationSet != null;
			decorationSet = cosmeticProfile.BottomDecorationSet;
			coordinate = cell.Coordinate + Vector3Int.down;
			break;
		}
		if (decorationSet != null && flag && cell.DecorationAtIndex(index) == null)
		{
			AttemptSpawnDecoration(cell, decorationSet, new System.Random(Hash(coordinate)), index, (float)num * 90f);
		}
		else if (!flag)
		{
			DestroyDecorationIfExists(cell, destroy, index);
		}
	}

	private void DestroyDecorationIfExists(TerrainCell cell, Action<GameObject> destroy, DecorationIndex index)
	{
		if (cell.HasDecorationAtIndex(index))
		{
			destroy(cell.DecorationAtIndex(index).gameObject);
		}
	}

	private void AttemptSpawnDecoration(TerrainCell cell, DecorationSet set, System.Random random, DecorationIndex decorationIndex, float rotationOffset = 0f)
	{
		Transform transform = set.GenerateDecoration(random);
		if (transform != null)
		{
			cell.AddDecoration(decorationIndex, transform);
			transform.SetParent(cell.CellBase, worldPositionStays: false);
			transform.localRotation = Quaternion.Euler(0f, rotationOffset, 0f);
		}
	}

	public int Hash(Vector3Int coordinate)
	{
		return Hash(Hash(coordinate.x) * 229 + Hash(coordinate.z) * 193) + Hash(coordinate.y);
	}

	private int Hash(int input)
	{
		input ^= input << 13;
		input ^= input >> 17;
		input ^= input << 5;
		return input;
	}

	private int Hash(int a, int b)
	{
		return Mathf.RoundToInt(0.5f * (float)(a + b) * (float)(a + b + 1) + (float)b);
	}

	public void UpdateToAsset(TerrainTilesetCosmeticAsset profile)
	{
		if (!(profile.AssetID == asset.AssetID))
		{
			asset = profile;
		}
	}
}
