using System;
using System.Collections.Generic;
using UnityEngine;

namespace Endless.Shared.DataTypes
{
	// Token: 0x02000013 RID: 19
	[Serializable]
	public class SerializableDictionary<TKey, TValue>
	{
		// Token: 0x1700000F RID: 15
		public TValue this[TKey key]
		{
			get
			{
				if (!this.initialized)
				{
					this.Initialize();
				}
				if (this.dictionary.ContainsKey(key))
				{
					return this.dictionary[key];
				}
				Debug.LogException(new Exception(string.Format("There is no {0} of {1}! Returning the default: {2}...", "Key", key, this.defaultValueIfNoEntry)));
				return this.defaultValueIfNoEntry;
			}
		}

		// Token: 0x17000010 RID: 16
		// (get) Token: 0x06000084 RID: 132 RVA: 0x0000394A File Offset: 0x00001B4A
		public IReadOnlyCollection<DictionaryEntry<TKey, TValue>> Items
		{
			get
			{
				return this.items;
			}
		}

		// Token: 0x17000011 RID: 17
		// (get) Token: 0x06000085 RID: 133 RVA: 0x00003952 File Offset: 0x00001B52
		public int Length
		{
			get
			{
				return this.items.Length;
			}
		}

		// Token: 0x17000012 RID: 18
		// (get) Token: 0x06000086 RID: 134 RVA: 0x0000395C File Offset: 0x00001B5C
		public TValue DefaultValueIfNoEntry
		{
			get
			{
				return this.defaultValueIfNoEntry;
			}
		}

		// Token: 0x17000013 RID: 19
		// (get) Token: 0x06000087 RID: 135 RVA: 0x00003964 File Offset: 0x00001B64
		public IEnumerable<TKey> Keys
		{
			get
			{
				if (!this.initialized)
				{
					this.Initialize();
				}
				return this.dictionary.Keys;
			}
		}

		// Token: 0x17000014 RID: 20
		// (get) Token: 0x06000088 RID: 136 RVA: 0x0000397F File Offset: 0x00001B7F
		public IEnumerable<TValue> Values
		{
			get
			{
				if (!this.initialized)
				{
					this.Initialize();
				}
				return this.dictionary.Values;
			}
		}

		// Token: 0x17000015 RID: 21
		// (get) Token: 0x06000089 RID: 137 RVA: 0x0000399C File Offset: 0x00001B9C
		protected HashSet<TKey> KeysHashSet
		{
			get
			{
				HashSet<TKey> hashSet = new HashSet<TKey>();
				for (int i = 0; i < this.items.Length; i++)
				{
					TKey key = this.items[i].Key;
					if (hashSet.Contains(key))
					{
						this.OnDuplicateKey(key);
					}
					else
					{
						hashSet.Add(key);
					}
				}
				return hashSet;
			}
		}

		// Token: 0x0600008A RID: 138 RVA: 0x000039EF File Offset: 0x00001BEF
		public bool Contains(TKey key)
		{
			if (!this.initialized)
			{
				this.Initialize();
			}
			return this.dictionary.ContainsKey(key);
		}

		// Token: 0x0600008B RID: 139 RVA: 0x00003A0C File Offset: 0x00001C0C
		public void DebugLogEachKeyValuePair()
		{
			if (!this.initialized)
			{
				this.Initialize();
			}
			foreach (KeyValuePair<TKey, TValue> keyValuePair in this.dictionary)
			{
				Debug.Log(string.Format("{0}: {1}", keyValuePair.Key, keyValuePair.Value));
			}
		}

		// Token: 0x0600008C RID: 140 RVA: 0x00003A90 File Offset: 0x00001C90
		public void DebugEntries()
		{
			foreach (DictionaryEntry<TKey, TValue> dictionaryEntry in this.items)
			{
				Debug.Log(string.Format("{0}: {1}", dictionaryEntry.Key, dictionaryEntry.Value));
			}
		}

		// Token: 0x0600008D RID: 141 RVA: 0x00003AE0 File Offset: 0x00001CE0
		private void ValidateForDuplicateKeys()
		{
			HashSet<TKey> hashSet = new HashSet<TKey>();
			for (int i = 0; i < this.items.Length; i++)
			{
				TKey key = this.items[i].Key;
				if (hashSet.Contains(key))
				{
					this.OnDuplicateKey(key);
				}
				else
				{
					hashSet.Add(key);
				}
			}
		}

		// Token: 0x0600008E RID: 142 RVA: 0x00003B34 File Offset: 0x00001D34
		private void Initialize()
		{
			this.initialized = true;
			for (int i = 0; i < this.items.Length; i++)
			{
				DictionaryEntry<TKey, TValue> dictionaryEntry = this.items[i];
				TKey key = dictionaryEntry.Key;
				TValue value = dictionaryEntry.Value;
				if (this.dictionary.ContainsKey(key))
				{
					this.OnDuplicateKey(key);
				}
				else
				{
					this.dictionary.Add(key, value);
				}
			}
		}

		// Token: 0x0600008F RID: 143 RVA: 0x00003B98 File Offset: 0x00001D98
		private void OnDuplicateKey(TKey key)
		{
			Debug.LogException(new Exception(string.Format("There is a duplicate {0} of {1}! {2}s must not be duplicated!", "Key", key, "Key")));
		}

		// Token: 0x04000033 RID: 51
		[SerializeField]
		private DictionaryEntry<TKey, TValue>[] items = Array.Empty<DictionaryEntry<TKey, TValue>>();

		// Token: 0x04000034 RID: 52
		[SerializeField]
		private TValue defaultValueIfNoEntry;

		// Token: 0x04000035 RID: 53
		private readonly Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();

		// Token: 0x04000036 RID: 54
		[NonSerialized]
		private bool initialized;
	}
}
