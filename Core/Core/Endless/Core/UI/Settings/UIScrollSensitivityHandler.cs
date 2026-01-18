using System;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Core.UI.Settings
{
	// Token: 0x020000BF RID: 191
	public class UIScrollSensitivityHandler : MonoBehaviour
	{
		// Token: 0x06000458 RID: 1112 RVA: 0x000158BC File Offset: 0x00013ABC
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.sensitivitySlider.SetMinMax(0, this.scrollSensitivityRangeSettings.UserFacingMin, this.scrollSensitivityRangeSettings.UserFacingMax);
			this.sensitivitySlider.OnModelChanged += this.OnSliderChanged;
			float @float = PlayerPrefs.GetFloat("User Facing Scroll Sensitivity", this.scrollSensitivityRangeSettings.UserFacingDefault);
			this.sensitivitySlider.SetModel(@float, false);
		}

		// Token: 0x06000459 RID: 1113 RVA: 0x0001593D File Offset: 0x00013B3D
		private void OnDestroy()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDestroy", Array.Empty<object>());
			}
			this.sensitivitySlider.OnModelChanged -= this.OnSliderChanged;
		}

		// Token: 0x0600045A RID: 1114 RVA: 0x00015970 File Offset: 0x00013B70
		private void OnSliderChanged(object newValue)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnSliderChanged", new object[] { newValue });
			}
			float model = this.sensitivitySlider.Model;
			PlayerPrefs.SetFloat("User Facing Scroll Sensitivity", model);
			float num = this.scrollSensitivityRangeSettings.UserFacingValueToInternalValue(model);
			UIScrollRect.GlobalScrollSensitivity = num;
			if (this.verboseLogging)
			{
				Debug.Log(string.Format("{0}: {1}", "userFacingScrollSensitivity", model), this);
				Debug.Log(string.Format("{0}: {1}", "internalScrollSensitivity", num), this);
			}
		}

		// Token: 0x040002E6 RID: 742
		[SerializeField]
		private ScrollSensitivityRangeSettings scrollSensitivityRangeSettings;

		// Token: 0x040002E7 RID: 743
		[SerializeField]
		private UIFloatPresenter sensitivitySlider;

		// Token: 0x040002E8 RID: 744
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
