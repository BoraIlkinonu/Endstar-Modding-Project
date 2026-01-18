using System.Collections.Generic;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIConsoleMessageListController : UIBaseLocalFilterableListController<UIConsoleMessageModel>
{
	[SerializeField]
	private UIToggle showInfo;

	[SerializeField]
	private TextMeshProUGUI infoToggleLabel;

	[SerializeField]
	private UIToggle showWarning;

	[SerializeField]
	private TextMeshProUGUI warningToggleLabel;

	[SerializeField]
	private UIToggle showError;

	[SerializeField]
	private TextMeshProUGUI errorToggleLabel;

	protected override void Start()
	{
		base.Start();
		showInfo.OnChange.AddListener(OnToggleChange);
		showWarning.OnChange.AddListener(OnToggleChange);
		showError.OnChange.AddListener(OnToggleChange);
		LocalFilterableListModel.ModelChangedUnityEvent.AddListener(OnModelChange);
		OnModelChange();
	}

	private void OnModelChange()
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		IReadOnlyList<UIConsoleMessageModel> readOnlyList = LocalFilterableListModel.ReadOnlyList;
		for (int i = 0; i < readOnlyList.Count; i++)
		{
			switch (readOnlyList[i].Message.LogType)
			{
			case LogType.Log:
				num++;
				break;
			case LogType.Warning:
				num2++;
				break;
			case LogType.Error:
			case LogType.Assert:
			case LogType.Exception:
				num3++;
				break;
			default:
				Debug.LogError($"Unhandled Message LogType: {LocalFilterableListModel[i].Message.LogType}");
				break;
			}
		}
		infoToggleLabel.text = ((num == 0) ? string.Empty : num.ToString());
		warningToggleLabel.text = ((num2 == 0) ? string.Empty : num2.ToString());
		errorToggleLabel.text = ((num3 == 0) ? string.Empty : num3.ToString());
	}

	private void OnToggleChange(bool newValue)
	{
		LocalFilterableListModel.Filter(IncludeInFilteredResults, triggerEvents: true);
	}

	protected override bool IncludeInFilteredResults(UIConsoleMessageModel item)
	{
		if (base.SuperVerboseLogging)
		{
			DebugUtility.LogMethod(this, "IncludeInFilteredResults", item);
		}
		if (item == null)
		{
			DebugUtility.LogError("PropLibrary.RuntimePropInfo was null!", this);
			return false;
		}
		string text = item.Message.Message;
		if (!base.CaseSensitive)
		{
			text = text.ToLower();
		}
		string text2 = base.StringFilter;
		if (!base.CaseSensitive)
		{
			text2 = text2.ToLower();
		}
		switch (item.Message.LogType)
		{
		case LogType.Log:
			if (!showInfo.IsOn)
			{
				return false;
			}
			break;
		case LogType.Warning:
			if (!showWarning.IsOn)
			{
				return false;
			}
			break;
		case LogType.Error:
		case LogType.Assert:
		case LogType.Exception:
			if (!showError.IsOn)
			{
				return false;
			}
			break;
		default:
			Debug.LogError($"Unhandled Message LogType: {item.Message.LogType}");
			break;
		}
		return text.Contains(text2);
	}
}
