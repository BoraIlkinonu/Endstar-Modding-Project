using System;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Shared
{
	// Token: 0x0200007F RID: 127
	public class NetworkPooledPrefabHandler<T> : INetworkPrefabInstanceHandler where T : NetworkBehaviour, IPoolableT
	{
		// Token: 0x060003BC RID: 956 RVA: 0x00010A18 File Offset: 0x0000EC18
		public NetworkPooledPrefabHandler(T prefab)
		{
			this.prefab = prefab;
		}

		// Token: 0x060003BD RID: 957 RVA: 0x00010A28 File Offset: 0x0000EC28
		public static void RegisterNetworkPrefab<T>(T prefab, int? prewarmCount = null) where T : NetworkBehaviour, IPoolableT
		{
			NetworkManager.Singleton.PrefabHandler.AddHandler(prefab.gameObject, new NetworkPooledPrefabHandler<T>(prefab));
			if (prewarmCount != null && prewarmCount.Value > 0)
			{
				MonoBehaviourSingleton<PoolManagerT>.Instance.PrewarmPoolOverTime<T>(prefab, prewarmCount.Value);
			}
		}

		// Token: 0x060003BE RID: 958 RVA: 0x00010A7B File Offset: 0x0000EC7B
		public NetworkObject Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation)
		{
			return MonoBehaviourSingleton<PoolManagerT>.Instance.Spawn<T>(this.prefab, position, rotation, null).NetworkObject;
		}

		// Token: 0x060003BF RID: 959 RVA: 0x00010A9A File Offset: 0x0000EC9A
		public void Destroy(NetworkObject networkObject)
		{
			MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn<T>(networkObject.GetComponent<T>());
		}

		// Token: 0x040001CB RID: 459
		private T prefab;
	}
}
