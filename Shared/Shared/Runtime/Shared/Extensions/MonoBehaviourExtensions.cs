using System;
using UnityEngine;

namespace Runtime.Shared.Extensions
{
	// Token: 0x02000028 RID: 40
	public static class MonoBehaviourExtensions
	{
		// Token: 0x06000127 RID: 295 RVA: 0x00007F55 File Offset: 0x00006155
		public static void Enable(this MonoBehaviour monoBehaviour)
		{
			monoBehaviour.enabled = true;
		}

		// Token: 0x06000128 RID: 296 RVA: 0x00007F5E File Offset: 0x0000615E
		public static void Disable(this MonoBehaviour monoBehaviour)
		{
			monoBehaviour.enabled = false;
		}
	}
}
