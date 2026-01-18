using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Creator.UI;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;

namespace Endless.Core.UI
{
	// Token: 0x0200008A RID: 138
	public class UICreateAndPlaySectionView : UIGameObject
	{
		// Token: 0x060002B9 RID: 697 RVA: 0x0000EF3C File Offset: 0x0000D13C
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			List<string> list = Enum.GetNames(typeof(UIGameAssetTypes)).ToList<string>();
			list.RemoveAt(0);
			if (!Application.isEditor)
			{
				list.Remove("SFX");
				list.Remove("Ambient");
				list.Remove("Music");
			}
			this.initialValue[0] = list[0];
			this.gameAssetTypeFilterDropdown.SetOptionsAndValue(list, this.initialValue, false);
		}

		// Token: 0x060002BA RID: 698 RVA: 0x0000EFCA File Offset: 0x0000D1CA
		public void SetSectionTitleText(string text)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetSectionTitleText", new object[] { text });
			}
			this.sectionTitleText.text = text;
		}

		// Token: 0x0400020C RID: 524
		[SerializeField]
		private TextMeshProUGUI sectionTitleText;

		// Token: 0x0400020D RID: 525
		[SerializeField]
		private UIDropdownString gameAssetTypeFilterDropdown;

		// Token: 0x0400020E RID: 526
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x0400020F RID: 527
		private readonly string[] initialValue = new string[1];
	}
}
