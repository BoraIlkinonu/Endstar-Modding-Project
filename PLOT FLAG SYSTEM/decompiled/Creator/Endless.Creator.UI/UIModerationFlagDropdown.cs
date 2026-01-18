using System;
using Endless.Matchmaking;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIModerationFlagDropdown : UIBaseDropdown<ModerationFlag>
{
	protected override void Start()
	{
		base.Start();
		SetOptionsAndValue(EndlessCloudService.AllModerationFlags, Array.Empty<ModerationFlag>(), triggerValueChanged: false);
	}

	protected override string GetLabelFromOption(int optionIndex)
	{
		return base.Options[optionIndex].NiceName;
	}

	protected override Sprite GetIconFromOption(int optionIndex)
	{
		return null;
	}

	public override bool OptionShouldBeHidden(int index)
	{
		return !base.Options[index].IsActive;
	}
}
