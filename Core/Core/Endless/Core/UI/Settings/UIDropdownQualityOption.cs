using System;
using Endless.Shared.Debugging;
using Endless.Shared.EndlessQualitySettings;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Core.UI.Settings
{
	// Token: 0x020000AE RID: 174
	public class UIDropdownQualityOption : UIBaseDropdown<QualityOption>
	{
		// Token: 0x060003CD RID: 973 RVA: 0x000138B4 File Offset: 0x00011AB4
		protected override string GetLabelFromOption(int optionIndex)
		{
			if (base.SuperVerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "GetLabelFromOption", "optionIndex", optionIndex), this);
			}
			base.ValidateIndex(optionIndex, base.Count);
			return base.Options[optionIndex].DisplayName;
		}

		// Token: 0x060003CE RID: 974 RVA: 0x00013907 File Offset: 0x00011B07
		protected override Sprite GetIconFromOption(int optionIndex)
		{
			return null;
		}
	}
}
