using System;
using UnityEngine;

namespace Endless.Gameplay.LevelEditing.Tilesets
{
	// Token: 0x02000535 RID: 1333
	[CreateAssetMenu(menuName = "Level Editing/Tileset Palette")]
	public class TilesetPalette : ScriptableObject
	{
		// Token: 0x1700061C RID: 1564
		// (get) Token: 0x0600200E RID: 8206 RVA: 0x00090EDB File Offset: 0x0008F0DB
		// (set) Token: 0x0600200F RID: 8207 RVA: 0x00090EE3 File Offset: 0x0008F0E3
		public Tileset[] Tilesets { get; private set; }
	}
}
