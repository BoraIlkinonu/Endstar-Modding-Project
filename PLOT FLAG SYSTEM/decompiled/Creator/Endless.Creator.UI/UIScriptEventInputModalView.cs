using Endless.Gameplay.Serialization;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.SoVariables;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIScriptEventInputModalView : UIScriptModalView
{
	[Header("UIScriptEventInputModalView")]
	[SerializeField]
	private TextMeshProUGUI modalNameText;

	[SerializeField]
	private IntVariable endlessParameterInfoLimit;

	[SerializeField]
	private UIInputField memberNameInputField;

	[SerializeField]
	private UIInputField descriptionInputField;

	[Header("EndlessParameterInfo Creation")]
	[SerializeField]
	private UIEndlessParameterInfoListModel endlessParameterInfoListModel;

	[SerializeField]
	private UIInputField endlessParameterInfoDisplayNameInputField;

	[SerializeField]
	private UIInspectorScriptValueTypeRadio endlessParameterInfoDataTypeRadio;

	[SerializeField]
	private UIToggle isCollectionToggle;

	[SerializeField]
	private TextMeshProUGUI endlessParameterInfoCounterText;

	[SerializeField]
	private UIButton createEndlessParameterButton;

	public bool IsEvent { get; private set; }

	protected override void Start()
	{
		base.Start();
		endlessParameterInfoListModel.ModelChangedUnityEvent.AddListener(OnEndlessParameterInfoListModelChanged);
	}

	public override void OnDisplay(params object[] modalData)
	{
		base.OnDisplay(modalData);
		IsEvent = (bool)modalData[0];
		modalNameText.text = (IsEvent ? "Create Event" : "Create Receiver");
		memberNameInputField.Clear();
		descriptionInputField.Clear();
		endlessParameterInfoListModel.Clear(triggerEvents: true);
		endlessParameterInfoDisplayNameInputField.Clear();
		endlessParameterInfoDataTypeRadio.SetDefaultValue(EndlessTypeMapping.Instance.LuaInspectorTypes[0]);
		endlessParameterInfoDataTypeRadio.SetValueToDefault(triggerOnValueChanged: true);
		isCollectionToggle.SetIsOn(state: false, suppressOnChange: false);
		OnEndlessParameterInfoListModelChanged();
	}

	public override void OnBack()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnBack");
		}
		MonoBehaviourSingleton<UIModalManager>.Instance.CloseWithoutClearingStack();
	}

	private void OnEndlessParameterInfoListModelChanged()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnEndlessParameterInfoListModelChanged");
		}
		endlessParameterInfoCounterText.text = $"{endlessParameterInfoListModel.Count}/{endlessParameterInfoLimit.Value} Endless Parameter Info";
		createEndlessParameterButton.interactable = endlessParameterInfoListModel.Count < endlessParameterInfoLimit.Value;
	}
}
