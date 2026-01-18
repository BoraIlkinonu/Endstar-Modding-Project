using System;

namespace Endless.FileManagement;

public class DownloadBytesException : Exception
{
	public DownloadBytesException(string message, Exception innerException = null)
		: base(message, innerException)
	{
	}
}
