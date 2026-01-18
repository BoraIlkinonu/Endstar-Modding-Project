using System;
using Endless.Gameplay.LevelEditing.Tilesets;
using Endless.Shared;
using Endless.TerrainCosmetics;
using UnityEngine;

namespace Endless.Gameplay.LevelEditing.Level
{
	// Token: 0x0200058A RID: 1418
	public class TerrainCell : Cell
	{
		// Token: 0x17000678 RID: 1656
		// (get) Token: 0x0600223D RID: 8765 RVA: 0x0009DC86 File Offset: 0x0009BE86
		// (set) Token: 0x0600223E RID: 8766 RVA: 0x0009DC8E File Offset: 0x0009BE8E
		public int TileIndex { get; set; }

		// Token: 0x17000679 RID: 1657
		// (get) Token: 0x0600223F RID: 8767 RVA: 0x0009DC97 File Offset: 0x0009BE97
		public override int TilesetIndex
		{
			get
			{
				return this.tilesetIndex;
			}
		}

		// Token: 0x06002240 RID: 8768 RVA: 0x0009DC9F File Offset: 0x0009BE9F
		public TerrainCell(Vector3Int position, Transform cellBase)
			: base(position, cellBase)
		{
		}

		// Token: 0x06002241 RID: 8769 RVA: 0x0009DCBC File Offset: 0x0009BEBC
		public void SetTilesetDetails(int index)
		{
			this.tilesetIndex = index;
		}

		// Token: 0x06002242 RID: 8770 RVA: 0x0009DCC5 File Offset: 0x0009BEC5
		public void AddDecoration(DecorationIndex index, Transform decoration)
		{
			if (this.decorations == null || this.decorations.Length < 6)
			{
				this.decorations = new Transform[6];
			}
			this.decorations[(int)index] = decoration;
		}

		// Token: 0x06002243 RID: 8771 RVA: 0x0009DCEF File Offset: 0x0009BEEF
		public bool HasDecorationAtIndex(DecorationIndex index)
		{
			return this.decorations != null && index < (DecorationIndex)this.decorations.Length && this.decorations[(int)index] != null;
		}

		// Token: 0x06002244 RID: 8772 RVA: 0x0009DD14 File Offset: 0x0009BF14
		public Transform DecorationAtIndex(DecorationIndex index)
		{
			if (this.decorations == null || index >= (DecorationIndex)this.decorations.Length)
			{
				return null;
			}
			return this.decorations[(int)index];
		}

		// Token: 0x1700067A RID: 1658
		// (get) Token: 0x06002245 RID: 8773 RVA: 0x00017586 File Offset: 0x00015786
		public override CellType Type
		{
			get
			{
				return CellType.Terrain;
			}
		}

		// Token: 0x06002246 RID: 8774 RVA: 0x00017586 File Offset: 0x00015786
		public override bool BlocksBaseFringe()
		{
			return true;
		}

		// Token: 0x06002247 RID: 8775 RVA: 0x0009DD33 File Offset: 0x0009BF33
		public bool IsSlope()
		{
			return MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetTilesetFromCell(this).TilesetType == TilesetType.Slope;
		}

		// Token: 0x06002248 RID: 8776 RVA: 0x0009DD4D File Offset: 0x0009BF4D
		public SlopeNeighbors GetSlope()
		{
			return (SlopeNeighbors)this.TileIndex;
		}

		// Token: 0x06002249 RID: 8777 RVA: 0x0009DD58 File Offset: 0x0009BF58
		public void Destroy()
		{
			if (base.Visual != null && base.Visual.gameObject != null)
			{
				global::UnityEngine.Object.Destroy(base.Visual.gameObject);
			}
			for (int i = 0; i < 6; i++)
			{
				if (this.decorations[i])
				{
					global::UnityEngine.Object.Destroy(this.decorations[i].gameObject);
					this.decorations[i] = null;
				}
			}
		}

		// Token: 0x04001B4A RID: 6986
		[SerializeField]
		private int tilesetIndex = -1;

		// Token: 0x04001B4B RID: 6987
		[SerializeField]
		private Transform[] decorations = new Transform[6];
	}
}
