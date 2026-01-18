using System;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIRolesListCellController : UIBaseListCellController<Roles>
{
	public static Action<Roles> OnSelected;

	[Header("UIRolesListCellController")]
	[SerializeField]
	private UIButton selectButton;

	protected override void Start()
	{
		base.Start();
		selectButton.onClick.AddListener(Select);
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
		OnSelected?.Invoke(base.Model);
	}
}
