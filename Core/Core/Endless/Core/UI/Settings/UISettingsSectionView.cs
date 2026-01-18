using System;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Endless.Shared.Validation;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Core.UI.Settings
{
	// Token: 0x020000C0 RID: 192
	public class UISettingsSectionView : UIGameObject, IValidatable
	{
		// Token: 0x0600045C RID: 1116 RVA: 0x00015A04 File Offset: 0x00013C04
		public void Validate()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Validate", Array.Empty<object>());
			}
			if (this.tabs.Interface.OptionsLength != this.sectionSubTitles.Length)
			{
				DebugUtility.LogError(string.Format("The amount of {0} ({1}) must equal the amount of {2}'s values ({3})!", new object[]
				{
					"sectionSubTitles",
					this.sectionSubTitles.Length,
					"tabs",
					this.tabs.Interface.OptionsLength
				}), this);
			}
		}

		// Token: 0x0600045D RID: 1117 RVA: 0x00015A94 File Offset: 0x00013C94
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.tabs.Interface.OnValueChangedWithIndex.AddListener(new UnityAction<int>(this.DisplaySectionSubTitle));
			this.DisplaySectionSubTitle(0);
		}

		// Token: 0x0600045E RID: 1118 RVA: 0x00015AE1 File Offset: 0x00013CE1
		private void DisplaySectionSubTitle(int index)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "DisplaySectionSubTitle", new object[] { index });
			}
			this.sectionSubTitleText.text = this.sectionSubTitles[index];
		}

		// Token: 0x040002E9 RID: 745
		[SerializeField]
		private InterfaceReference<IUITabGroup> tabs;

		// Token: 0x040002EA RID: 746
		[SerializeField]
		private string[] sectionSubTitles = Array.Empty<string>();

		// Token: 0x040002EB RID: 747
		[SerializeField]
		private TextMeshProUGUI sectionSubTitleText;

		// Token: 0x040002EC RID: 748
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
