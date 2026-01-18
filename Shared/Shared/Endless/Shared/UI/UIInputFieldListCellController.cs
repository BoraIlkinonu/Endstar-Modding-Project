using System;
using Endless.Shared.Debugging;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x020001A6 RID: 422
	public class UIInputFieldListCellController : UIBaseListCellController<string>
	{
		// Token: 0x06000AEF RID: 2799 RVA: 0x00030204 File Offset: 0x0002E404
		protected override void Start()
		{
			base.Start();
			this.inputField.DeselectAndValueChangedUnityEvent.AddListener(new UnityAction<string>(this.SetValue));
			this.removeButton.onClick.AddListener(new UnityAction(this.Remove));
		}

		// Token: 0x06000AF0 RID: 2800 RVA: 0x00030250 File Offset: 0x0002E450
		protected override void OnAddButton()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnAddButton", Array.Empty<object>());
			}
			UIInputFieldListModel uiinputFieldListModel = (UIInputFieldListModel)base.ListModel;
			UIInputFieldListView uiinputFieldListView = (UIInputFieldListView)base.ListView;
			bool flag = uiinputFieldListView.ContentType == TMP_InputField.ContentType.IntegerNumber || uiinputFieldListView.ContentType == TMP_InputField.ContentType.DecimalNumber || uiinputFieldListView.CharacterValidation == TMP_InputField.CharacterValidation.Integer || uiinputFieldListView.CharacterValidation == TMP_InputField.CharacterValidation.Decimal;
			base.ListModel.Add(flag ? "0" : string.Empty, true);
		}

		// Token: 0x06000AF1 RID: 2801 RVA: 0x000302D0 File Offset: 0x0002E4D0
		private void SetValue(string newValue)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetValue", new object[] { newValue });
			}
			base.ListModel.SetItem(base.DataIndex, newValue, true);
		}

		// Token: 0x0400070D RID: 1805
		[Header("UIInputFieldListCellController")]
		[SerializeField]
		private UIInputField inputField;

		// Token: 0x0400070E RID: 1806
		[SerializeField]
		private UIButton removeButton;
	}
}
