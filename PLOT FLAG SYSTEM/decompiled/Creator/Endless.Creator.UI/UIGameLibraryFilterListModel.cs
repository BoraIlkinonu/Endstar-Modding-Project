using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIGameLibraryFilterListModel : UIBaseLocalFilterableListModel<UIGameAssetTypes>
{
	[Header("UIGameLibraryFilterListModel")]
	[SerializeField]
	private InterfaceReference<IGameAssetListModel>[] gameAssetListModels = Array.Empty<InterfaceReference<IGameAssetListModel>>();

	[SerializeField]
	private bool doNotAllowNoneSelection;

	public bool DoNotAllowNoneSelection => doNotAllowNoneSelection;

	protected override Comparison<UIGameAssetTypes> DefaultSort => (UIGameAssetTypes x, UIGameAssetTypes y) => x.CompareTo(y);

	private void Start()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		if (Count == 0)
		{
			bool triggerEvents = !base.RestrictSelectionCountTo1;
			Initialize(triggerEvents);
		}
		if (base.RestrictSelectionCountTo1 && Count > 0 && SelectedTypedList.Count < 1)
		{
			Select(1, triggerEvents: true);
		}
		base.ModelChangedUnityEvent.AddListener(ApplyFilter);
	}

	public override void Unselect(int index, bool triggerEvents)
	{
		if (this[index] == UIGameAssetTypes.None || !base.RestrictSelectionCountTo1 || !doNotAllowNoneSelection)
		{
			base.Unselect(index, triggerEvents);
		}
	}

	public void SelectFilters(UIGameAssetTypes toSelect)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "SelectFilters", toSelect);
		}
		if (Count == 0)
		{
			Initialize(triggerEvents: true);
		}
		bool flag = toSelect == UIGameAssetTypes.None;
		ClearSelected(flag);
		if (flag)
		{
			return;
		}
		Array values = Enum.GetValues(typeof(UIGameAssetTypes));
		for (int i = 0; i < values.Length; i++)
		{
			UIGameAssetTypes uIGameAssetTypes = (UIGameAssetTypes)values.GetValue(i);
			if (uIGameAssetTypes != UIGameAssetTypes.None && (toSelect & uIGameAssetTypes) == uIGameAssetTypes)
			{
				if (base.VerboseLogging)
				{
					DebugUtility.Log(string.Format("Selecting {0} with an {1} of {2}", uIGameAssetTypes, "index", i), this);
				}
				Select(i, triggerEvents: false);
				if (base.RestrictSelectionCountTo1)
				{
					break;
				}
			}
		}
		TriggerModelChanged();
	}

	private void Initialize(bool triggerEvents)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Initialize", triggerEvents);
		}
		if (Count <= 0)
		{
			List<UIGameAssetTypes> list = ((UIGameAssetTypes[])Enum.GetValues(typeof(UIGameAssetTypes))).ToList();
			if (!Application.isEditor)
			{
				list.Remove(UIGameAssetTypes.SFX);
				list.Remove(UIGameAssetTypes.Ambient);
				list.Remove(UIGameAssetTypes.Music);
			}
			Set(list, triggerEvents);
		}
	}

	private void ApplyFilter()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "ApplyFilter");
		}
		UIGameAssetTypes uIGameAssetTypes = UIGameAssetTypes.None;
		foreach (UIGameAssetTypes selectedTyped in SelectedTypedList)
		{
			uIGameAssetTypes |= selectedTyped;
		}
		InterfaceReference<IGameAssetListModel>[] array = gameAssetListModels;
		foreach (InterfaceReference<IGameAssetListModel> interfaceReference in array)
		{
			interfaceReference.Interface.SetAssetTypeFilter(uIGameAssetTypes, interfaceReference.Interface.GameObject.activeInHierarchy);
		}
	}

	private void ReselectUnselected(int index)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "ReselectUnselected", index);
		}
		Select(index, triggerEvents: true);
	}
}
