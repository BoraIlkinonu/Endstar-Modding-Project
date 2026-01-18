using Endless.Creator.LevelEditing.Runtime;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UILevelEditorWindowController : UIWindowController
{
	[Header("UILevelEditorWindowController")]
	[SerializeField]
	private UIScreenshotFileInstancesListModel screenshotFileInstancesListModel;

	[SerializeField]
	private UISpawnPointListModel spawnPointListModel;

	protected override void Start()
	{
		base.Start();
		BaseWindowView.CloseUnityEvent.AddListener(OnClosing);
		spawnPointListModel.ItemSelectedUnityEvent.AddListener(OnSpawnPointSelected);
		spawnPointListModel.ItemUnselectedUnityEvent.AddListener(OnSpawnPointUnselected);
		spawnPointListModel.ItemSwappedUnityEvent.AddListener(OnItemSwappedUnityEvent);
	}

	public override void Close()
	{
		base.Close();
		MonoBehaviourSingleton<ToolManager>.Instance.SetActiveTool(ToolType.Empty);
	}

	private void OnSpawnPointsChanged()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnSpawnPointsChanged");
		}
		SerializableGuid[] spawnPointIdOrder = spawnPointListModel.SpawnPointIds.ToArray();
		SerializableGuid[] selectedSpawnPoints = spawnPointListModel.SelectedSpawnPointIds.ToArray();
		NetworkBehaviourSingleton<CreatorManager>.Instance.LevelEditor.UpdateLevelSpawnPointOrder_ServerRpc(spawnPointIdOrder, selectedSpawnPoints);
	}

	private void OnClosing()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnClosing");
		}
		if (MonoBehaviourSingleton<ToolManager>.Instance.ActiveTool.ToolType == ToolType.LevelEditor)
		{
			MonoBehaviourSingleton<ToolManager>.Instance.SetActiveTool(ToolType.Empty);
		}
	}

	private void OnSpawnPointSelected(int index)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnSpawnPointSelected", index);
		}
		OnSpawnPointsChanged();
	}

	private void OnSpawnPointUnselected(int index)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnSpawnPointUnselected", index);
		}
		OnSpawnPointsChanged();
	}

	private void OnItemSwappedUnityEvent(int indexA, int indexB)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnItemSwappedUnityEvent", indexA, indexB);
		}
		OnSpawnPointsChanged();
	}
}
