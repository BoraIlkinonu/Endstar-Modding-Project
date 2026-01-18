using System;
using System.Collections.Generic;
using Endless.Shared.Debugging;
using Endless.Shared.EndlessQualitySettings;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Core.UI.Settings
{
	// Token: 0x020000AB RID: 171
	public class UIAntiAliasingDropdownHandler : UIBasePresetChangingQualityLevelHandler
	{
		// Token: 0x17000064 RID: 100
		// (get) Token: 0x060003B6 RID: 950 RVA: 0x000135A0 File Offset: 0x000117A0
		private AntialiasingQuality DisplayedQualityOption
		{
			get
			{
				return this.QualityMenu.AntiAliasingOptions[this.dropdown.IndexOfFirstValue];
			}
		}

		// Token: 0x060003B7 RID: 951 RVA: 0x000135BD File Offset: 0x000117BD
		private void Start()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.dropdown.OnValueChanged.AddListener(new UnityAction(this.OnChange));
		}

		// Token: 0x060003B8 RID: 952 RVA: 0x000135F3 File Offset: 0x000117F3
		private void OnEnable()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEnable", Array.Empty<object>());
			}
			UIQualityLevelStepperHandler.SetToPresetAction = (Action<QualityPreset>)Delegate.Combine(UIQualityLevelStepperHandler.SetToPresetAction, new Action<QualityPreset>(this.OnSetToPreset));
		}

		// Token: 0x060003B9 RID: 953 RVA: 0x0001362D File Offset: 0x0001182D
		protected override void OnDisable()
		{
			base.OnDisable();
			UIQualityLevelStepperHandler.SetToPresetAction = (Action<QualityPreset>)Delegate.Remove(UIQualityLevelStepperHandler.SetToPresetAction, new Action<QualityPreset>(this.OnSetToPreset));
		}

		// Token: 0x060003BA RID: 954 RVA: 0x00013658 File Offset: 0x00011858
		public override void Initialize()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Initialize", Array.Empty<object>());
			}
			if (base.IsCustom)
			{
				this.originalQualityOption = this.QualityMenu.GetQualityOption<AntialiasingQuality>(this.QualityMenu.DefaultQualityPreset.AntiAliasingQuality, this.QualityMenu.AntiAliasingOptions);
			}
			else
			{
				this.originalQualityOption = this.QualityMenu.CurrentPreset.AntiAliasingQuality;
			}
			this.SetOptionsAndValue(this.originalQualityOption);
		}

		// Token: 0x060003BB RID: 955 RVA: 0x000136D5 File Offset: 0x000118D5
		public override void Apply()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethodWithAppension(this, "Apply", this.dropdown.DisplayedValueText, Array.Empty<object>());
			}
			this.QualityMenu.SetIndividualSetting(this.DisplayedQualityOption, false);
		}

		// Token: 0x060003BC RID: 956 RVA: 0x0001370C File Offset: 0x0001190C
		private void OnChange()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnChange", Array.Empty<object>());
			}
			base.IsChanged = this.originalQualityOption != this.DisplayedQualityOption;
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

		// Token: 0x060003BD RID: 957 RVA: 0x0001375F File Offset: 0x0001195F
		private void OnSetToPreset(QualityPreset qualityPreset)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnSetToPreset", new object[] { qualityPreset.DisplayName });
			}
			this.SetOptionsAndValue(qualityPreset.AntiAliasingQuality);
		}

		// Token: 0x060003BE RID: 958 RVA: 0x00013790 File Offset: 0x00011990
		private void SetOptionsAndValue(AntialiasingQuality targetDropdownValue)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnSetToPreset", new object[] { targetDropdownValue.DisplayName });
			}
			List<AntialiasingQuality> list = new List<AntialiasingQuality>();
			for (int i = 0; i < this.QualityMenu.AntiAliasingOptions.Count; i++)
			{
				AntialiasingQuality antialiasingQuality = this.QualityMenu.AntiAliasingOptions[i];
				list.Add(antialiasingQuality);
				if (antialiasingQuality == targetDropdownValue)
				{
					this.dropdownValue[0] = antialiasingQuality;
				}
			}
			this.dropdown.SetOptionsAndValue(list, this.dropdownValue, false);
		}

		// Token: 0x040002BC RID: 700
		[Header("UIAntiAliasingDropdownHandler")]
		[SerializeField]
		private UIDropdownQualityOption dropdown;

		// Token: 0x040002BD RID: 701
		private readonly AntialiasingQuality[] dropdownValue = new AntialiasingQuality[1];

		// Token: 0x040002BE RID: 702
		private AntialiasingQuality originalQualityOption;
	}
}
