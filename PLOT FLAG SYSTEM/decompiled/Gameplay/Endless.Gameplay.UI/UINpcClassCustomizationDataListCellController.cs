using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Gameplay.UI;

public class UINpcClassCustomizationDataListCellController : UIBaseListCellController<NpcClassCustomizationData>
{
	[Header("UINpcClassCustomizationDataListCellController")]
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
		NpcClassCustomizationData item = new GruntNpcCustomizationData();
		base.ListModel.Add(item, triggerEvents: true);
	}
}
