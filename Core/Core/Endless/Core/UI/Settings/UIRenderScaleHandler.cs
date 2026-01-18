using System;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;

namespace Endless.Core.UI.Settings
{
	// Token: 0x020000B4 RID: 180
	public class UIRenderScaleHandler : UIBaseQualityLevelHandler
	{
		// Token: 0x1700006F RID: 111
		// (get) Token: 0x06000403 RID: 1027 RVA: 0x000027B9 File Offset: 0x000009B9
		public override bool IsMobileSupported
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000070 RID: 112
		// (get) Token: 0x06000404 RID: 1028 RVA: 0x0001469C File Offset: 0x0001289C
		private UniversalRenderPipelineAsset UniversalRenderPipelineAsset
		{
			get
			{
				return QualitySettings.renderPipeline as UniversalRenderPipelineAsset;
			}
		}

		// Token: 0x06000405 RID: 1029 RVA: 0x000146A8 File Offset: 0x000128A8
		private void Start()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.slider.OnChange.AddListener(new UnityAction<float>(this.OnChange));
		}

		// Token: 0x06000406 RID: 1030 RVA: 0x000146E0 File Offset: 0x000128E0
		public override void Initialize()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Initialize", Array.Empty<object>());
			}
			this.originalValue = this.UniversalRenderPipelineAsset.renderScale;
			this.slider.value = this.UniversalRenderPipelineAsset.renderScale;
		}

		// Token: 0x06000407 RID: 1031 RVA: 0x0001472C File Offset: 0x0001292C
		public override void Apply()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethodWithAppension(this, "Apply", this.slider.value.ToString(), Array.Empty<object>());
			}
			this.UniversalRenderPipelineAsset.renderScale = this.slider.value;
			PlayerPrefs.SetFloat("Render Scale", this.slider.value);
		}

		// Token: 0x06000408 RID: 1032 RVA: 0x0001478F File Offset: 0x0001298F
		private void OnChange(float newValue)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnChange", new object[] { newValue });
			}
			base.IsChanged = !Mathf.Approximately(this.originalValue, newValue);
		}

		// Token: 0x040002CE RID: 718
		[Header("UIRenderScaleHandler")]
		[SerializeField]
		private UISlider slider;

		// Token: 0x040002CF RID: 719
		private float originalValue;
	}
}
