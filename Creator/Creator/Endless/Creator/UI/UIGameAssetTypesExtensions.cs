using System;

namespace Endless.Creator.UI
{
	// Token: 0x020000C1 RID: 193
	public static class UIGameAssetTypesExtensions
	{
		// Token: 0x06000309 RID: 777 RVA: 0x00013581 File Offset: 0x00011781
		public static bool IsAudio(this UIGameAssetTypes gameAssetType)
		{
			return gameAssetType == UIGameAssetTypes.SFX || gameAssetType == UIGameAssetTypes.Ambient || gameAssetType == UIGameAssetTypes.Music;
		}

		// Token: 0x0600030A RID: 778 RVA: 0x00013592 File Offset: 0x00011792
		public static bool AnyAudioFlagsSet(this UIGameAssetTypes flags)
		{
			return (flags & UIGameAssetTypes.SFX) == UIGameAssetTypes.SFX || (flags & UIGameAssetTypes.Ambient) == UIGameAssetTypes.Ambient || (flags & UIGameAssetTypes.Music) == UIGameAssetTypes.Music;
		}
	}
}
