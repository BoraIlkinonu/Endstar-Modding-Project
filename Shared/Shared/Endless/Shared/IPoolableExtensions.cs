using System;
using System.Collections.Generic;
using UnityEngine;

namespace Endless.Shared
{
	// Token: 0x0200007D RID: 125
	public static class IPoolableExtensions
	{
		// Token: 0x060003B6 RID: 950 RVA: 0x000109B0 File Offset: 0x0000EBB0
		public static void DespawnAllItemsAndClear<T>(this ICollection<T> collection) where T : MonoBehaviour, IPoolableT
		{
			if (collection.Count == 0)
			{
				return;
			}
			foreach (T t in collection)
			{
				if (t)
				{
					MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn<T>(t);
				}
			}
			collection.Clear();
		}
	}
}
