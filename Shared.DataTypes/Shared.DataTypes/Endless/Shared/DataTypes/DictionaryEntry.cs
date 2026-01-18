using System;

namespace Endless.Shared.DataTypes
{
	// Token: 0x0200000F RID: 15
	[Serializable]
	public struct DictionaryEntry<TKey, TValue>
	{
		// Token: 0x04000028 RID: 40
		public TKey Key;

		// Token: 0x04000029 RID: 41
		public TValue Value;
	}
}
