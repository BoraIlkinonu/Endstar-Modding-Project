using System;
using Endless.Shared;

namespace Endless.Assets
{
	// Token: 0x02000008 RID: 8
	public class ChangeTypeAttribute : UserFacingTextAttribute
	{
		// Token: 0x0600001A RID: 26 RVA: 0x00002351 File Offset: 0x00000551
		public ChangeTypeAttribute(string userFacingText)
			: base(userFacingText)
		{
		}

		// Token: 0x0600001B RID: 27 RVA: 0x0000235A File Offset: 0x0000055A
		public string ResolveMetaString(string metaString)
		{
			return base.UserFacingText.Replace("%metastring%", metaString);
		}

		// Token: 0x04000012 RID: 18
		private const string METASTRING_KEY = "%metastring%";
	}
}
