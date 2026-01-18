using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.Scripting;
using Endless.Shared;
using Endless.Shared.DataTypes;

namespace Endless.Gameplay;

[Serializable]
public class LevelDestination
{
	public SerializableGuid TargetLevelId = SerializableGuid.Empty;

	public List<SerializableGuid> TargetSpawnPointIds = new List<SerializableGuid>();

	public void ChangeLevel(Context context)
	{
		MonoBehaviourSingleton<GameplayManager>.Instance.ChangeLevel(this);
	}

	public bool IsValidLevel()
	{
		if (!TargetLevelId.IsEmpty)
		{
			return MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame.levels.Any((LevelReference level) => (SerializableGuid)level.AssetID == TargetLevelId);
		}
		return false;
	}

	public override string ToString()
	{
		return $"Targeting level {TargetLevelId} and {TargetSpawnPointIds.Count} spawn points";
	}
}
