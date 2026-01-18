using System;
using Endless.Shared.Debugging;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x0200013A RID: 314
	public class UIDropdownString : UIBaseDropdown<string>
	{
		// Token: 0x060007EA RID: 2026 RVA: 0x00021730 File Offset: 0x0001F930
		protected override string GetLabelFromOption(int optionIndex)
		{
			if (base.SuperVerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "GetLabelFromOption", "optionIndex", optionIndex), this);
			}
			base.ValidateIndex(optionIndex, base.Count);
			return base.Options[optionIndex];
		}

		// Token: 0x060007EB RID: 2027 RVA: 0x0002177E File Offset: 0x0001F97E
		protected override Sprite GetIconFromOption(int optionIndex)
		{
			if (base.SuperVerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "GetIconFromOption", "optionIndex", optionIndex), this);
			}
			return null;
		}
	}
}
