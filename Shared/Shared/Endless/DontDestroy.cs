using System;
using UnityEngine;

namespace Endless
{
	// Token: 0x02000029 RID: 41
	public class DontDestroy : MonoBehaviour
	{
		// Token: 0x06000129 RID: 297 RVA: 0x00007F67 File Offset: 0x00006167
		private void Awake()
		{
			global::UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		}
	}
}
