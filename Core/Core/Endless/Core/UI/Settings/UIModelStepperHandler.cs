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
	// Token: 0x020000B1 RID: 177
	public class UIModelStepperHandler : UIBasePresetChangingQualityLevelHandler
	{
		// Token: 0x1700006B RID: 107
		// (get) Token: 0x060003E3 RID: 995 RVA: 0x00013E08 File Offset: 0x00012008
		private ModelQuality DisplayedQualityOption
		{
			get
			{
				return this.QualityMenu.ModelOptions[this.stepper.ValueIndex];
			}
		}

		// Token: 0x060003E4 RID: 996 RVA: 0x00013E25 File Offset: 0x00012025
		private void Start()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.stepper.ChangeUnityEvent.AddListener(new UnityAction(this.OnChange));
		}

		// Token: 0x060003E5 RID: 997 RVA: 0x00013E5B File Offset: 0x0001205B
		private void OnEnable()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEnable", Array.Empty<object>());
			}
			UIQualityLevelStepperHandler.SetToPresetAction = (Action<QualityPreset>)Delegate.Combine(UIQualityLevelStepperHandler.SetToPresetAction, new Action<QualityPreset>(this.OnSetToPreset));
		}

		// Token: 0x060003E6 RID: 998 RVA: 0x00013E95 File Offset: 0x00012095
		protected override void OnDisable()
		{
			base.OnDisable();
			UIQualityLevelStepperHandler.SetToPresetAction = (Action<QualityPreset>)Delegate.Remove(UIQualityLevelStepperHandler.SetToPresetAction, new Action<QualityPreset>(this.OnSetToPreset));
		}

		// Token: 0x060003E7 RID: 999 RVA: 0x00013EC0 File Offset: 0x000120C0
		public override void Initialize()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Initialize", Array.Empty<object>());
			}
			if (base.IsCustom)
			{
				this.originalQualityOption = this.QualityMenu.GetQualityOption<ModelQuality>(this.QualityMenu.DefaultQualityPreset.ModelQuality, this.QualityMenu.ModelOptions);
			}
			else
			{
				this.originalQualityOption = this.QualityMenu.CurrentPreset.ModelQuality;
			}
			ValueTuple<List<string>, int> displayNamesAndValueIndex = this.GetDisplayNamesAndValueIndex(this.originalQualityOption);
			List<string> item = displayNamesAndValueIndex.Item1;
			int item2 = displayNamesAndValueIndex.Item2;
			this.stepper.SetValues(item, true);
			this.stepper.SetValue(item2, true);
		}

		// Token: 0x060003E8 RID: 1000 RVA: 0x00013F64 File Offset: 0x00012164
		public override void Apply()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethodWithAppension(this, "Apply", this.stepper.Value, Array.Empty<object>());
			}
			this.QualityMenu.SetIndividualSetting(this.DisplayedQualityOption, false);
		}

		// Token: 0x060003E9 RID: 1001 RVA: 0x00013F9C File Offset: 0x0001219C
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

		// Token: 0x060003EA RID: 1002 RVA: 0x00013FF0 File Offset: 0x000121F0
		private void OnSetToPreset(QualityPreset qualityPreset)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnSetToPreset", new object[] { qualityPreset.DisplayName });
			}
			ValueTuple<List<string>, int> displayNamesAndValueIndex = this.GetDisplayNamesAndValueIndex(qualityPreset.ModelQuality);
			List<string> item = displayNamesAndValueIndex.Item1;
			int item2 = displayNamesAndValueIndex.Item2;
			this.stepper.SetValues(item, true);
			this.stepper.SetValue(item2, true);
		}

		// Token: 0x060003EB RID: 1003 RVA: 0x00014054 File Offset: 0x00012254
		[return: TupleElementNames(new string[] { "displayNames", "displayedValueIndex" })]
		private ValueTuple<List<string>, int> GetDisplayNamesAndValueIndex(ModelQuality indexMatch)
		{
			int num = 0;
			List<string> list = new List<string>();
			for (int i = 0; i < this.QualityMenu.ModelOptions.Count; i++)
			{
				ModelQuality modelQuality = this.QualityMenu.ModelOptions[i];
				list.Add(modelQuality.DisplayName);
				if (modelQuality == indexMatch)
				{
					num = i;
				}
			}
			return new ValueTuple<List<string>, int>(list, num);
		}

		// Token: 0x040002C7 RID: 711
		[Header("UIModelStepperHandler")]
		[SerializeField]
		private UIStepper stepper;

		// Token: 0x040002C8 RID: 712
		private ModelQuality originalQualityOption;
	}
}
