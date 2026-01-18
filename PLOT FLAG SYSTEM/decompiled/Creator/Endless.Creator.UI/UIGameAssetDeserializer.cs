using System;
using Endless.Props.Assets;
using Endless.Shared.Pagination;
using Endless.Shared.UI;
using Endless.TerrainCosmetics;

namespace Endless.Creator.UI;

public static class UIGameAssetDeserializer
{
	public static UIPageRequestResult<UIGameAsset> Deserialize(string json, UIGameAssetTypes assetType)
	{
		switch (assetType)
		{
		case UIGameAssetTypes.Terrain:
			return DeserializeTyped(json, (TerrainTilesetCosmeticAsset asset) => new UIGameAsset(asset));
		case UIGameAssetTypes.Prop:
			return DeserializeTyped(json, (Prop asset) => new UIGameAsset(asset));
		case UIGameAssetTypes.SFX:
		case UIGameAssetTypes.Ambient:
		case UIGameAssetTypes.Music:
			return DeserializeTyped(json, (AudioAsset asset) => new UIGameAsset(asset, assetType));
		default:
			throw new ArgumentOutOfRangeException("assetType", assetType, null);
		}
	}

	private static UIPageRequestResult<UIGameAsset> DeserializeTyped<TAsset>(string json, Func<TAsset, UIGameAsset> mapFunc)
	{
		UIPageRequestResult<TAsset> uIPageRequestResult = UIPageRequestResult<TAsset>.Parse(json);
		UIGameAsset[] items = Array.ConvertAll(uIPageRequestResult.Items ?? Array.Empty<TAsset>(), mapFunc.Invoke);
		Pagination pagination = uIPageRequestResult.Pagination;
		return new UIPageRequestResult<UIGameAsset>(items, pagination);
	}
}
