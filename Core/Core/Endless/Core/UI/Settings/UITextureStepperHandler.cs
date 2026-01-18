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
	// Token: 0x020000B8 RID: 184
	public class UITextureStepperHandler : UIBasePresetChangingQualityLevelHandler
	{
		// Token: 0x17000077 RID: 119
		// (get) Token: 0x0600042B RID: 1067 RVA: 0x000151B8 File Offset: 0x000133B8
		private TextureQuality DisplayedQualityOption
		{
			get
			{
				return this.QualityMenu.TextureOptions[this.stepper.ValueIndex];
			}
		}

		// Token: 0x0600042C RID: 1068 RVA: 0x000151D5 File Offset: 0x000133D5
		private void Start()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.stepper.ChangeUnityEvent.AddListener(new UnityAction(this.OnChange));
		}

		// Token: 0x0600042D RID: 1069 RVA: 0x0001520B File Offset: 0x0001340B
		private void OnEnable()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEnable", Array.Empty<object>());
			}
			UIQualityLevelStepperHandler.SetToPresetAction = (Action<QualityPreset>)Delegate.Combine(UIQualityLevelStepperHandler.SetToPresetAction, new Action<QualityPreset>(this.OnSetToPreset));
		}

		// Token: 0x0600042E RID: 1070 RVA: 0x00015245 File Offset: 0x00013445
		protected override void OnDisable()
		{
			base.OnDisable();
			UIQualityLevelStepperHandler.SetToPresetAction = (Action<QualityPreset>)Delegate.Remove(UIQualityLevelStepperHandler.SetToPresetAction, new Action<QualityPreset>(this.OnSetToPreset));
		}

		// Token: 0x0600042F RID: 1071 RVA: 0x00015270 File Offset: 0x00013470
		public override void Initialize()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Initialize", Array.Empty<object>());
			}
			if (base.IsCustom)
			{
				this.originalQualityOption = this.QualityMenu.GetQualityOption<TextureQuality>(this.QualityMenu.DefaultQualityPreset.TextureQuality, this.QualityMenu.TextureOptions);
			}
			else
			{
				this.originalQualityOption = this.QualityMenu.CurrentPreset.TextureQuality;
			}
			ValueTuple<List<string>, int> displayNamesAndValueIndex = this.GetDisplayNamesAndValueIndex(this.originalQualityOption);
			List<string> item = displayNamesAndValueIndex.Item1;
			int item2 = displayNamesAndValueIndex.Item2;
			this.stepper.SetValues(item, true);
			this.stepper.SetValue(item2, true);
		}

		// Token: 0x06000430 RID: 1072 RVA: 0x00015314 File Offset: 0x00013514
		public override void Apply()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethodWithAppension(this, "Apply", this.stepper.Value, Array.Empty<object>());
			}
			this.QualityMenu.SetIndividualSetting(this.DisplayedQualityOption, false);
		}

		// Token: 0x06000431 RID: 1073 RVA: 0x0001534C File Offset: 0x0001354C
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

		// Token: 0x06000432 RID: 1074 RVA: 0x000153A0 File Offset: 0x000135A0
		private void OnSetToPreset(QualityPreset qualityPreset)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnSetToPreset", new object[] { qualityPreset.DisplayName });
			}
			ValueTuple<List<string>, int> displayNamesAndValueIndex = this.GetDisplayNamesAndValueIndex(qualityPreset.TextureQuality);
			List<string> item = displayNamesAndValueIndex.Item1;
			int item2 = displayNamesAndValueIndex.Item2;
			this.stepper.SetValues(item, true);
			this.stepper.SetValue(item2, true);
		}

		// Token: 0x06000433 RID: 1075 RVA: 0x00015404 File Offset: 0x00013604
		[return: TupleElementNames(new string[] { "displayNames", "displayedValueIndex" })]
		private ValueTuple<List<string>, int> GetDisplayNamesAndValueIndex(TextureQuality indexMatch)
		{
			int num = 0;
			List<string> list = new List<string>();
			for (int i = 0; i < this.QualityMenu.TextureOptions.Count; i++)
			{
				TextureQuality textureQuality = this.QualityMenu.TextureOptions[i];
				list.Add(textureQuality.DisplayName);
				if (textureQuality == indexMatch)
				{
					num = i;
				}
			}
			return new ValueTuple<List<string>, int>(list, num);
		}

		// Token: 0x040002D8 RID: 728
		[Header("UITextureStepperHandler")]
		[SerializeField]
		private UIStepper stepper;

		// Token: 0x040002D9 RID: 729
		private TextureQuality originalQualityOption;
	}
}
