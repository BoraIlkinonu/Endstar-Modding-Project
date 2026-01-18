using System;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.DataTypes;

namespace Endless.Gameplay;

[Serializable]
public class AudioReference : AssetLibraryReferenceClass
{
	public float GetDuration()
	{
		if (Id != SerializableGuid.Empty && (bool)MonoBehaviourSingleton<StageManager>.Instance && MonoBehaviourSingleton<StageManager>.Instance.ActiveAudioLibrary.TryGetRuntimeAudioInfo(Id, out var metadata))
		{
			return metadata.AudioAsset.Duration;
		}
		return -1f;
	}
}
