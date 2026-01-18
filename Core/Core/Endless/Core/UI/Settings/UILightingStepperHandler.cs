using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Endless.Shared.Debugging;
using Endless.Shared.EndlessQualitySettings;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Core.UI.Settings
{
	// Token: 0x020000B0 RID: 176
	public class UILightingStepperHandler : UIBasePresetChangingQualityLevelHandler
	{
		// Token: 0x1700006A RID: 106
		// (get) Token: 0x060003D9 RID: 985 RVA: 0x00013B5C File Offset: 0x00011D5C
		private LightingQuality DisplayedQualityOption
		{
			get
			{
				return this.QualityMenu.LightingOptions[this.stepper.ValueIndex];
			}
		}

		// Token: 0x060003DA RID: 986 RVA: 0x00013B79 File Offset: 0x00011D79
		private void Start()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.stepper.ChangeUnityEvent.AddListener(new UnityAction(this.OnChange));
		}

		// Token: 0x060003DB RID: 987 RVA: 0x00013BAF File Offset: 0x00011DAF
		private void OnEnable()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEnable", Array.Empty<object>());
			}
			UIQualityLevelStepperHandler.SetToPresetAction = (Action<QualityPreset>)Delegate.Combine(UIQualityLevelStepperHandler.SetToPresetAction, new Action<QualityPreset>(this.OnSetToPreset));
		}

		// Token: 0x060003DC RID: 988 RVA: 0x00013BE9 File Offset: 0x00011DE9
		protected override void OnDisable()
		{
			base.OnDisable();
			UIQualityLevelStepperHandler.SetToPresetAction = (Action<QualityPreset>)Delegate.Remove(UIQualityLevelStepperHandler.SetToPresetAction, new Action<QualityPreset>(this.OnSetToPreset));
		}

		// Token: 0x060003DD RID: 989 RVA: 0x00013C14 File Offset: 0x00011E14
		public override void Initialize()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Initialize", Array.Empty<object>());
			}
			if (base.IsCustom)
			{
				this.originalQualityOption = this.QualityMenu.GetQualityOption<LightingQuality>(this.QualityMenu.DefaultQualityPreset.LightingQuality, this.QualityMenu.LightingOptions);
			}
			else
			{
				this.originalQualityOption = this.QualityMenu.CurrentPreset.LightingQuality;
			}
			ValueTuple<List<string>, int> displayNamesAndValueIndex = this.GetDisplayNamesAndValueIndex(this.originalQualityOption);
			List<string> item = displayNamesAndValueIndex.Item1;
			int item2 = displayNamesAndValueIndex.Item2;
			this.stepper.SetValues(item, true);
			this.stepper.SetValue(item2, true);
		}

		// Token: 0x060003DE RID: 990 RVA: 0x00013CB8 File Offset: 0x00011EB8
		public override void Apply()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethodWithAppension(this, "Apply", this.stepper.Value, Array.Empty<object>());
			}
			this.QualityMenu.SetIndividualSetting(this.DisplayedQualityOption, false);
		}

		// Token: 0x060003DF RID: 991 RVA: 0x00013CF0 File Offset: 0x00011EF0
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

		// Token: 0x060003E0 RID: 992 RVA: 0x00013D44 File Offset: 0x00011F44
		private void OnSetToPreset(QualityPreset qualityPreset)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnSetToPreset", new object[] { qualityPreset.DisplayName });
			}
			ValueTuple<List<string>, int> displayNamesAndValueIndex = this.GetDisplayNamesAndValueIndex(qualityPreset.LightingQuality);
			List<string> item = displayNamesAndValueIndex.Item1;
			int item2 = displayNamesAndValueIndex.Item2;
			this.stepper.SetValues(item, true);
			this.stepper.SetValue(item2, true);
		}

		// Token: 0x060003E1 RID: 993 RVA: 0x00013DA8 File Offset: 0x00011FA8
		[return: TupleElementNames(new string[] { "displayNames", "displayedValueIndex" })]
		private ValueTuple<List<string>, int> GetDisplayNamesAndValueIndex(LightingQuality indexMatch)
		{
			int num = 0;
			List<string> list = new List<string>();
			for (int i = 0; i < this.QualityMenu.LightingOptions.Count; i++)
			{
				LightingQuality lightingQuality = this.QualityMenu.LightingOptions[i];
				list.Add(lightingQuality.DisplayName);
				if (lightingQuality == indexMatch)
				{
					num = i;
				}
			}
			return new ValueTuple<List<string>, int>(list, num);
		}

		// Token: 0x040002C5 RID: 709
		[Header("UILightingStepperHandler")]
		[SerializeField]
		private UIStepper stepper;

		// Token: 0x040002C6 RID: 710
		private LightingQuality originalQualityOption;
	}
}
