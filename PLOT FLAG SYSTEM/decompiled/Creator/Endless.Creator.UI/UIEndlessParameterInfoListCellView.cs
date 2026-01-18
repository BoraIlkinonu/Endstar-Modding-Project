using System;
using Endless.Gameplay.Serialization;
using Endless.Props.Scripting;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIEndlessParameterInfoListCellView : UIBaseListCellView<EndlessParameterInfo>
{
	[Header("UIEndlessParameterInfoListCellView")]
	[SerializeField]
	private TextMeshProUGUI displayNameText;

	[SerializeField]
	private TextMeshProUGUI dataTypeText;

	public override void View(UIBaseListView<EndlessParameterInfo> listView, int dataIndex)
	{
		base.View(listView, dataIndex);
		displayNameText.text = base.Model.DisplayName;
		Type typeFromId = EndlessTypeMapping.Instance.GetTypeFromId(base.Model.DataType);
		string text = typeFromId.Name;
		if (typeFromId == typeof(float))
		{
			text = "Float";
		}
		else if (typeFromId == typeof(float[]))
		{
			text = "Float[]";
		}
		else if (typeFromId == typeof(int))
		{
			text = "Int";
		}
		else if (typeFromId == typeof(int[]))
		{
			text = "Int[]";
		}
		else if (typeFromId == typeof(bool))
		{
			text = "Bool";
		}
		else if (typeFromId == typeof(bool[]))
		{
			text = "Bool[]";
		}
		dataTypeText.text = text;
	}
}
