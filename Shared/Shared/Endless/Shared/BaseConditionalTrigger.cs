using System;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared
{
	// Token: 0x0200004A RID: 74
	public abstract class BaseConditionalTrigger : MonoBehaviour
	{
		// Token: 0x1700005F RID: 95
		// (get) Token: 0x0600028C RID: 652 RVA: 0x0000D2DF File Offset: 0x0000B4DF
		// (set) Token: 0x0600028D RID: 653 RVA: 0x0000D2E7 File Offset: 0x0000B4E7
		protected bool VerboseLogging { get; set; }

		// Token: 0x0400014B RID: 331
		[SerializeField]
		protected UnityEvent Trigger = new UnityEvent();
	}
}
