using System;
using System.Collections.Generic;
using Endless.Gameplay.Scripting;

public interface IScriptInjector
{
	string LuaObjectName => LuaObjectType?.Name;

	object LuaObject => null;

	Type LuaObjectType => null;

	List<Type> EnumTypes => null;

	bool AllowLuaReference => LuaObjectType != null;

	void ScriptInitialize(EndlessScriptComponent endlessScriptComponent)
	{
	}
}
