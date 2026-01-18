using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Endless.Gameplay
{
	// Token: 0x02000089 RID: 137
	internal class SelfCleanup : MonoBehaviour
	{
		// Token: 0x06000279 RID: 633 RVA: 0x0000DA34 File Offset: 0x0000BC34
		private void OnDestroy()
		{
			Addressables.ReleaseInstance(base.gameObject);
		}
	}
}
