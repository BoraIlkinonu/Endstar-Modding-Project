using System.Collections.Generic;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UICopyToolPanelView : UIDockableToolPanelView<CopyTool>
{
	[Header("UICopyToolPanelView")]
	[SerializeField]
	private UIDisplayAndHideHandler selectMoreDisplayAndHideHandler;

	[SerializeField]
	private UIClientCopyHistoryEntryListModel clientCopyHistoryEntryListModel;

	[SerializeField]
	private UIClientCopyHistoryEntryListView clientCopyHistoryEntryListView;

	protected override float ListSize => clientCopyHistoryEntryListView.CompleteHeight;

	protected override void Start()
	{
		base.Start();
		selectMoreDisplayAndHideHandler.SetToHideEnd(triggerUnityEvent: true);
		Tool.CopyHistoryEntryInserted.AddListener(ViewClientCopyHistoryList);
		Tool.CopyHistoryTrimmed.AddListener(ViewClientCopyHistoryList);
		Tool.OnSelectedCopyIndexSet.AddListener(ViewClientCopyHistoryList);
		clientCopyHistoryEntryListModel.ItemRemovedUnityEvent.AddListener(OnItemRemoved);
		clientCopyHistoryEntryListModel.ModelChangedUnityEvent.AddListener(HandleSelectMoreVisibility);
	}

	public override void Display()
	{
		base.Display();
		ViewClientCopyHistoryList();
	}

	protected override float GetMaxPanelHeight()
	{
		float num = base.GetMaxPanelHeight();
		if (clientCopyHistoryEntryListModel.Count > 0)
		{
			num += selectMoreDisplayAndHideHandler.RectTransform.rect.height;
		}
		return num;
	}

	private void ViewClientCopyHistoryList(ClientCopyHistoryEntry clientCopyHistoryEntry)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "ViewClientCopyHistoryList", "clientCopyHistoryEntry", clientCopyHistoryEntry), this);
		}
		ViewClientCopyHistoryList();
	}

	private void ViewClientCopyHistoryList(int index)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "ViewClientCopyHistoryList", "index", index), this);
		}
		ViewClientCopyHistoryList();
	}

	private void ViewClientCopyHistoryList()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log("ViewClientCopyHistoryList", this);
		}
		List<ClientCopyHistoryEntry> list = new List<ClientCopyHistoryEntry>(Tool.ClientCopyHistoryEntries);
		clientCopyHistoryEntryListModel.Clear(triggerEvents: false);
		clientCopyHistoryEntryListModel.Set(list, triggerEvents: false);
		clientCopyHistoryEntryListModel.ModelChangedUnityEvent.Invoke();
		TweenToMaxPanelHeight();
	}

	private void OnItemRemoved(int index, ClientCopyHistoryEntry clientCopyHistoryEntry)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnItemRemoved", index, clientCopyHistoryEntry.Label);
		}
		ViewClientCopyHistoryList();
	}

	private void HandleSelectMoreVisibility()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "HandleSelectMoreVisibility");
		}
		if (clientCopyHistoryEntryListModel.Count > 0)
		{
			selectMoreDisplayAndHideHandler.Display();
		}
		else
		{
			selectMoreDisplayAndHideHandler.Hide();
		}
	}
}
