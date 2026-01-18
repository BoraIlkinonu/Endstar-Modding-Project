using System;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x0200029A RID: 666
	public class BaseTypeDefinition : ComponentDefinition
	{
		// Token: 0x170002C7 RID: 711
		// (get) Token: 0x06000EC7 RID: 3783 RVA: 0x0004E901 File Offset: 0x0004CB01
		public bool IsUserExposed
		{
			get
			{
				return this.isUserExposed;
			}
		}

		// Token: 0x170002C8 RID: 712
		// (get) Token: 0x06000EC8 RID: 3784 RVA: 0x0004E909 File Offset: 0x0004CB09
		public bool IsSpawnPoint
		{
			get
			{
				return this.isSpawnPoint;
			}
		}

		// Token: 0x04000D3A RID: 3386
		[SerializeField]
		private bool isUserExposed = true;

		// Token: 0x04000D3B RID: 3387
		[SerializeField]
		private bool isSpawnPoint;
	}
}
