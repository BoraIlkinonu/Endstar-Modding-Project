using Endless.Gameplay;
using Endless.Shared.Debugging;
using Endless.Shared.UI;

namespace Endless.Creator.UI;

public class UIAutoStatPresenter : UIBasePresenter<GameEndBlock.AutoStat>
{
	private UIAutoStatView autoStatView;

	protected override void Start()
	{
		base.Start();
		autoStatView = base.View.Interface as UIAutoStatView;
		autoStatView.StatChanged += SetStat;
		autoStatView.PriorityChanged += SetPriority;
		autoStatView.StatTypeChanged += SetStatType;
	}

	private void OnDestroy()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnDestroy");
		}
		autoStatView.StatChanged -= SetStat;
		autoStatView.PriorityChanged -= SetPriority;
		autoStatView.StatTypeChanged -= SetStatType;
	}

	private void SetStat(GameEndBlock.Stats stat)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "SetStat", stat);
		}
		base.Model.Stat = stat;
		InvokeOnModelChanged();
	}

	private void SetPriority(int priority)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "SetPriority", priority);
		}
		base.Model.Order = priority;
		InvokeOnModelChanged();
	}

	private void SetStatType(GameEndBlock.StatType statType)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "SetStatType", statType);
		}
		base.Model.StatType = statType;
		InvokeOnModelChanged();
	}
}
