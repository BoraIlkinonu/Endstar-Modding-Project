using System;
using Endless.Props.Scripting;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIEndlessParameterInfoListCellController : UIBaseListCellController<EndlessParameterInfo>
{
	[Header("UIEndlessParameterInfoListCellController")]
	[SerializeField]
	private UIButton removeButton;

	protected override void Start()
	{
		base.Start();
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
}
