using System;
using Endless.Shared.Debugging;
using UnityEngine;

namespace Endless.Shared
{
	// Token: 0x02000080 RID: 128
	public class PooledGameObject : MonoBehaviour, IPoolableT
	{
		// Token: 0x1700009D RID: 157
		// (get) Token: 0x060003C0 RID: 960 RVA: 0x00010AAC File Offset: 0x0000ECAC
		// (set) Token: 0x060003C1 RID: 961 RVA: 0x00010AB4 File Offset: 0x0000ECB4
		public MonoBehaviour Prefab { get; set; }

		// Token: 0x1700009E RID: 158
		// (get) Token: 0x060003C2 RID: 962 RVA: 0x00010ABD File Offset: 0x0000ECBD
		public bool IsUi
		{
			get
			{
				return this.isUi;
			}
		}

		// Token: 0x060003C3 RID: 963 RVA: 0x00010AC5 File Offset: 0x0000ECC5
		public void OnSpawn()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnSpawn", Array.Empty<object>());
			}
		}

		// Token: 0x060003C4 RID: 964 RVA: 0x00010ADF File Offset: 0x0000ECDF
		public void OnDespawn()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDespawn", Array.Empty<object>());
			}
		}

		// Token: 0x040001CC RID: 460
		[SerializeField]
		private bool isUi;

		// Token: 0x040001CD RID: 461
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
