using System;
using System.Collections.Generic;
using Endless.Creator.DynamicPropCreation;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x020001CB RID: 459
	public class UIPropCreationMenuModalView : UIBaseModalView
	{
		// Token: 0x060006DA RID: 1754 RVA: 0x0001F330 File Offset: 0x0001D530
		public override void OnBack()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("OnBack", this);
			}
			MonoBehaviourSingleton<UIModalManager>.Instance.CloseWithoutClearingStack();
		}

		// Token: 0x060006DB RID: 1755 RVA: 0x00022C74 File Offset: 0x00020E74
		public override void OnDisplay(params object[] modalData)
		{
			base.OnDisplay(modalData);
			PropCreationMenuData propCreationMenuData = (PropCreationMenuData)modalData[0];
			this.displayNameText.text = propCreationMenuData.DisplayName;
			this.backButton.gameObject.SetActive(propCreationMenuData.IsSubMenu);
			List<PropCreationData> list = new List<PropCreationData>(propCreationMenuData.Options);
			this.propCreationDataListModel.Set(list, true);
		}

		// Token: 0x04000625 RID: 1573
		[Header("UIPropCreationMenuModalView")]
		[SerializeField]
		private TextMeshProUGUI displayNameText;

		// Token: 0x04000626 RID: 1574
		[SerializeField]
		private UIButton backButton;

		// Token: 0x04000627 RID: 1575
		[SerializeField]
		private UIPropCreationDataListModel propCreationDataListModel;
	}
}
