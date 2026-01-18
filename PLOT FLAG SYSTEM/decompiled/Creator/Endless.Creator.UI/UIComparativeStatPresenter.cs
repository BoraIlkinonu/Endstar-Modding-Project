using Endless.Gameplay.Stats;
using Endless.Shared.Debugging;

namespace Endless.Creator.UI;

public class UIComparativeStatPresenter : UIStatBasePresenter<ComparativeStat>
{
	private UIComparativeStatView comparativeStatView;

	protected override void Start()
	{
		base.Start();
		comparativeStatView = base.View.Interface as UIComparativeStatView;
		comparativeStatView.ComparisonChanged += SetComparison;
		comparativeStatView.DisplayFormatChanged += SetDisplayFormat;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		comparativeStatView.ComparisonChanged -= SetComparison;
		comparativeStatView.DisplayFormatChanged -= SetDisplayFormat;
	}

	private void SetComparison(ComparativeStat.ValueComparison comparison)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "SetComparison", comparison);
		}
		base.Model.Comparison = comparison;
		InvokeOnModelChanged();
	}

	private void SetDisplayFormat(NumericDisplayFormat displayFormat)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "SetDisplayFormat", displayFormat);
		}
		base.Model.DisplayFormat = displayFormat;
		InvokeOnModelChanged();
	}
}
