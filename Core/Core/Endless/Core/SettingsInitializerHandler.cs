using System;
using Endless.Core.UI.Settings;
using Endless.Gameplay;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.EndlessQualitySettings;
using Endless.Shared.UI;
using Endless.Shared.Validation;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering.Universal;

namespace Endless.Core
{
	// Token: 0x0200002F RID: 47
	public class SettingsInitializerHandler : MonoBehaviour, IValidatable
	{
		// Token: 0x060000CD RID: 205 RVA: 0x00006268 File Offset: 0x00004468
		private void Awake()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Awake", Array.Empty<object>());
			}
			this.SetUpQuality();
			this.SetUpClientCharacterCosmeticsDefinition();
			this.SetUpAudio();
			this.SetUpScrollSensitivity();
		}

		// Token: 0x060000CE RID: 206 RVA: 0x0000629A File Offset: 0x0000449A
		[ContextMenu("Validate")]
		public void Validate()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Validate", Array.Empty<object>());
			}
			DebugUtility.DebugIsNull("qualityMenu", this.qualityMenu, this);
			DebugUtility.DebugIsNull("defaultCharacterCosmeticsDefinition", this.defaultCharacterCosmeticsDefinition, this);
		}

		// Token: 0x060000CF RID: 207 RVA: 0x000062D8 File Offset: 0x000044D8
		private void SetUpQuality()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetUpQuality", Array.Empty<object>());
			}
			this.qualityMenu.Initialize();
			this.qualityMenu.LoadQualitySettings();
			UniversalRenderPipelineAsset universalRenderPipelineAsset = QualitySettings.renderPipeline as UniversalRenderPipelineAsset;
			if (universalRenderPipelineAsset == null)
			{
				DebugUtility.LogError("Could not convert QualitySettings.renderPipeline to UniversalRenderPipelineAsset!", this);
				return;
			}
			if (PlayerPrefs.HasKey("Render Scale"))
			{
				float @float = PlayerPrefs.GetFloat("Render Scale");
				universalRenderPipelineAsset.renderScale = @float;
				return;
			}
			PlayerPrefs.SetFloat("Render Scale", universalRenderPipelineAsset.renderScale);
		}

		// Token: 0x060000D0 RID: 208 RVA: 0x00006364 File Offset: 0x00004564
		private void SetUpClientCharacterCosmeticsDefinition()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetUpClientCharacterCosmeticsDefinition", Array.Empty<object>());
			}
			if (CharacterCosmeticsDefinitionUtility.GetClientCharacterVisualId().IsEmpty)
			{
				CharacterCosmeticsDefinitionUtility.SetClientCharacterVisualId(this.defaultCharacterCosmeticsDefinition.AssetId);
			}
		}

		// Token: 0x060000D1 RID: 209 RVA: 0x000063A8 File Offset: 0x000045A8
		private void SetUpAudio()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetUpAudio", Array.Empty<object>());
			}
			AudioMixerGroup[] value = this.audioMixerGroupArray.Value;
			for (int i = 0; i < value.Length; i++)
			{
				string text = value[i].VolumePropertyName();
				if (PlayerPrefs.HasKey(text))
				{
					float num = AudioUtility.VolumeToDecibel(PlayerPrefs.GetFloat(text) / 100f);
					this.audioMixer.SetFloat(text, num);
				}
			}
		}

		// Token: 0x060000D2 RID: 210 RVA: 0x00006418 File Offset: 0x00004618
		private void SetUpScrollSensitivity()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetUpScrollSensitivity", Array.Empty<object>());
			}
			float @float = PlayerPrefs.GetFloat("User Facing Scroll Sensitivity", this.scrollSensitivityRangeSettings.UserFacingDefault);
			float num = this.scrollSensitivityRangeSettings.UserFacingValueToInternalValue(@float);
			UIScrollRect.GlobalScrollSensitivity = num;
			if (this.verboseLogging)
			{
				Debug.Log(string.Format("{0}: {1}", "userFacingScrollSensitivity", @float), this);
				Debug.Log(string.Format("{0}: {1}", "internalScrollSensitivity", num), this);
			}
		}

		// Token: 0x0400007F RID: 127
		[SerializeField]
		private QualityMenu qualityMenu;

		// Token: 0x04000080 RID: 128
		[SerializeField]
		private CharacterCosmeticsDefinition defaultCharacterCosmeticsDefinition;

		// Token: 0x04000081 RID: 129
		[SerializeField]
		private AudioMixer audioMixer;

		// Token: 0x04000082 RID: 130
		[SerializeField]
		private AudioMixerGroupArray audioMixerGroupArray;

		// Token: 0x04000083 RID: 131
		[SerializeField]
		private ScrollSensitivityRangeSettings scrollSensitivityRangeSettings;

		// Token: 0x04000084 RID: 132
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
