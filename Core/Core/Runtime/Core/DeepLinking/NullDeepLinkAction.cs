using System;

namespace Runtime.Core.DeepLinking
{
	// Token: 0x02000015 RID: 21
	public class NullDeepLinkAction : DeepLinkAction
	{
		// Token: 0x06000050 RID: 80 RVA: 0x00003CF2 File Offset: 0x00001EF2
		public override bool Parse(string argString)
		{
			return false;
		}
	}
}
