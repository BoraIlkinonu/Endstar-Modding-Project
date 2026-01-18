using System;
using Endless.Gameplay.LuaEnums;
using Endless.Props.Scripting;
using Newtonsoft.Json;

namespace Endless.Gameplay.Serialization;

public class NpcClassUpgrader : EndlessTypeUpgrader
{
	public override void Upgrade(InspectorScriptValue inspectorScriptValue)
	{
		GetUpgradedData(inspectorScriptValue.Name, inspectorScriptValue.DataType, inspectorScriptValue.DefaultValue, isLua: true, out var _, out var newDataType, out var newJsonData);
		inspectorScriptValue.DataType = newDataType;
		inspectorScriptValue.DefaultValue = newJsonData;
	}

	public override void Upgrade(MemberChange memberChange, bool isLua)
	{
		GetUpgradedData(memberChange.MemberName, memberChange.DataType, memberChange.JsonData, isLua, out var newMemberName, out var newDataType, out var newJsonData);
		memberChange.MemberName = newMemberName;
		memberChange.DataType = newDataType;
		memberChange.JsonData = newJsonData;
	}

	private void GetUpgradedData(string memberName, int memberDataType, string jsonData, bool isLua, out string newMemberName, out int newDataType, out string newJsonData)
	{
		NpcClass npcClass = JsonConvert.DeserializeObject<NpcClass>(jsonData);
		if (!isLua)
		{
			newMemberName = "NpcClass";
		}
		else
		{
			newMemberName = memberName;
		}
		switch (npcClass)
		{
		case NpcClass.Blank:
			newDataType = EndlessTypeMapping.Instance.GetTypeId(typeof(BlankNpcCustomizationData).AssemblyQualifiedName);
			newJsonData = JsonConvert.SerializeObject(new BlankNpcCustomizationData());
			break;
		case NpcClass.Grunt:
			newDataType = EndlessTypeMapping.Instance.GetTypeId(typeof(GruntNpcCustomizationData).AssemblyQualifiedName);
			newJsonData = JsonConvert.SerializeObject(new GruntNpcCustomizationData());
			break;
		case NpcClass.Rifleman:
			newDataType = EndlessTypeMapping.Instance.GetTypeId(typeof(RiflemanNpcCustomizationData).AssemblyQualifiedName);
			newJsonData = JsonConvert.SerializeObject(new RiflemanNpcCustomizationData());
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}
}
