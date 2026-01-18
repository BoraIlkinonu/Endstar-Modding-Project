using System;
using UnityEngine;

namespace Endless.Gameplay.LevelEditing.Level
{
	// Token: 0x02000539 RID: 1337
	[Serializable]
	public abstract class Cell
	{
		// Token: 0x17000626 RID: 1574
		// (get) Token: 0x06002024 RID: 8228
		public abstract CellType Type { get; }

		// Token: 0x17000627 RID: 1575
		// (get) Token: 0x06002025 RID: 8229 RVA: 0x00090F85 File Offset: 0x0008F185
		public virtual int TilesetIndex
		{
			get
			{
				return -1;
			}
		}

		// Token: 0x17000628 RID: 1576
		// (get) Token: 0x06002026 RID: 8230 RVA: 0x00090F88 File Offset: 0x0008F188
		public Vector3Int Coordinate
		{
			get
			{
				return this.position;
			}
		}

		// Token: 0x17000629 RID: 1577
		// (get) Token: 0x06002027 RID: 8231 RVA: 0x00090F90 File Offset: 0x0008F190
		public Transform Visual
		{
			get
			{
				return this.cellVisual;
			}
		}

		// Token: 0x1700062A RID: 1578
		// (get) Token: 0x06002028 RID: 8232 RVA: 0x00090F98 File Offset: 0x0008F198
		// (set) Token: 0x06002029 RID: 8233 RVA: 0x00090FA0 File Offset: 0x0008F1A0
		public Transform CellBase
		{
			get
			{
				return this.cellBase;
			}
			set
			{
				if (this.cellBase)
				{
					return;
				}
				if (this.cellVisual)
				{
					for (int i = this.cellVisual.childCount - 1; i >= 1; i--)
					{
						this.cellVisual.GetChild(i).SetParent(value);
					}
					this.cellVisual.SetParent(value);
				}
				this.cellBase = value;
			}
		}

		// Token: 0x0600202A RID: 8234 RVA: 0x00091005 File Offset: 0x0008F205
		public Cell(Vector3Int position, Transform cellBase)
		{
			this.position = position;
			this.cellBase = cellBase;
		}

		// Token: 0x0600202B RID: 8235 RVA: 0x00091027 File Offset: 0x0008F227
		public bool IsVacant()
		{
			return this.cellVisual == null;
		}

		// Token: 0x0600202C RID: 8236 RVA: 0x00091035 File Offset: 0x0008F235
		public void SetCellVisual(Transform cellVisual)
		{
			this.cellVisual = cellVisual;
		}

		// Token: 0x0600202D RID: 8237 RVA: 0x0001965C File Offset: 0x0001785C
		public virtual bool BlocksDecorations()
		{
			return false;
		}

		// Token: 0x0600202E RID: 8238 RVA: 0x0001965C File Offset: 0x0001785C
		public virtual bool BlocksBaseFringe()
		{
			return false;
		}

		// Token: 0x040019BB RID: 6587
		[SerializeField]
		private Vector3Int position;

		// Token: 0x040019BC RID: 6588
		[SerializeField]
		private Transform cellVisual;

		// Token: 0x040019BD RID: 6589
		[SerializeField]
		private Transform cellBase;

		// Token: 0x040019BE RID: 6590
		[SerializeField]
		private Transform[] decorations = new Transform[6];

		// Token: 0x040019BF RID: 6591
		public NpcEnum.Edge edgeKind;
	}
}
