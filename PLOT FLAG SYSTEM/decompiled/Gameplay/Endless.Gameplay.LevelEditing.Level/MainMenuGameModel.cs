using System;
using System.Collections.Generic;
using Endless.Assets;
using Newtonsoft.Json;

namespace Endless.Gameplay.LevelEditing.Level;

[Serializable]
public class MainMenuGameModel : Asset
{
	public static string AssetReturnArgs => "{ asset_id asset_version Name Description levels { asset_ref_id asset_ref_version } screenshots { thumbnail mainImage } revision_meta_data }";

	[JsonProperty("levels")]
	public List<AssetReference> Levels { get; private set; }

	[JsonProperty("screenshots")]
	public List<ScreenshotFileInstances> Screenshots { get; private set; } = new List<ScreenshotFileInstances>();
}
