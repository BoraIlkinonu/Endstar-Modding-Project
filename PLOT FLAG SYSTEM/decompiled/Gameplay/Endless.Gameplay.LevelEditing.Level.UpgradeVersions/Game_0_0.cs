using System;
using System.Collections.Generic;
using Endless.Assets;
using Endless.Shared.DataTypes;
using Newtonsoft.Json;
using UnityEngine;

namespace Endless.Gameplay.LevelEditing.Level.UpgradeVersions;

[Serializable]
public class Game_0_0 : Asset
{
	[Serializable]
	public class GameLibrary_0_0
	{
		[SerializeField]
		[JsonProperty]
		public List<TerrainUsage_0_0> terrainEntries = new List<TerrainUsage_0_0>();
	}

	[Serializable]
	public class TerrainUsage_0_0
	{
		[SerializeField]
		[JsonProperty("TerrainId")]
		public SerializableGuid terrainId;

		public string TerrainVersion;

		public bool IsActive;

		public int RedirectIndex;
	}

	public static readonly SemanticVersion INTERNAL_VERSION = new SemanticVersion(0, 0, 0);

	public List<LevelReference> levels = new List<LevelReference>();

	[JsonProperty]
	public GameLibrary_0_0 GameLibrary = new GameLibrary_0_0();

	[JsonProperty("screenshots")]
	public List<ScreenshotFileInstances> Screenshots = new List<ScreenshotFileInstances>();

	public int MininumNumberOfPlayers = 1;

	public int MaximumNumberOfPlayers = 10;
}
