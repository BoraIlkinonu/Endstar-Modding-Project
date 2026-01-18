using System;
using Endless.Props.Assets;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIScriptReferenceListCellController : UIBaseListCellController<ScriptReference>
{
	[Header("UIScriptReferenceListCellView")]
	[SerializeField]
	private UIButton removeButton;

	protected override void Start()
	{
		base.Start();
		removeButton.onClick.AddListener(Remove);
	}

	protected override void OnAddButton()
	{
		throw new NotImplementedException();
	}
}
