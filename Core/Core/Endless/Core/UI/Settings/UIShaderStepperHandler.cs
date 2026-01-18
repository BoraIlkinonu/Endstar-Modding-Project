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
	// Token: 0x020000B7 RID: 183
	public class UIShaderStepperHandler : UIBasePresetChangingQualityLevelHandler
	{
		// Token: 0x17000076 RID: 118
		// (get) Token: 0x06000421 RID: 1057 RVA: 0x00014F0E File Offset: 0x0001310E
		private ShaderQuality DisplayedQualityOption
		{
			get
			{
				return this.QualityMenu.ShaderOptions[this.stepper.ValueIndex];
			}
		}

		// Token: 0x06000422 RID: 1058 RVA: 0x00014F2B File Offset: 0x0001312B
		private void Start()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.stepper.ChangeUnityEvent.AddListener(new UnityAction(this.OnChange));
		}

		// Token: 0x06000423 RID: 1059 RVA: 0x00014F61 File Offset: 0x00013161
		private void OnEnable()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEnable", Array.Empty<object>());
			}
			UIQualityLevelStepperHandler.SetToPresetAction = (Action<QualityPreset>)Delegate.Combine(UIQualityLevelStepperHandler.SetToPresetAction, new Action<QualityPreset>(this.OnSetToPreset));
		}

		// Token: 0x06000424 RID: 1060 RVA: 0x00014F9B File Offset: 0x0001319B
		protected override void OnDisable()
		{
			base.OnDisable();
			UIQualityLevelStepperHandler.SetToPresetAction = (Action<QualityPreset>)Delegate.Remove(UIQualityLevelStepperHandler.SetToPresetAction, new Action<QualityPreset>(this.OnSetToPreset));
		}

		// Token: 0x06000425 RID: 1061 RVA: 0x00014FC4 File Offset: 0x000131C4
		public override void Initialize()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Initialize", Array.Empty<object>());
			}
			if (base.IsCustom)
			{
				this.originalQualityOption = this.QualityMenu.GetQualityOption<ShaderQuality>(this.QualityMenu.DefaultQualityPreset.ShaderQuality, this.QualityMenu.ShaderOptions);
			}
			else
			{
				this.originalQualityOption = this.QualityMenu.CurrentPreset.ShaderQuality;
			}
			ValueTuple<List<string>, int> displayNamesAndValueIndex = this.GetDisplayNamesAndValueIndex(this.originalQualityOption);
			List<string> item = displayNamesAndValueIndex.Item1;
			int item2 = displayNamesAndValueIndex.Item2;
			this.stepper.SetValues(item, true);
			this.stepper.SetValue(item2, true);
		}

		// Token: 0x06000426 RID: 1062 RVA: 0x00015068 File Offset: 0x00013268
		public override void Apply()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethodWithAppension(this, "Apply", this.stepper.Value, Array.Empty<object>());
			}
			this.QualityMenu.SetIndividualSetting(this.DisplayedQualityOption, false);
		}

		// Token: 0x06000427 RID: 1063 RVA: 0x000150A0 File Offset: 0x000132A0
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

		// Token: 0x06000428 RID: 1064 RVA: 0x000150F4 File Offset: 0x000132F4
		private void OnSetToPreset(QualityPreset qualityPreset)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnSetToPreset", new object[] { qualityPreset.DisplayName });
			}
			ValueTuple<List<string>, int> displayNamesAndValueIndex = this.GetDisplayNamesAndValueIndex(qualityPreset.ShaderQuality);
			List<string> item = displayNamesAndValueIndex.Item1;
			int item2 = displayNamesAndValueIndex.Item2;
			this.stepper.SetValues(item, true);
			this.stepper.SetValue(item2, true);
		}

		// Token: 0x06000429 RID: 1065 RVA: 0x00015158 File Offset: 0x00013358
		[return: TupleElementNames(new string[] { "displayNames", "displayedValueIndex" })]
		private ValueTuple<List<string>, int> GetDisplayNamesAndValueIndex(ShaderQuality indexMatch)
		{
			int num = 0;
			List<string> list = new List<string>();
			for (int i = 0; i < this.QualityMenu.ShaderOptions.Count; i++)
			{
				ShaderQuality shaderQuality = this.QualityMenu.ShaderOptions[i];
				list.Add(shaderQuality.DisplayName);
				if (shaderQuality == indexMatch)
				{
					num = i;
				}
			}
			return new ValueTuple<List<string>, int>(list, num);
		}

		// Token: 0x040002D6 RID: 726
		[Header("UIShaderStepperHandler")]
		[SerializeField]
		private UIStepper stepper;

		// Token: 0x040002D7 RID: 727
		private ShaderQuality originalQualityOption;
	}
}
