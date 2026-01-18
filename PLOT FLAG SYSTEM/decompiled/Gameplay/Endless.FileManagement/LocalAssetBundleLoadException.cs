using System;

namespace Endless.FileManagement;

public class LocalAssetBundleLoadException : Exception
{
	public LocalAssetBundleLoadException(string message, Exception innerException = null)
		: base(message, innerException)
	{
	}
}
