using System;
using Endless.Props.Scripting;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIInspectorScriptValueListCellController : UIBaseListCellController<InspectorScriptValue>
{
	[Header("UIInspectorScriptValueListCellController")]
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
