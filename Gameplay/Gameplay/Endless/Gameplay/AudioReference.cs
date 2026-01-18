using System;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.DataTypes;

namespace Endless.Gameplay
{
	// Token: 0x020000F1 RID: 241
	[Serializable]
	public class AudioReference : AssetLibraryReferenceClass
	{
		// Token: 0x06000555 RID: 1365 RVA: 0x0001B5CC File Offset: 0x000197CC
		public float GetDuration()
		{
			RuntimeAudioInfo runtimeAudioInfo;
			if (this.Id != SerializableGuid.Empty && MonoBehaviourSingleton<StageManager>.Instance && MonoBehaviourSingleton<StageManager>.Instance.ActiveAudioLibrary.TryGetRuntimeAudioInfo(this.Id, out runtimeAudioInfo))
			{
				return runtimeAudioInfo.AudioAsset.Duration;
			}
			return -1f;
		}
	}
}
