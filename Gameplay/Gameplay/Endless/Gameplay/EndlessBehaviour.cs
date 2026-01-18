using System;
using Endless.Shared;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x0200008C RID: 140
	public class EndlessBehaviour : MonoBehaviour
	{
		// Token: 0x1700007B RID: 123
		// (get) Token: 0x06000282 RID: 642 RVA: 0x0000DD0B File Offset: 0x0000BF0B
		protected bool IsServer
		{
			get
			{
				return NetworkManager.Singleton.IsServer;
			}
		}

		// Token: 0x06000283 RID: 643 RVA: 0x0000DD17 File Offset: 0x0000BF17
		protected virtual void Start()
		{
			MonoBehaviourSingleton<EndlessLoop>.Instance.RegisterBehaviour(this);
		}

		// Token: 0x06000284 RID: 644 RVA: 0x0000DD24 File Offset: 0x0000BF24
		protected virtual void OnDestroy()
		{
			if (MonoBehaviourSingleton<EndlessLoop>.Instance)
			{
				MonoBehaviourSingleton<EndlessLoop>.Instance.RemoveBehaviour(this);
			}
		}
	}
}
