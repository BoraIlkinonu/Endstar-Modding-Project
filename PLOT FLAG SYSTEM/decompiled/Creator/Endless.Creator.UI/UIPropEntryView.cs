using System;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Creator.UI;

public class UIPropEntryView : UIBaseView<PropEntry, UIPropEntryView.Styles>
{
	public enum Styles
	{
		Default
	}

	private static readonly SerializableGuid useContextPropEntryInstanceId = SerializableGuid.NewGuid();

	[SerializeField]
	private Image iconImage;

	[SerializeField]
	private TextMeshProUGUI labelText;

	[SerializeField]
	private Sprite nullDisplayIconImage;

	[SerializeField]
	private Sprite useContextIconImage;

	[field: Header("UIPropEntryView")]
	[field: SerializeField]
	public override Styles Style { get; protected set; }

	public static PropEntry UseContext { get; private set; } = new PropEntry
	{
		InstanceId = useContextPropEntryInstanceId,
		Label = "Use Context"
	};

	public override void View(PropEntry model)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "View", model);
		}
		labelText.text = model.Label;
		if (model.InstanceId == useContextPropEntryInstanceId)
		{
			iconImage.sprite = useContextIconImage;
			return;
		}
		try
		{
			PropLibrary.RuntimePropInfo runtimePropInfo = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(model.AssetId);
			iconImage.sprite = runtimePropInfo.Icon;
		}
		catch (Exception)
		{
			labelText.text = "null";
			iconImage.sprite = nullDisplayIconImage;
		}
	}

	public override void Clear()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Clear");
		}
		iconImage.sprite = null;
	}
}
