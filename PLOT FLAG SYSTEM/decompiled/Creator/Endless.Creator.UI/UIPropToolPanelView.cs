using System;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIPropToolPanelView : UIItemSelectionToolPanelView<PropTool, PropLibrary.RuntimePropInfo>
{
	[Header("UIPropToolPanelView")]
	[SerializeField]
	private UIRuntimePropInfoListModel runtimePropInfoListModel;

	private SerializableGuid selectedAssetId;

	private bool inCreatorMode;

	private UIScriptWindowView scriptWindow;

	protected override bool HasSelectedItem => Tool.SelectedAssetId != SerializableGuid.Empty;

	protected override bool CanViewDetail => true;

	protected override void Start()
	{
		base.Start();
		NetworkBehaviourSingleton<CreatorManager>.Instance.OnCreatorStarted.AddListener(OnCreatorStarted);
		NetworkBehaviourSingleton<CreatorManager>.Instance.OnCreatorEnded.AddListener(OnCreatorEnded);
		NetworkBehaviourSingleton<CreatorManager>.Instance.OnPropsRepopulated += OnLibraryRepopulated;
		Tool.OnSelectedAssetChanged.AddListener(OnSelectedAssetChanged);
		MonoBehaviourSingleton<UIWindowManager>.Instance.DisplayUnityEvent.AddListener(OnWindowDisplayed);
	}

	protected override void OnToolChange(EndlessTool activeTool)
	{
		base.OnToolChange(activeTool);
		if (!(activeTool.GetType() != typeof(PropTool)))
		{
			runtimePropInfoListModel.Synchronize();
			if (!Tool.PreviousSelectedAssetId.IsEmpty)
			{
				Tool.UpdateSelectedAssetId(Tool.PreviousSelectedAssetId);
			}
		}
	}

	private void OnLibraryRepopulated()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnLibraryRepopulated");
		}
		runtimePropInfoListModel.Synchronize();
		if (!Tool.PreviousSelectedAssetId.IsEmpty)
		{
			Tool.UpdateSelectedAssetId(Tool.PreviousSelectedAssetId);
		}
	}

	private void OnSelectedAssetChanged(SerializableGuid selectedAssetId)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnSelectedAssetChanged", selectedAssetId);
		}
		if (this.selectedAssetId == selectedAssetId)
		{
			_ = !selectedAssetId.IsEmpty;
		}
		else
			_ = 0;
		this.selectedAssetId = selectedAssetId;
		if (selectedAssetId.IsEmpty)
		{
			OnItemSelectionEmpty();
			return;
		}
		PropLibrary.RuntimePropInfo runtimePropInfo = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(selectedAssetId);
		if (runtimePropInfo.IsMissingObject)
		{
			Tool.UpdateSelectedAssetId(SerializableGuid.Empty);
		}
		else
		{
			ViewSelectedItem(runtimePropInfo);
		}
	}

	private void OnCreatorStarted()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnCreatorStarted");
		}
		inCreatorMode = true;
	}

	private void OnCreatorEnded()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnCreatorEnded");
		}
		inCreatorMode = false;
	}

	private void OnWindowDisplayed(UIBaseWindowView displayedWindow)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnWindowDisplayed", displayedWindow.GetType());
			DebugUtility.Log(string.Format("{0}: {1}", "inCreatorMode", inCreatorMode), this);
			DebugUtility.Log(string.Format("{0}.{1}: {2}", "Tool", "IsActive", Tool.IsActive), this);
			DebugUtility.Log(string.Format("is{0}: {1}", "UIScriptWindowView", displayedWindow is UIScriptWindowView), this);
		}
		if (inCreatorMode && Tool.IsActive && displayedWindow is UIScriptWindowView)
		{
			scriptWindow = (UIScriptWindowView)displayedWindow;
			scriptWindow.CloseUnityEvent.AddListener(OnScriptWindowClosed);
			Dock();
		}
	}

	private void OnScriptWindowClosed()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnScriptWindowClosed");
		}
		Undock();
		if ((bool)scriptWindow)
		{
			scriptWindow.CloseUnityEvent.RemoveListener(OnScriptWindowClosed);
			scriptWindow = null;
		}
		else
		{
			DebugUtility.LogException(new NullReferenceException("scriptWindow is null on OnScriptWindowClosed!"), this);
		}
	}
}
