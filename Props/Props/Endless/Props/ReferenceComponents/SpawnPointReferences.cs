using System;
using System.Collections.Generic;
using UnityEngine;

namespace Endless.Props.ReferenceComponents
{
	// Token: 0x0200002F RID: 47
	public class SpawnPointReferences : BaseTypeReferences
	{
		// Token: 0x1700004B RID: 75
		// (get) Token: 0x060000C2 RID: 194 RVA: 0x000030CA File Offset: 0x000012CA
		public List<Renderer> CreatorOnlyRenderers
		{
			get
			{
				return this.creatorOnlyRenderers;
			}
		}

		// Token: 0x1700004C RID: 76
		// (get) Token: 0x060000C3 RID: 195 RVA: 0x000030D2 File Offset: 0x000012D2
		public Transform[] SpawnPoints
		{
			get
			{
				return this.spawnPoints;
			}
		}

		// Token: 0x0400007E RID: 126
		[SerializeField]
		private List<Renderer> creatorOnlyRenderers = new List<Renderer>();

		// Token: 0x0400007F RID: 127
		[SerializeField]
		private Transform[] spawnPoints;
	}
}
