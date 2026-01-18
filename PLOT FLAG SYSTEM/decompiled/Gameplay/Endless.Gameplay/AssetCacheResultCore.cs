using System;
using Endless.Assets;
using Endless.GraphQl;

namespace Endless.Gameplay;

public abstract class AssetCacheResultCore<T> where T : Asset
{
	public GraphQlResult Result;

	public bool HasErrors => Result?.HasErrors ?? false;

	public Exception GetErrorMessage()
	{
		return Result.GetErrorMessage();
	}
}
