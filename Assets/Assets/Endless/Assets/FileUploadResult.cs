using System;

namespace Endless.Assets
{
	// Token: 0x0200000A RID: 10
	public class FileUploadResult
	{
		// Token: 0x17000006 RID: 6
		// (get) Token: 0x0600001C RID: 28 RVA: 0x0000236D File Offset: 0x0000056D
		public bool HasErrors
		{
			get
			{
				return this.Exception != null;
			}
		}

		// Token: 0x0400004E RID: 78
		public int FileInstanceId = -1;

		// Token: 0x0400004F RID: 79
		public Exception Exception;
	}
}
