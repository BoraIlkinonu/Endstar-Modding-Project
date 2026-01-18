using System;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay
{
	// Token: 0x02000057 RID: 87
	public abstract class AmbientEntry : MonoBehaviour
	{
		// Token: 0x06000149 RID: 329 RVA: 0x000079CB File Offset: 0x00005BCB
		public virtual void Activate()
		{
			this.OnActivated.Invoke(this);
		}

		// Token: 0x0600014A RID: 330 RVA: 0x000079D9 File Offset: 0x00005BD9
		public virtual void Deactivate()
		{
			this.OnDeactivated.Invoke(this);
		}

		// Token: 0x04000114 RID: 276
		[HideInInspector]
		public UnityEvent<AmbientEntry> OnActivated = new UnityEvent<AmbientEntry>();

		// Token: 0x04000115 RID: 277
		[HideInInspector]
		public UnityEvent<AmbientEntry> OnDeactivated = new UnityEvent<AmbientEntry>();
	}
}
