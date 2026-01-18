using System;

namespace Endless.Matchmaking
{
	// Token: 0x0200002F RID: 47
	public class AssetParams
	{
		// Token: 0x06000126 RID: 294 RVA: 0x00005D0B File Offset: 0x00003F0B
		public AssetParams(string assetQueryFilter = null, bool populateRefs = false, string assetReturnArgs = null)
		{
			this.AssetQueryFilter = assetQueryFilter;
			this.PopulateRefs = populateRefs;
			this.AssetReturnArgs = (string.IsNullOrWhiteSpace(assetReturnArgs) ? null : assetReturnArgs);
		}

		// Token: 0x04000074 RID: 116
		public string AssetQueryFilter;

		// Token: 0x04000075 RID: 117
		public string AssetReturnArgs;

		// Token: 0x04000076 RID: 118
		public bool PopulateRefs;
	}
}
