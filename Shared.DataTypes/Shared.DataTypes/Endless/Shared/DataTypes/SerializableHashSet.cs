using System;
using System.Collections.Generic;
using UnityEngine;

namespace Endless.Shared.DataTypes
{
	// Token: 0x02000017 RID: 23
	[Serializable]
	public class SerializableHashSet<T>
	{
		// Token: 0x17000018 RID: 24
		// (get) Token: 0x060000AB RID: 171 RVA: 0x00003ED2 File Offset: 0x000020D2
		public int Length
		{
			get
			{
				return this.items.Length;
			}
		}

		// Token: 0x17000019 RID: 25
		// (get) Token: 0x060000AC RID: 172 RVA: 0x00003EDC File Offset: 0x000020DC
		public IReadOnlyCollection<T> Items
		{
			get
			{
				return this.items;
			}
		}

		// Token: 0x1700001A RID: 26
		// (get) Token: 0x060000AD RID: 173 RVA: 0x00003EE4 File Offset: 0x000020E4
		public IEnumerable<T> Values
		{
			get
			{
				if (!this.initialized)
				{
					this.Initialize();
				}
				return this.hashSet;
			}
		}

		// Token: 0x060000AE RID: 174 RVA: 0x00003EFA File Offset: 0x000020FA
		public bool Contains(T item)
		{
			if (!this.initialized)
			{
				this.Initialize();
			}
			return this.hashSet.Contains(item);
		}

		// Token: 0x060000AF RID: 175 RVA: 0x00003F18 File Offset: 0x00002118
		public void DebugLogEachItem()
		{
			if (!this.initialized)
			{
				this.Initialize();
			}
			foreach (T t in this.hashSet)
			{
				Debug.Log(t);
			}
		}

		// Token: 0x060000B0 RID: 176 RVA: 0x00003F7C File Offset: 0x0000217C
		private void Initialize()
		{
			this.initialized = true;
			for (int i = 0; i < this.items.Length; i++)
			{
				T t = this.items[i];
				if (!this.hashSet.Add(t))
				{
					this.OnDuplicateItem(t);
				}
			}
		}

		// Token: 0x060000B1 RID: 177 RVA: 0x00003FC5 File Offset: 0x000021C5
		private void OnDuplicateItem(T item)
		{
			Debug.LogException(new Exception(string.Format("There is a duplicate item of {0}! Items in a {1} must be unique.", item, "SerializableHashSet")));
		}

		// Token: 0x0400003A RID: 58
		[SerializeField]
		private T[] items = Array.Empty<T>();

		// Token: 0x0400003B RID: 59
		private readonly HashSet<T> hashSet = new HashSet<T>();

		// Token: 0x0400003C RID: 60
		[NonSerialized]
		private bool initialized;
	}
}
