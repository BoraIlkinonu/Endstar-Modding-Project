using System;
using Endless.Props.Scripting;
using Newtonsoft.Json;

namespace Endless.Gameplay.Serialization;

public class ResourcePickupAmountUpgrader : EndlessTypeUpgrader
{
	private void GetUpgradedData(string memberName, int memberDataType, string jsonData, bool isLua, out string newMemberName, out int newDataType, out string newJsonData)
	{
		newDataType = EndlessTypeMapping.Instance.GetTypeId(typeof(int).AssemblyQualifiedName);
		ResourcePickup.ResourceQuantity resourceQuantity = JsonConvert.DeserializeObject<ResourcePickup.ResourceQuantity>(jsonData);
		int num = 1;
		newJsonData = JsonConvert.SerializeObject(resourceQuantity switch
		{
			ResourcePickup.ResourceQuantity.One => 1, 
			ResourcePickup.ResourceQuantity.Five => 5, 
			ResourcePickup.ResourceQuantity.Ten => 10, 
			_ => throw new ArgumentOutOfRangeException(), 
		});
		if (!isLua)
		{
			newMemberName = "Quantity";
		}
		else
		{
			newMemberName = memberName;
		}
	}

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
}
