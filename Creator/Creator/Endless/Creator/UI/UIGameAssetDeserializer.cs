using System;
using Endless.Props.Assets;
using Endless.Shared.Pagination;
using Endless.Shared.UI;
using Endless.TerrainCosmetics;

namespace Endless.Creator.UI
{
	// Token: 0x02000105 RID: 261
	public static class UIGameAssetDeserializer
	{
		// Token: 0x06000433 RID: 1075 RVA: 0x00019910 File Offset: 0x00017B10
		public static UIPageRequestResult<UIGameAsset> Deserialize(string json, UIGameAssetTypes assetType)
		{
			UIGameAssetTypes assetType2 = assetType;
			switch (assetType2)
			{
			case UIGameAssetTypes.Terrain:
				return UIGameAssetDeserializer.DeserializeTyped<TerrainTilesetCosmeticAsset>(json, (TerrainTilesetCosmeticAsset asset) => new UIGameAsset(asset));
			case UIGameAssetTypes.Prop:
				return UIGameAssetDeserializer.DeserializeTyped<Prop>(json, (Prop asset) => new UIGameAsset(asset));
			case UIGameAssetTypes.Terrain | UIGameAssetTypes.Prop:
				goto IL_0096;
			case UIGameAssetTypes.SFX:
				break;
			default:
				if (assetType2 != UIGameAssetTypes.Ambient && assetType2 != UIGameAssetTypes.Music)
				{
					goto IL_0096;
				}
				break;
			}
			return UIGameAssetDeserializer.DeserializeTyped<AudioAsset>(json, (AudioAsset asset) => new UIGameAsset(asset, assetType));
			IL_0096:
			throw new ArgumentOutOfRangeException("assetType", assetType, null);
		}

		// Token: 0x06000434 RID: 1076 RVA: 0x000199CC File Offset: 0x00017BCC
		private static UIPageRequestResult<UIGameAsset> DeserializeTyped<TAsset>(string json, Func<TAsset, UIGameAsset> mapFunc)
		{
			UIPageRequestResult<TAsset> uipageRequestResult = UIPageRequestResult<TAsset>.Parse(json);
			UIGameAsset[] array = Array.ConvertAll<TAsset, UIGameAsset>(uipageRequestResult.Items ?? Array.Empty<TAsset>(), new Converter<TAsset, UIGameAsset>(mapFunc.Invoke));
			Pagination pagination = uipageRequestResult.Pagination;
			return new UIPageRequestResult<UIGameAsset>(array, pagination);
		}
	}
}
