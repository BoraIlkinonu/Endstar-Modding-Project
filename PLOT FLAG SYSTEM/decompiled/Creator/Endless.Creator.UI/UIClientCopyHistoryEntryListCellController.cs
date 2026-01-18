using System;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIClientCopyHistoryEntryListCellController : UIBaseListCellController<ClientCopyHistoryEntry>
{
	[Header("UIClientCopyHistoryEntryListCellController")]
	[SerializeField]
	private UIToggle selectedToggle;

	[SerializeField]
	private UIInputField labelInputField;

	[SerializeField]
	private UIButton removeButton;

	private CopyTool copyTool;

	protected override void Start()
	{
		base.Start();
		copyTool = MonoBehaviourSingleton<ToolManager>.Instance.RequestToolInstance<CopyTool>();
		selectedToggle.OnChange.AddListener(SetSelectedCopyIndex);
		labelInputField.onSubmit.AddListener(UpdateLabel);
		removeButton.onClick.AddListener(Remove);
	}

	protected override void OnAddButton()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnAddButton");
		}
		throw new NotImplementedException();
	}

	protected override void Remove()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Remove");
		}
		base.Remove();
		copyTool.RemoveIndex(base.DataIndex);
	}

	private void SetSelectedCopyIndex(bool state)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "SetSelectedCopyIndex", state);
		}
		if (state)
		{
			copyTool.SetSelectedCopyIndex(base.DataIndex);
		}
		else
		{
			selectedToggle.SetIsOn(state: true, suppressOnChange: true);
		}
	}

	private void UpdateLabel(string newValue)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "UpdateLabel", newValue);
		}
		copyTool.UpdateCopyLabel(newValue, base.DataIndex);
	}
}
