using System;
using Endless.Gameplay.Stats;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIPerPlayerStatView : UIStatBaseView<PerPlayerStat>
{
	[SerializeField]
	private UIInputField defaultValueInputField;

	[SerializeField]
	private UIEnumPresenter displayFormatControl;

	public event Action<string> DefaultValueChanged;

	public event Action<NumericDisplayFormat> DisplayFormatChanged;

	protected override void Start()
	{
		base.Start();
		defaultValueInputField.onValueChanged.AddListener(InvokeDefaultValueChanged);
		displayFormatControl.OnModelChanged += InvokeDisplayFormatChanged;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		defaultValueInputField.onValueChanged.RemoveListener(InvokeDefaultValueChanged);
		displayFormatControl.OnModelChanged -= InvokeDisplayFormatChanged;
	}

	public override void View(PerPlayerStat model)
	{
		base.View(model);
		defaultValueInputField.SetTextWithoutNotify(model.DefaultValue);
	}

	public override void Clear()
	{
		base.Clear();
		defaultValueInputField.Clear(triggerEvent: false);
	}

	private void InvokeDefaultValueChanged(string newValue)
	{
		this.DefaultValueChanged?.Invoke(newValue);
	}

	private void InvokeDisplayFormatChanged(object displayFormat)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "InvokeDisplayFormatChanged", "displayFormat", displayFormat), this);
		}
		NumericDisplayFormat obj = (NumericDisplayFormat)displayFormat;
		this.DisplayFormatChanged?.Invoke(obj);
	}
}
