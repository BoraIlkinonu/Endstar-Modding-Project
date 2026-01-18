using System;
using System.Collections.Generic;
using Endless.Gameplay.Scripting;

// Token: 0x02000012 RID: 18
public interface IScriptInjector
{
	// Token: 0x17000014 RID: 20
	// (get) Token: 0x06000046 RID: 70 RVA: 0x00002D8C File Offset: 0x00000F8C
	string LuaObjectName
	{
		get
		{
			Type luaObjectType = this.LuaObjectType;
			if (luaObjectType == null)
			{
				return null;
			}
			return luaObjectType.Name;
		}
	}

	// Token: 0x17000015 RID: 21
	// (get) Token: 0x06000047 RID: 71 RVA: 0x00002D9F File Offset: 0x00000F9F
	object LuaObject
	{
		get
		{
			return null;
		}
	}

	// Token: 0x17000016 RID: 22
	// (get) Token: 0x06000048 RID: 72 RVA: 0x00002D9F File Offset: 0x00000F9F
	Type LuaObjectType
	{
		get
		{
			return null;
		}
	}

	// Token: 0x17000017 RID: 23
	// (get) Token: 0x06000049 RID: 73 RVA: 0x00002D9F File Offset: 0x00000F9F
	List<Type> EnumTypes
	{
		get
		{
			return null;
		}
	}

	// Token: 0x17000018 RID: 24
	// (get) Token: 0x0600004A RID: 74 RVA: 0x00002DA2 File Offset: 0x00000FA2
	bool AllowLuaReference
	{
		get
		{
			return this.LuaObjectType != null;
		}
	}

	// Token: 0x0600004B RID: 75 RVA: 0x00002DB0 File Offset: 0x00000FB0
	void ScriptInitialize(EndlessScriptComponent endlessScriptComponent)
	{
	}
}
