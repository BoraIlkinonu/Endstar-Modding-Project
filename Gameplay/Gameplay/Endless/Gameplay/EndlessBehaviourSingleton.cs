using System;
using Endless.Shared;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x0200008D RID: 141
	public class EndlessBehaviourSingleton<T> : MonoBehaviourSingleton<T> where T : MonoBehaviour
	{
		// Token: 0x1700007C RID: 124
		// (get) Token: 0x06000286 RID: 646 RVA: 0x0000DD0B File Offset: 0x0000BF0B
		protected bool IsServer
		{
			get
			{
				return NetworkManager.Singleton.IsServer;
			}
		}

		// Token: 0x06000287 RID: 647 RVA: 0x0000DD17 File Offset: 0x0000BF17
		protected virtual void Start()
		{
			MonoBehaviourSingleton<EndlessLoop>.Instance.RegisterBehaviour(this);
		}

		// Token: 0x06000288 RID: 648 RVA: 0x0000DD3D File Offset: 0x0000BF3D
		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (MonoBehaviourSingleton<EndlessLoop>.Instance)
			{
				MonoBehaviourSingleton<EndlessLoop>.Instance.RemoveBehaviour(this);
			}
		}
	}
}
