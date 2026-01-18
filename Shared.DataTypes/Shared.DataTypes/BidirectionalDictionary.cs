using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Token: 0x02000002 RID: 2
public class BidirectionalDictionary<TForwardKey, TReverseKey> : IEnumerable<KeyValuePair<TForwardKey, TReverseKey>>, IEnumerable
{
	// Token: 0x17000001 RID: 1
	// (get) Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
	// (set) Token: 0x06000002 RID: 2 RVA: 0x00002058 File Offset: 0x00000258
	public BidirectionalDictionary<TForwardKey, TReverseKey>.Indexer<TForwardKey, TReverseKey> Forward { get; private set; } = new BidirectionalDictionary<TForwardKey, TReverseKey>.Indexer<TForwardKey, TReverseKey>();

	// Token: 0x17000002 RID: 2
	// (get) Token: 0x06000003 RID: 3 RVA: 0x00002061 File Offset: 0x00000261
	// (set) Token: 0x06000004 RID: 4 RVA: 0x00002069 File Offset: 0x00000269
	public BidirectionalDictionary<TForwardKey, TReverseKey>.Indexer<TReverseKey, TForwardKey> Reverse { get; private set; } = new BidirectionalDictionary<TForwardKey, TReverseKey>.Indexer<TReverseKey, TForwardKey>();

	// Token: 0x06000005 RID: 5 RVA: 0x00002072 File Offset: 0x00000272
	public BidirectionalDictionary()
	{
	}

	// Token: 0x06000006 RID: 6 RVA: 0x00002090 File Offset: 0x00000290
	public BidirectionalDictionary(IDictionary<TForwardKey, TReverseKey> oneWayMap)
	{
		this.Forward = new BidirectionalDictionary<TForwardKey, TReverseKey>.Indexer<TForwardKey, TReverseKey>(oneWayMap);
		Dictionary<TReverseKey, TForwardKey> dictionary = oneWayMap.ToDictionary((KeyValuePair<TForwardKey, TReverseKey> kvp) => kvp.Value, (KeyValuePair<TForwardKey, TReverseKey> kvp) => kvp.Key);
		this.Reverse = new BidirectionalDictionary<TForwardKey, TReverseKey>.Indexer<TReverseKey, TForwardKey>(dictionary);
	}

	// Token: 0x06000007 RID: 7 RVA: 0x00002118 File Offset: 0x00000318
	public void Add(TForwardKey t1, TReverseKey t2)
	{
		if (this.Forward.ContainsKey(t1))
		{
			throw new ArgumentException("", "t1");
		}
		if (this.Reverse.ContainsKey(t2))
		{
			throw new ArgumentException("", "t2");
		}
		this.Forward.Add(t1, t2);
		this.Reverse.Add(t2, t1);
	}

	// Token: 0x06000008 RID: 8 RVA: 0x0000217C File Offset: 0x0000037C
	public bool Remove(TForwardKey forwardKey)
	{
		if (!this.Forward.ContainsKey(forwardKey))
		{
			return false;
		}
		TReverseKey treverseKey = this.Forward[forwardKey];
		bool flag;
		if (this.Forward.Remove(forwardKey))
		{
			if (this.Reverse.Remove(treverseKey))
			{
				flag = true;
			}
			else
			{
				this.Forward.Add(forwardKey, treverseKey);
				flag = false;
			}
		}
		else
		{
			flag = false;
		}
		return flag;
	}

	// Token: 0x06000009 RID: 9 RVA: 0x000021DA File Offset: 0x000003DA
	public int Count()
	{
		return this.Forward.Count();
	}

	// Token: 0x0600000A RID: 10 RVA: 0x000021E7 File Offset: 0x000003E7
	IEnumerator<KeyValuePair<TForwardKey, TReverseKey>> IEnumerable<KeyValuePair<TForwardKey, TReverseKey>>.GetEnumerator()
	{
		return this.Forward.GetEnumerator();
	}

	// Token: 0x0600000B RID: 11 RVA: 0x000021E7 File Offset: 0x000003E7
	public IEnumerator GetEnumerator()
	{
		return this.Forward.GetEnumerator();
	}

	// Token: 0x04000003 RID: 3
	private const string DuplicateKeyErrorMessage = "";

	// Token: 0x02000003 RID: 3
	public class Indexer<Key, Value> : IEnumerable<KeyValuePair<Key, Value>>, IEnumerable
	{
		// Token: 0x0600000C RID: 12 RVA: 0x000021F4 File Offset: 0x000003F4
		public Indexer()
		{
			this._dictionary = new Dictionary<Key, Value>();
		}

		// Token: 0x0600000D RID: 13 RVA: 0x00002207 File Offset: 0x00000407
		public Indexer(IDictionary<Key, Value> dictionary)
		{
			this._dictionary = dictionary;
		}

		// Token: 0x17000003 RID: 3
		public Value this[Key index]
		{
			get
			{
				return this._dictionary[index];
			}
		}

		// Token: 0x0600000F RID: 15 RVA: 0x00002224 File Offset: 0x00000424
		public static implicit operator Dictionary<Key, Value>(BidirectionalDictionary<TForwardKey, TReverseKey>.Indexer<Key, Value> indexer)
		{
			return new Dictionary<Key, Value>(indexer._dictionary);
		}

		// Token: 0x06000010 RID: 16 RVA: 0x00002231 File Offset: 0x00000431
		internal void Add(Key key, Value value)
		{
			this._dictionary.Add(key, value);
		}

		// Token: 0x06000011 RID: 17 RVA: 0x00002240 File Offset: 0x00000440
		internal bool Remove(Key key)
		{
			return this._dictionary.Remove(key);
		}

		// Token: 0x06000012 RID: 18 RVA: 0x0000224E File Offset: 0x0000044E
		internal int Count()
		{
			return this._dictionary.Count;
		}

		// Token: 0x06000013 RID: 19 RVA: 0x0000225B File Offset: 0x0000045B
		public bool ContainsKey(Key key)
		{
			return this._dictionary.ContainsKey(key);
		}

		// Token: 0x17000004 RID: 4
		// (get) Token: 0x06000014 RID: 20 RVA: 0x00002269 File Offset: 0x00000469
		public IEnumerable<Key> Keys
		{
			get
			{
				return this._dictionary.Keys;
			}
		}

		// Token: 0x17000005 RID: 5
		// (get) Token: 0x06000015 RID: 21 RVA: 0x00002276 File Offset: 0x00000476
		public IEnumerable<Value> Values
		{
			get
			{
				return this._dictionary.Values;
			}
		}

		// Token: 0x06000016 RID: 22 RVA: 0x00002224 File Offset: 0x00000424
		public Dictionary<Key, Value> ToDictionary()
		{
			return new Dictionary<Key, Value>(this._dictionary);
		}

		// Token: 0x06000017 RID: 23 RVA: 0x00002283 File Offset: 0x00000483
		public bool TryGetValue(Key key, out Value value)
		{
			return this._dictionary.TryGetValue(key, out value);
		}

		// Token: 0x06000018 RID: 24 RVA: 0x00002292 File Offset: 0x00000492
		public IEnumerator<KeyValuePair<Key, Value>> GetEnumerator()
		{
			return this._dictionary.GetEnumerator();
		}

		// Token: 0x06000019 RID: 25 RVA: 0x00002292 File Offset: 0x00000492
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this._dictionary.GetEnumerator();
		}

		// Token: 0x04000004 RID: 4
		private IDictionary<Key, Value> _dictionary;
	}
}
