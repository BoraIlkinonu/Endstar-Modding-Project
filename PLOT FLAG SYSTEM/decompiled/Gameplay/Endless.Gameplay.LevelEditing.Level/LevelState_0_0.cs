using System;
using System.Collections.Generic;
using Endless.Shared.DataTypes;
using Newtonsoft.Json;

namespace Endless.Gameplay.LevelEditing.Level;

[Serializable]
public class LevelState_0_0 : LevelAsset
{
	[JsonProperty]
	public List<PropEntry> propEntries = new List<PropEntry>();

	[JsonProperty]
	public List<TerrainEntry> terrainEntries = new List<TerrainEntry>();

	[JsonProperty]
	public List<WireBundle> wireBundles = new List<WireBundle>();

	[JsonProperty]
	public List<SerializableGuid> spawnPointIds = new List<SerializableGuid>();

	[JsonProperty]
	public List<SerializableGuid> selectedSpawnPointIds = new List<SerializableGuid>();

	[JsonProperty]
	public List<int> ScreenshotFileInstanceIds = new List<int>();

	[JsonProperty]
	public SerializableGuid defaultEnvironmentInstanceId = SerializableGuid.Empty;

	[JsonProperty("asset_need_update_parent_version")]
	public bool UpdateParentVersion => true;
}
