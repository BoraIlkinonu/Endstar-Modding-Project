using System;
using Endless.Props.Scripting;
using Newtonsoft.Json;
using UnityEngine;

namespace Endless.Gameplay.Serialization;

public abstract class EndlessTypeJsonSerializer : EndlessTypeUpgrader
{
	protected abstract JsonConverter Converter { get; }

	public override void Upgrade(MemberChange memberChange, bool isLua)
	{
		Type typeFromId = EndlessTypeMapping.Instance.GetTypeFromId(memberChange.DataType);
		JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
		jsonSerializerSettings.Converters.Add(Converter);
		Debug.LogWarning(memberChange.JsonData);
		object value = JsonConvert.DeserializeObject(memberChange.JsonData, typeFromId, jsonSerializerSettings);
		memberChange.JsonData = JsonConvert.SerializeObject(value);
	}

	public override void Upgrade(InspectorScriptValue inspectorScriptValue)
	{
		Type typeFromId = EndlessTypeMapping.Instance.GetTypeFromId(inspectorScriptValue.DataType);
		JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
		jsonSerializerSettings.Converters.Add(Converter);
		Debug.LogWarning(inspectorScriptValue.DefaultValue);
		object value = JsonConvert.DeserializeObject(inspectorScriptValue.DefaultValue, typeFromId, jsonSerializerSettings);
		inspectorScriptValue.DefaultValue = JsonConvert.SerializeObject(value);
	}
}
