using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;

namespace Unity.Cloud.UserReporting.Plugin.SimpleJson
{
	// Token: 0x02000028 RID: 40
	[GeneratedCode("simple-json", "1.0.0")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class JsonArray : List<object>
	{
		// Token: 0x0600011F RID: 287 RVA: 0x00006305 File Offset: 0x00004505
		public JsonArray()
		{
		}

		// Token: 0x06000120 RID: 288 RVA: 0x0000630D File Offset: 0x0000450D
		public JsonArray(int capacity)
			: base(capacity)
		{
		}

		// Token: 0x06000121 RID: 289 RVA: 0x00006316 File Offset: 0x00004516
		public override string ToString()
		{
			return SimpleJson.SerializeObject(this) ?? string.Empty;
		}
	}
}
