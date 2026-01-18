using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.CrashReportHandler;
using UnityEngine.Rendering.Universal;

namespace Endless.Shared.EndlessQualitySettings
{
	// Token: 0x020000F0 RID: 240
	[CreateAssetMenu(menuName = "ScriptableObject/Quality Settings/QualityMenu")]
	public class QualityMenu : ScriptableObject
	{
		// Token: 0x170000E1 RID: 225
		// (get) Token: 0x060005B5 RID: 1461 RVA: 0x00018654 File Offset: 0x00016854
		public FrameRateQuality DefaultFrameRateQuality
		{
			get
			{
				if (!MobileUtility.IsMobile)
				{
					return this.defaultFrameRateQuality;
				}
				return this.defaultMobileFrameRateQuality;
			}
		}

		// Token: 0x170000E2 RID: 226
		// (get) Token: 0x060005B6 RID: 1462 RVA: 0x0001866A File Offset: 0x0001686A
		public ScreenModeSetting DefaultScreenMode
		{
			get
			{
				if (!MobileUtility.IsMobile)
				{
					return this.defaultScreenMode;
				}
				return this.defaultMobileScreenMode;
			}
		}

		// Token: 0x170000E3 RID: 227
		// (get) Token: 0x060005B7 RID: 1463 RVA: 0x00018680 File Offset: 0x00016880
		public QualityPreset DefaultQualityPreset
		{
			get
			{
				if (!MobileUtility.IsMobile)
				{
					return this.defaultQualityPreset;
				}
				return this.defaultMobileQualityPreset;
			}
		}

		// Token: 0x170000E4 RID: 228
		// (get) Token: 0x060005B8 RID: 1464 RVA: 0x00018696 File Offset: 0x00016896
		public IReadOnlyList<FrameRateQuality> FrameRateOptions
		{
			get
			{
				return this.frameRateOptions;
			}
		}

		// Token: 0x170000E5 RID: 229
		// (get) Token: 0x060005B9 RID: 1465 RVA: 0x0001869E File Offset: 0x0001689E
		public IReadOnlyList<ScreenModeSetting> ScreenModeOptions
		{
			get
			{
				return this.screenModeOptions;
			}
		}

		// Token: 0x170000E6 RID: 230
		// (get) Token: 0x060005BA RID: 1466 RVA: 0x000186A6 File Offset: 0x000168A6
		public IReadOnlyList<ResolutionSetting> ResolutionOptions
		{
			get
			{
				return this.resolutionOptions;
			}
		}

		// Token: 0x170000E7 RID: 231
		// (get) Token: 0x060005BB RID: 1467 RVA: 0x000186AE File Offset: 0x000168AE
		public IReadOnlyList<QualityPreset> QualityPresets
		{
			get
			{
				return this.qualityPresets;
			}
		}

		// Token: 0x170000E8 RID: 232
		// (get) Token: 0x060005BC RID: 1468 RVA: 0x000186B6 File Offset: 0x000168B6
		public IReadOnlyList<HighDynamicRangeQuality> HighDynamicRangeOptions
		{
			get
			{
				return this.highDynamicRangeOptions;
			}
		}

		// Token: 0x170000E9 RID: 233
		// (get) Token: 0x060005BD RID: 1469 RVA: 0x000186BE File Offset: 0x000168BE
		public IReadOnlyList<LightingQuality> LightingOptions
		{
			get
			{
				return this.lightingOptions;
			}
		}

		// Token: 0x170000EA RID: 234
		// (get) Token: 0x060005BE RID: 1470 RVA: 0x000186C6 File Offset: 0x000168C6
		public IReadOnlyList<TextureQuality> TextureOptions
		{
			get
			{
				return this.textureOptions;
			}
		}

		// Token: 0x170000EB RID: 235
		// (get) Token: 0x060005BF RID: 1471 RVA: 0x000186CE File Offset: 0x000168CE
		public IReadOnlyList<AntialiasingQuality> AntiAliasingOptions
		{
			get
			{
				return this.antiAliasingOptions;
			}
		}

		// Token: 0x170000EC RID: 236
		// (get) Token: 0x060005C0 RID: 1472 RVA: 0x000186D6 File Offset: 0x000168D6
		public IReadOnlyList<ShaderQuality> ShaderOptions
		{
			get
			{
				return this.shaderOptions;
			}
		}

		// Token: 0x170000ED RID: 237
		// (get) Token: 0x060005C1 RID: 1473 RVA: 0x000186DE File Offset: 0x000168DE
		public IReadOnlyList<PostProcessQuality> PostProcessOptions
		{
			get
			{
				return this.postProcessOptions;
			}
		}

		// Token: 0x170000EE RID: 238
		// (get) Token: 0x060005C2 RID: 1474 RVA: 0x000186E6 File Offset: 0x000168E6
		public IReadOnlyList<ModelQuality> ModelOptions
		{
			get
			{
				return this.modelOptions;
			}
		}

		// Token: 0x170000EF RID: 239
		// (get) Token: 0x060005C3 RID: 1475 RVA: 0x000186EE File Offset: 0x000168EE
		// (set) Token: 0x060005C4 RID: 1476 RVA: 0x000186F6 File Offset: 0x000168F6
		public QualityPreset CurrentPreset { get; private set; }

		// Token: 0x060005C5 RID: 1477 RVA: 0x000186FF File Offset: 0x000168FF
		public void Initialize()
		{
			this.pipelineHandler = new UniversalRenderPipelineHandler(QualitySettings.renderPipeline as UniversalRenderPipelineAsset);
		}

		// Token: 0x060005C6 RID: 1478 RVA: 0x00018716 File Offset: 0x00016916
		public string GetCurrentQualityPresetName()
		{
			if (!(this.CurrentPreset != null))
			{
				return "Custom";
			}
			return this.CurrentPreset.DisplayName;
		}

		// Token: 0x060005C7 RID: 1479 RVA: 0x00018738 File Offset: 0x00016938
		public QualityPreset LoadQualitySettings()
		{
			this.LoadResolution();
			string loadedPresetName = PlayerPrefs.GetString("QualityPreset", "");
			DebugUtility.Log("loadedPresetName: " + loadedPresetName, this);
			if (loadedPresetName == "Custom")
			{
				this.CurrentPreset = null;
				DebugUtility.Log("CurrentPreset set to null", this);
			}
			else if (string.IsNullOrEmpty(loadedPresetName))
			{
				this.CurrentPreset = this.DefaultQualityPreset;
				DebugUtility.Log("loadedPresetName is not set. CurrentPreset set to DefaultQualityPreset: " + this.DefaultQualityPreset.DisplayName, this);
			}
			else
			{
				this.CurrentPreset = this.qualityPresets.FirstOrDefault((QualityPreset preset) => preset.DisplayName == loadedPresetName);
				DebugUtility.Log("loadedPresetName: " + loadedPresetName, this);
				if (this.CurrentPreset == null)
				{
					this.CurrentPreset = this.DefaultQualityPreset;
					DebugUtility.Log("loadedPresetName failed to load. CurrentPreset set to DefaultQualityPreset: " + this.DefaultQualityPreset.DisplayName, this);
				}
			}
			this.Apply();
			return this.CurrentPreset;
		}

		// Token: 0x060005C8 RID: 1480 RVA: 0x00018850 File Offset: 0x00016A50
		public void SwitchQualityPreset(string qualityPresetName, bool apply = true)
		{
			if (string.Equals(qualityPresetName, "Custom"))
			{
				if (this.CurrentPreset != null)
				{
					PlayerPrefs.SetString("QualityPreset", "Custom");
				}
				else
				{
					Debug.LogWarning("You cannot switch from custom to custom!");
				}
				this.CurrentPreset = null;
			}
			else if (this.CurrentPreset != null)
			{
				QualityPreset qualityPreset = this.qualityPresets.First((QualityPreset preset) => preset.DisplayName == qualityPresetName);
				if (qualityPreset != this.CurrentPreset)
				{
					PlayerPrefs.SetString("QualityPreset", qualityPreset.DisplayName);
				}
				else
				{
					Debug.LogWarning("Changing from one quality preset to the same quality preset!");
				}
				this.CurrentPreset = qualityPreset;
			}
			else
			{
				QualityPreset qualityPreset2 = this.qualityPresets.First((QualityPreset preset) => preset.DisplayName == qualityPresetName);
				qualityPreset2.ClearIndividualSettings();
				this.CurrentPreset = qualityPreset2;
				PlayerPrefs.SetString("QualityPreset", qualityPreset2.DisplayName);
			}
			if (apply)
			{
				this.Apply();
			}
		}

		// Token: 0x060005C9 RID: 1481 RVA: 0x00018948 File Offset: 0x00016B48
		public void Apply()
		{
			QualityMenu.ApplyCustomOption(this.DefaultFrameRateQuality, this.FrameRateOptions, this.pipelineHandler);
			if (this.CurrentPreset)
			{
				using (IEnumerator<QualityOption> enumerator = this.CurrentPreset.QualityOptions.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						QualityOption qualityOption = enumerator.Current;
						qualityOption.ApplySetting(this.pipelineHandler);
					}
					goto IL_0107;
				}
			}
			QualityMenu.ApplyCustomOption(this.DefaultQualityPreset.LightingQuality, this.LightingOptions, this.pipelineHandler);
			QualityMenu.ApplyCustomOption(this.DefaultQualityPreset.TextureQuality, this.TextureOptions, this.pipelineHandler);
			QualityMenu.ApplyCustomOption(this.DefaultQualityPreset.AntiAliasingQuality, this.AntiAliasingOptions, this.pipelineHandler);
			QualityMenu.ApplyCustomOption(this.DefaultQualityPreset.ShaderQuality, this.ShaderOptions, this.pipelineHandler);
			QualityMenu.ApplyCustomOption(this.DefaultQualityPreset.PostProcessQuality, this.PostProcessOptions, this.pipelineHandler);
			QualityMenu.ApplyCustomOption(this.DefaultQualityPreset.ModelQuality, this.ModelOptions, this.pipelineHandler);
			IL_0107:
			this.SetUserMetadata();
		}

		// Token: 0x060005CA RID: 1482 RVA: 0x00018A74 File Offset: 0x00016C74
		public void SetIndividualSetting(QualityOption customSetting, bool apply = true)
		{
			PlayerPrefs.SetString(customSetting.SaveKey, customSetting.DisplayName);
			if (apply)
			{
				this.Apply();
			}
		}

		// Token: 0x060005CB RID: 1483 RVA: 0x00018A90 File Offset: 0x00016C90
		public void ApplyIndividualSetting(QualityOption option)
		{
			option.ApplySetting(this.pipelineHandler);
		}

		// Token: 0x060005CC RID: 1484 RVA: 0x00018AA0 File Offset: 0x00016CA0
		public T GetQualityOption<T>(QualityOption defaultOption, IReadOnlyList<T> options) where T : QualityOption
		{
			string optionName = PlayerPrefs.GetString(typeof(T).Name, string.Empty);
			QualityOption qualityOption;
			if (string.IsNullOrEmpty(optionName))
			{
				qualityOption = defaultOption;
			}
			else
			{
				qualityOption = options.FirstOrDefault((T option) => option.DisplayName == optionName);
				if (qualityOption == null)
				{
					qualityOption = defaultOption;
				}
			}
			return qualityOption as T;
		}

		// Token: 0x060005CD RID: 1485 RVA: 0x00018B14 File Offset: 0x00016D14
		private void LoadResolution()
		{
			foreach (Resolution resolution in Screen.resolutions)
			{
				ResolutionSetting resolutionSetting = ScriptableObject.CreateInstance<ResolutionSetting>();
				resolutionSetting.Init(resolution);
				this.resolutionOptions.Add(resolutionSetting);
			}
		}

		// Token: 0x060005CE RID: 1486 RVA: 0x00018B58 File Offset: 0x00016D58
		private static void ApplyCustomOption(QualityOption defaultOption, IReadOnlyList<QualityOption> optionList, UniversalRenderPipelineHandler pipelineHandler)
		{
			string lightingSavedName = PlayerPrefs.GetString(defaultOption.SaveKey, string.Empty);
			QualityOption qualityOption = null;
			if (!string.IsNullOrEmpty(lightingSavedName))
			{
				qualityOption = optionList.FirstOrDefault((QualityOption lightingOption) => lightingOption.DisplayName == lightingSavedName);
			}
			if (qualityOption != null)
			{
				qualityOption.ApplySetting(pipelineHandler);
				return;
			}
			defaultOption.ApplySetting(pipelineHandler);
		}

		// Token: 0x060005CF RID: 1487 RVA: 0x00018BBC File Offset: 0x00016DBC
		private void SetUserMetadata()
		{
			CrashReportHandler.SetUserMetadata("QualityPreset", (this.CurrentPreset == null) ? "Custom" : this.CurrentPreset.DisplayName);
			CrashReportHandler.SetUserMetadata("Resolution", Screen.currentResolution.ToString());
			UniversalRenderPipelineAsset universalRenderPipelineAsset = QualitySettings.renderPipeline as UniversalRenderPipelineAsset;
			CrashReportHandler.SetUserMetadata("renderScale", universalRenderPipelineAsset.renderScale.ToString());
			CrashReportHandler.SetUserMetadata("fullScreenMode", Screen.fullScreenMode.ToString());
			FrameRateQuality qualityOption = this.GetQualityOption<FrameRateQuality>(this.defaultFrameRateQuality, this.frameRateOptions);
			CrashReportHandler.SetUserMetadata("FrameRateQuality", qualityOption.DisplayName);
			AntialiasingQuality qualityOption2 = this.GetQualityOption<AntialiasingQuality>(this.defaultQualityPreset.AntiAliasingQuality, this.antiAliasingOptions);
			CrashReportHandler.SetUserMetadata("AntialiasingQuality", qualityOption2.DisplayName);
			LightingQuality qualityOption3 = this.GetQualityOption<LightingQuality>(this.defaultQualityPreset.LightingQuality, this.LightingOptions);
			CrashReportHandler.SetUserMetadata("LightingQuality", qualityOption3.DisplayName);
			TextureQuality qualityOption4 = this.GetQualityOption<TextureQuality>(this.defaultQualityPreset.TextureQuality, this.textureOptions);
			CrashReportHandler.SetUserMetadata("TextureQuality", qualityOption4.DisplayName);
			ShaderQuality qualityOption5 = this.GetQualityOption<ShaderQuality>(this.defaultQualityPreset.ShaderQuality, this.shaderOptions);
			CrashReportHandler.SetUserMetadata("ShaderQuality", qualityOption5.DisplayName);
		}

		// Token: 0x0400031B RID: 795
		public const string CUSTOM_QUALITY_NAME = "Custom";

		// Token: 0x0400031C RID: 796
		private const string QUALITY_PRESET_KEY = "QualityPreset";

		// Token: 0x0400031D RID: 797
		[Header("Defaults")]
		[SerializeField]
		private FrameRateQuality defaultFrameRateQuality;

		// Token: 0x0400031E RID: 798
		[SerializeField]
		private ScreenModeSetting defaultScreenMode;

		// Token: 0x0400031F RID: 799
		[SerializeField]
		private QualityPreset defaultQualityPreset;

		// Token: 0x04000320 RID: 800
		[Header("Mobile defaults")]
		[SerializeField]
		private FrameRateQuality defaultMobileFrameRateQuality;

		// Token: 0x04000321 RID: 801
		[SerializeField]
		private ScreenModeSetting defaultMobileScreenMode;

		// Token: 0x04000322 RID: 802
		[SerializeField]
		private QualityPreset defaultMobileQualityPreset;

		// Token: 0x04000323 RID: 803
		[Header("Options")]
		[SerializeField]
		private List<FrameRateQuality> frameRateOptions = new List<FrameRateQuality>();

		// Token: 0x04000324 RID: 804
		[SerializeField]
		private List<ScreenModeSetting> screenModeOptions = new List<ScreenModeSetting>();

		// Token: 0x04000325 RID: 805
		[SerializeField]
		private List<ScreenModeSetting> macScreenModeOptions = new List<ScreenModeSetting>();

		// Token: 0x04000326 RID: 806
		[SerializeField]
		private List<QualityPreset> qualityPresets = new List<QualityPreset>();

		// Token: 0x04000327 RID: 807
		[Header("Advanced Options")]
		[SerializeField]
		private List<HighDynamicRangeQuality> highDynamicRangeOptions = new List<HighDynamicRangeQuality>();

		// Token: 0x04000328 RID: 808
		[SerializeField]
		private List<LightingQuality> lightingOptions = new List<LightingQuality>();

		// Token: 0x04000329 RID: 809
		[SerializeField]
		private List<TextureQuality> textureOptions = new List<TextureQuality>();

		// Token: 0x0400032A RID: 810
		[SerializeField]
		private List<AntialiasingQuality> antiAliasingOptions = new List<AntialiasingQuality>();

		// Token: 0x0400032B RID: 811
		[SerializeField]
		private List<ShaderQuality> shaderOptions = new List<ShaderQuality>();

		// Token: 0x0400032C RID: 812
		[SerializeField]
		private List<PostProcessQuality> postProcessOptions = new List<PostProcessQuality>();

		// Token: 0x0400032D RID: 813
		[SerializeField]
		private List<ModelQuality> modelOptions = new List<ModelQuality>();

		// Token: 0x0400032E RID: 814
		[NonSerialized]
		private readonly List<ResolutionSetting> resolutionOptions = new List<ResolutionSetting>();

		// Token: 0x0400032F RID: 815
		private UniversalRenderPipelineHandler pipelineHandler;
	}
}
