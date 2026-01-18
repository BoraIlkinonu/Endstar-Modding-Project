using System;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIUserScriptAutocompleteListCellController : UIBaseListCellController<UIUserScriptAutocompleteListModelItem>
{
	public static Action<string> OnSelect;

	[Header("UIUserScriptAutocompleteListCellController")]
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
		OnSelect?.Invoke(base.Model.Value);
	}
}
