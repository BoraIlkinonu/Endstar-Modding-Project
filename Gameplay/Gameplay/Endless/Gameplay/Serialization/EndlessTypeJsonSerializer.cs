using System;
using Endless.Props.Scripting;
using Newtonsoft.Json;
using UnityEngine;

namespace Endless.Gameplay.Serialization
{
	// Token: 0x020004C7 RID: 1223
	public abstract class EndlessTypeJsonSerializer : EndlessTypeUpgrader
	{
		// Token: 0x170005EF RID: 1519
		// (get) Token: 0x06001E64 RID: 7780
		protected abstract JsonConverter Converter { get; }

		// Token: 0x06001E65 RID: 7781 RVA: 0x00084784 File Offset: 0x00082984
		public override void Upgrade(MemberChange memberChange, bool isLua)
		{
			Type typeFromId = EndlessTypeMapping.Instance.GetTypeFromId(memberChange.DataType);
			JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
			jsonSerializerSettings.Converters.Add(this.Converter);
			Debug.LogWarning(memberChange.JsonData);
			object obj = JsonConvert.DeserializeObject(memberChange.JsonData, typeFromId, jsonSerializerSettings);
			memberChange.JsonData = JsonConvert.SerializeObject(obj);
		}

		// Token: 0x06001E66 RID: 7782 RVA: 0x000847E0 File Offset: 0x000829E0
		public override void Upgrade(InspectorScriptValue inspectorScriptValue)
		{
			Type typeFromId = EndlessTypeMapping.Instance.GetTypeFromId(inspectorScriptValue.DataType);
			JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
			jsonSerializerSettings.Converters.Add(this.Converter);
			Debug.LogWarning(inspectorScriptValue.DefaultValue);
			object obj = JsonConvert.DeserializeObject(inspectorScriptValue.DefaultValue, typeFromId, jsonSerializerSettings);
			inspectorScriptValue.DefaultValue = JsonConvert.SerializeObject(obj);
		}
	}
}
