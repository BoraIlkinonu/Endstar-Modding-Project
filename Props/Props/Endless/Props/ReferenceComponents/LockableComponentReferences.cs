using System;
using UnityEngine;

namespace Endless.Props.ReferenceComponents
{
	// Token: 0x02000021 RID: 33
	public class LockableComponentReferences : ReferenceBase
	{
		// Token: 0x17000040 RID: 64
		// (get) Token: 0x06000095 RID: 149 RVA: 0x00002CED File Offset: 0x00000EED
		// (set) Token: 0x06000096 RID: 150 RVA: 0x00002CF5 File Offset: 0x00000EF5
		public Transform LockVisualsSpawnTransform { get; private set; }

		// Token: 0x17000041 RID: 65
		// (get) Token: 0x06000097 RID: 151 RVA: 0x00002CFE File Offset: 0x00000EFE
		// (set) Token: 0x06000098 RID: 152 RVA: 0x00002D06 File Offset: 0x00000F06
		public Transform BackLockVisualsSpawnTransform { get; private set; }
	}
}
