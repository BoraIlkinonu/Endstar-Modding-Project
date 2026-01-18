using System;
using Endless.Shared.DataTypes;

namespace Endless.Gameplay
{
	// Token: 0x020000C5 RID: 197
	[Serializable]
	public class ResourceLibraryReference : PropLibraryReference
	{
		// Token: 0x1700009C RID: 156
		// (get) Token: 0x060003B1 RID: 945 RVA: 0x000142B0 File Offset: 0x000124B0
		internal override ReferenceFilter Filter
		{
			get
			{
				return ReferenceFilter.NonStatic | ReferenceFilter.Resource;
			}
		}

		// Token: 0x060003B2 RID: 946 RVA: 0x000142B4 File Offset: 0x000124B4
		public ResourceLibraryReference(SerializableGuid assetId)
		{
			this.Id = assetId;
		}

		// Token: 0x060003B3 RID: 947 RVA: 0x000142C3 File Offset: 0x000124C3
		public ResourceLibraryReference()
		{
			this.Id = SerializableGuid.Empty;
			this.CosmeticId = SerializableGuid.Empty;
		}
	}
}
