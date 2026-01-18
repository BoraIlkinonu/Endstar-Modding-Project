using System;
using Endless.Gameplay.LevelEditing.Level;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020000D4 RID: 212
	public static class CellReferenceUtility
	{
		// Token: 0x06000433 RID: 1075 RVA: 0x00016B30 File Offset: 0x00014D30
		public static void SetCell(CellReference reference, Vector3? cell, float? rotation)
		{
			reference.SetCell(cell, rotation);
		}

		// Token: 0x06000434 RID: 1076 RVA: 0x00016B3A File Offset: 0x00014D3A
		public static Cell GetCell(CellReference reference)
		{
			return reference.GetCell();
		}
	}
}
