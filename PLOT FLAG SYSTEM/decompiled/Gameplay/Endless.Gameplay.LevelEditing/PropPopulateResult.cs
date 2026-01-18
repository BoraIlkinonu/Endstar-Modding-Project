using System.Collections.Generic;
using Endless.Gameplay.LevelEditing.Tilesets;

namespace Endless.Gameplay.LevelEditing;

public class PropPopulateResult
{
	public List<AssetIdVersionKey> ModifiedProps { get; }

	public PropPopulateResult(List<AssetIdVersionKey> modifiedProps)
	{
		ModifiedProps = modifiedProps;
	}
}
