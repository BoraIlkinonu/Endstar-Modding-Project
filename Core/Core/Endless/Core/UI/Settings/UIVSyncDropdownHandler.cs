using System;
using Endless.Shared.Debugging;
using Endless.Shared.EndlessQualitySettings;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Core.UI.Settings
{
	// Token: 0x020000B9 RID: 185
	public class UIVSyncDropdownHandler : UIBaseQualityLevelHandler
	{
		// Token: 0x17000078 RID: 120
		// (get) Token: 0x06000435 RID: 1077 RVA: 0x00003CF2 File Offset: 0x00001EF2
		public override bool IsMobileSupported
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000079 RID: 121
		// (get) Token: 0x06000436 RID: 1078 RVA: 0x00015464 File Offset: 0x00013664
		private FrameRateQuality DisplayedQualityOption
		{
			get
			{
				return this.QualityMenu.FrameRateOptions[this.dropdown.IndexOfFirstValue];
			}
		}

		// Token: 0x06000437 RID: 1079 RVA: 0x00015481 File Offset: 0x00013681
		private void Start()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.dropdown.OnValueChanged.AddListener(new UnityAction(this.OnChange));
		}

		// Token: 0x06000438 RID: 1080 RVA: 0x000154B8 File Offset: 0x000136B8
		public override void Initialize()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Initialize", Array.Empty<object>());
			}
			this.originalQualityOption[0] = this.QualityMenu.GetQualityOption<FrameRateQuality>(this.QualityMenu.DefaultFrameRateQuality, this.QualityMenu.FrameRateOptions);
			this.dropdown.SetOptionsAndValue(this.QualityMenu.FrameRateOptions, this.originalQualityOption, false);
		}

		// Token: 0x06000439 RID: 1081 RVA: 0x00015523 File Offset: 0x00013723
		public override void Apply()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethodWithAppension(this, "Apply", this.dropdown.DisplayedValueText, Array.Empty<object>());
			}
			this.QualityMenu.SetIndividualSetting(this.DisplayedQualityOption, false);
		}

		// Token: 0x0600043A RID: 1082 RVA: 0x0001555A File Offset: 0x0001375A
		private void OnChange()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnChange", Array.Empty<object>());
			}
			base.IsChanged = this.originalQualityOption[0] != this.DisplayedQualityOption;
		}

		// Token: 0x040002DA RID: 730
		[Header("UIVSyncDropdownHandler")]
		[SerializeField]
		private UIDropdownQualityOption dropdown;

		// Token: 0x040002DB RID: 731
		private readonly FrameRateQuality[] originalQualityOption = new FrameRateQuality[1];
	}
}
