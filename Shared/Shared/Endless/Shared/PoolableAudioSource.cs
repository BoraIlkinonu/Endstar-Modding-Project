using System;
using UnityEngine;

namespace Endless.Shared
{
	// Token: 0x02000084 RID: 132
	public class PoolableAudioSource : MonoBehaviour, IPoolableT
	{
		// Token: 0x170000A1 RID: 161
		// (get) Token: 0x060003D4 RID: 980 RVA: 0x00010EDA File Offset: 0x0000F0DA
		// (set) Token: 0x060003D5 RID: 981 RVA: 0x00010EE2 File Offset: 0x0000F0E2
		public AudioSource AudioSource { get; private set; }

		// Token: 0x170000A2 RID: 162
		// (get) Token: 0x060003D6 RID: 982 RVA: 0x00010EEB File Offset: 0x0000F0EB
		// (set) Token: 0x060003D7 RID: 983 RVA: 0x00010EF3 File Offset: 0x0000F0F3
		public MonoBehaviour Prefab { get; set; }
	}
}
