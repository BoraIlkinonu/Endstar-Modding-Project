using System;
using Endless.Gameplay.Serialization;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.SoVariables;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020001D5 RID: 469
	public class UIScriptEventInputModalView : UIScriptModalView
	{
		// Token: 0x170000C9 RID: 201
		// (get) Token: 0x0600070A RID: 1802 RVA: 0x00023A27 File Offset: 0x00021C27
		// (set) Token: 0x0600070B RID: 1803 RVA: 0x00023A2F File Offset: 0x00021C2F
		public bool IsEvent { get; private set; }

		// Token: 0x0600070C RID: 1804 RVA: 0x00023A38 File Offset: 0x00021C38
		protected override void Start()
		{
			base.Start();
			this.endlessParameterInfoListModel.ModelChangedUnityEvent.AddListener(new UnityAction(this.OnEndlessParameterInfoListModelChanged));
		}

		// Token: 0x0600070D RID: 1805 RVA: 0x00023A5C File Offset: 0x00021C5C
		public override void OnDisplay(params object[] modalData)
		{
			base.OnDisplay(modalData);
			this.IsEvent = (bool)modalData[0];
			this.modalNameText.text = (this.IsEvent ? "Create Event" : "Create Receiver");
			this.memberNameInputField.Clear(true);
			this.descriptionInputField.Clear(true);
			this.endlessParameterInfoListModel.Clear(true);
			this.endlessParameterInfoDisplayNameInputField.Clear(true);
			this.endlessParameterInfoDataTypeRadio.SetDefaultValue(EndlessTypeMapping.Instance.LuaInspectorTypes[0]);
			this.endlessParameterInfoDataTypeRadio.SetValueToDefault(true);
			this.isCollectionToggle.SetIsOn(false, false, true);
			this.OnEndlessParameterInfoListModelChanged();
		}

		// Token: 0x0600070E RID: 1806 RVA: 0x0001FDD0 File Offset: 0x0001DFD0
		public override void OnBack()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnBack", Array.Empty<object>());
			}
			MonoBehaviourSingleton<UIModalManager>.Instance.CloseWithoutClearingStack();
		}

		// Token: 0x0600070F RID: 1807 RVA: 0x00023B04 File Offset: 0x00021D04
		private void OnEndlessParameterInfoListModelChanged()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEndlessParameterInfoListModelChanged", Array.Empty<object>());
			}
			this.endlessParameterInfoCounterText.text = string.Format("{0}/{1} Endless Parameter Info", this.endlessParameterInfoListModel.Count, this.endlessParameterInfoLimit.Value);
			this.createEndlessParameterButton.interactable = this.endlessParameterInfoListModel.Count < this.endlessParameterInfoLimit.Value;
		}

		// Token: 0x0400065E RID: 1630
		[Header("UIScriptEventInputModalView")]
		[SerializeField]
		private TextMeshProUGUI modalNameText;

		// Token: 0x0400065F RID: 1631
		[SerializeField]
		private IntVariable endlessParameterInfoLimit;

		// Token: 0x04000660 RID: 1632
		[SerializeField]
		private UIInputField memberNameInputField;

		// Token: 0x04000661 RID: 1633
		[SerializeField]
		private UIInputField descriptionInputField;

		// Token: 0x04000662 RID: 1634
		[Header("EndlessParameterInfo Creation")]
		[SerializeField]
		private UIEndlessParameterInfoListModel endlessParameterInfoListModel;

		// Token: 0x04000663 RID: 1635
		[SerializeField]
		private UIInputField endlessParameterInfoDisplayNameInputField;

		// Token: 0x04000664 RID: 1636
		[SerializeField]
		private UIInspectorScriptValueTypeRadio endlessParameterInfoDataTypeRadio;

		// Token: 0x04000665 RID: 1637
		[SerializeField]
		private UIToggle isCollectionToggle;

		// Token: 0x04000666 RID: 1638
		[SerializeField]
		private TextMeshProUGUI endlessParameterInfoCounterText;

		// Token: 0x04000667 RID: 1639
		[SerializeField]
		private UIButton createEndlessParameterButton;
	}
}
