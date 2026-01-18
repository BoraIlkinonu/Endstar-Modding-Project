using System;

namespace Endless.Props.Assets
{
	// Token: 0x0200003B RID: 59
	[Serializable]
	public class StringPair
	{
		// Token: 0x060000EA RID: 234 RVA: 0x0000345B File Offset: 0x0000165B
		public StringPair(string key, string value)
		{
			this.Key = key;
			this.Value = value;
		}

		// Token: 0x0400009D RID: 157
		public string Key;

		// Token: 0x0400009E RID: 158
		public string Value;
	}
}
