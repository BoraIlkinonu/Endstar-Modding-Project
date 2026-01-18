using System;
using System.Collections.Generic;
using Endless.Gameplay;
using Endless.Gameplay.Serialization;
using Endless.Shared.Debugging;
using Endless.Shared.UI;

namespace Endless.Creator.UI;

public class UIInspectorScriptValueTypeRadio : UIBaseRadio<Type>
{
	private HashSet<Type> typesToHide = new HashSet<Type> { typeof(TradeInfo.InventoryAndQuantityReference) };

	protected override void Initialize()
	{
		SetDefaultValue(EndlessTypeMapping.Instance.LuaInspectorTypes[0]);
		base.Initialize();
	}

	protected override Type[] GetValues()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "GetValues");
		}
		Type[] luaInspectorTypes = EndlessTypeMapping.Instance.LuaInspectorTypes;
		List<Type> list = new List<Type>(luaInspectorTypes.Length);
		for (int i = 0; i < luaInspectorTypes.Length; i++)
		{
			if (!typesToHide.Contains(luaInspectorTypes[i]))
			{
				list.Add(luaInspectorTypes[i]);
			}
		}
		return list.ToArray();
	}
}
