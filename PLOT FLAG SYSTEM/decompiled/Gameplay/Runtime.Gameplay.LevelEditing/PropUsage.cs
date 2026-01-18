using System;
using Endless.Assets;
using Newtonsoft.Json;

namespace Runtime.Gameplay.LevelEditing;

[Serializable]
public class PropUsage
{
	[JsonProperty("asset_ref_id")]
	public string AssetID;

	[JsonProperty("asset_ref_version")]
	public string AssetVersion;

	[JsonProperty("asset_type")]
	public string AssetType = "Prop";

	public static implicit operator AssetReference(PropUsage propUsage)
	{
		return new AssetReference
		{
			AssetID = propUsage.AssetID,
			AssetType = propUsage.AssetType,
			AssetVersion = propUsage.AssetVersion,
			UpdateParentVersion = false
		};
	}
}
