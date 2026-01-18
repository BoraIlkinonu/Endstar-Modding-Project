using System;
using Endless.Gameplay;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UICreatorPlayerListView : UIBaseListView<PlayerReferenceManager>
{
	[Header("UICreatorPlayerListView")]
	[SerializeField]
	private UIDisplayAndHideHandler displayAndHideHandler;

	protected override void Start()
	{
		base.Start();
		UIScreenManager.OnScreenSystemOpen = (Action)Delegate.Combine(UIScreenManager.OnScreenSystemOpen, new Action(displayAndHideHandler.Hide));
		UIScreenManager.OnScreenSystemClose = (Action)Delegate.Combine(UIScreenManager.OnScreenSystemClose, new Action(displayAndHideHandler.Display));
	}
}
