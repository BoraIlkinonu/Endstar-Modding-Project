using System;
using System.Collections.Generic;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Audio;

namespace Endless.Core.UI.Settings
{
	// Token: 0x020000A8 RID: 168
	public class UIAudioSettingsView : UIGameObject
	{
		// Token: 0x060003A2 RID: 930 RVA: 0x00012F08 File Offset: 0x00011108
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			HashSet<AudioMixerGroup> hashSet = new HashSet<AudioMixerGroup>(this.audioMixerGroupsToIgnore.Value);
			foreach (AudioMixerGroup audioMixerGroup in this.audioMixer.FindMatchingGroups(string.Empty))
			{
				if (!hashSet.Contains(audioMixerGroup))
				{
					if (this.verboseLogging)
					{
						DebugUtility.Log("Spawning audioMixerGroupSource for a AudioMixerGroup of " + audioMixerGroup.name, this);
					}
					PoolManagerT instance = MonoBehaviourSingleton<PoolManagerT>.Instance;
					UIAudioMixerGroup uiaudioMixerGroup = this.audioMixerGroupSource;
					Transform transform = this.audioMixerGroupContainer;
					instance.Spawn<UIAudioMixerGroup>(uiaudioMixerGroup, default(Vector3), default(Quaternion), transform).Initialize(audioMixerGroup, this.audioMixerGroupParentDictionary);
				}
			}
		}

		// Token: 0x040002A9 RID: 681
		[SerializeField]
		private AudioMixer audioMixer;

		// Token: 0x040002AA RID: 682
		[SerializeField]
		private UIAudioMixerGroup audioMixerGroupSource;

		// Token: 0x040002AB RID: 683
		[SerializeField]
		private RectTransform audioMixerGroupContainer;

		// Token: 0x040002AC RID: 684
		[SerializeField]
		private AudioMixerGroupArray audioMixerGroupsToIgnore;

		// Token: 0x040002AD RID: 685
		[SerializeField]
		private SerializableDictionary<AudioMixerGroup, List<AudioMixerGroup>> audioMixerGroupParentDictionary = new SerializableDictionary<AudioMixerGroup, List<AudioMixerGroup>>();

		// Token: 0x040002AE RID: 686
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
