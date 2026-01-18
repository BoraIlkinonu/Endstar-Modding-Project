using System;
using System.Collections.Generic;
using Endless.Shared.DataTypes;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x02000103 RID: 259
	public interface IGameAssetListModel
	{
		// Token: 0x1700005B RID: 91
		// (get) Token: 0x0600041E RID: 1054
		GameObject GameObject { get; }

		// Token: 0x1700005C RID: 92
		// (get) Token: 0x0600041F RID: 1055
		AssetContexts Context { get; }

		// Token: 0x06000420 RID: 1056
		void Synchronize();

		// Token: 0x06000421 RID: 1057
		void SetAssetIdsToIgnore(HashSet<SerializableGuid> assetIdsToIgnore);

		// Token: 0x06000422 RID: 1058
		void SetAssetTypeFilter(UIGameAssetTypes value, bool triggerRequest);
	}
}
