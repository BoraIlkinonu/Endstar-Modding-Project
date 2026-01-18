using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Endless.Props.Assets
{
	// Token: 0x0200003A RID: 58
	[Serializable]
	public struct StringPairDictionary
	{
		// Token: 0x060000E8 RID: 232 RVA: 0x000033BC File Offset: 0x000015BC
		public string GetEntry(string key)
		{
			List<StringPair> list = this.stringPairs;
			StringPair stringPair = ((list != null) ? list.FirstOrDefault((StringPair pair) => pair.Key == key) : null);
			if (stringPair != null)
			{
				return stringPair.Value;
			}
			return null;
		}

		// Token: 0x060000E9 RID: 233 RVA: 0x00003400 File Offset: 0x00001600
		public void SetEntry(string key, string value)
		{
			List<StringPair> list = this.stringPairs;
			StringPair stringPair = ((list != null) ? list.FirstOrDefault((StringPair pair) => pair.Key == key) : null);
			if (stringPair != null)
			{
				stringPair.Value = value;
				return;
			}
			this.stringPairs.Add(new StringPair(key, value));
		}

		// Token: 0x0400009C RID: 156
		[SerializeField]
		private List<StringPair> stringPairs;
	}
}
