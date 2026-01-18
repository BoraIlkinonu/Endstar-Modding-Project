using Endless.Creator.LevelEditing.Runtime;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Creator.UI;

public class UIClientCopyHistoryEntryListCellView : UIBaseListCellView<ClientCopyHistoryEntry>
{
	[Header("UIClientCopyHistoryEntryListCellView")]
	[SerializeField]
	private Image displayIconImage;

	[SerializeField]
	private UIToggle selectedToggle;

	[SerializeField]
	private UIInputField labelInputField;

	private CopyTool copyTool;

	public override void OnSpawn()
	{
		base.OnSpawn();
		if (!copyTool)
		{
			copyTool = MonoBehaviourSingleton<ToolManager>.Instance.RequestToolInstance<CopyTool>();
		}
		copyTool.CopyHistoryEntryInserted.AddListener(CopyHistoryEntryInserted);
		copyTool.CopyHistoryTrimmed.AddListener(OnCopyHistoryTrimmed);
	}

	public override void OnDespawn()
	{
		base.OnDespawn();
		copyTool.CopyHistoryEntryInserted.RemoveListener(CopyHistoryEntryInserted);
		copyTool.CopyHistoryTrimmed.RemoveListener(OnCopyHistoryTrimmed);
	}

	public override void View(UIBaseListView<ClientCopyHistoryEntry> listView, int dataIndex)
	{
		base.View(listView, dataIndex);
		PropLibrary.RuntimePropInfo runtimePropInfo = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(base.Model.AssetId);
		displayIconImage.sprite = runtimePropInfo.Icon;
		UpdateSelectedToggle();
		labelInputField.text = base.Model.Label;
	}

	private void UpdateSelectedToggle()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "UpdateSelectedToggle");
		}
		bool state = copyTool.SelectedIndex == base.DataIndex;
		selectedToggle.SetIsOn(state, suppressOnChange: true, tweenVisuals: false);
	}

	private void CopyHistoryEntryInserted(ClientCopyHistoryEntry newValue)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "CopyHistoryEntryInserted", newValue.Label);
		}
		UpdateSelectedToggle();
	}

	private void OnCopyHistoryTrimmed(int endTrimCount)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnCopyHistoryTrimmed", endTrimCount);
		}
		UpdateSelectedToggle();
	}
}
