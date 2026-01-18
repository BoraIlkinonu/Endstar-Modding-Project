using System;
using Endless.Shared.DataTypes;

namespace Endless.Gameplay
{
	// Token: 0x020000F7 RID: 247
	[Serializable]
	public class KeyLibraryReference : InventoryLibraryReference
	{
		// Token: 0x170000E8 RID: 232
		// (get) Token: 0x0600057D RID: 1405 RVA: 0x0001BCEF File Offset: 0x00019EEF
		internal override ReferenceFilter Filter
		{
			get
			{
				return ReferenceFilter.Key;
			}
		}

		// Token: 0x0600057E RID: 1406 RVA: 0x0001BCF3 File Offset: 0x00019EF3
		internal KeyLibraryReference()
		{
		}

		// Token: 0x0600057F RID: 1407 RVA: 0x0001BCFB File Offset: 0x00019EFB
		internal KeyLibraryReference(SerializableGuid assetId)
			: base(assetId)
		{
		}
	}
}
