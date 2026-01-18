using Endless.Gameplay.LevelEditing.Tilesets;
using Endless.Shared;
using Endless.TerrainCosmetics;
using UnityEngine;

namespace Endless.Gameplay.LevelEditing.Level;

public class TerrainCell : Cell
{
	[SerializeField]
	private int tilesetIndex = -1;

	[SerializeField]
	private Transform[] decorations = new Transform[6];

	public int TileIndex { get; set; }

	public override int TilesetIndex => tilesetIndex;

	public override CellType Type => CellType.Terrain;

	public TerrainCell(Vector3Int position, Transform cellBase)
		: base(position, cellBase)
	{
	}

	public void SetTilesetDetails(int index)
	{
		tilesetIndex = index;
	}

	public void AddDecoration(DecorationIndex index, Transform decoration)
	{
		if (decorations == null || decorations.Length < 6)
		{
			decorations = new Transform[6];
		}
		decorations[(int)index] = decoration;
	}

	public bool HasDecorationAtIndex(DecorationIndex index)
	{
		if (decorations == null || (int)index >= decorations.Length)
		{
			return false;
		}
		return decorations[(int)index] != null;
	}

	public Transform DecorationAtIndex(DecorationIndex index)
	{
		if (decorations == null || (int)index >= decorations.Length)
		{
			return null;
		}
		return decorations[(int)index];
	}

	public override bool BlocksBaseFringe()
	{
		return true;
	}

	public bool IsSlope()
	{
		return MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetTilesetFromCell(this).TilesetType == TilesetType.Slope;
	}

	public SlopeNeighbors GetSlope()
	{
		return (SlopeNeighbors)TileIndex;
	}

	public void Destroy()
	{
		if (base.Visual != null && base.Visual.gameObject != null)
		{
			Object.Destroy(base.Visual.gameObject);
		}
		for (int i = 0; i < 6; i++)
		{
			if ((bool)decorations[i])
			{
				Object.Destroy(decorations[i].gameObject);
				decorations[i] = null;
			}
		}
	}
}
