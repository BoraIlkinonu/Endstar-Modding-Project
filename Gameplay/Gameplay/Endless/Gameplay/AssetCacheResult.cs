using System;
using Endless.Assets;
using Endless.GraphQl;

namespace Endless.Gameplay
{
	// Token: 0x0200009C RID: 156
	public class AssetCacheResult<T> : AssetCacheResultCore<T> where T : Asset
	{
		// Token: 0x060002C4 RID: 708 RVA: 0x0000E96D File Offset: 0x0000CB6D
		public AssetCacheResult(GraphQlResult result, T asset)
		{
			this.Result = result;
			this.Asset = asset;
		}

		// Token: 0x0400028B RID: 651
		public readonly T Asset;
	}
}
