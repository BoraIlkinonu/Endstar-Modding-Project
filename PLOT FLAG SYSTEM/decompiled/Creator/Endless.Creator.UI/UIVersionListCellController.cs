using System;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIVersionListCellController : UIBaseListCellController<string>
{
	[Header("UIVersionListCellView")]
	[SerializeField]
	private PointerUpHandler pointerUpHandler;

	[SerializeField]
	private UIButton revertButton;

	protected override void Start()
	{
		base.Start();
		pointerUpHandler.PointerUpUnityEvent.AddListener(Select);
		revertButton.onClick.AddListener(ConfirmRevert);
	}

	protected override void OnAddButton()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnAddButton");
		}
		throw new NotImplementedException();
	}

	protected override void Select()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Select");
		}
		base.ListModel.Select(base.DataIndex, triggerEvents: true);
	}

	private void ConfirmRevert()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "ConfirmRevert");
		}
		MonoBehaviourSingleton<UIModalManager>.Instance.Confirm("Are you sure you want to revert to v" + base.Model + "?", Revert, MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack);
	}

	private void Revert()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethodWithAppension(this, "Revert", "Model: " + base.Model);
		}
		NetworkBehaviourSingleton<CreatorManager>.Instance.LevelEditor.RevertLevelToVersion_ServerRpc(base.Model);
		MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
	}
}
