using System;
using System.Collections.Generic;
using Endless.Assets;
using Endless.GraphQl;

namespace Endless.Gameplay
{
	// Token: 0x0200009D RID: 157
	public class BulkAssetCacheResult<T> : AssetCacheResultCore<T> where T : Asset
	{
		// Token: 0x060002C5 RID: 709 RVA: 0x0000E983 File Offset: 0x0000CB83
		public BulkAssetCacheResult(GraphQlResult result, List<T> assets)
		{
			this.Result = result;
			this.Assets = assets;
		}

		// Token: 0x0400028C RID: 652
		public readonly List<T> Assets;
	}
}
