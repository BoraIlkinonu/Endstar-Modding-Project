using System;
using System.Collections.Generic;
using Endless.Assets;
using Newtonsoft.Json;

namespace Endless.Gameplay.LevelEditing.Level;

[Serializable]
public class LevelAsset : Asset
{
	[JsonProperty("screenshots")]
	public List<ScreenshotFileInstances> Screenshots { get; set; } = new List<ScreenshotFileInstances>();

	public bool Archived { get; set; }

	public static string QueryString => "game { levels { asset_id asset_version Name Description RevisionMetaData Archived screenshots { thumbnail mainImage } } }";

	public LevelAsset()
	{
		AssetType = "level";
		InternalVersion = LevelState.INTERNAL_VERSION.ToString();
	}

	public LevelAsset(LevelState levelState)
	{
		AssetType = "level";
		AssetID = levelState.AssetID;
		AssetVersion = levelState.AssetVersion;
		Name = levelState.Name;
		Description = levelState.Description;
		InternalVersion = LevelState.INTERNAL_VERSION.ToString();
	}

	public override object GetAnonymousObjectForUpload()
	{
		LevelState? levelState = JsonConvert.DeserializeObject<LevelState>(JsonConvert.SerializeObject(this));
		levelState.AssetVersion = string.Empty;
		return levelState;
	}
}
