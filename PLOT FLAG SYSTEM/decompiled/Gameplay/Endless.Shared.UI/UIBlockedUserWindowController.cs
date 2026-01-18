using System.Collections.Generic;
using Endless.Data;
using Endless.Gameplay.UI;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Runtime.Shared.Matchmaking;
using UnityEngine;

namespace Endless.Shared.UI;

public class UIBlockedUserWindowController : UIWindowController
{
	[Header("UIBlockedUserWindowController")]
	[SerializeField]
	private UIBlockedUserWindowView view;

	[SerializeField]
	private EndlessStudiosUserId endlessStudiosUserId;

	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private UIIEnumerablePresenter blockedUsers;

	[SerializeField]
	private UIButton addButton;

	private UIBlockedUserWindowModel Model => view.Model;

	protected override void Start()
	{
		base.Start();
		addButton.onClick.AddListener(OpenAddUserSelectionWindow);
	}

	private void OpenAddUserSelectionWindow()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OpenAddUserSelectionWindow");
		}
		UIIEnumerableWindowView.Display(new UIIEnumerableWindowModel(canvas.sortingOrder + 1, "Friends", UIBaseIEnumerableView.ArrangementStyle.StraightVerticalVirtualized, SelectionType.Select0OrMore, Model.Friends, null, new List<object>(), null, BlockUsers));
	}

	private void BlockUsers(List<object> users)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "BlockUsers", users.Count);
		}
		List<object> list = new List<object>(blockedUsers.ModelList);
		foreach (object user2 in users)
		{
			User user = user2 as User;
			list.Add(user);
			EndlessServices.Instance.CloudService.BlockUserAsync(user.Id, base.VerboseLogging);
			EndlessServices.Instance.CloudService.UnfriendAsync(user.Id, base.VerboseLogging);
		}
		blockedUsers.SetModel(list, triggerOnModelChanged: true);
	}
}
