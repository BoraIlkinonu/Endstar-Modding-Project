using System;
using System.Collections.Generic;
using Endless.Shared.Audio;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x0200010A RID: 266
	public class UiAudioManager : MonoBehaviourSingleton<UiAudioManager>
	{
		// Token: 0x1700010B RID: 267
		// (get) Token: 0x06000669 RID: 1641 RVA: 0x0001B8B4 File Offset: 0x00019AB4
		private Dictionary<NormalUiSoundType, AudioGroup> AudioGroupMap
		{
			get
			{
				if (this.audioGroupMap == null)
				{
					this.audioGroupMap = new Dictionary<NormalUiSoundType, AudioGroup>();
					foreach (UiAudioManager.UiAudioGroup uiAudioGroup in this.audioGroups)
					{
						this.audioGroupMap.Add(uiAudioGroup.SoundType, uiAudioGroup.AudioGroup);
					}
				}
				return this.audioGroupMap;
			}
		}

		// Token: 0x0600066A RID: 1642 RVA: 0x0001B90C File Offset: 0x00019B0C
		public void PlayUiAudio(NormalUiSoundType soundType)
		{
			if (ExitManager.IsQuitting)
			{
				return;
			}
			if (this.screenCover && this.screenCover.IsDisplaying)
			{
				return;
			}
			AudioGroup audioGroup;
			if (this.AudioGroupMap.TryGetValue(soundType, out audioGroup))
			{
				audioGroup.SpawnAndPlayWithManagedPool(this, this.audioSourcePrefab, default(Vector3), default(Quaternion));
				return;
			}
			Debug.LogError(string.Format("No audio settings for {0}. Please add an entry on Assets/Prefabs/UI/Shared/Managers/UiAudioManager.prefab to use this sound type", soundType));
		}

		// Token: 0x040003B0 RID: 944
		[SerializeField]
		private PoolableAudioSource audioSourcePrefab;

		// Token: 0x040003B1 RID: 945
		[SerializeField]
		private UiAudioManager.UiAudioGroup[] audioGroups;

		// Token: 0x040003B2 RID: 946
		[SerializeField]
		private UIScreenCover screenCover;

		// Token: 0x040003B3 RID: 947
		private Dictionary<NormalUiSoundType, AudioGroup> audioGroupMap;

		// Token: 0x0200010B RID: 267
		[Serializable]
		private class UiAudioGroup
		{
			// Token: 0x040003B4 RID: 948
			public NormalUiSoundType SoundType;

			// Token: 0x040003B5 RID: 949
			public AudioGroup AudioGroup;
		}
	}
}
