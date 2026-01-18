using System;
using Endless.Gameplay.Scripting;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Endless.Creator.UI
{
	// Token: 0x020000E8 RID: 232
	public class UIConsoleMessageListCellView : UIBaseListCellView<UIConsoleMessageModel>
	{
		// Token: 0x060003E0 RID: 992 RVA: 0x00018B23 File Offset: 0x00016D23
		protected override void Start()
		{
			base.Start();
			this.copyButton.onClick.AddListener(new UnityAction(this.OnCopyButtonClicked));
		}

		// Token: 0x060003E1 RID: 993 RVA: 0x00018B47 File Offset: 0x00016D47
		private void OnCopyButtonClicked()
		{
			GUIUtility.systemCopyBuffer = this.messageModel.Message.Message;
		}

		// Token: 0x060003E2 RID: 994 RVA: 0x00018B60 File Offset: 0x00016D60
		public override void View(UIBaseListView<UIConsoleMessageModel> listView, int dataIndex)
		{
			base.View(listView, dataIndex);
			this.messageModel = listView.Model[dataIndex];
			ConsoleMessage message = this.messageModel.Message;
			switch (this.messageModel.Message.LogType)
			{
			case LogType.Error:
			case LogType.Assert:
			case LogType.Exception:
				this.iconImage.sprite = this.errorIcon;
				this.iconImage.color = this.errorColor;
				break;
			case LogType.Warning:
				this.iconImage.sprite = this.warningIcon;
				this.iconImage.color = this.warningColor;
				break;
			case LogType.Log:
				this.iconImage.sprite = this.infoIcon;
				this.iconImage.color = this.infoColor;
				break;
			}
			this.backerBar.color = ((dataIndex % 2 == 0) ? this.evenColor : this.oddColor);
			this.label.text = string.Concat(new string[]
			{
				"[",
				message.Timestamp.ToLocalTime().ToLongTimeString(),
				"] ",
				message.ShouldDisplayLineNumber ? string.Format("Line: {0} | ", message.LineNumber) : string.Empty,
				message.Message
			});
		}

		// Token: 0x040003F2 RID: 1010
		[SerializeField]
		[Header("Icons")]
		private Sprite infoIcon;

		// Token: 0x040003F3 RID: 1011
		[SerializeField]
		private global::UnityEngine.Color infoColor;

		// Token: 0x040003F4 RID: 1012
		[SerializeField]
		private Sprite errorIcon;

		// Token: 0x040003F5 RID: 1013
		[SerializeField]
		private global::UnityEngine.Color errorColor;

		// Token: 0x040003F6 RID: 1014
		[SerializeField]
		private Sprite warningIcon;

		// Token: 0x040003F7 RID: 1015
		[SerializeField]
		private global::UnityEngine.Color warningColor;

		// Token: 0x040003F8 RID: 1016
		[SerializeField]
		private Image iconImage;

		// Token: 0x040003F9 RID: 1017
		[SerializeField]
		private TextMeshProUGUI label;

		// Token: 0x040003FA RID: 1018
		[SerializeField]
		private Image backerBar;

		// Token: 0x040003FB RID: 1019
		[SerializeField]
		private global::UnityEngine.Color evenColor;

		// Token: 0x040003FC RID: 1020
		[SerializeField]
		private global::UnityEngine.Color oddColor;

		// Token: 0x040003FD RID: 1021
		[SerializeField]
		private UIButton copyButton;

		// Token: 0x040003FE RID: 1022
		private UIConsoleMessageModel messageModel;
	}
}
