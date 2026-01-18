using System;
using UnityEngine;

namespace Endless.TerrainCosmetics
{
	// Token: 0x0200000B RID: 11
	public class SlopeFringeCosmeticProfile : ScriptableObject
	{
		// Token: 0x1700000E RID: 14
		// (get) Token: 0x06000022 RID: 34 RVA: 0x00002370 File Offset: 0x00000570
		public GameObject[] FlatFringeSingles
		{
			get
			{
				return this.flatFringeSingles;
			}
		}

		// Token: 0x1700000F RID: 15
		// (get) Token: 0x06000023 RID: 35 RVA: 0x00002378 File Offset: 0x00000578
		public GameObject[] FlatFringeCorners
		{
			get
			{
				return this.flatFringeCorners;
			}
		}

		// Token: 0x17000010 RID: 16
		// (get) Token: 0x06000024 RID: 36 RVA: 0x00002380 File Offset: 0x00000580
		public GameObject[] FlatFringePeninsulas
		{
			get
			{
				return this.flatFringePeninsulas;
			}
		}

		// Token: 0x17000011 RID: 17
		// (get) Token: 0x06000025 RID: 37 RVA: 0x00002388 File Offset: 0x00000588
		public GameObject[] OuterCornerFringePeninsulas
		{
			get
			{
				return this.outerCornerFringePeninsulas;
			}
		}

		// Token: 0x17000012 RID: 18
		// (get) Token: 0x06000026 RID: 38 RVA: 0x00002390 File Offset: 0x00000590
		public GameObject[] InnerCornerFringePeninsulas
		{
			get
			{
				return this.innerCornerFringePeninsulas;
			}
		}

		// Token: 0x17000013 RID: 19
		// (get) Token: 0x06000027 RID: 39 RVA: 0x00002398 File Offset: 0x00000598
		public GameObject[] VerticalFringes
		{
			get
			{
				return this.verticalFringes;
			}
		}

		// Token: 0x17000014 RID: 20
		// (get) Token: 0x06000028 RID: 40 RVA: 0x000023A0 File Offset: 0x000005A0
		public bool FlatFringeIslandValid
		{
			get
			{
				return this.flatFringeIsland != null;
			}
		}

		// Token: 0x17000015 RID: 21
		// (get) Token: 0x06000029 RID: 41 RVA: 0x000023AE File Offset: 0x000005AE
		public bool OuterCornerFringeValid
		{
			get
			{
				return this.outerCornerFringe != null;
			}
		}

		// Token: 0x17000016 RID: 22
		// (get) Token: 0x0600002A RID: 42 RVA: 0x000023BC File Offset: 0x000005BC
		public bool InnerCornerFringeValid
		{
			get
			{
				return this.innerCornerFringe != null;
			}
		}

		// Token: 0x17000017 RID: 23
		// (get) Token: 0x0600002B RID: 43 RVA: 0x000023CA File Offset: 0x000005CA
		public GameObject FlatFringeIsland
		{
			get
			{
				return this.flatFringeIsland;
			}
		}

		// Token: 0x17000018 RID: 24
		// (get) Token: 0x0600002C RID: 44 RVA: 0x000023D2 File Offset: 0x000005D2
		public GameObject OuterCornerFringe
		{
			get
			{
				return this.outerCornerFringe;
			}
		}

		// Token: 0x17000019 RID: 25
		// (get) Token: 0x0600002D RID: 45 RVA: 0x000023DA File Offset: 0x000005DA
		public GameObject InnerCornerFringe
		{
			get
			{
				return this.innerCornerFringe;
			}
		}

		// Token: 0x0600002E RID: 46 RVA: 0x000023E4 File Offset: 0x000005E4
		private void Reset()
		{
			if (this.flatFringeSingles == null || (this.flatFringeSingles != null && this.flatFringeSingles.Length == 0))
			{
				this.flatFringeSingles = new GameObject[4];
			}
			if (this.flatFringeCorners != null)
			{
				if (this.flatFringeCorners == null)
				{
					goto IL_0054;
				}
				GameObject[] array = this.flatFringeCorners;
				if (array == null || array.Length != 0)
				{
					goto IL_0054;
				}
			}
			this.flatFringeCorners = new GameObject[4];
			IL_0054:
			if (this.flatFringePeninsulas != null)
			{
				if (this.flatFringePeninsulas == null)
				{
					goto IL_0083;
				}
				GameObject[] array2 = this.flatFringePeninsulas;
				if (array2 == null || array2.Length != 0)
				{
					goto IL_0083;
				}
			}
			this.flatFringePeninsulas = new GameObject[4];
			IL_0083:
			if (this.outerCornerFringePeninsulas != null)
			{
				if (this.outerCornerFringePeninsulas == null)
				{
					goto IL_00B2;
				}
				GameObject[] array3 = this.outerCornerFringePeninsulas;
				if (array3 == null || array3.Length != 0)
				{
					goto IL_00B2;
				}
			}
			this.outerCornerFringePeninsulas = new GameObject[4];
			IL_00B2:
			if (this.innerCornerFringePeninsulas != null)
			{
				if (this.innerCornerFringePeninsulas == null)
				{
					goto IL_00E1;
				}
				GameObject[] array4 = this.innerCornerFringePeninsulas;
				if (array4 == null || array4.Length != 0)
				{
					goto IL_00E1;
				}
			}
			this.innerCornerFringePeninsulas = new GameObject[4];
			IL_00E1:
			if (this.verticalFringes != null)
			{
				if (this.verticalFringes == null)
				{
					return;
				}
				GameObject[] array5 = this.verticalFringes;
				if (array5 == null || array5.Length != 0)
				{
					return;
				}
			}
			this.verticalFringes = new GameObject[4];
		}

		// Token: 0x04000012 RID: 18
		[SerializeField]
		[Header("Flat")]
		[Tooltip("Flat slopes are the single direction facing slopes that ramp up towards N/E/S/W")]
		private GameObject[] flatFringeSingles;

		// Token: 0x04000013 RID: 19
		[SerializeField]
		private GameObject[] flatFringeCorners;

		// Token: 0x04000014 RID: 20
		[SerializeField]
		private GameObject[] flatFringePeninsulas;

		// Token: 0x04000015 RID: 21
		[SerializeField]
		private GameObject flatFringeIsland;

		// Token: 0x04000016 RID: 22
		[SerializeField]
		[Header("Outer Corner")]
		[Tooltip("Outer corners are usually between two slope blocks, and an opposite diagonal full block")]
		private GameObject outerCornerFringe;

		// Token: 0x04000017 RID: 23
		[SerializeField]
		private GameObject[] outerCornerFringePeninsulas;

		// Token: 0x04000018 RID: 24
		[SerializeField]
		[Header("Inner Corner")]
		[Tooltip("Inner corners are usually between two slope neighbors and two full blocks")]
		private GameObject innerCornerFringe;

		// Token: 0x04000019 RID: 25
		[SerializeField]
		private GameObject[] innerCornerFringePeninsulas;

		// Token: 0x0400001A RID: 26
		[SerializeField]
		private GameObject[] verticalFringes;
	}
}
