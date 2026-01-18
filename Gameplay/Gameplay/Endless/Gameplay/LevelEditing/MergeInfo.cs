using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Endless.Gameplay.LevelEditing
{
	// Token: 0x020004E7 RID: 1255
	public class MergeInfo
	{
		// Token: 0x06001EC3 RID: 7875 RVA: 0x00086600 File Offset: 0x00084800
		public MergeInfo(ShadowCastingMode shadowCastingMode, string namePrefix)
		{
			this.Materials = new List<Material>();
			this.CombineInstances = new List<List<CombineInstance>>();
			this.MergedMesh = new GameObject(namePrefix + " - ShadowCastingMode." + shadowCastingMode.ToString());
		}

		// Token: 0x040017F2 RID: 6130
		public List<Material> Materials;

		// Token: 0x040017F3 RID: 6131
		public List<List<CombineInstance>> CombineInstances;

		// Token: 0x040017F4 RID: 6132
		public GameObject MergedMesh;
	}
}
