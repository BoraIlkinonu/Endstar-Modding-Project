using System;
using Endless.Props.Scripting;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIEndlessEventInfoListCellController : UIBaseListCellController<EndlessEventInfo>
{
	[Header("UIEndlessEventInfoListCellController")]
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
