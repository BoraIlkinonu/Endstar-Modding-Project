using Endless.Gameplay.LevelEditing.Level;
using Newtonsoft.Json;
using Runtime.Gameplay.LevelEditing;

namespace Endless.Gameplay;

public class StaticLevelTemplateAsset
{
	[JsonProperty("levelState")]
	public readonly LevelState LevelState;

	[JsonProperty("gameLibrary")]
	public readonly GameLibrary GameLibrary;

	public StaticLevelTemplateAsset(LevelState levelState, GameLibrary gameLibrary)
	{
		LevelState = levelState;
		GameLibrary = gameLibrary;
	}
}
