using System;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UISpawnPointListCellController : UIBaseListCellController<UISpawnPoint>
{
	[Header("UISpawnPointListCellController")]
	[SerializeField]
	private UIButton toggleSelectedButton;

	[SerializeField]
	private UIButton displayExtraEditsButton;

	[SerializeField]
	private UIButton hideExtraEditsButton;

	[SerializeField]
	private UIButton moveUpButton;

	[SerializeField]
	private UIButton moveDownButton;

	[SerializeField]
	private UIButton removeButton;

	private UISpawnPointListCellView TypedView => (UISpawnPointListCellView)View;

	private UISpawnPointListModel TypedListModel => (UISpawnPointListModel)base.ListModel;

	protected override void Start()
	{
		base.Start();
		toggleSelectedButton.onClick.AddListener(ToggleSelected);
		displayExtraEditsButton.onClick.AddListener(TypedView.ToggleExtraEditButtons);
		hideExtraEditsButton.onClick.AddListener(TypedView.ToggleExtraEditButtons);
		moveUpButton.onClick.AddListener(MoveUp);
		moveDownButton.onClick.AddListener(MoveDown);
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

	private void MoveUp()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "MoveUp");
		}
		TypedListModel.MoveUp(base.DataIndex, triggerEvents: true);
	}

	private void MoveDown()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "MoveDown");
		}
		TypedListModel.MoveDown(base.DataIndex, triggerEvents: true);
	}
}
