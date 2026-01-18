using System;
using System.Collections.Generic;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIGameLibrarySelectionWindowView : UIBaseWindowView
{
	[Header("UIGameLibrarySelectionWindowView")]
	[SerializeField]
	private UIGameLibraryListModel gameLibraryListModel;

	public Action<SerializableGuid> OnSelected { get; private set; }

	public static UIGameLibrarySelectionWindowView Display(UIGameAssetTypes filter, SerializableGuid selected, Action<SerializableGuid> onSelected, Transform parent = null)
	{
		Dictionary<string, object> supplementalData = new Dictionary<string, object>
		{
			{ "filter", filter },
			{ "selected", selected },
			{ "OnSelected", onSelected }
		};
		return (UIGameLibrarySelectionWindowView)MonoBehaviourSingleton<UIWindowManager>.Instance.Display<UIGameLibrarySelectionWindowView>(parent, supplementalData);
	}

	public override void Initialize(Dictionary<string, object> supplementalData)
	{
		base.Initialize(supplementalData);
		UIGameAssetTypes value = (UIGameAssetTypes)supplementalData["filter"];
		SerializableGuid serializableGuid = (SerializableGuid)supplementalData["selected"];
		OnSelected = supplementalData["OnSelected"] as Action<SerializableGuid>;
		gameLibraryListModel.SetAssetTypeFilter(value, triggerEvents: true);
		int num = -1;
		if (!serializableGuid.IsEmpty)
		{
			for (int i = 0; i < gameLibraryListModel.FilteredList.Count; i++)
			{
				if (!((SerializableGuid)gameLibraryListModel.FilteredList[i].AssetID != serializableGuid))
				{
					num = i;
					break;
				}
			}
		}
		if (num > -1)
		{
			gameLibraryListModel.Select(num, triggerEvents: true);
		}
	}

	public override void OnDespawn()
	{
		base.OnDespawn();
		gameLibraryListModel.Clear(triggerEvents: true);
	}
}
