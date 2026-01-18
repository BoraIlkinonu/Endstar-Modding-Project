using System;
using Endless.Gameplay.Serialization;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

[CreateAssetMenu(menuName = "ScriptableObject/UI/Creator/Dictionaries/Inspector Script Value Type Info Dictionary", fileName = "Inspector Script Value Type Info Dictionary")]
public class InspectorScriptValueTypeInfoDictionary : BaseScriptableObjectDictionary<string, DisplayNameAndDescription>
{
	[ContextMenu("Validate")]
	public override void Validate()
	{
		base.Validate();
		Type[] luaInspectorTypes = EndlessTypeMapping.Instance.LuaInspectorTypes;
		foreach (Type type in luaInspectorTypes)
		{
			if (!Contains(type.Name))
			{
				DebugUtility.LogError(type.Name + " is missing!", this);
			}
		}
	}
}
