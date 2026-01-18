using System;
using UnityEngine;

namespace Endless.TerrainCosmetics
{
	// Token: 0x02000007 RID: 7
	public class BaseFringeCosmeticProfile : ScriptableObject
	{
		// Token: 0x17000003 RID: 3
		// (get) Token: 0x0600000F RID: 15 RVA: 0x000022A3 File Offset: 0x000004A3
		public GameObject SingleFringe
		{
			get
			{
				return this.singleFringe;
			}
		}

		// Token: 0x17000004 RID: 4
		// (get) Token: 0x06000010 RID: 16 RVA: 0x000022AB File Offset: 0x000004AB
		public GameObject CornerFringe
		{
			get
			{
				return this.cornerFringe;
			}
		}

		// Token: 0x17000005 RID: 5
		// (get) Token: 0x06000011 RID: 17 RVA: 0x000022B3 File Offset: 0x000004B3
		public GameObject PeninsulaFringe
		{
			get
			{
				return this.peninsulaFringe;
			}
		}

		// Token: 0x17000006 RID: 6
		// (get) Token: 0x06000012 RID: 18 RVA: 0x000022BB File Offset: 0x000004BB
		public GameObject IslandFringe
		{
			get
			{
				return this.islandFringe;
			}
		}

		// Token: 0x17000007 RID: 7
		// (get) Token: 0x06000013 RID: 19 RVA: 0x000022C3 File Offset: 0x000004C3
		public GameObject CornerCapFringe
		{
			get
			{
				return this.cornerCapFringe;
			}
		}

		// Token: 0x17000008 RID: 8
		// (get) Token: 0x06000014 RID: 20 RVA: 0x000022CB File Offset: 0x000004CB
		public bool IsVerticalFringe
		{
			get
			{
				return this.isVerticalFringe;
			}
		}

		// Token: 0x04000007 RID: 7
		[SerializeField]
		protected GameObject singleFringe;

		// Token: 0x04000008 RID: 8
		[SerializeField]
		protected GameObject cornerFringe;

		// Token: 0x04000009 RID: 9
		[SerializeField]
		protected GameObject peninsulaFringe;

		// Token: 0x0400000A RID: 10
		[SerializeField]
		protected GameObject islandFringe;

		// Token: 0x0400000B RID: 11
		[SerializeField]
		protected GameObject cornerCapFringe;

		// Token: 0x0400000C RID: 12
		[SerializeField]
		private bool isVerticalFringe;
	}
}
