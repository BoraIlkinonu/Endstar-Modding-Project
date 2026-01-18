using System;
using System.Collections.Generic;
using Endless.Shared.DataTypes;

namespace Endless.Assets
{
	// Token: 0x02000014 RID: 20
	public class LibraryContentPack : AssetList
	{
		// Token: 0x0600004B RID: 75 RVA: 0x0000277D File Offset: 0x0000097D
		public static LibraryContentPack FromReferences(string name, string description, SerializableGuid assetId, IEnumerable<AssetReference> references)
		{
			return new LibraryContentPack
			{
				Name = name,
				Description = description,
				AssetType = "libraryContentPack",
				AssetID = assetId,
				assets = new List<AssetReference>(references)
			};
		}

		// Token: 0x04000065 RID: 101
		private const string ASSET_TYPE = "libraryContentPack";
	}
}
