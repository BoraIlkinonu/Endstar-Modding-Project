using System;
using System.Collections.Generic;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;

namespace Endless.Core.UI.Settings
{
	// Token: 0x020000B5 RID: 181
	public class UIResolutionDropdownHandler : UIBaseQualityLevelHandler
	{
		// Token: 0x17000071 RID: 113
		// (get) Token: 0x0600040A RID: 1034 RVA: 0x00003CF2 File Offset: 0x00001EF2
		public override bool IsMobileSupported
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000072 RID: 114
		// (get) Token: 0x0600040B RID: 1035 RVA: 0x000147C8 File Offset: 0x000129C8
		public bool CanApply
		{
			get
			{
				return this.dropdown.DisplayedValueText != this.originalDisplayName[0] && this.dropdown.DisplayedValueText != "Custom";
			}
		}

		// Token: 0x0600040C RID: 1036 RVA: 0x000147FC File Offset: 0x000129FC
		private void Start()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.dropdown.OnValueChanged.AddListener(new UnityAction(this.OnChange));
			this.pipelineHandler = new UniversalRenderPipelineHandler(QualitySettings.renderPipeline as UniversalRenderPipelineAsset);
		}

		// Token: 0x0600040D RID: 1037 RVA: 0x00014854 File Offset: 0x00012A54
		private void OnEnable()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEnable", Array.Empty<object>());
			}
			UIScreenObserver.OnFullScreenModeChange = (Action)Delegate.Combine(UIScreenObserver.OnFullScreenModeChange, new Action(this.Initialize));
			UIScreenObserver.OnSizeChange = (Action)Delegate.Combine(UIScreenObserver.OnSizeChange, new Action(this.Initialize));
		}

		// Token: 0x0600040E RID: 1038 RVA: 0x000148BC File Offset: 0x00012ABC
		protected override void OnDisable()
		{
			base.OnDisable();
			UIScreenObserver.OnFullScreenModeChange = (Action)Delegate.Remove(UIScreenObserver.OnFullScreenModeChange, new Action(this.Initialize));
			UIScreenObserver.OnSizeChange = (Action)Delegate.Remove(UIScreenObserver.OnSizeChange, new Action(this.Initialize));
		}

		// Token: 0x0600040F RID: 1039 RVA: 0x00014914 File Offset: 0x00012B14
		public override void Initialize()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Initialize", Array.Empty<object>());
				Debug.Log(string.Format("Screen.currentResolution: {0}", Screen.currentResolution), this);
				Debug.Log(string.Format("Screen (width, height, refreshRateRatio): {0}, {1}, , {2}", Screen.width, Screen.height, Screen.currentResolution.refreshRateRatio.value), this);
			}
			this.resolutions.Clear();
			int num = -1;
			List<string> list = new List<string>();
			if (Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen)
			{
				for (int i = 0; i < this.QualityMenu.ResolutionOptions.Count; i++)
				{
					Resolution resolution = this.QualityMenu.ResolutionOptions[i].Resolution;
					string resolutionAndRefreshRateDisplayName = UIResolutionDropdownHandler.GetResolutionAndRefreshRateDisplayName(resolution);
					list.Add(resolutionAndRefreshRateDisplayName);
					if (Screen.width == resolution.width && Screen.height == resolution.height && Screen.currentResolution.refreshRateRatio.value == resolution.refreshRateRatio.value)
					{
						num = i;
						this.originalDisplayName[0] = resolutionAndRefreshRateDisplayName;
					}
					this.resolutions.Add(resolution);
				}
			}
			else
			{
				HashSet<Vector2Int> hashSet = new HashSet<Vector2Int>();
				for (int j = 0; j < this.QualityMenu.ResolutionOptions.Count; j++)
				{
					Resolution resolution2 = this.QualityMenu.ResolutionOptions[j].Resolution;
					Vector2Int vector2Int = new Vector2Int(resolution2.width, resolution2.height);
					if (!hashSet.Contains(vector2Int))
					{
						string resolutionDisplayName = UIResolutionDropdownHandler.GetResolutionDisplayName(resolution2);
						list.Add(resolutionDisplayName);
						hashSet.Add(vector2Int);
						if (Screen.width == resolution2.width && Screen.height == resolution2.height)
						{
							num = list.Count - 1;
							this.originalDisplayName[0] = resolutionDisplayName;
						}
						this.resolutions.Add(resolution2);
					}
				}
			}
			if (num == -1)
			{
				list.Insert(0, "Custom");
				this.originalDisplayName[0] = "Custom";
			}
			this.dropdown.SetOptionsAndValue(list, this.originalDisplayName, true);
		}

		// Token: 0x06000410 RID: 1040 RVA: 0x00014B50 File Offset: 0x00012D50
		public override void Apply()
		{
			if (base.VerboseLogging)
			{
				Debug.Log("Apply | " + this.dropdown.DisplayedValueText, this);
			}
			if (!this.CanApply)
			{
				return;
			}
			Resolution resolutionToApply = this.GetResolutionToApply();
			if (Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen)
			{
				Screen.SetResolution(resolutionToApply.width, resolutionToApply.height, Screen.fullScreenMode, resolutionToApply.refreshRateRatio);
				return;
			}
			Screen.SetResolution(resolutionToApply.width, resolutionToApply.height, Screen.fullScreenMode);
		}

		// Token: 0x06000411 RID: 1041 RVA: 0x00014BD0 File Offset: 0x00012DD0
		public Resolution GetResolutionToApply()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "GetResolutionToApply", Array.Empty<object>());
			}
			int num = this.dropdown.IndexOfFirstValue;
			if (this.originalDisplayName[0] == "Custom")
			{
				num--;
			}
			return this.resolutions[num];
		}

		// Token: 0x06000412 RID: 1042 RVA: 0x00014C28 File Offset: 0x00012E28
		public void Apply(FullScreenMode fullScreenMode)
		{
			if (base.VerboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2} )", "Apply", "fullScreenMode", fullScreenMode), this);
			}
			Resolution resolutionToApply = this.GetResolutionToApply();
			Screen.SetResolution(resolutionToApply.width, resolutionToApply.height, fullScreenMode);
		}

		// Token: 0x06000413 RID: 1043 RVA: 0x00014C78 File Offset: 0x00012E78
		private void OnChange()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnChange", Array.Empty<object>());
			}
			base.IsChanged = this.dropdown.DisplayedValueText != this.originalDisplayName[0];
		}

		// Token: 0x06000414 RID: 1044 RVA: 0x00014CB0 File Offset: 0x00012EB0
		private static string GetResolutionDisplayName(Resolution resolution)
		{
			return string.Format("{0}x{1}", resolution.width, resolution.height);
		}

		// Token: 0x06000415 RID: 1045 RVA: 0x00014CD4 File Offset: 0x00012ED4
		private static string GetResolutionAndRefreshRateDisplayName(Resolution resolution)
		{
			return string.Format("{0}x{1} {2} Hz", resolution.width, resolution.height, resolution.refreshRateRatio.value);
		}

		// Token: 0x040002D0 RID: 720
		[Header("UIResolutionDropdownHandler")]
		[SerializeField]
		private UIDropdownString dropdown;

		// Token: 0x040002D1 RID: 721
		private readonly List<Resolution> resolutions = new List<Resolution>();

		// Token: 0x040002D2 RID: 722
		private readonly string[] originalDisplayName = new string[1];

		// Token: 0x040002D3 RID: 723
		private UniversalRenderPipelineHandler pipelineHandler;
	}
}
