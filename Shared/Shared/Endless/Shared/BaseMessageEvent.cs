using System;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared
{
	// Token: 0x0200006F RID: 111
	public abstract class BaseMessageEvent : MonoBehaviour
	{
		// Token: 0x17000091 RID: 145
		// (get) Token: 0x0600037B RID: 891 RVA: 0x000102CC File Offset: 0x0000E4CC
		// (set) Token: 0x0600037C RID: 892 RVA: 0x000102D4 File Offset: 0x0000E4D4
		protected bool VerboseLogging { get; set; }

		// Token: 0x040001AE RID: 430
		public UnityEvent OnMessageTriggered = new UnityEvent();
	}
}
