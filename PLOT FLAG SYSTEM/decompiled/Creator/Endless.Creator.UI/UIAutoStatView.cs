using System;
using Endless.Gameplay;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIAutoStatView : UIBaseView<GameEndBlock.AutoStat, UIAutoStatView.Styles>
{
	public enum Styles
	{
		Default
	}

	[SerializeField]
	private UIDropdownEnum statDropdown;

	[SerializeField]
	private UIIntPresenter priorityIntPresenter;

	[SerializeField]
	private UIDropdownEnum statTypeDropdown;

	private bool controlsSetUp;

	[field: Header("UIAutoStatView")]
	[field: SerializeField]
	public override Styles Style { get; protected set; }

	public event Action<GameEndBlock.Stats> StatChanged;

	public event Action<int> PriorityChanged;

	public event Action<GameEndBlock.StatType> StatTypeChanged;

	private void Start()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		statDropdown.SetLabel("Stat");
		statTypeDropdown.SetLabel("Stat Type");
		statDropdown.OnEnumValueChanged.AddListener(InvokeStatChanged);
		priorityIntPresenter.OnModelChanged += InvokePriorityChanged;
		statTypeDropdown.OnEnumValueChanged.AddListener(InvokeStatTypeChanged);
	}

	private void OnDestroy()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnDestroy");
		}
		priorityIntPresenter.OnModelChanged -= InvokePriorityChanged;
	}

	public override void View(GameEndBlock.AutoStat model)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "View", model);
		}
		if (!controlsSetUp)
		{
			SetUpControls();
		}
		statDropdown.SetEnumValue(model.Stat, triggerValueChanged: false);
		priorityIntPresenter.SetModel(model.Order, triggerOnModelChanged: false);
		statTypeDropdown.SetEnumValue(model.StatType, triggerValueChanged: false);
	}

	public override void Clear()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Clear");
		}
		priorityIntPresenter.Clear();
	}

	private void InvokeStatChanged(Enum stat)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "InvokeStatChanged", stat);
		}
		GameEndBlock.Stats obj = (GameEndBlock.Stats)(object)stat;
		this.StatChanged?.Invoke(obj);
	}

	private void InvokePriorityChanged(object priority)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "InvokePriorityChanged", priority);
		}
		int obj = (int)priority;
		this.PriorityChanged?.Invoke(obj);
	}

	private void InvokeStatTypeChanged(Enum statType)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "InvokeStatTypeChanged", statType);
		}
		GameEndBlock.StatType obj = (GameEndBlock.StatType)(object)statType;
		this.StatTypeChanged?.Invoke(obj);
	}

	private void SetUpControls()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "SetUpControls");
		}
		if (!controlsSetUp)
		{
			statDropdown.InitializeDropdownWithEnum(typeof(GameEndBlock.Stats));
			statTypeDropdown.InitializeDropdownWithEnum(typeof(GameEndBlock.StatType));
			controlsSetUp = true;
		}
	}
}
