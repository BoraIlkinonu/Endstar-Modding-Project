using Endless.Gameplay.Scripting;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Creator.UI;

public class UIConsoleMessageListCellView : UIBaseListCellView<UIConsoleMessageModel>
{
	[SerializeField]
	[Header("Icons")]
	private Sprite infoIcon;

	[SerializeField]
	private UnityEngine.Color infoColor;

	[SerializeField]
	private Sprite errorIcon;

	[SerializeField]
	private UnityEngine.Color errorColor;

	[SerializeField]
	private Sprite warningIcon;

	[SerializeField]
	private UnityEngine.Color warningColor;

	[SerializeField]
	private Image iconImage;

	[SerializeField]
	private TextMeshProUGUI label;

	[SerializeField]
	private Image backerBar;

	[SerializeField]
	private UnityEngine.Color evenColor;

	[SerializeField]
	private UnityEngine.Color oddColor;

	[SerializeField]
	private UIButton copyButton;

	private UIConsoleMessageModel messageModel;

	protected override void Start()
	{
		base.Start();
		copyButton.onClick.AddListener(OnCopyButtonClicked);
	}

	private void OnCopyButtonClicked()
	{
		GUIUtility.systemCopyBuffer = messageModel.Message.Message;
	}

	public override void View(UIBaseListView<UIConsoleMessageModel> listView, int dataIndex)
	{
		base.View(listView, dataIndex);
		messageModel = listView.Model[dataIndex];
		ConsoleMessage message = messageModel.Message;
		switch (messageModel.Message.LogType)
		{
		case LogType.Log:
			iconImage.sprite = infoIcon;
			iconImage.color = infoColor;
			break;
		case LogType.Warning:
			iconImage.sprite = warningIcon;
			iconImage.color = warningColor;
			break;
		case LogType.Error:
		case LogType.Assert:
		case LogType.Exception:
			iconImage.sprite = errorIcon;
			iconImage.color = errorColor;
			break;
		}
		backerBar.color = ((dataIndex % 2 == 0) ? evenColor : oddColor);
		label.text = "[" + message.Timestamp.ToLocalTime().ToLongTimeString() + "] " + (message.ShouldDisplayLineNumber ? $"Line: {message.LineNumber} | " : string.Empty) + message.Message;
	}
}
