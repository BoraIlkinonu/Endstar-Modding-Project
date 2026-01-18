using System;
using Endless.Gameplay.Stats;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIComparativeStatView : UIStatBaseView<ComparativeStat>
{
	[Header("UIComparativeStatView")]
	[SerializeField]
	private UIEnumPresenter comparisonControl;

	[SerializeField]
	private UIEnumPresenter displayFormatControl;

	public event Action<ComparativeStat.ValueComparison> ComparisonChanged;

	public event Action<NumericDisplayFormat> DisplayFormatChanged;

	protected override void Start()
	{
		base.Start();
		comparisonControl.OnModelChanged += InvokeComparisonChanged;
		displayFormatControl.OnModelChanged += InvokeDisplayFormatChanged;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		comparisonControl.OnModelChanged -= InvokeComparisonChanged;
		displayFormatControl.OnModelChanged -= InvokeDisplayFormatChanged;
	}

	public override void View(ComparativeStat model)
	{
		base.View(model);
		comparisonControl.SetModel(model.Comparison, triggerOnModelChanged: false);
		displayFormatControl.SetModel(model.DisplayFormat, triggerOnModelChanged: false);
	}

	private void InvokeComparisonChanged(object comparison)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "InvokeComparisonChanged", "comparison", comparison), this);
		}
		ComparativeStat.ValueComparison obj = (ComparativeStat.ValueComparison)comparison;
		this.ComparisonChanged?.Invoke(obj);
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
