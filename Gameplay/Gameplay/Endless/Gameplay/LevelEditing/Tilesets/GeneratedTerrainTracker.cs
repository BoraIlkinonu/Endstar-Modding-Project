using System;
using System.Collections.Generic;
using Endless.Assets;
using UnityEngine;

namespace Endless.Gameplay.LevelEditing.Tilesets
{
	// Token: 0x02000514 RID: 1300
	public class GeneratedTerrainTracker
	{
		// Token: 0x06001F76 RID: 8054 RVA: 0x0008B64A File Offset: 0x0008984A
		public GeneratedTerrainTracker()
		{
			this.assetIdToGeneratedTerrainMap = new Dictionary<AssetReference, List<GameObject>>();
		}

		// Token: 0x06001F77 RID: 8055 RVA: 0x0008B660 File Offset: 0x00089860
		public void Add(AssetReference assetReference, GameObject generatedTerrain)
		{
			List<GameObject> list;
			if (!this.assetIdToGeneratedTerrainMap.TryGetValue(assetReference, out list))
			{
				list = new List<GameObject>();
				this.assetIdToGeneratedTerrainMap.Add(assetReference, list);
			}
			list.Add(generatedTerrain);
		}

		// Token: 0x06001F78 RID: 8056 RVA: 0x0008B698 File Offset: 0x00089898
		public void ClearGeneratedTerrainForId(AssetReference assetReference)
		{
			List<GameObject> list;
			if (!this.assetIdToGeneratedTerrainMap.TryGetValue(assetReference, out list))
			{
				return;
			}
			for (int i = list.Count - 1; i >= 0; i--)
			{
				global::UnityEngine.Object.Destroy(list[i]);
			}
		}

		// Token: 0x040018F5 RID: 6389
		private Dictionary<AssetReference, List<GameObject>> assetIdToGeneratedTerrainMap;
	}
}
