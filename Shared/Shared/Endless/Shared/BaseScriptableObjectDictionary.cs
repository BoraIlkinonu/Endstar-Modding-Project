using System;
using System.Collections;
using System.Collections.Generic;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.Validation;
using UnityEngine;
using UnityEngine.Serialization;

namespace Endless.Shared
{
	// Token: 0x0200008B RID: 139
	public abstract class BaseScriptableObjectDictionary<TKey, TValue> : ScriptableObject, IValidatable, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable
	{
		// Token: 0x170000A6 RID: 166
		// (get) Token: 0x060003EC RID: 1004 RVA: 0x000113EC File Offset: 0x0000F5EC
		// (set) Token: 0x060003ED RID: 1005 RVA: 0x000113F4 File Offset: 0x0000F5F4
		protected bool VerboseLogging { get; set; }

		// Token: 0x170000A7 RID: 167
		public TValue this[TKey key]
		{
			get
			{
				if (!this.initialized)
				{
					this.Initialize();
				}
				TValue tvalue;
				if (this.dictionary.TryGetValue(key, out tvalue))
				{
					return tvalue;
				}
				DebugUtility.LogException(new Exception(string.Format("There is no {0} of {1}! Returning the default: {2}...", "Key", key, this.defaultValueIfNoEntry)), this);
				return this.defaultValueIfNoEntry;
			}
		}

		// Token: 0x170000A8 RID: 168
		// (get) Token: 0x060003EF RID: 1007 RVA: 0x0001145E File Offset: 0x0000F65E
		public int Length
		{
			get
			{
				return this.Entries.Length;
			}
		}

		// Token: 0x170000A9 RID: 169
		// (get) Token: 0x060003F0 RID: 1008 RVA: 0x00011468 File Offset: 0x0000F668
		public TValue DefaultValueIfNoEntry
		{
			get
			{
				return this.defaultValueIfNoEntry;
			}
		}

		// Token: 0x170000AA RID: 170
		// (get) Token: 0x060003F1 RID: 1009 RVA: 0x00011470 File Offset: 0x0000F670
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

		// Token: 0x170000AB RID: 171
		// (get) Token: 0x060003F2 RID: 1010 RVA: 0x0001148B File Offset: 0x0000F68B
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

		// Token: 0x170000AC RID: 172
		// (get) Token: 0x060003F3 RID: 1011 RVA: 0x000114A8 File Offset: 0x0000F6A8
		protected HashSet<TKey> KeysHashSet
		{
			get
			{
				HashSet<TKey> hashSet = new HashSet<TKey>();
				for (int i = 0; i < this.Entries.Length; i++)
				{
					TKey key = this.Entries[i].Key;
					if (!hashSet.Add(key))
					{
						this.OnDuplicateKey(key);
					}
				}
				return hashSet;
			}
		}

		// Token: 0x060003F4 RID: 1012 RVA: 0x000114F1 File Offset: 0x0000F6F1
		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			if (!this.initialized)
			{
				this.Initialize();
			}
			return this.dictionary.GetEnumerator();
		}

		// Token: 0x060003F5 RID: 1013 RVA: 0x00011511 File Offset: 0x0000F711
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		// Token: 0x060003F6 RID: 1014 RVA: 0x00011519 File Offset: 0x0000F719
		[ContextMenu("Validate")]
		public virtual void Validate()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("Validate", this);
			}
			this.Reinitialize();
			this.ValidateForDuplicateKeys();
		}

		// Token: 0x060003F7 RID: 1015 RVA: 0x0001153C File Offset: 0x0000F73C
		public bool Contains(TKey key)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "Contains", "key", key), this);
			}
			if (!this.initialized)
			{
				this.Initialize();
			}
			return this.dictionary.ContainsKey(key);
		}

		// Token: 0x060003F8 RID: 1016 RVA: 0x0001158C File Offset: 0x0000F78C
		public void DebugLogEachKeyValuePair()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("DebugLogEachKeyValuePair", this);
			}
			if (!this.initialized)
			{
				this.Initialize();
			}
			foreach (KeyValuePair<TKey, TValue> keyValuePair in this.dictionary)
			{
				DebugUtility.Log(string.Format("{0}: {1}", keyValuePair.Key, keyValuePair.Value), this);
			}
		}

		// Token: 0x060003F9 RID: 1017 RVA: 0x00011624 File Offset: 0x0000F824
		public void DebugEntries()
		{
			DebugUtility.Log(base.name, null);
			foreach (DictionaryEntry<TKey, TValue> dictionaryEntry in this.Entries)
			{
				DebugUtility.Log(string.Format("{0}: {1}", dictionaryEntry.Key, dictionaryEntry.Value), null);
			}
		}

		// Token: 0x060003FA RID: 1018 RVA: 0x00011680 File Offset: 0x0000F880
		private void ValidateForDuplicateKeys()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("ValidateForDuplicateKeys", this);
			}
			HashSet<TKey> hashSet = new HashSet<TKey>();
			for (int i = 0; i < this.Entries.Length; i++)
			{
				TKey key = this.Entries[i].Key;
				if (!hashSet.Add(key))
				{
					this.OnDuplicateKey(key);
				}
			}
		}

		// Token: 0x060003FB RID: 1019 RVA: 0x000116DB File Offset: 0x0000F8DB
		private void Reinitialize()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("Reinitialize", this);
			}
			this.initialized = false;
			this.dictionary.Clear();
			this.Initialize();
		}

		// Token: 0x060003FC RID: 1020 RVA: 0x00011708 File Offset: 0x0000F908
		private void Initialize()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("Initialize", this);
			}
			this.initialized = true;
			for (int i = 0; i < this.Entries.Length; i++)
			{
				DictionaryEntry<TKey, TValue> dictionaryEntry = this.Entries[i];
				TKey key = dictionaryEntry.Key;
				TValue value = dictionaryEntry.Value;
				if (!this.dictionary.TryAdd(key, value))
				{
					this.OnDuplicateKey(key);
				}
			}
		}

		// Token: 0x060003FD RID: 1021 RVA: 0x00011771 File Offset: 0x0000F971
		private void OnDuplicateKey(TKey key)
		{
			DebugUtility.LogException(new Exception(string.Format("There is a duplicate {0} of {1}! {2}s must not be duplicated!", "Key", key, "Key")), this);
		}

		// Token: 0x040001E7 RID: 487
		[FormerlySerializedAs("entries")]
		[SerializeField]
		protected DictionaryEntry<TKey, TValue>[] Entries = Array.Empty<DictionaryEntry<TKey, TValue>>();

		// Token: 0x040001E8 RID: 488
		[SerializeField]
		private TValue defaultValueIfNoEntry;

		// Token: 0x040001E9 RID: 489
		private readonly Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();

		// Token: 0x040001EA RID: 490
		[NonSerialized]
		private bool initialized;
	}
}
