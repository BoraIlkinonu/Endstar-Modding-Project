using System.Collections.Generic;
using Endless.Assets;
using Endless.GraphQl;

namespace Endless.Gameplay;

public class BulkAssetCacheResult<T> : AssetCacheResultCore<T> where T : Asset
{
	public readonly List<T> Assets;

	public BulkAssetCacheResult(GraphQlResult result, List<T> assets)
	{
		Result = result;
		Assets = assets;
	}
}
