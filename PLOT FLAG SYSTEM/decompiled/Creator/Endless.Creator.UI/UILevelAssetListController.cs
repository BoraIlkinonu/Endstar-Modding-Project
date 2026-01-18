using System;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UILevelAssetListController : UIBaseLocalFilterableListController<LevelAsset>
{
	[Header("UILevelAssetListController")]
	[SerializeField]
	private UIToggle showArchivedToggled;

	protected override void Start()
	{
		base.Start();
		showArchivedToggled.OnChange.AddListener(OnShowArchivedToggledChanged);
	}

	protected override bool IncludeInFilteredResults(LevelAsset item)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "IncludeInFilteredResults", item.Name);
		}
		if (item == null)
		{
			DebugUtility.LogError("LevelAsset was null!", this);
			return false;
		}
		string text = item.Name;
		if (!base.CaseSensitive)
		{
			text = text.ToLower();
		}
		string text2 = base.StringFilter;
		if (!base.CaseSensitive)
		{
			text2 = text2.ToLower();
		}
		return text.Contains(text2);
	}

	private void OnShowArchivedToggledChanged(bool showArchived)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnShowArchivedToggledChanged", showArchived);
		}
		throw new NotImplementedException();
	}
}
