using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Endless.TerrainCosmetics
{
	// Token: 0x0200000D RID: 13
	[Serializable]
	public class TileCosmetic
	{
		// Token: 0x1700001B RID: 27
		// (get) Token: 0x06000032 RID: 50 RVA: 0x00002524 File Offset: 0x00000724
		public List<Transform> Visuals
		{
			get
			{
				return this.visuals;
			}
		}

		// Token: 0x1700001C RID: 28
		// (get) Token: 0x06000033 RID: 51 RVA: 0x0000252C File Offset: 0x0000072C
		public int Index
		{
			get
			{
				return this.index;
			}
		}

		// Token: 0x1700001D RID: 29
		// (get) Token: 0x06000034 RID: 52 RVA: 0x00002534 File Offset: 0x00000734
		public List<string> VariantMaterialBaseNames
		{
			get
			{
				return this.variantMaterialBaseNames;
			}
		}

		// Token: 0x06000035 RID: 53 RVA: 0x0000253C File Offset: 0x0000073C
		public void AddVisual(Transform transform)
		{
			if (this.visuals.Contains(transform) || this.visuals.Any((Transform visual) => visual.GetInstanceID() == transform.GetInstanceID()))
			{
				return;
			}
			this.visuals.Add(transform);
		}

		// Token: 0x06000036 RID: 54 RVA: 0x00002594 File Offset: 0x00000794
		public void SetIndex(int newIndex)
		{
			this.index = newIndex;
		}

		// Token: 0x06000037 RID: 55 RVA: 0x0000259D File Offset: 0x0000079D
		public TileCosmetic Copy()
		{
			return new TileCosmetic
			{
				index = this.index,
				visuals = this.visuals.ToList<Transform>(),
				variantMaterialBaseNames = this.variantMaterialBaseNames.ToList<string>()
			};
		}

		// Token: 0x04000024 RID: 36
		[SerializeField]
		private List<Transform> visuals = new List<Transform>();

		// Token: 0x04000025 RID: 37
		[HideInInspector]
		[SerializeField]
		private int index;

		// Token: 0x04000026 RID: 38
		[SerializeField]
		private List<string> variantMaterialBaseNames = new List<string>();
	}
}
