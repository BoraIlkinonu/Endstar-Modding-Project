namespace Endless.Creator.UI;

public static class UIGameAssetTypesExtensions
{
	public static bool IsAudio(this UIGameAssetTypes gameAssetType)
	{
		if (gameAssetType != UIGameAssetTypes.SFX && gameAssetType != UIGameAssetTypes.Ambient)
		{
			return gameAssetType == UIGameAssetTypes.Music;
		}
		return true;
	}

	public static bool AnyAudioFlagsSet(this UIGameAssetTypes flags)
	{
		if ((flags & UIGameAssetTypes.SFX) != UIGameAssetTypes.SFX && (flags & UIGameAssetTypes.Ambient) != UIGameAssetTypes.Ambient)
		{
			return (flags & UIGameAssetTypes.Music) == UIGameAssetTypes.Music;
		}
		return true;
	}
}
