using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace Unity.Cloud.UserReporting.Plugin.SimpleJson
{
	// Token: 0x02000029 RID: 41
	[GeneratedCode("simple-json", "1.0.0")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class JsonObject : IDictionary<string, object>, ICollection<KeyValuePair<string, object>>, IEnumerable<KeyValuePair<string, object>>, IEnumerable
	{
		// Token: 0x06000122 RID: 290 RVA: 0x00006327 File Offset: 0x00004527
		public JsonObject()
		{
			this._members = new Dictionary<string, object>();
		}

		// Token: 0x06000123 RID: 291 RVA: 0x0000633A File Offset: 0x0000453A
		public JsonObject(IEqualityComparer<string> comparer)
		{
			this._members = new Dictionary<string, object>(comparer);
		}

		// Token: 0x1700004E RID: 78
		public object this[int index]
		{
			get
			{
				return JsonObject.GetAtIndex(this._members, index);
			}
		}

		// Token: 0x06000125 RID: 293 RVA: 0x0000635C File Offset: 0x0000455C
		internal static object GetAtIndex(IDictionary<string, object> obj, int index)
		{
			if (obj == null)
			{
				throw new ArgumentNullException("obj");
			}
			if (index >= obj.Count)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			int num = 0;
			foreach (KeyValuePair<string, object> keyValuePair in obj)
			{
				if (num++ == index)
				{
					return keyValuePair.Value;
				}
			}
			return null;
		}

		// Token: 0x06000126 RID: 294 RVA: 0x000063D8 File Offset: 0x000045D8
		public void Add(string key, object value)
		{
			this._members.Add(key, value);
		}

		// Token: 0x06000127 RID: 295 RVA: 0x000063E7 File Offset: 0x000045E7
		public bool ContainsKey(string key)
		{
			return this._members.ContainsKey(key);
		}

		// Token: 0x1700004F RID: 79
		// (get) Token: 0x06000128 RID: 296 RVA: 0x000063F5 File Offset: 0x000045F5
		public ICollection<string> Keys
		{
			get
			{
				return this._members.Keys;
			}
		}

		// Token: 0x06000129 RID: 297 RVA: 0x00006402 File Offset: 0x00004602
		public bool Remove(string key)
		{
			return this._members.Remove(key);
		}

		// Token: 0x0600012A RID: 298 RVA: 0x00006410 File Offset: 0x00004610
		public bool TryGetValue(string key, out object value)
		{
			return this._members.TryGetValue(key, out value);
		}

		// Token: 0x17000050 RID: 80
		// (get) Token: 0x0600012B RID: 299 RVA: 0x0000641F File Offset: 0x0000461F
		public ICollection<object> Values
		{
			get
			{
				return this._members.Values;
			}
		}

		// Token: 0x17000051 RID: 81
		public object this[string key]
		{
			get
			{
				return this._members[key];
			}
			set
			{
				this._members[key] = value;
			}
		}

		// Token: 0x0600012E RID: 302 RVA: 0x00006449 File Offset: 0x00004649
		public void Add(KeyValuePair<string, object> item)
		{
			this._members.Add(item.Key, item.Value);
		}

		// Token: 0x0600012F RID: 303 RVA: 0x00006464 File Offset: 0x00004664
		public void Clear()
		{
			this._members.Clear();
		}

		// Token: 0x06000130 RID: 304 RVA: 0x00006471 File Offset: 0x00004671
		public bool Contains(KeyValuePair<string, object> item)
		{
			return this._members.ContainsKey(item.Key) && this._members[item.Key] == item.Value;
		}

		// Token: 0x06000131 RID: 305 RVA: 0x000064A4 File Offset: 0x000046A4
		public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			int num = this.Count;
			foreach (KeyValuePair<string, object> keyValuePair in this)
			{
				array[arrayIndex++] = keyValuePair;
				if (--num <= 0)
				{
					break;
				}
			}
		}

		// Token: 0x17000052 RID: 82
		// (get) Token: 0x06000132 RID: 306 RVA: 0x00006514 File Offset: 0x00004714
		public int Count
		{
			get
			{
				return this._members.Count;
			}
		}

		// Token: 0x17000053 RID: 83
		// (get) Token: 0x06000133 RID: 307 RVA: 0x00006521 File Offset: 0x00004721
		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06000134 RID: 308 RVA: 0x00006524 File Offset: 0x00004724
		public bool Remove(KeyValuePair<string, object> item)
		{
			return this._members.Remove(item.Key);
		}

		// Token: 0x06000135 RID: 309 RVA: 0x00006538 File Offset: 0x00004738
		public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
		{
			return this._members.GetEnumerator();
		}

		// Token: 0x06000136 RID: 310 RVA: 0x0000654A File Offset: 0x0000474A
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this._members.GetEnumerator();
		}

		// Token: 0x06000137 RID: 311 RVA: 0x0000655C File Offset: 0x0000475C
		public override string ToString()
		{
			return SimpleJson.SerializeObject(this);
		}

		// Token: 0x04000091 RID: 145
		private readonly Dictionary<string, object> _members;
	}
}
