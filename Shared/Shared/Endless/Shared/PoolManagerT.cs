using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Endless.Shared
{
	// Token: 0x02000081 RID: 129
	public class PoolManagerT : MonoBehaviourSingleton<PoolManagerT>
	{
		// Token: 0x060003C6 RID: 966 RVA: 0x00010AFC File Offset: 0x0000ECFC
		public void PrewarmPool<T>(T prefab, int size) where T : MonoBehaviour, IPoolableT
		{
			if (!this.pools.ContainsKey(prefab))
			{
				this.pools.Add(prefab, new Queue<MonoBehaviour>());
			}
			while (this.pools[prefab].Count < size)
			{
				T t = this.SpawnNewObject<T>(prefab, false);
				this.pools[prefab].Enqueue(t);
			}
		}

		// Token: 0x060003C7 RID: 967 RVA: 0x00010B72 File Offset: 0x0000ED72
		public void PrewarmPoolOverTime<T>(T prefab, int size) where T : MonoBehaviour, IPoolableT
		{
			base.StartCoroutine(this.PrewarmCoroutine<T>(prefab, size));
		}

		// Token: 0x060003C8 RID: 968 RVA: 0x00010B83 File Offset: 0x0000ED83
		private IEnumerator PrewarmCoroutine<T>(T prefab, int size) where T : MonoBehaviour, IPoolableT
		{
			if (!this.pools.ContainsKey(prefab))
			{
				this.pools.Add(prefab, new Queue<MonoBehaviour>());
			}
			while (this.pools[prefab].Count < size)
			{
				T t = this.SpawnNewObject<T>(prefab, false);
				this.pools[prefab].Enqueue(t);
				yield return null;
			}
			yield break;
		}

		// Token: 0x060003C9 RID: 969 RVA: 0x00010BA0 File Offset: 0x0000EDA0
		private T SpawnNewObject<T>(T prefab, bool fireOnSpawn = true) where T : MonoBehaviour, IPoolableT
		{
			T t = global::UnityEngine.Object.Instantiate<T>(prefab, prefab.IsUi ? this.uiSpawnTransform : this.spawnTransform);
			t.gameObject.name = prefab.gameObject.name + " - " + global::UnityEngine.Random.Range(0, 100000).ToString();
			t.Prefab = prefab;
			if (fireOnSpawn)
			{
				t.OnSpawn();
			}
			return t;
		}

		// Token: 0x060003CA RID: 970 RVA: 0x00010C2C File Offset: 0x0000EE2C
		public T Spawn<T>(T prefab, Vector3 position = default(Vector3), Quaternion rotation = default(Quaternion), Transform parent = null) where T : MonoBehaviour, IPoolableT
		{
			if (!this.pools.ContainsKey(prefab))
			{
				this.pools.Add(prefab, new Queue<MonoBehaviour>());
			}
			T t;
			do
			{
				if (this.pools[prefab].Count == 0)
				{
					t = this.SpawnNewObject<T>(prefab, false);
				}
				else
				{
					t = this.pools[prefab].Dequeue() as T;
					if (t == null)
					{
						Debug.LogError("A null object was found in the pool!");
					}
				}
			}
			while (t == null);
			if (parent != null)
			{
				t.transform.SetParent(parent, !prefab.IsUi);
			}
			if (prefab.IsUi)
			{
				t.transform.localScale = Vector3.one;
			}
			t.transform.SetPositionAndRotation(position, rotation);
			t.OnSpawn();
			return t;
		}

		// Token: 0x060003CB RID: 971 RVA: 0x00010D38 File Offset: 0x0000EF38
		public void Despawn<T>(T pooledObject) where T : MonoBehaviour, IPoolableT
		{
			if (pooledObject.Prefab == null)
			{
				return;
			}
			if (!this.pools.ContainsKey(pooledObject.Prefab))
			{
				throw new NotSupportedException("Cannot return an object to a pool that doesnt exist. Make sure they are only spawned via PoolManagerT.Spawn");
			}
			this.pools[pooledObject.Prefab].Enqueue(pooledObject);
			pooledObject.OnDespawn();
			pooledObject.transform.SetParent(pooledObject.IsUi ? this.uiSpawnTransform : this.spawnTransform);
		}

		// Token: 0x040001CF RID: 463
		[SerializeField]
		private Transform spawnTransform;

		// Token: 0x040001D0 RID: 464
		[SerializeField]
		private RectTransform uiSpawnTransform;

		// Token: 0x040001D1 RID: 465
		private Dictionary<MonoBehaviour, Queue<MonoBehaviour>> pools = new Dictionary<MonoBehaviour, Queue<MonoBehaviour>>();
	}
}
