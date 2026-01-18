using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIGameAssetListCellSelectableController : UIGameAssetListCellController
{
	[Header("UIGameAssetListCellSelectableController")]
	[SerializeField]
	private UIButton select;

	protected override void Start()
	{
		base.Start();
		select.onClick.AddListener(Select);
	}

	protected override void Select()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} | {1}: {2}, {3}: {4}", "Select", "DataIndex", base.DataIndex, "Model", base.Model), this);
		}
		base.ListModel.Select(base.DataIndex, triggerEvents: true);
	}
}
