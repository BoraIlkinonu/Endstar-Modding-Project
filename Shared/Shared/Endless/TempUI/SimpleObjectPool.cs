using System;
using UnityEngine;
using UnityEngine.Pool;

namespace Endless.TempUI
{
	// Token: 0x02000033 RID: 51
	public class SimpleObjectPool<T> where T : Component
	{
		// Token: 0x1700003D RID: 61
		// (get) Token: 0x06000151 RID: 337 RVA: 0x00008E02 File Offset: 0x00007002
		public IObjectPool<T> Pool { get; }

		// Token: 0x06000152 RID: 338 RVA: 0x00008E0C File Offset: 0x0000700C
		public SimpleObjectPool(T prefab, Transform container, Action<T> onCreated = null)
		{
			this.prefab = prefab;
			this.container = container;
			this.onCreated = onCreated;
			this.Pool = new ObjectPool<T>(new Func<T>(this.CreateObject), new Action<T>(this.OnTakeFromPool), new Action<T>(this.OnReturnedToPool), new Action<T>(this.OnDestroyPoolObject), true, 10, 10000);
		}

		// Token: 0x06000153 RID: 339 RVA: 0x00008E78 File Offset: 0x00007078
		private T CreateObject()
		{
			T t = global::UnityEngine.Object.Instantiate<T>(this.prefab, this.container);
			Action<T> action = this.onCreated;
			if (action != null)
			{
				action(t);
			}
			return t;
		}

		// Token: 0x06000154 RID: 340 RVA: 0x00008EAA File Offset: 0x000070AA
		private void OnTakeFromPool(T @object)
		{
			@object.gameObject.SetActive(true);
		}

		// Token: 0x06000155 RID: 341 RVA: 0x00008EBD File Offset: 0x000070BD
		private void OnReturnedToPool(T @object)
		{
			@object.gameObject.SetActive(false);
		}

		// Token: 0x06000156 RID: 342 RVA: 0x00008ED0 File Offset: 0x000070D0
		private void OnDestroyPoolObject(T @object)
		{
			global::UnityEngine.Object.Destroy(@object.gameObject);
		}

		// Token: 0x040000BC RID: 188
		private T prefab;

		// Token: 0x040000BD RID: 189
		private Transform container;

		// Token: 0x040000BE RID: 190
		private Action<T> onCreated;
	}
}
