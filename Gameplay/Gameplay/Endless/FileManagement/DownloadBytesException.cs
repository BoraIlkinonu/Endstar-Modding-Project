using System;

namespace Endless.FileManagement
{
	// Token: 0x02000041 RID: 65
	public class DownloadBytesException : Exception
	{
		// Token: 0x06000115 RID: 277 RVA: 0x00005AFC File Offset: 0x00003CFC
		public DownloadBytesException(string message, Exception innerException = null)
			: base(message, innerException)
		{
		}
	}
}
