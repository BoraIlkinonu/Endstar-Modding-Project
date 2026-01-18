using System;
using UnityEngine;

namespace Endless.Shared
{
	// Token: 0x0200007C RID: 124
	public interface IPoolable
	{
		// Token: 0x1700009A RID: 154
		// (get) Token: 0x060003B3 RID: 947
		GameObject GameObject { get; }

		// Token: 0x060003B4 RID: 948
		void OnSpawn();

		// Token: 0x060003B5 RID: 949
		void OnDespawn();
	}
}
