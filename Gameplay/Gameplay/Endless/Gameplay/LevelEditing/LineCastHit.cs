using System;
using UnityEngine;

namespace Endless.Gameplay.LevelEditing
{
	// Token: 0x020004E6 RID: 1254
	public struct LineCastHit
	{
		// Token: 0x170005F6 RID: 1526
		// (get) Token: 0x06001EBB RID: 7867 RVA: 0x000865BA File Offset: 0x000847BA
		// (set) Token: 0x06001EBC RID: 7868 RVA: 0x000865C2 File Offset: 0x000847C2
		public bool IntersectionOccured { readonly get; set; }

		// Token: 0x170005F7 RID: 1527
		// (get) Token: 0x06001EBD RID: 7869 RVA: 0x000865CB File Offset: 0x000847CB
		// (set) Token: 0x06001EBE RID: 7870 RVA: 0x000865D3 File Offset: 0x000847D3
		public float Distance { readonly get; set; }

		// Token: 0x170005F8 RID: 1528
		// (get) Token: 0x06001EBF RID: 7871 RVA: 0x000865DC File Offset: 0x000847DC
		// (set) Token: 0x06001EC0 RID: 7872 RVA: 0x000865E4 File Offset: 0x000847E4
		public Vector3Int IntersectedObjectPosition { readonly get; set; }

		// Token: 0x170005F9 RID: 1529
		// (get) Token: 0x06001EC1 RID: 7873 RVA: 0x000865ED File Offset: 0x000847ED
		// (set) Token: 0x06001EC2 RID: 7874 RVA: 0x000865F5 File Offset: 0x000847F5
		public Vector3Int NearestPositionToObject { readonly get; set; }
	}
}
