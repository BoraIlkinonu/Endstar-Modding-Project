using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Endless.Shared.Validation;
using UnityEngine;

namespace Endless.Creator.UI;

public class UILevelStateTemplateListModel : UIBaseLocalFilterableListModel<LevelStateTemplateSourceBase>, IValidatable
{
	[Header("UILevelStateTemplateListModel")]
	[SerializeField]
	private LevelStateTemplateArray levelStateTemplateArray;

	[SerializeField]
	private LevelStateTemplateSourceBase defaultSelected;

	protected override Comparison<LevelStateTemplateSourceBase> DefaultSort => (LevelStateTemplateSourceBase x, LevelStateTemplateSourceBase y) => 0;

	private void OnEnable()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnEnable");
		}
		SetAllEntriesToLevelStateTemplateContainer();
	}

	[ContextMenu("Validate")]
	public void Validate()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Validate");
		}
		if (defaultSelected == null)
		{
			DebugUtility.LogError("defaultSelected can not be null!", this);
			return;
		}
		if (levelStateTemplateArray == null)
		{
			DebugUtility.LogError("levelStateTemplateArray can not be null!", this);
			return;
		}
		bool flag = false;
		foreach (LevelStateTemplateSourceBase item in levelStateTemplateArray)
		{
			if (!(item != defaultSelected))
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			DebugUtility.LogError("The defaultSelected must be in levelStateTemplateArray!", this);
		}
	}

	public override void Clear(bool triggerEvents)
	{
		base.Clear(triggerEvents);
		SetAllEntriesToLevelStateTemplateContainer();
	}

	private void SetAllEntriesToLevelStateTemplateContainer()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "SetAllEntriesToLevelStateTemplateContainer");
		}
		List<LevelStateTemplateSourceBase> list = levelStateTemplateArray.Value.ToList();
		Set(list, triggerEvents: false);
		int index = levelStateTemplateArray.Value.IndexOf(defaultSelected);
		Select(index, triggerEvents: true);
	}
}
