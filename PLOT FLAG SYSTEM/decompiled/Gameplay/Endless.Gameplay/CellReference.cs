using System;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Newtonsoft.Json;
using UnityEngine;

namespace Endless.Gameplay;

[Serializable]
public class CellReference
{
	[JsonProperty]
	internal Vector3? Cell;

	[JsonProperty("Rot")]
	internal float? Rotation;

	[JsonIgnore]
	public bool HasValue => Cell.HasValue;

	[JsonIgnore]
	public bool RotationHasValue => Rotation.HasValue;

	public Vector3Int GetCellPositionAsVector3Int()
	{
		if (!Cell.HasValue)
		{
			return Vector3Int.zero;
		}
		return Vector3Int.RoundToInt(Cell.Value);
	}

	public Vector3 GetCellPosition()
	{
		return Cell ?? Vector3.zero;
	}

	internal void SetCell(Vector3? cell, float? rotation)
	{
		Cell = cell;
		Rotation = rotation;
	}

	internal Cell GetCell()
	{
		Vector3Int cellPositionAsVector3Int = GetCellPositionAsVector3Int();
		if (!Cell.HasValue)
		{
			return null;
		}
		return MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetCellFromCoordinate(cellPositionAsVector3Int);
	}

	public float GetRotation()
	{
		return Rotation.GetValueOrDefault();
	}

	public override string ToString()
	{
		return string.Format("{{ {0}: {1}, {2}: {3} }}", "Cell", Cell.HasValue ? ((object)Cell.Value) : "null", "Rotation", Rotation.HasValue ? ((object)Rotation.Value) : "null");
	}
}
