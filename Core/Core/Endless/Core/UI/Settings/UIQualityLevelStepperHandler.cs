using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Endless.Shared.Debugging;
using Endless.Shared.EndlessQualitySettings;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Core.UI.Settings
{
	// Token: 0x020000B3 RID: 179
	public class UIQualityLevelStepperHandler : UIBaseQualityLevelHandler
	{
		// Token: 0x1700006D RID: 109
		// (get) Token: 0x060003F7 RID: 1015 RVA: 0x000027B9 File Offset: 0x000009B9
		public override bool IsMobileSupported
		{
			get
			{
				return true;
			}
		}

		// Token: 0x1700006E RID: 110
		// (get) Token: 0x060003F8 RID: 1016 RVA: 0x00014360 File Offset: 0x00012560
		public string DisplayedQualityOption
		{
			get
			{
				return this.stepper.Value;
			}
		}

		// Token: 0x060003F9 RID: 1017 RVA: 0x0001436D File Offset: 0x0001256D
		private void Start()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.stepper.ChangeUnityEvent.AddListener(new UnityAction(this.OnChange));
		}

		// Token: 0x060003FA RID: 1018 RVA: 0x000143A3 File Offset: 0x000125A3
		private void OnEnable()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEnable", Array.Empty<object>());
			}
			UIBasePresetChangingQualityLevelHandler.DeviatedFromPresetAction = (Action)Delegate.Combine(UIBasePresetChangingQualityLevelHandler.DeviatedFromPresetAction, new Action(this.DisplayAsCustom));
		}

		// Token: 0x060003FB RID: 1019 RVA: 0x000143DD File Offset: 0x000125DD
		protected override void OnDisable()
		{
			base.OnDisable();
			UIBasePresetChangingQualityLevelHandler.DeviatedFromPresetAction = (Action)Delegate.Remove(UIBasePresetChangingQualityLevelHandler.DeviatedFromPresetAction, new Action(this.DisplayAsCustom));
		}

		// Token: 0x060003FC RID: 1020 RVA: 0x00014408 File Offset: 0x00012608
		public override void Initialize()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Initialize", Array.Empty<object>());
			}
			this.originalQualityOption = this.QualityMenu.GetCurrentQualityPresetName();
			ValueTuple<List<string>, int> displayNamesAndValueIndex = this.GetDisplayNamesAndValueIndex(this.originalQualityOption);
			List<string> item = displayNamesAndValueIndex.Item1;
			int item2 = displayNamesAndValueIndex.Item2;
			if (base.IsCustom)
			{
				item.Insert(0, "Custom");
			}
			this.stepper.SetValues(item, true);
			this.stepper.SetValue(item2, true);
		}

		// Token: 0x060003FD RID: 1021 RVA: 0x00014485 File Offset: 0x00012685
		public override void Apply()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethodWithAppension(this, "Apply", this.stepper.Value, Array.Empty<object>());
			}
			this.QualityMenu.SwitchQualityPreset(this.stepper.Value, false);
		}

		// Token: 0x060003FE RID: 1022 RVA: 0x000144C4 File Offset: 0x000126C4
		private void OnChange()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnChange", Array.Empty<object>());
			}
			base.IsChanged = this.originalQualityOption != this.DisplayedQualityOption;
			if (this.stepper.Values[0] == "Custom")
			{
				ValueTuple<List<string>, int> displayNamesAndValueIndex = this.GetDisplayNamesAndValueIndex(this.DisplayedQualityOption);
				List<string> item = displayNamesAndValueIndex.Item1;
				int item2 = displayNamesAndValueIndex.Item2;
				this.stepper.SetValues(item, true);
				this.stepper.SetValue(item2, true);
			}
			QualityPreset qualityPreset = this.QualityMenu.QualityPresets.FirstOrDefault((QualityPreset p) => p.DisplayName == this.DisplayedQualityOption);
			Action<QualityPreset> setToPresetAction = UIQualityLevelStepperHandler.SetToPresetAction;
			if (setToPresetAction == null)
			{
				return;
			}
			setToPresetAction(qualityPreset);
		}

		// Token: 0x060003FF RID: 1023 RVA: 0x0001457C File Offset: 0x0001277C
		private void DisplayAsCustom()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "DisplayAsCustom", Array.Empty<object>());
			}
			if (this.stepper.Values[0] == "Custom")
			{
				this.stepper.SetValue(0, true);
			}
			else
			{
				ValueTuple<List<string>, int> displayNamesAndValueIndex = this.GetDisplayNamesAndValueIndex(this.originalQualityOption);
				List<string> item = displayNamesAndValueIndex.Item1;
				int num = displayNamesAndValueIndex.Item2;
				item.Insert(0, "Custom");
				num = 0;
				this.stepper.SetValues(item, true);
				this.stepper.SetValue(num, true);
			}
			base.IsChanged = this.originalQualityOption != this.DisplayedQualityOption;
		}

		// Token: 0x06000400 RID: 1024 RVA: 0x00014624 File Offset: 0x00012824
		[return: TupleElementNames(new string[] { "displayNames", "displayedValueIndex" })]
		private ValueTuple<List<string>, int> GetDisplayNamesAndValueIndex(string indexMatch)
		{
			int num = 0;
			List<string> list = new List<string>();
			for (int i = 0; i < this.QualityMenu.QualityPresets.Count; i++)
			{
				QualityPreset qualityPreset = this.QualityMenu.QualityPresets[i];
				list.Add(qualityPreset.DisplayName);
				if (qualityPreset.DisplayName == indexMatch)
				{
					num = i;
				}
			}
			return new ValueTuple<List<string>, int>(list, num);
		}

		// Token: 0x040002CB RID: 715
		public static Action<QualityPreset> SetToPresetAction;

		// Token: 0x040002CC RID: 716
		[Header("UIQualityLevelStepperHandler")]
		[SerializeField]
		private UIStepper stepper;

		// Token: 0x040002CD RID: 717
		private string originalQualityOption;
	}
}
