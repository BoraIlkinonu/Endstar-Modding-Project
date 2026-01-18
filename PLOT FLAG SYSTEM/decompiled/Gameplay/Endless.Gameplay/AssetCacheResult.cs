using Endless.Assets;
using Endless.GraphQl;

namespace Endless.Gameplay;

public class AssetCacheResult<T> : AssetCacheResultCore<T> where T : Asset
{
	public readonly T Asset;

	public AssetCacheResult(GraphQlResult result, T asset)
	{
		Result = result;
		Asset = asset;
	}
}
