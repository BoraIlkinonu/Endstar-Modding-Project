using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Endless.Assets
{
	// Token: 0x02000006 RID: 6
	public abstract class AssetList : Asset
	{
		// Token: 0x17000005 RID: 5
		// (get) Token: 0x06000012 RID: 18 RVA: 0x0000225B File Offset: 0x0000045B
		[JsonIgnore]
		public IReadOnlyList<AssetReference> Assets
		{
			get
			{
				return this.assets;
			}
		}

		// Token: 0x0400000C RID: 12
		[SerializeField]
		[JsonProperty]
		protected List<AssetReference> assets = new List<AssetReference>();

		// Token: 0x0400000D RID: 13
		[SerializeField]
		[JsonProperty]
		protected int iconFileInstanceId;
	}
}
