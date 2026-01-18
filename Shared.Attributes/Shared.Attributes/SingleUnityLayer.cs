using System;
using UnityEngine;

namespace Endless.Shared
{
	// Token: 0x02000008 RID: 8
	[Serializable]
	public class SingleUnityLayer
	{
		// Token: 0x17000001 RID: 1
		// (get) Token: 0x0600000A RID: 10 RVA: 0x0000213D File Offset: 0x0000033D
		public int LayerIndex
		{
			get
			{
				return this.m_LayerIndex;
			}
		}

		// Token: 0x0600000B RID: 11 RVA: 0x00002145 File Offset: 0x00000345
		public void Set(int layerIndex)
		{
			if (layerIndex > 0 && layerIndex < 32)
			{
				this.m_LayerIndex = layerIndex;
			}
		}

		// Token: 0x17000002 RID: 2
		// (get) Token: 0x0600000C RID: 12 RVA: 0x00002157 File Offset: 0x00000357
		public int Mask
		{
			get
			{
				return 1 << this.m_LayerIndex;
			}
		}

		// Token: 0x0600000D RID: 13 RVA: 0x00002164 File Offset: 0x00000364
		public static implicit operator int(SingleUnityLayer layer)
		{
			return layer.Mask;
		}

		// Token: 0x04000006 RID: 6
		[SerializeField]
		private int m_LayerIndex;
	}
}
