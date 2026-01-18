using System.Collections.Generic;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Gameplay.UI;

public class UIUserSearchWindowView : UIBaseWindowView
{
	[Header("UIUserSearchWindowView")]
	[SerializeField]
	private UIText titleText;

	[SerializeField]
	private string defaultTitle = "User Search";

	[SerializeField]
	private UIInputField userNameSearchInputField;

	[SerializeField]
	private UIUserPaginatedGraphQlIEnumerableHandler userPaginatedGraphQlIEnumerableHandler;

	[SerializeField]
	private UIButton confirmSelectionButton;

	public UIUserSearchWindowModel Model { get; set; }

	public static UIUserSearchWindowView Display(UIUserSearchWindowModel model, Transform parent = null)
	{
		Dictionary<string, object> supplementalData = new Dictionary<string, object> { { "model", model } };
		return (UIUserSearchWindowView)MonoBehaviourSingleton<UIWindowManager>.Instance.Display<UIUserSearchWindowView>(parent, supplementalData);
	}

	public override void Initialize(Dictionary<string, object> supplementalData)
	{
		base.Initialize(supplementalData);
		Model = (UIUserSearchWindowModel)supplementalData["Model".ToLower()];
		if (base.VerboseLogging)
		{
			DebugUtility.DebugEnumerable("UsersToHide", Model.UsersToHide, this);
		}
		titleText.Value = (Model.WindowTitle.IsNullOrEmptyOrWhiteSpace() ? defaultTitle : Model.WindowTitle);
		userNameSearchInputField.Select();
		userPaginatedGraphQlIEnumerableHandler.SetSelectionType(Model.SelectionType);
		confirmSelectionButton.interactable = false;
	}

	public override void Close()
	{
		base.Close();
		userNameSearchInputField.Clear();
		userPaginatedGraphQlIEnumerableHandler.Clear();
	}
}
