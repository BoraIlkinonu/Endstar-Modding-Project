using System;
using Endless.Shared.DataTypes;

namespace Endless.Creator.UI
{
	// Token: 0x02000180 RID: 384
	public struct UISpawnPoint
	{
		// Token: 0x060005A9 RID: 1449 RVA: 0x0001DB5F File Offset: 0x0001BD5F
		public UISpawnPoint(SerializableGuid id, string displayName)
		{
			this.Id = id;
			this.DisplayName = displayName;
		}

		// Token: 0x04000502 RID: 1282
		public SerializableGuid Id;

		// Token: 0x04000503 RID: 1283
		public readonly string DisplayName;
	}
}
