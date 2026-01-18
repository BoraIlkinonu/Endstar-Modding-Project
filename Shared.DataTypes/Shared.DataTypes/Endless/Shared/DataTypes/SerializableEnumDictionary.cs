using System;
using System.Collections.Generic;
using UnityEngine;

namespace Endless.Shared.DataTypes
{
	// Token: 0x02000014 RID: 20
	[Serializable]
	public class SerializableEnumDictionary<TKey, TValue> : SerializableDictionary<TKey, TValue> where TKey : Enum
	{
		// Token: 0x06000091 RID: 145 RVA: 0x00003BDC File Offset: 0x00001DDC
		public void ValidateForMissingEnumKeys()
		{
			HashSet<TKey> keysHashSet = base.KeysHashSet;
			foreach (object obj in Enum.GetValues(typeof(TKey)))
			{
				TKey tkey = (TKey)((object)obj);
				if (!keysHashSet.Contains(tkey))
				{
					Debug.LogWarning(string.Format("There is no entry with a Key of {0}! If that is accessed, the {1} will be returned!", tkey, "DefaultValueIfNoEntry"));
				}
			}
		}
	}
}
