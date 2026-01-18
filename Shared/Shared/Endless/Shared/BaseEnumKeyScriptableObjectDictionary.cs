using System;
using System.Collections.Generic;
using Endless.Shared.Debugging;
using UnityEngine;

namespace Endless.Shared
{
	// Token: 0x0200008A RID: 138
	public abstract class BaseEnumKeyScriptableObjectDictionary<TKey, TValue> : BaseScriptableObjectDictionary<TKey, TValue> where TKey : Enum
	{
		// Token: 0x060003E9 RID: 1001 RVA: 0x0001132F File Offset: 0x0000F52F
		public override void Validate()
		{
			base.Validate();
			if (this.validateForMissingEnumKeys)
			{
				this.ValidateForMissingEnumKeys();
			}
		}

		// Token: 0x060003EA RID: 1002 RVA: 0x00011348 File Offset: 0x0000F548
		private void ValidateForMissingEnumKeys()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("ValidateForMissingEnumKeys", this);
			}
			HashSet<TKey> keysHashSet = base.KeysHashSet;
			foreach (object obj in Enum.GetValues(typeof(TKey)))
			{
				TKey tkey = (TKey)((object)obj);
				if (!keysHashSet.Contains(tkey))
				{
					DebugUtility.LogWarning(string.Format("There is no entry with a Key of {0}! If that is accessed, the {1} will be returned!", tkey, "DefaultValueIfNoEntry"), this);
				}
			}
		}

		// Token: 0x040001E5 RID: 485
		[SerializeField]
		private bool validateForMissingEnumKeys;
	}
}
