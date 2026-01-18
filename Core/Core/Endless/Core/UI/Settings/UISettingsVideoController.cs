using System;
using System.Collections.Generic;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.EndlessQualitySettings;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Core.UI.Settings
{
	// Token: 0x020000C1 RID: 193
	public class UISettingsVideoController : UIGameObject
	{
		// Token: 0x06000460 RID: 1120 RVA: 0x00015B2C File Offset: 0x00013D2C
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			if (MobileUtility.IsMobile)
			{
				this.generalQualityLevelHandlers.RemoveAll((UIBaseQualityLevelHandler qualityLevelHandler) => !qualityLevelHandler.IsMobileSupported);
				this.advancedQualityLevelHandlers.RemoveAll((UIBaseQualityLevelHandler qualityLevelHandler) => !qualityLevelHandler.IsMobileSupported);
			}
			this.togglesToTriggerApplyButtonVisibility = base.GetComponentsInChildren<UIToggle>(true);
			this.steppersToTriggerApplyButtonVisibility = base.GetComponentsInChildren<UIStepper>(true);
			this.dropdownsToTriggerApplyButtonVisibility = base.GetComponentsInChildren<IUIDropdownable>(true);
			UIToggle[] array = this.togglesToTriggerApplyButtonVisibility;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].OnChange.AddListener(new UnityAction<bool>(this.view.HandleApplyAndRevertVisibility));
			}
			UIStepper[] array2 = this.steppersToTriggerApplyButtonVisibility;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].ChangeUnityEvent.AddListener(new UnityAction(this.view.HandleApplyAndRevertVisibility));
			}
			IUIDropdownable[] array3 = this.dropdownsToTriggerApplyButtonVisibility;
			for (int i = 0; i < array3.Length; i++)
			{
				array3[i].OnValueChanged.AddListener(new UnityAction(this.view.HandleApplyAndRevertVisibility));
			}
			this.resolutionScaleSlider.onValueChanged.AddListener(new UnityAction<float>(this.HandleApplyButtonVisibility));
			this.toggleAdvancedSettingsButton.onClick.AddListener(new UnityAction(this.view.ToggleAdvancedSettings));
			this.applyButton.onClick.AddListener(new UnityAction(this.Apply));
			this.revertButton.onClick.AddListener(new UnityAction(this.ReinitializeAll));
		}

		// Token: 0x06000461 RID: 1121 RVA: 0x00015CE4 File Offset: 0x00013EE4
		public void Apply()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Apply", Array.Empty<object>());
			}
			if (this.resolutionDropdownHandler.CanApply && this.screenModeDropdownHandler.IsChanged)
			{
				Resolution resolutionToApply = this.resolutionDropdownHandler.GetResolutionToApply();
				FullScreenMode displayedFullScreenMode = this.screenModeDropdownHandler.DisplayedFullScreenMode;
				DebugUtility.Log(string.Format("Setting resolution {0}{1}{2}", resolutionToApply.width, resolutionToApply.height, displayedFullScreenMode), this);
				Screen.SetResolution(resolutionToApply.width, resolutionToApply.height, displayedFullScreenMode, resolutionToApply.refreshRateRatio);
			}
			if (this.resolutionDropdownHandler.IsChanged || this.screenModeDropdownHandler.IsChanged)
			{
				Resolution currentResolution = Screen.currentResolution;
				currentResolution.width = Screen.width;
				currentResolution.height = Screen.height;
				MonoBehaviourSingleton<UIModalManager>.Instance.Display(this.screenConfirmationModalSource, UIModalManagerStackActions.ClearStack, new object[]
				{
					currentResolution,
					Screen.fullScreenMode
				});
			}
			if (this.resolutionDropdownHandler.IsChanged && this.resolutionDropdownHandler.CanApply && this.screenModeDropdownHandler.IsChanged)
			{
				this.resolutionDropdownHandler.Apply(this.screenModeDropdownHandler.DisplayedFullScreenMode);
			}
			this.ApplyChanged(this.generalQualityLevelHandlers, false);
			if (!this.qualityLevelStepperHandler.IsChanged || this.qualityLevelStepperHandler.DisplayedQualityOption == "Custom")
			{
				this.ApplyChanged(this.advancedQualityLevelHandlers, true);
			}
			this.qualityMenu.Apply();
			this.ReinitializeAll();
		}

		// Token: 0x06000462 RID: 1122 RVA: 0x00015E78 File Offset: 0x00014078
		public void ReinitializeAll()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ReinitializeAll", Array.Empty<object>());
			}
			this.qualityLevelStepperHandler.Reinitialize();
			this.Reinitialize(this.generalQualityLevelHandlers);
			this.Reinitialize(this.advancedQualityLevelHandlers);
			this.view.HideApplyAndRevertButton();
		}

		// Token: 0x06000463 RID: 1123 RVA: 0x00015ECB File Offset: 0x000140CB
		private void HandleApplyButtonVisibility(float newValue)
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "HandleApplyButtonVisibility", "newValue", newValue), this);
			}
			this.view.HandleApplyAndRevertVisibility();
		}

		// Token: 0x06000464 RID: 1124 RVA: 0x00015F00 File Offset: 0x00014100
		private void ApplyChanged(List<UIBaseQualityLevelHandler> qualityLevelHandlers, bool applyAll = false)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ApplyChanged", new object[] { qualityLevelHandlers.Count, applyAll });
			}
			bool flag = this.resolutionDropdownHandler.CanApply && this.screenModeDropdownHandler.IsChanged;
			foreach (UIBaseQualityLevelHandler uibaseQualityLevelHandler in qualityLevelHandlers)
			{
				if (applyAll || uibaseQualityLevelHandler.IsChanged)
				{
					bool flag2 = true;
					if (flag && (uibaseQualityLevelHandler == this.resolutionDropdownHandler || uibaseQualityLevelHandler == this.screenModeDropdownHandler))
					{
						flag2 = false;
					}
					if (flag2)
					{
						if (this.verboseLogging)
						{
							Debug.Log("Applying " + uibaseQualityLevelHandler.gameObject.name, uibaseQualityLevelHandler);
						}
						uibaseQualityLevelHandler.Apply();
					}
				}
			}
		}

		// Token: 0x06000465 RID: 1125 RVA: 0x00015FF0 File Offset: 0x000141F0
		private void Reinitialize(List<UIBaseQualityLevelHandler> qualityLevelHandlers)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Reinitialize", new object[] { qualityLevelHandlers.Count });
			}
			foreach (UIBaseQualityLevelHandler uibaseQualityLevelHandler in qualityLevelHandlers)
			{
				uibaseQualityLevelHandler.Reinitialize();
			}
		}

		// Token: 0x040002ED RID: 749
		[SerializeField]
		private UISettingsVideoView view;

		// Token: 0x040002EE RID: 750
		[SerializeField]
		private QualityMenu qualityMenu;

		// Token: 0x040002EF RID: 751
		[SerializeField]
		private List<UIBaseQualityLevelHandler> generalQualityLevelHandlers = new List<UIBaseQualityLevelHandler>();

		// Token: 0x040002F0 RID: 752
		[SerializeField]
		private List<UIBaseQualityLevelHandler> advancedQualityLevelHandlers = new List<UIBaseQualityLevelHandler>();

		// Token: 0x040002F1 RID: 753
		[SerializeField]
		private UIQualityLevelStepperHandler qualityLevelStepperHandler;

		// Token: 0x040002F2 RID: 754
		[Header("General")]
		[SerializeField]
		private UISlider resolutionScaleSlider;

		// Token: 0x040002F3 RID: 755
		[Header("Advanced")]
		[SerializeField]
		private UIButton toggleAdvancedSettingsButton;

		// Token: 0x040002F4 RID: 756
		[SerializeField]
		private UIButton applyButton;

		// Token: 0x040002F5 RID: 757
		[SerializeField]
		private UIButton revertButton;

		// Token: 0x040002F6 RID: 758
		[Header("Screen Related")]
		[SerializeField]
		private UIResolutionDropdownHandler resolutionDropdownHandler;

		// Token: 0x040002F7 RID: 759
		[SerializeField]
		private UIScreenModeDropdownHandler screenModeDropdownHandler;

		// Token: 0x040002F8 RID: 760
		[SerializeField]
		private UIScreenConfirmationModalView screenConfirmationModalSource;

		// Token: 0x040002F9 RID: 761
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x040002FA RID: 762
		private UIToggle[] togglesToTriggerApplyButtonVisibility = Array.Empty<UIToggle>();

		// Token: 0x040002FB RID: 763
		private UIStepper[] steppersToTriggerApplyButtonVisibility = Array.Empty<UIStepper>();

		// Token: 0x040002FC RID: 764
		private IUIDropdownable[] dropdownsToTriggerApplyButtonVisibility = Array.Empty<IUIDropdownable>();
	}
}
