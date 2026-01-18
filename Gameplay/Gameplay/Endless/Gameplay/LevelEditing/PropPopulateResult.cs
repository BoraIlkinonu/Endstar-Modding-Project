using System;
using System.Collections.Generic;
using Endless.Gameplay.LevelEditing.Tilesets;

namespace Endless.Gameplay.LevelEditing
{
	// Token: 0x020004E9 RID: 1257
	public class PropPopulateResult
	{
		// Token: 0x170005FA RID: 1530
		// (get) Token: 0x06001EC9 RID: 7881 RVA: 0x00086C6E File Offset: 0x00084E6E
		public List<AssetIdVersionKey> ModifiedProps { get; }

		// Token: 0x06001ECA RID: 7882 RVA: 0x00086C76 File Offset: 0x00084E76
		public PropPopulateResult(List<AssetIdVersionKey> modifiedProps)
		{
			this.ModifiedProps = modifiedProps;
		}
	}
}
