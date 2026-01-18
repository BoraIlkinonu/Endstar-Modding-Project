using Endless.Gameplay;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIPlayerReferenceListCellController : UIBaseListCellController<PlayerReference>
{
	[Header("UIPlayerReferenceListCellController")]
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
		PlayerReference item = new PlayerReference();
		base.ListModel.Add(item, triggerEvents: true);
	}
}
