using System;

namespace Endless.Gameplay.LevelEditing.Tilesets
{
	// Token: 0x02000536 RID: 1334
	public class TileSpawnContext
	{
		// Token: 0x1700061D RID: 1565
		// (get) Token: 0x06002011 RID: 8209 RVA: 0x00090EEC File Offset: 0x0008F0EC
		// (set) Token: 0x06002012 RID: 8210 RVA: 0x00090EF4 File Offset: 0x0008F0F4
		public Tileset Tileset { get; set; }

		// Token: 0x1700061E RID: 1566
		// (get) Token: 0x06002013 RID: 8211 RVA: 0x00090EFD File Offset: 0x0008F0FD
		// (set) Token: 0x06002014 RID: 8212 RVA: 0x00090F05 File Offset: 0x0008F105
		public bool AllowTopDecoration { get; set; }

		// Token: 0x1700061F RID: 1567
		// (get) Token: 0x06002015 RID: 8213 RVA: 0x00090F0E File Offset: 0x0008F10E
		// (set) Token: 0x06002016 RID: 8214 RVA: 0x00090F16 File Offset: 0x0008F116
		public bool TopFilled { get; set; }

		// Token: 0x17000620 RID: 1568
		// (get) Token: 0x06002017 RID: 8215 RVA: 0x00090F1F File Offset: 0x0008F11F
		// (set) Token: 0x06002018 RID: 8216 RVA: 0x00090F27 File Offset: 0x0008F127
		public bool BottomFilled { get; set; }

		// Token: 0x17000621 RID: 1569
		// (get) Token: 0x06002019 RID: 8217 RVA: 0x00090F30 File Offset: 0x0008F130
		// (set) Token: 0x0600201A RID: 8218 RVA: 0x00090F38 File Offset: 0x0008F138
		public bool FrontFilled { get; set; }

		// Token: 0x17000622 RID: 1570
		// (get) Token: 0x0600201B RID: 8219 RVA: 0x00090F41 File Offset: 0x0008F141
		// (set) Token: 0x0600201C RID: 8220 RVA: 0x00090F49 File Offset: 0x0008F149
		public bool BackFilled { get; set; }

		// Token: 0x17000623 RID: 1571
		// (get) Token: 0x0600201D RID: 8221 RVA: 0x00090F52 File Offset: 0x0008F152
		// (set) Token: 0x0600201E RID: 8222 RVA: 0x00090F5A File Offset: 0x0008F15A
		public bool RightFilled { get; set; }

		// Token: 0x17000624 RID: 1572
		// (get) Token: 0x0600201F RID: 8223 RVA: 0x00090F63 File Offset: 0x0008F163
		// (set) Token: 0x06002020 RID: 8224 RVA: 0x00090F6B File Offset: 0x0008F16B
		public bool LeftFilled { get; set; }

		// Token: 0x17000625 RID: 1573
		// (get) Token: 0x06002021 RID: 8225 RVA: 0x00090F74 File Offset: 0x0008F174
		// (set) Token: 0x06002022 RID: 8226 RVA: 0x00090F7C File Offset: 0x0008F17C
		public Tile Tile { get; set; }
	}
}
