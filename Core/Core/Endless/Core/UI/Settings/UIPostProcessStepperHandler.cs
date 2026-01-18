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
	// Token: 0x020000B2 RID: 178
	public class UIPostProcessStepperHandler : UIBasePresetChangingQualityLevelHandler
	{
		// Token: 0x1700006C RID: 108
		// (get) Token: 0x060003ED RID: 1005 RVA: 0x000140B4 File Offset: 0x000122B4
		private PostProcessQuality DisplayedQualityOption
		{
			get
			{
				return this.QualityMenu.PostProcessOptions[this.stepper.ValueIndex];
			}
		}

		// Token: 0x060003EE RID: 1006 RVA: 0x000140D1 File Offset: 0x000122D1
		private void Start()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.stepper.ChangeUnityEvent.AddListener(new UnityAction(this.OnChange));
		}

		// Token: 0x060003EF RID: 1007 RVA: 0x00014107 File Offset: 0x00012307
		private void OnEnable()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEnable", Array.Empty<object>());
			}
			UIQualityLevelStepperHandler.SetToPresetAction = (Action<QualityPreset>)Delegate.Combine(UIQualityLevelStepperHandler.SetToPresetAction, new Action<QualityPreset>(this.OnSetToPreset));
		}

		// Token: 0x060003F0 RID: 1008 RVA: 0x00014141 File Offset: 0x00012341
		protected override void OnDisable()
		{
			base.OnDisable();
			UIQualityLevelStepperHandler.SetToPresetAction = (Action<QualityPreset>)Delegate.Remove(UIQualityLevelStepperHandler.SetToPresetAction, new Action<QualityPreset>(this.OnSetToPreset));
		}

		// Token: 0x060003F1 RID: 1009 RVA: 0x0001416C File Offset: 0x0001236C
		public override void Initialize()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Initialize", Array.Empty<object>());
			}
			if (base.IsCustom)
			{
				this.originalQualityOption = this.QualityMenu.GetQualityOption<PostProcessQuality>(this.QualityMenu.DefaultQualityPreset.PostProcessQuality, this.QualityMenu.PostProcessOptions);
			}
			else
			{
				this.originalQualityOption = this.QualityMenu.CurrentPreset.PostProcessQuality;
			}
			ValueTuple<List<string>, int> displayNamesAndValueIndex = this.GetDisplayNamesAndValueIndex(this.originalQualityOption);
			List<string> item = displayNamesAndValueIndex.Item1;
			int item2 = displayNamesAndValueIndex.Item2;
			this.stepper.SetValues(item, true);
			this.stepper.SetValue(item2, true);
		}

		// Token: 0x060003F2 RID: 1010 RVA: 0x00014210 File Offset: 0x00012410
		public override void Apply()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethodWithAppension(this, "Apply", this.stepper.Value, Array.Empty<object>());
			}
			this.QualityMenu.SetIndividualSetting(this.DisplayedQualityOption, false);
		}

		// Token: 0x060003F3 RID: 1011 RVA: 0x00014248 File Offset: 0x00012448
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

		// Token: 0x060003F4 RID: 1012 RVA: 0x0001429C File Offset: 0x0001249C
		private void OnSetToPreset(QualityPreset qualityPreset)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnSetToPreset", new object[] { qualityPreset.DisplayName });
			}
			ValueTuple<List<string>, int> displayNamesAndValueIndex = this.GetDisplayNamesAndValueIndex(qualityPreset.PostProcessQuality);
			List<string> item = displayNamesAndValueIndex.Item1;
			int item2 = displayNamesAndValueIndex.Item2;
			this.stepper.SetValues(item, true);
			this.stepper.SetValue(item2, true);
		}

		// Token: 0x060003F5 RID: 1013 RVA: 0x00014300 File Offset: 0x00012500
		[return: TupleElementNames(new string[] { "displayNames", "displayedValueIndex" })]
		private ValueTuple<List<string>, int> GetDisplayNamesAndValueIndex(PostProcessQuality indexMatch)
		{
			int num = 0;
			List<string> list = new List<string>();
			for (int i = 0; i < this.QualityMenu.PostProcessOptions.Count; i++)
			{
				PostProcessQuality postProcessQuality = this.QualityMenu.PostProcessOptions[i];
				list.Add(postProcessQuality.DisplayName);
				if (postProcessQuality == indexMatch)
				{
					num = i;
				}
			}
			return new ValueTuple<List<string>, int>(list, num);
		}

		// Token: 0x040002C9 RID: 713
		[Header("UIPostProcessStepperHandler")]
		[SerializeField]
		private UIStepper stepper;

		// Token: 0x040002CA RID: 714
		private PostProcessQuality originalQualityOption;
	}
}
