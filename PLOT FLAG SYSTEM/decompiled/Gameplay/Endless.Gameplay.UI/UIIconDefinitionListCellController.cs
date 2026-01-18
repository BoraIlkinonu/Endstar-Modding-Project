using System;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Gameplay.UI;

public class UIIconDefinitionListCellController : UIBaseListCellController<IconDefinition>
{
	[Header("UIIconDefinitionListCellController")]
	[SerializeField]
	private UIIconDefinitionController iconDefinitionController;

	protected override void Start()
	{
		base.Start();
		iconDefinitionController.SelectUnityEvent.AddListener(SelectIconDefinition);
	}

	protected override void OnAddButton()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnAddButton");
		}
		throw new NotImplementedException();
	}

	private void SelectIconDefinition(IconDefinition iconDefinition)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "SelectIconDefinition", iconDefinition);
		}
		ToggleSelected();
	}
}
