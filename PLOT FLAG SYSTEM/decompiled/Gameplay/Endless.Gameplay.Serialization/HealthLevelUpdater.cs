using System;
using Endless.Gameplay.LuaEnums;
using Endless.Props.Scripting;
using Newtonsoft.Json;

namespace Endless.Gameplay.Serialization;

public class HealthLevelUpdater : EndlessTypeUpgrader
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
		newDataType = EndlessTypeMapping.Instance.GetTypeId(typeof(int).AssemblyQualifiedName);
		HealthLevel healthLevel = JsonConvert.DeserializeObject<HealthLevel>(jsonData);
		int num = 1;
		newJsonData = JsonConvert.SerializeObject(healthLevel switch
		{
			HealthLevel.Fragile => 5, 
			HealthLevel.Regular => 10, 
			HealthLevel.Tough => 15, 
			_ => throw new ArgumentOutOfRangeException(), 
		});
		if (isLua)
		{
			newMemberName = memberName;
		}
		else
		{
			newMemberName = "StartingHealth";
		}
	}
}
