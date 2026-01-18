using System;
using System.Collections.Generic;
using Endless.Gameplay;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIWireColorDropdown : UIBaseDropdown<UIWireColorDropdownValue>
{
	[Header("UIWireColorDropdown")]
	[SerializeField]
	private WireColorDictionary wireColorDictionary;

	public bool Initialized { get; private set; }

	public event Action<WireColor> OnColorChanged;

	protected override void Start()
	{
		base.Start();
		if (!Initialized)
		{
			Initialize();
		}
	}

	public void Initialize()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Initialize");
		}
		if (Initialized)
		{
			return;
		}
		Initialized = true;
		List<UIWireColorDropdownValue> list = new List<UIWireColorDropdownValue>();
		foreach (KeyValuePair<WireColor, UIWireColorDictionaryEntryValue> item2 in wireColorDictionary)
		{
			UIWireColorDropdownValue item = new UIWireColorDropdownValue(item2.Value, item2.Key);
			list.Add(item);
		}
		UIWireColorDropdownValue[] newValue = new UIWireColorDropdownValue[1] { list[0] };
		SetOptionsAndValue(list, newValue, triggerValueChanged: false);
		base.OnValueChanged.AddListener(InvokeOnColorChanged);
	}

	protected override void View()
	{
		base.View();
		if (!Initialized)
		{
			Initialize();
		}
		SetValueIconColor(base.Value[0].WireColorDictionaryEntryValue.Color);
	}

	protected override string GetLabelFromOption(int optionIndex)
	{
		if (base.SuperVerboseLogging)
		{
			DebugUtility.LogMethod(this, "GetLabelFromOption", optionIndex);
		}
		if (!Initialized)
		{
			Initialize();
		}
		ValidateIndex(optionIndex, base.Count);
		return base.Options[optionIndex].WireColorName;
	}

	protected override Sprite GetIconFromOption(int optionIndex)
	{
		if (base.SuperVerboseLogging)
		{
			DebugUtility.LogMethod(this, "GetIconFromOption", optionIndex);
		}
		ValidateIndex(optionIndex, base.Count);
		return base.Options[optionIndex].WireColorDictionaryEntryValue.Sprite;
	}

	private void InvokeOnColorChanged()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "InvokeOnColorChanged");
		}
		this.OnColorChanged?.Invoke(base.Value[0].WireColor);
	}

	public void Clear()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Clear");
		}
		if (!Initialized)
		{
			Initialize();
		}
	}
}
