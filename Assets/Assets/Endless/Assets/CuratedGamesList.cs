using System;
using System.Collections.Generic;
using Endless.Shared.DataTypes;

namespace Endless.Assets
{
	// Token: 0x02000010 RID: 16
	public class CuratedGamesList : AssetList
	{
		// Token: 0x06000041 RID: 65 RVA: 0x00002643 File Offset: 0x00000843
		public static CuratedGamesList FromReferences(string name, string description, SerializableGuid assetId, IEnumerable<AssetReference> references)
		{
			return new CuratedGamesList
			{
				Name = name,
				Description = description,
				AssetType = "gameList",
				AssetID = assetId,
				assets = new List<AssetReference>(references)
			};
		}

		// Token: 0x0400005F RID: 95
		private const string ASSET_TYPE = "gameList";
	}
}
