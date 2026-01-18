using System;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIGameView : UIAssetWithScreenshotsView<Game>
{
	[Header("UIGameView")]
	[SerializeField]
	private UIButton[] addScreenshotButtons = Array.Empty<UIButton>();

	public override void SetLocalUserCanInteract(bool localUserCanInteract)
	{
		base.SetLocalUserCanInteract(localUserCanInteract);
		UIButton[] array = addScreenshotButtons;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].interactable = localUserCanInteract;
		}
	}
}
