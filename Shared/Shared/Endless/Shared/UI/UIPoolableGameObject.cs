using System;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x02000276 RID: 630
	public class UIPoolableGameObject : UIGameObject, IPoolableT
	{
		// Token: 0x170002FC RID: 764
		// (get) Token: 0x06000FD0 RID: 4048 RVA: 0x00043CE7 File Offset: 0x00041EE7
		// (set) Token: 0x06000FD1 RID: 4049 RVA: 0x00043CEF File Offset: 0x00041EEF
		protected bool VerboseLogging { get; set; }

		// Token: 0x170002FD RID: 765
		// (get) Token: 0x06000FD2 RID: 4050 RVA: 0x00043CF8 File Offset: 0x00041EF8
		// (set) Token: 0x06000FD3 RID: 4051 RVA: 0x00043D00 File Offset: 0x00041F00
		public MonoBehaviour Prefab { get; set; }

		// Token: 0x170002FE RID: 766
		// (get) Token: 0x06000FD4 RID: 4052 RVA: 0x000050D2 File Offset: 0x000032D2
		public bool IsUi
		{
			get
			{
				return true;
			}
		}

		// Token: 0x170002FF RID: 767
		// (get) Token: 0x06000FD5 RID: 4053 RVA: 0x00043D09 File Offset: 0x00041F09
		public UnityEvent OnDespawnUnityEvent { get; } = new UnityEvent();

		// Token: 0x06000FD6 RID: 4054 RVA: 0x00043D11 File Offset: 0x00041F11
		public virtual void OnSpawn()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("OnSpawn", this);
			}
		}

		// Token: 0x06000FD7 RID: 4055 RVA: 0x00043D26 File Offset: 0x00041F26
		public virtual void OnDespawn()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("OnDespawn", this);
			}
			this.OnDespawnUnityEvent.Invoke();
		}

		// Token: 0x06000FD8 RID: 4056 RVA: 0x00043D48 File Offset: 0x00041F48
		public UIPoolableGameObject SpawnPooledInstance(Transform parent = null)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SpawnPooledInstance", new object[] { parent });
			}
			return MonoBehaviourSingleton<PoolManagerT>.Instance.Spawn<UIPoolableGameObject>(this, default(Vector3), default(Quaternion), parent);
		}

		// Token: 0x06000FD9 RID: 4057 RVA: 0x00043D92 File Offset: 0x00041F92
		public void ReturnToPool()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ReturnToPool", Array.Empty<object>());
			}
			MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn<UIPoolableGameObject>(this);
		}

		// Token: 0x06000FDA RID: 4058 RVA: 0x00043DB7 File Offset: 0x00041FB7
		public void PrewarmPool(int count)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "PrewarmPool", new object[] { count });
			}
			MonoBehaviourSingleton<PoolManagerT>.Instance.PrewarmPoolOverTime<UIPoolableGameObject>(this, count);
		}
	}
}
