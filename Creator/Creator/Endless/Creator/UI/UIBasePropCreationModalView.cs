using System;
using Endless.Creator.DynamicPropCreation;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020001C7 RID: 455
	public abstract class UIBasePropCreationModalView : UIBaseModalView
	{
		// Token: 0x060006CC RID: 1740 RVA: 0x000229B7 File Offset: 0x00020BB7
		protected override void Start()
		{
			base.Start();
			this.nameInputField.onValueChanged.AddListener(new UnityAction<string>(this.SetNameText));
		}

		// Token: 0x060006CD RID: 1741 RVA: 0x0001F330 File Offset: 0x0001D530
		public override void OnBack()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("OnBack", this);
			}
			MonoBehaviourSingleton<UIModalManager>.Instance.CloseWithoutClearingStack();
		}

		// Token: 0x060006CE RID: 1742 RVA: 0x000229DC File Offset: 0x00020BDC
		public override void OnDisplay(params object[] modalData)
		{
			base.OnDisplay(modalData);
			PropCreationScreenData propCreationScreenData = (PropCreationScreenData)modalData[0];
			this.typeName.text = propCreationScreenData.DisplayName;
			this.nameInputField.text = propCreationScreenData.DefaultName;
			this.descriptionInputField.text = propCreationScreenData.DefaultDescription;
			this.nameText.text = propCreationScreenData.DefaultName;
			this.grantEditRightsToCollaboratorsToggle.SetIsOn(false, false, true);
		}

		// Token: 0x060006CF RID: 1743 RVA: 0x00022A4B File Offset: 0x00020C4B
		private void SetNameText(string newName)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("SetNameText ( newName: " + newName + " )", this);
			}
			this.nameText.text = newName;
		}

		// Token: 0x04000619 RID: 1561
		[Header("UIBasePropCreationModalView")]
		[SerializeField]
		private TextMeshProUGUI typeName;

		// Token: 0x0400061A RID: 1562
		[SerializeField]
		private UIInputField nameInputField;

		// Token: 0x0400061B RID: 1563
		[SerializeField]
		private UIInputField descriptionInputField;

		// Token: 0x0400061C RID: 1564
		[SerializeField]
		private TextMeshProUGUI nameText;

		// Token: 0x0400061D RID: 1565
		[SerializeField]
		private UIToggle grantEditRightsToCollaboratorsToggle;
	}
}
