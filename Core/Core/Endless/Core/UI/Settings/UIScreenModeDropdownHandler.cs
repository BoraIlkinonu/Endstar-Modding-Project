using System;
using Endless.Shared.Debugging;
using Endless.Shared.EndlessQualitySettings;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Core.UI.Settings
{
	// Token: 0x020000B6 RID: 182
	public class UIScreenModeDropdownHandler : UIBaseQualityLevelHandler
	{
		// Token: 0x17000073 RID: 115
		// (get) Token: 0x06000417 RID: 1047 RVA: 0x00003CF2 File Offset: 0x00001EF2
		public override bool IsMobileSupported
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000074 RID: 116
		// (get) Token: 0x06000418 RID: 1048 RVA: 0x00014D36 File Offset: 0x00012F36
		public FullScreenMode DisplayedFullScreenMode
		{
			get
			{
				return this.DisplayedQualityOption.FullScreenMode;
			}
		}

		// Token: 0x17000075 RID: 117
		// (get) Token: 0x06000419 RID: 1049 RVA: 0x00014D43 File Offset: 0x00012F43
		private ScreenModeSetting DisplayedQualityOption
		{
			get
			{
				return this.QualityMenu.ScreenModeOptions[this.dropdown.IndexOfFirstValue];
			}
		}

		// Token: 0x0600041A RID: 1050 RVA: 0x00014D60 File Offset: 0x00012F60
		private void Start()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.dropdown.OnValueChanged.AddListener(new UnityAction(this.OnChange));
		}

		// Token: 0x0600041B RID: 1051 RVA: 0x00014D96 File Offset: 0x00012F96
		private void OnEnable()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEnable", Array.Empty<object>());
			}
			UIScreenObserver.OnFullScreenModeChange = (Action)Delegate.Combine(UIScreenObserver.OnFullScreenModeChange, new Action(this.Initialize));
		}

		// Token: 0x0600041C RID: 1052 RVA: 0x00014DD1 File Offset: 0x00012FD1
		protected override void OnDisable()
		{
			base.OnDisable();
			UIScreenObserver.OnFullScreenModeChange = (Action)Delegate.Remove(UIScreenObserver.OnFullScreenModeChange, new Action(this.Initialize));
		}

		// Token: 0x0600041D RID: 1053 RVA: 0x00014DFC File Offset: 0x00012FFC
		public override void Initialize()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Initialize", Array.Empty<object>());
			}
			for (int i = 0; i < this.QualityMenu.ScreenModeOptions.Count; i++)
			{
				if (this.QualityMenu.ScreenModeOptions[i].FullScreenMode == Screen.fullScreenMode)
				{
					this.originalQualityOption[0] = this.QualityMenu.ScreenModeOptions[i];
					break;
				}
			}
			this.dropdown.SetOptionsAndValue(this.QualityMenu.ScreenModeOptions, this.originalQualityOption, false);
		}

		// Token: 0x0600041E RID: 1054 RVA: 0x00014E91 File Offset: 0x00013091
		public override void Apply()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethodWithAppension(this, "Apply", this.dropdown.DisplayedValueText, Array.Empty<object>());
			}
			this.QualityMenu.ApplyIndividualSetting(this.DisplayedQualityOption);
		}

		// Token: 0x0600041F RID: 1055 RVA: 0x00014EC7 File Offset: 0x000130C7
		private void OnChange()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnChange", Array.Empty<object>());
			}
			base.IsChanged = this.originalQualityOption[0] != this.DisplayedQualityOption;
		}

		// Token: 0x040002D4 RID: 724
		[Header("UIScreenModeDropdownHandler")]
		[SerializeField]
		private UIDropdownQualityOption dropdown;

		// Token: 0x040002D5 RID: 725
		private readonly ScreenModeSetting[] originalQualityOption = new ScreenModeSetting[1];
	}
}
