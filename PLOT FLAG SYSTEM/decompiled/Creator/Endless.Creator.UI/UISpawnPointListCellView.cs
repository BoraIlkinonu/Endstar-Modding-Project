using System;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;

namespace Endless.Creator.UI;

public class UISpawnPointListCellView : UIBaseListCellView<UISpawnPoint>
{
	[Header("UISpawnPointListCellView")]
	[SerializeField]
	private UIButton toggleSelectedButton;

	[SerializeField]
	private TextMeshProUGUI displayNameText;

	[SerializeField]
	private GameObject[] setActiveIfLocalClientCanSelect = Array.Empty<GameObject>();

	[SerializeField]
	private UIButton moveUpButton;

	[SerializeField]
	private UIButton moveDownButton;

	[SerializeField]
	private UIButton removeButton;

	[SerializeField]
	private TweenCollection displayExtraEditButtonsTweens;

	[SerializeField]
	private TweenCollection hideExtraEditButtonsTweens;

	private bool extraEditButtonsActive;

	protected override void Start()
	{
		base.Start();
		hideExtraEditButtonsTweens.SetToEnd();
	}

	public override void OnDespawn()
	{
		base.OnDespawn();
		extraEditButtonsActive = false;
		hideExtraEditButtonsTweens.SetToEnd();
	}

	public override void View(UIBaseListView<UISpawnPoint> listView, int dataIndex)
	{
		base.View(listView, dataIndex);
		UISpawnPointListModel uISpawnPointListModel = (UISpawnPointListModel)base.ListModel;
		bool localClientCanSelect = uISpawnPointListModel.LocalClientCanSelect;
		UISpawnPointListView uISpawnPointListView = base.ListView as UISpawnPointListView;
		toggleSelectedButton.interactable = localClientCanSelect && uISpawnPointListView.CanSelect;
		displayNameText.text = base.Model.DisplayName;
		GameObject[] array = setActiveIfLocalClientCanSelect;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(localClientCanSelect);
		}
		moveUpButton.interactable = dataIndex > 0;
		moveDownButton.interactable = dataIndex < base.ListModel.Count - 1;
		removeButton.gameObject.SetActive(uISpawnPointListModel.UserCanRemove);
	}

	public void ToggleExtraEditButtons()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "ToggleExtraEditButtons");
		}
		if (extraEditButtonsActive)
		{
			hideExtraEditButtonsTweens.Tween();
		}
		else
		{
			displayExtraEditButtonsTweens.Tween();
		}
		extraEditButtonsActive = !extraEditButtonsActive;
	}
}
