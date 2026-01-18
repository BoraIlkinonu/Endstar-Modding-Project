using System;
using UnityEngine;

namespace Endless.Gameplay.LevelEditing.Level;

[Serializable]
public abstract class Cell
{
	[SerializeField]
	private Vector3Int position;

	[SerializeField]
	private Transform cellVisual;

	[SerializeField]
	private Transform cellBase;

	[SerializeField]
	private Transform[] decorations = new Transform[6];

	public NpcEnum.Edge edgeKind;

	public abstract CellType Type { get; }

	public virtual int TilesetIndex => -1;

	public Vector3Int Coordinate => position;

	public Transform Visual => cellVisual;

	public Transform CellBase
	{
		get
		{
			return cellBase;
		}
		set
		{
			if ((bool)cellBase)
			{
				return;
			}
			if ((bool)cellVisual)
			{
				for (int num = cellVisual.childCount - 1; num >= 1; num--)
				{
					cellVisual.GetChild(num).SetParent(value);
				}
				cellVisual.SetParent(value);
			}
			cellBase = value;
		}
	}

	public Cell(Vector3Int position, Transform cellBase)
	{
		this.position = position;
		this.cellBase = cellBase;
	}

	public bool IsVacant()
	{
		return cellVisual == null;
	}

	public void SetCellVisual(Transform cellVisual)
	{
		this.cellVisual = cellVisual;
	}

	public virtual bool BlocksDecorations()
	{
		return false;
	}

	public virtual bool BlocksBaseFringe()
	{
		return false;
	}
}
