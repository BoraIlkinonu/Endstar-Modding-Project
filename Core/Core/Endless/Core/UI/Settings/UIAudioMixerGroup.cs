using System;
using System.Collections.Generic;
using Endless.Gameplay;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;

namespace Endless.Core.UI.Settings
{
	// Token: 0x020000A7 RID: 167
	public class UIAudioMixerGroup : UIGameObject, IPoolableT
	{
		// Token: 0x1700005E RID: 94
		// (get) Token: 0x06000394 RID: 916 RVA: 0x00012BEE File Offset: 0x00010DEE
		// (set) Token: 0x06000395 RID: 917 RVA: 0x00012BF6 File Offset: 0x00010DF6
		public MonoBehaviour Prefab { get; set; }

		// Token: 0x1700005F RID: 95
		// (get) Token: 0x06000396 RID: 918 RVA: 0x000027B9 File Offset: 0x000009B9
		public bool IsUi
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000060 RID: 96
		// (get) Token: 0x06000397 RID: 919 RVA: 0x00012BFF File Offset: 0x00010DFF
		private AudioMixer AudioMixer
		{
			get
			{
				return this.audioMixerGroup.audioMixer;
			}
		}

		// Token: 0x17000061 RID: 97
		// (get) Token: 0x06000398 RID: 920 RVA: 0x00012C0C File Offset: 0x00010E0C
		// (set) Token: 0x06000399 RID: 921 RVA: 0x00012C80 File Offset: 0x00010E80
		private float Decibels
		{
			get
			{
				string text = this.audioMixerGroup.VolumePropertyName();
				float num;
				this.AudioMixer.GetFloat(text, out num);
				if (this.verboseLogging)
				{
					float num2 = AudioUtility.DecibelToVolume(num);
					DebugUtility.Log(string.Format("{0} is {1}: {2}, {3}: {4}", new object[] { text, "decibels", num, "volume", num2 }), null);
				}
				return num;
			}
			set
			{
				string text = this.audioMixerGroup.VolumePropertyName();
				this.AudioMixer.SetFloat(text, value);
				if (this.verboseLogging)
				{
					float num = AudioUtility.DecibelToVolume(value);
					DebugUtility.Log(string.Format("{0} set to {1}: {2}, {3}: {4}", new object[] { text, "decibels", value, "volume", num }), null);
				}
			}
		}

		// Token: 0x17000062 RID: 98
		// (get) Token: 0x0600039A RID: 922 RVA: 0x00012CF4 File Offset: 0x00010EF4
		// (set) Token: 0x0600039B RID: 923 RVA: 0x00012D04 File Offset: 0x00010F04
		private float Volume
		{
			get
			{
				return AudioUtility.DecibelToVolume(this.Decibels);
			}
			set
			{
				float num = AudioUtility.VolumeToDecibel(value);
				this.Decibels = num;
			}
		}

		// Token: 0x0600039C RID: 924 RVA: 0x00012D1F File Offset: 0x00010F1F
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.slider.onValueChanged.AddListener(new UnityAction<float>(this.SetVolume));
		}

		// Token: 0x0600039D RID: 925 RVA: 0x00012D55 File Offset: 0x00010F55
		public void OnSpawn()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnSpawn", Array.Empty<object>());
			}
		}

		// Token: 0x0600039E RID: 926 RVA: 0x00012D6F File Offset: 0x00010F6F
		public void OnDespawn()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDespawn", Array.Empty<object>());
			}
		}

		// Token: 0x0600039F RID: 927 RVA: 0x00012D8C File Offset: 0x00010F8C
		public void Initialize(AudioMixerGroup audioMixerGroup, SerializableDictionary<AudioMixerGroup, List<AudioMixerGroup>> audioMixerGroupParentDictionary)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Initialize", new object[] { audioMixerGroup.name, audioMixerGroupParentDictionary.Length });
			}
			this.audioMixerGroup = audioMixerGroup;
			if (audioMixerGroupParentDictionary.Contains(audioMixerGroup))
			{
				float num = (float)audioMixerGroupParentDictionary[audioMixerGroup].Count * 50f;
				this.labeledSliderContainer.SetHorizontalPadding(num, 0f);
			}
			string text = audioMixerGroup.VolumePropertyName();
			if (PlayerPrefs.HasKey(text))
			{
				this.Volume = PlayerPrefs.GetFloat(text) / 100f;
			}
			this.nameText.text = ((audioMixerGroup.name == "Master") ? this.overrideMasterName : audioMixerGroup.name);
			if (this.verboseLogging)
			{
				DebugUtility.Log(string.Format("{0}: {1}", "Volume", this.Volume), this);
			}
			this.slider.SetValue(this.Volume * 100f, true);
		}

		// Token: 0x060003A0 RID: 928 RVA: 0x00012E8C File Offset: 0x0001108C
		private void SetVolume(float volume)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetVolume", new object[] { volume });
			}
			string text = this.audioMixerGroup.VolumePropertyName();
			if (this.verboseLogging)
			{
				DebugUtility.Log("propertyName: " + text, this);
			}
			PlayerPrefs.SetFloat(text, volume);
			this.Volume = volume / 100f;
		}

		// Token: 0x0400029F RID: 671
		private const int MIN_DECIBELS = -80;

		// Token: 0x040002A0 RID: 672
		private const int MAX_DECIBELS = 0;

		// Token: 0x040002A1 RID: 673
		private const int PADDING_PER_DEPTH = 50;

		// Token: 0x040002A2 RID: 674
		[Tooltip("Is applied if AudioMixerGroup is 'Master'")]
		[SerializeField]
		private string overrideMasterName = "Master Volume";

		// Token: 0x040002A3 RID: 675
		[SerializeField]
		private RectTransform labeledSliderContainer;

		// Token: 0x040002A4 RID: 676
		[SerializeField]
		private TextMeshProUGUI nameText;

		// Token: 0x040002A5 RID: 677
		[SerializeField]
		private UISlider slider;

		// Token: 0x040002A6 RID: 678
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x040002A7 RID: 679
		private AudioMixerGroup audioMixerGroup;
	}
}
