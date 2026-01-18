using Endless.Gameplay.Stats;
using Endless.Shared.Debugging;

namespace Endless.Creator.UI;

public class UIPerPlayerStatPresenter : UIStatBasePresenter<PerPlayerStat>
{
	private UIPerPlayerStatView perPlayerStatView;

	protected override void Start()
	{
		base.Start();
		perPlayerStatView = base.View.Interface as UIPerPlayerStatView;
		perPlayerStatView.DefaultValueChanged += SetDefaultValue;
		perPlayerStatView.DisplayFormatChanged += SetDisplayFormat;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		perPlayerStatView.DefaultValueChanged -= SetDefaultValue;
		perPlayerStatView.DisplayFormatChanged -= SetDisplayFormat;
	}

	private void SetDefaultValue(string newValue)
	{
		base.Model.DefaultValue = newValue;
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
