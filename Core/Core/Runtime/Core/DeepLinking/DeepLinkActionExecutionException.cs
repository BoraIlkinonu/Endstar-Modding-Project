using System;
using Endless.Data;

namespace Runtime.Core.DeepLinking
{
	// Token: 0x0200000E RID: 14
	public class DeepLinkActionExecutionException : Exception
	{
		// Token: 0x17000007 RID: 7
		// (get) Token: 0x0600003B RID: 59 RVA: 0x0000378E File Offset: 0x0000198E
		public ErrorCodes ErrorCode { get; }

		// Token: 0x0600003C RID: 60 RVA: 0x00003796 File Offset: 0x00001996
		public DeepLinkActionExecutionException(ErrorCodes errorCode)
			: base("DeepLinkActionExecutionException")
		{
			this.ErrorCode = errorCode;
		}

		// Token: 0x0600003D RID: 61 RVA: 0x000037AA File Offset: 0x000019AA
		public DeepLinkActionExecutionException(ErrorCodes errorCode, Exception inner)
			: base("DeepLinkActionExecutionException", inner)
		{
			this.ErrorCode = errorCode;
		}
	}
}
