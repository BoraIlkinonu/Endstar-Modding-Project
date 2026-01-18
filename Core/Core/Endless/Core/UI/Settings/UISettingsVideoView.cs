using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;

namespace Endless.Core.UI.Settings
{
	// Token: 0x020000C3 RID: 195
	public class UISettingsVideoView : UIGameObject
	{
		// Token: 0x17000080 RID: 128
		// (get) Token: 0x0600046B RID: 1131 RVA: 0x000160BA File Offset: 0x000142BA
		public bool HasChanges
		{
			get
			{
				return this.qualityLevelHandlers.Any((UIBaseQualityLevelHandler qualityLevelHandler) => qualityLevelHandler.IsChanged);
			}
		}

		// Token: 0x0600046C RID: 1132 RVA: 0x000160E8 File Offset: 0x000142E8
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.renderScaleSlider.onValueChanged.AddListener(new UnityAction<float>(this.OnScale));
			this.qualityLevelHandlers = base.GetComponentsInChildren<UIBaseQualityLevelHandler>(true).ToList<UIBaseQualityLevelHandler>();
			if (MobileUtility.IsMobile)
			{
				this.qualityLevelHandlers.RemoveAll((UIBaseQualityLevelHandler qualityLevelHandler) => !qualityLevelHandler.IsMobileSupported);
			}
			foreach (UIBaseQualityLevelHandler uibaseQualityLevelHandler in this.qualityLevelHandlers)
			{
				uibaseQualityLevelHandler.Initialize();
			}
			this.advancedSettingsDisplayAndHideHandler.SetToHideEnd(true);
		}

		// Token: 0x0600046D RID: 1133 RVA: 0x000161BC File Offset: 0x000143BC
		private void OnEnable()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEnable", Array.Empty<object>());
			}
			this.Display();
		}

		// Token: 0x0600046E RID: 1134 RVA: 0x000161DC File Offset: 0x000143DC
		public void Display()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Display", Array.Empty<object>());
			}
			foreach (UIBaseQualityLevelHandler uibaseQualityLevelHandler in this.qualityLevelHandlers)
			{
				uibaseQualityLevelHandler.Initialize();
			}
			UniversalRenderPipelineAsset universalRenderPipelineAsset = QualitySettings.renderPipeline as UniversalRenderPipelineAsset;
			if (universalRenderPipelineAsset != null)
			{
				this.OnScale(universalRenderPipelineAsset.renderScale);
			}
			this.applyAndRevertDisplayAndHideHandler.SetToHideEnd(true);
		}

		// Token: 0x0600046F RID: 1135 RVA: 0x00016270 File Offset: 0x00014470
		public void Hide()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Hide", Array.Empty<object>());
			}
			this.applyAndRevertDisplayAndHideHandler.SetToHideEnd(true);
		}

		// Token: 0x06000470 RID: 1136 RVA: 0x00016296 File Offset: 0x00014496
		public void HandleApplyAndRevertVisibility(bool isOn)
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "HandleApplyAndRevertVisibility", "isOn", isOn), this);
			}
			base.StartCoroutine(this.HandleApplyVisibilityCoroutine());
		}

		// Token: 0x06000471 RID: 1137 RVA: 0x000162CD File Offset: 0x000144CD
		public void HandleApplyAndRevertVisibility()
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log("HandleApplyAndRevertVisibility", this);
			}
			base.StartCoroutine(this.HandleApplyVisibilityCoroutine());
		}

		// Token: 0x06000472 RID: 1138 RVA: 0x000162EF File Offset: 0x000144EF
		public void HideApplyAndRevertButton()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "HideApplyAndRevertButton", Array.Empty<object>());
			}
			this.applyAndRevertDisplayAndHideHandler.Hide();
		}

		// Token: 0x06000473 RID: 1139 RVA: 0x00016314 File Offset: 0x00014514
		public void ToggleAdvancedSettings()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ToggleAdvancedSettings", Array.Empty<object>());
			}
			this.advancedSettingsDropdownDisplayAndHideHandler.Toggle();
			this.advancedSettingsDisplayAndHideHandler.Toggle();
		}

		// Token: 0x06000474 RID: 1140 RVA: 0x00016344 File Offset: 0x00014544
		private IEnumerator HandleApplyVisibilityCoroutine()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "HandleApplyVisibilityCoroutine", Array.Empty<object>());
			}
			yield return new WaitForEndOfFrame();
			if (this.qualityLevelHandlers.Any((UIBaseQualityLevelHandler item) => item.IsChanged))
			{
				this.applyAndRevertDisplayAndHideHandler.Display();
			}
			else
			{
				this.HideApplyAndRevertButton();
			}
			yield break;
		}

		// Token: 0x06000475 RID: 1141 RVA: 0x00016354 File Offset: 0x00014554
		private void OnScale(float scale)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnScale", new object[] { scale });
			}
			this.renderScaleText.text = (scale * 100f).ToString("F0") + "%";
		}

		// Token: 0x04000300 RID: 768
		[SerializeField]
		private UISlider renderScaleSlider;

		// Token: 0x04000301 RID: 769
		[SerializeField]
		private TextMeshProUGUI renderScaleText;

		// Token: 0x04000302 RID: 770
		[SerializeField]
		private UIDisplayAndHideHandler advancedSettingsDropdownDisplayAndHideHandler;

		// Token: 0x04000303 RID: 771
		[SerializeField]
		private UIDisplayAndHideHandler advancedSettingsDisplayAndHideHandler;

		// Token: 0x04000304 RID: 772
		[SerializeField]
		private UIDisplayAndHideHandler applyAndRevertDisplayAndHideHandler;

		// Token: 0x04000305 RID: 773
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x04000306 RID: 774
		private List<UIBaseQualityLevelHandler> qualityLevelHandlers = new List<UIBaseQualityLevelHandler>();
	}
}
