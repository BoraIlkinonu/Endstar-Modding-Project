using System;
using System.Collections;
using System.Collections.Generic;

// Token: 0x02000004 RID: 4
public class PriorityQueue<TPriority, TValue> : ICollection<KeyValuePair<TPriority, TValue>>, IEnumerable<KeyValuePair<TPriority, TValue>>, IEnumerable
{
	// Token: 0x06000034 RID: 52 RVA: 0x0000401D File Offset: 0x0000221D
	public PriorityQueue()
		: this(Comparer<TPriority>.Default)
	{
	}

	// Token: 0x06000035 RID: 53 RVA: 0x0000402A File Offset: 0x0000222A
	public PriorityQueue(IComparer<TPriority> comparer)
	{
		this.baseHeap = new List<KeyValuePair<TPriority, TValue>>();
		if (comparer == null)
		{
			throw new ArgumentNullException();
		}
		this.comparer = comparer;
	}

	// Token: 0x06000036 RID: 54 RVA: 0x0000404E File Offset: 0x0000224E
	public void Enqueue(TPriority priority, TValue value)
	{
		this.Insert(priority, value);
	}

	// Token: 0x06000037 RID: 55 RVA: 0x00004058 File Offset: 0x00002258
	private void Insert(TPriority priority, TValue value)
	{
		KeyValuePair<TPriority, TValue> keyValuePair = new KeyValuePair<TPriority, TValue>(priority, value);
		this.baseHeap.Add(keyValuePair);
		this.HeapifyFromEndToBeginning(this.baseHeap.Count - 1);
	}

	// Token: 0x06000038 RID: 56 RVA: 0x00004090 File Offset: 0x00002290
	private int HeapifyFromEndToBeginning(int pos)
	{
		if (pos >= this.baseHeap.Count)
		{
			return -1;
		}
		while (pos > 0)
		{
			int num = (pos - 1) / 2;
			if (this.comparer.Compare(this.baseHeap[num].Key, this.baseHeap[pos].Key) <= 0)
			{
				break;
			}
			this.ExchangeElements(num, pos);
			pos = num;
		}
		return pos;
	}

	// Token: 0x06000039 RID: 57 RVA: 0x000040FC File Offset: 0x000022FC
	private void ExchangeElements(int pos1, int pos2)
	{
		List<KeyValuePair<TPriority, TValue>> list = this.baseHeap;
		List<KeyValuePair<TPriority, TValue>> list2 = this.baseHeap;
		KeyValuePair<TPriority, TValue> keyValuePair = this.baseHeap[pos2];
		KeyValuePair<TPriority, TValue> keyValuePair2 = this.baseHeap[pos1];
		list[pos1] = keyValuePair;
		list2[pos2] = keyValuePair2;
	}

	// Token: 0x0600003A RID: 58 RVA: 0x0000414C File Offset: 0x0000234C
	public TValue DequeueValue()
	{
		return this.Dequeue().Value;
	}

	// Token: 0x0600003B RID: 59 RVA: 0x00004167 File Offset: 0x00002367
	public KeyValuePair<TPriority, TValue> Dequeue()
	{
		if (this.IsEmpty)
		{
			throw new InvalidOperationException("Priority queue is empty");
		}
		KeyValuePair<TPriority, TValue> keyValuePair = this.baseHeap[0];
		this.DeleteRoot();
		return keyValuePair;
	}

	// Token: 0x0600003C RID: 60 RVA: 0x00004190 File Offset: 0x00002390
	private void DeleteRoot()
	{
		if (this.baseHeap.Count <= 1)
		{
			this.baseHeap.Clear();
			return;
		}
		List<KeyValuePair<TPriority, TValue>> list = this.baseHeap;
		int num = 0;
		List<KeyValuePair<TPriority, TValue>> list2 = this.baseHeap;
		list[num] = list2[list2.Count - 1];
		this.baseHeap.RemoveAt(this.baseHeap.Count - 1);
		this.HeapifyFromBeginningToEnd(0);
	}

	// Token: 0x0600003D RID: 61 RVA: 0x000041F8 File Offset: 0x000023F8
	private void HeapifyFromBeginningToEnd(int pos)
	{
		if (pos >= this.baseHeap.Count)
		{
			return;
		}
		for (;;)
		{
			int num = pos;
			int num2 = 2 * pos + 1;
			int num3 = 2 * pos + 2;
			if (num2 < this.baseHeap.Count && this.comparer.Compare(this.baseHeap[num].Key, this.baseHeap[num2].Key) > 0)
			{
				num = num2;
			}
			if (num3 < this.baseHeap.Count && this.comparer.Compare(this.baseHeap[num].Key, this.baseHeap[num3].Key) > 0)
			{
				num = num3;
			}
			if (num == pos)
			{
				break;
			}
			this.ExchangeElements(num, pos);
			pos = num;
		}
	}

	// Token: 0x0600003E RID: 62 RVA: 0x000042C2 File Offset: 0x000024C2
	public KeyValuePair<TPriority, TValue> Peek()
	{
		if (!this.IsEmpty)
		{
			return this.baseHeap[0];
		}
		throw new InvalidOperationException("Priority queue is empty");
	}

	// Token: 0x0600003F RID: 63 RVA: 0x000042E4 File Offset: 0x000024E4
	public TValue PeekValue()
	{
		return this.Peek().Value;
	}

	// Token: 0x17000002 RID: 2
	// (get) Token: 0x06000040 RID: 64 RVA: 0x000042FF File Offset: 0x000024FF
	public bool IsEmpty
	{
		get
		{
			return this.baseHeap.Count == 0;
		}
	}

	// Token: 0x06000041 RID: 65 RVA: 0x0000430F File Offset: 0x0000250F
	public void Add(KeyValuePair<TPriority, TValue> item)
	{
		this.Enqueue(item.Key, item.Value);
	}

	// Token: 0x06000042 RID: 66 RVA: 0x00004325 File Offset: 0x00002525
	public void Clear()
	{
		this.baseHeap.Clear();
	}

	// Token: 0x06000043 RID: 67 RVA: 0x00004332 File Offset: 0x00002532
	public bool Contains(KeyValuePair<TPriority, TValue> item)
	{
		return this.baseHeap.Contains(item);
	}

	// Token: 0x06000044 RID: 68 RVA: 0x00004340 File Offset: 0x00002540
	public void CopyTo(KeyValuePair<TPriority, TValue>[] array, int arrayIndex)
	{
		this.baseHeap.CopyTo(array, arrayIndex);
	}

	// Token: 0x06000045 RID: 69 RVA: 0x00004350 File Offset: 0x00002550
	public bool Remove(KeyValuePair<TPriority, TValue> item)
	{
		int num = this.baseHeap.IndexOf(item);
		if (num < 0)
		{
			return false;
		}
		List<KeyValuePair<TPriority, TValue>> list = this.baseHeap;
		int num2 = num;
		List<KeyValuePair<TPriority, TValue>> list2 = this.baseHeap;
		list[num2] = list2[list2.Count - 1];
		this.baseHeap.RemoveAt(this.baseHeap.Count - 1);
		if (this.HeapifyFromEndToBeginning(num) == num)
		{
			this.HeapifyFromBeginningToEnd(num);
		}
		return true;
	}

	// Token: 0x17000003 RID: 3
	// (get) Token: 0x06000046 RID: 70 RVA: 0x000043B9 File Offset: 0x000025B9
	public int Count
	{
		get
		{
			return this.baseHeap.Count;
		}
	}

	// Token: 0x17000004 RID: 4
	// (get) Token: 0x06000047 RID: 71 RVA: 0x000043C6 File Offset: 0x000025C6
	public bool IsReadOnly
	{
		get
		{
			return false;
		}
	}

	// Token: 0x06000048 RID: 72 RVA: 0x000043C9 File Offset: 0x000025C9
	public IEnumerator<KeyValuePair<TPriority, TValue>> GetEnumerator()
	{
		return this.baseHeap.GetEnumerator();
	}

	// Token: 0x06000049 RID: 73 RVA: 0x000043DB File Offset: 0x000025DB
	IEnumerator IEnumerable.GetEnumerator()
	{
		return this.GetEnumerator();
	}

	// Token: 0x04000002 RID: 2
	private List<KeyValuePair<TPriority, TValue>> baseHeap;

	// Token: 0x04000003 RID: 3
	private IComparer<TPriority> comparer;
}
