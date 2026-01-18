using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIRuntimePropInfoSelectionWindowView : UIBaseWindowView
{
	[SerializeField]
	private UIRuntimePropInfoListModel runtimePropInfoListModel;

	[SerializeField]
	private UIButton confirmButton;

	[Header("Debugging")]
	[SerializeField]
	private bool superVerboseLogging;

	public Action<IReadOnlyList<PropLibrary.RuntimePropInfo>> OnConfirm { get; private set; }

	public static UIRuntimePropInfoSelectionWindowView Display(SerializableGuid selectedInstanceId, bool restrictSelectionCountTo1, ReferenceFilter referenceFilter, IReadOnlyList<PropLibrary.RuntimePropInfo> propsToIgnore, Action<IReadOnlyList<PropLibrary.RuntimePropInfo>> onConfirm, Transform parent = null)
	{
		Dictionary<string, object> supplementalData = new Dictionary<string, object>
		{
			{ "selectedInstanceId", selectedInstanceId },
			{ "restrictSelectionCountTo1", restrictSelectionCountTo1 },
			{ "referenceFilter", referenceFilter },
			{ "propsToIgnore", propsToIgnore },
			{
				"onConfirm".CapitalizeFirstCharacter(),
				onConfirm
			}
		};
		return (UIRuntimePropInfoSelectionWindowView)MonoBehaviourSingleton<UIWindowManager>.Instance.Display<UIRuntimePropInfoSelectionWindowView>(parent, supplementalData);
	}

	public override void Initialize(Dictionary<string, object> supplementalData)
	{
		base.Initialize(supplementalData);
		SerializableGuid selectedInstanceId = (SerializableGuid)supplementalData["selectedInstanceId"];
		bool newValue = (bool)supplementalData["restrictSelectionCountTo1"];
		ReferenceFilter referenceFilter = (ReferenceFilter)supplementalData["referenceFilter"];
		IReadOnlyList<PropLibrary.RuntimePropInfo> propsToIgnore = (IReadOnlyList<PropLibrary.RuntimePropInfo>)supplementalData["propsToIgnore"];
		OnConfirm = (Action<IReadOnlyList<PropLibrary.RuntimePropInfo>>)supplementalData["OnConfirm"];
		runtimePropInfoListModel.SetRestrictSelectionCountTo1(newValue, triggerEvents: false);
		runtimePropInfoListModel.ClearSelected(triggerEvents: false);
		runtimePropInfoListModel.Synchronize(referenceFilter, propsToIgnore);
		if (!(selectedInstanceId == SerializableGuid.Empty))
		{
			int index = runtimePropInfoListModel.FilteredList.ToList().FindIndex((PropLibrary.RuntimePropInfo item) => (SerializableGuid)item.PropData.AssetID == selectedInstanceId);
			runtimePropInfoListModel.Select(index, triggerEvents: true);
		}
	}
}
