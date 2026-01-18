using System;
using System.Linq;
using Endless.Shared.Debugging;
using Endless.Shared.EndlessQualitySettings;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Core.UI.Settings
{
	// Token: 0x020000AF RID: 175
	public class UIHighDynamicRangeToggleHandler : UIBasePresetChangingQualityLevelHandler
	{
		// Token: 0x060003D0 RID: 976 RVA: 0x00013912 File Offset: 0x00011B12
		private void Start()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.toggle.OnChange.AddListener(new UnityAction<bool>(this.OnChange));
		}

		// Token: 0x060003D1 RID: 977 RVA: 0x00013948 File Offset: 0x00011B48
		private void OnEnable()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEnable", Array.Empty<object>());
			}
			UIQualityLevelStepperHandler.SetToPresetAction = (Action<QualityPreset>)Delegate.Combine(UIQualityLevelStepperHandler.SetToPresetAction, new Action<QualityPreset>(this.OnSetToPreset));
		}

		// Token: 0x060003D2 RID: 978 RVA: 0x00013982 File Offset: 0x00011B82
		protected override void OnDisable()
		{
			base.OnDisable();
			UIQualityLevelStepperHandler.SetToPresetAction = (Action<QualityPreset>)Delegate.Remove(UIQualityLevelStepperHandler.SetToPresetAction, new Action<QualityPreset>(this.OnSetToPreset));
		}

		// Token: 0x060003D3 RID: 979 RVA: 0x000139AC File Offset: 0x00011BAC
		public override void Initialize()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Initialize", Array.Empty<object>());
			}
			if (base.IsCustom)
			{
				this.originalQualityOption = this.QualityMenu.GetQualityOption<HighDynamicRangeQuality>(this.QualityMenu.DefaultQualityPreset.HighDynamicRangeQuality, this.QualityMenu.HighDynamicRangeOptions);
			}
			else
			{
				this.originalQualityOption = this.QualityMenu.CurrentPreset.HighDynamicRangeQuality;
			}
			this.toggle.SetIsOn(this.originalQualityOption.Enabled, true, true);
		}

		// Token: 0x060003D4 RID: 980 RVA: 0x00013A38 File Offset: 0x00011C38
		public override void Apply()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethodWithAppension(this, "Apply", this.toggle.IsOn.ToString(), Array.Empty<object>());
			}
			HighDynamicRangeQuality highDynamicRangeQuality = this.QualityMenu.HighDynamicRangeOptions.FirstOrDefault((HighDynamicRangeQuality item) => item.Enabled == this.toggle.IsOn);
			this.QualityMenu.SetIndividualSetting(highDynamicRangeQuality, false);
		}

		// Token: 0x060003D5 RID: 981 RVA: 0x00013A9C File Offset: 0x00011C9C
		private void OnChange(bool enabled)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnChange", new object[] { enabled });
			}
			base.IsChanged = this.originalQualityOption.Enabled != this.toggle.IsOn;
			if (base.IsChanged)
			{
				Action deviatedFromPresetAction = UIBasePresetChangingQualityLevelHandler.DeviatedFromPresetAction;
				if (deviatedFromPresetAction == null)
				{
					return;
				}
				deviatedFromPresetAction();
			}
		}

		// Token: 0x060003D6 RID: 982 RVA: 0x00013B03 File Offset: 0x00011D03
		private void OnSetToPreset(QualityPreset qualityPreset)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnSetToPreset", new object[] { qualityPreset.DisplayName });
			}
			this.toggle.SetIsOn(qualityPreset.HighDynamicRangeQuality.Enabled, true, true);
		}

		// Token: 0x040002C3 RID: 707
		[Header("UIHighDynamicRangeToggleHandler")]
		[SerializeField]
		private UIToggle toggle;

		// Token: 0x040002C4 RID: 708
		private HighDynamicRangeQuality originalQualityOption;
	}
}
