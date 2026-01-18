using Endless.Gameplay.LevelEditing.Level;
using UnityEngine;

namespace Endless.Gameplay;

public static class CellReferenceUtility
{
	public static void SetCell(CellReference reference, Vector3? cell, float? rotation)
	{
		reference.SetCell(cell, rotation);
	}

	public static Cell GetCell(CellReference reference)
	{
		return reference.GetCell();
	}
}
