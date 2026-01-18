using System;
using UnityEngine;

namespace Endless.Shared
{
	// Token: 0x0200007E RID: 126
	public interface IPoolableT
	{
		// Token: 0x1700009B RID: 155
		// (get) Token: 0x060003B7 RID: 951
		// (set) Token: 0x060003B8 RID: 952
		MonoBehaviour Prefab { get; set; }

		// Token: 0x1700009C RID: 156
		// (get) Token: 0x060003B9 RID: 953 RVA: 0x000043C6 File Offset: 0x000025C6
		bool IsUi
		{
			get
			{
				return false;
			}
		}

		// Token: 0x060003BA RID: 954 RVA: 0x000050BB File Offset: 0x000032BB
		void OnSpawn()
		{
		}

		// Token: 0x060003BB RID: 955 RVA: 0x000050BB File Offset: 0x000032BB
		void OnDespawn()
		{
		}
	}
}
