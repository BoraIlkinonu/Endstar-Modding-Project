using Endless.Creator.LevelEditing.Runtime;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UICopyToolPanelController : UIGameObject, IBackable
{
	[SerializeField]
	private UIButton selectMoreButton;

	[SerializeField]
	private UIClientCopyHistoryEntryListModel clientCopyHistoryEntryListModel;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private CopyTool copyTool;

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		selectMoreButton.onClick.AddListener(SelectMore);
		copyTool = MonoBehaviourSingleton<ToolManager>.Instance.RequestToolInstance<CopyTool>();
		copyTool.CopyHistoryEntryInserted.AddListener(CopyHistoryEntryInserted);
		copyTool.CopyHistoryTrimmed.AddListener(CopyHistoryTrimmed);
		copyTool.CopyHistoryCleared.AddListener(CopyHistoryCleared);
	}

	public void OnBack()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnBack");
		}
		SelectMore();
		if (MonoBehaviourSingleton<BackManager>.Instance.HasContext(this))
		{
			MonoBehaviourSingleton<BackManager>.Instance.UnclaimContext(this);
		}
	}

	private void SelectMore()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "SelectMore");
		}
		copyTool.SetSelectedCopyIndex(-1);
		clientCopyHistoryEntryListModel.ClearSelected(triggerEvents: true);
		if (MonoBehaviourSingleton<BackManager>.Instance.HasContext(this))
		{
			MonoBehaviourSingleton<BackManager>.Instance.UnclaimContext(this);
		}
	}

	private void CopyHistoryTrimmed(int endTrimCount)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "CopyHistoryTrimmed", endTrimCount);
		}
		if (!MonoBehaviourSingleton<BackManager>.Instance.HasContext(this))
		{
			MonoBehaviourSingleton<BackManager>.Instance.ClaimContext(this);
		}
	}

	private void CopyHistoryEntryInserted(ClientCopyHistoryEntry clientCopyHistoryEntry)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "CopyHistoryEntryInserted", clientCopyHistoryEntry);
		}
		if (!MonoBehaviourSingleton<BackManager>.Instance.HasContext(this))
		{
			MonoBehaviourSingleton<BackManager>.Instance.ClaimContext(this);
		}
	}

	private void CopyHistoryCleared()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "CopyHistoryCleared");
		}
		if (MonoBehaviourSingleton<BackManager>.Instance.HasContext(this))
		{
			MonoBehaviourSingleton<BackManager>.Instance.UnclaimContext(this);
		}
	}
}
