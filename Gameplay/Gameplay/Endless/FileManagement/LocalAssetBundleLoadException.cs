using System;

namespace Endless.FileManagement
{
	// Token: 0x02000040 RID: 64
	public class LocalAssetBundleLoadException : Exception
	{
		// Token: 0x06000114 RID: 276 RVA: 0x00005AFC File Offset: 0x00003CFC
		public LocalAssetBundleLoadException(string message, Exception innerException = null)
			: base(message, innerException)
		{
		}
	}
}
