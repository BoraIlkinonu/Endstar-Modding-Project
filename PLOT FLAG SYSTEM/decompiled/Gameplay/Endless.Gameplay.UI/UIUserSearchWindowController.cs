using System.Collections.Generic;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Gameplay.UI;

public class UIUserSearchWindowController : UIWindowController
{
	[Header("UIUserSearchWindowController")]
	[SerializeField]
	private UIUserSearchWindowView view;

	[SerializeField]
	private UIInputField userNameSearchInputField;

	[SerializeField]
	private UIUserPaginatedGraphQlIEnumerableHandler userPaginatedGraphQlIEnumerableHandler;

	[SerializeField]
	private UIIEnumerablePresenter iEnumerablePresenter;

	[SerializeField]
	private UIButton confirmSelectionButton;

	private UIUserSearchWindowModel Model => view.Model;

	protected override void Start()
	{
		base.Start();
		userNameSearchInputField.onValueChanged.AddListener(Search);
		confirmSelectionButton.onClick.AddListener(ConfirmSelection);
		iEnumerablePresenter.OnModelChanged += HandleConfirmSelectionButtonInteractability;
	}

	private void Search(string userName)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Search", userName);
		}
		List<User> usersToHide = new List<User>(Model.UsersToHide);
		userPaginatedGraphQlIEnumerableHandler.SetUserNameToSearchFor(userName, usersToHide);
	}

	private void ConfirmSelection()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "ConfirmSelection");
		}
		Model.OnSelectionConfirmed?.Invoke(new List<object>(iEnumerablePresenter.SelectedItemsList));
		Close();
	}

	private void HandleConfirmSelectionButtonInteractability(object model)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "HandleConfirmSelectionButtonInteractability", model);
		}
		confirmSelectionButton.interactable = iEnumerablePresenter.SelectedItemsList.Count > 0;
	}
}
