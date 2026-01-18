using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Endless.Assets;
using Endless.Props.Scripting;
using Endless.Shared.DataTypes;
using Newtonsoft.Json;
using UnityEngine;

namespace Endless.Props.Assets
{
	// Token: 0x02000041 RID: 65
	[Serializable]
	public class Script : Asset
	{
		// Token: 0x17000066 RID: 102
		// (get) Token: 0x0600010E RID: 270 RVA: 0x00003C0E File Offset: 0x00001E0E
		// (set) Token: 0x0600010F RID: 271 RVA: 0x00003C16 File Offset: 0x00001E16
		[JsonIgnore]
		public bool HasErrors
		{
			get
			{
				return this.hasErrors;
			}
			set
			{
				this.hasErrors = value;
			}
		}

		// Token: 0x17000067 RID: 103
		// (get) Token: 0x06000110 RID: 272 RVA: 0x00003C1F File Offset: 0x00001E1F
		// (set) Token: 0x06000111 RID: 273 RVA: 0x00003C27 File Offset: 0x00001E27
		[JsonIgnore]
		public string Body
		{
			get
			{
				return this.body;
			}
			set
			{
				this.body = value;
			}
		}

		// Token: 0x17000068 RID: 104
		// (get) Token: 0x06000112 RID: 274 RVA: 0x00003C30 File Offset: 0x00001E30
		[JsonIgnore]
		public List<EnumEntry> EnumTypes
		{
			get
			{
				return this.enumTypes;
			}
		}

		// Token: 0x17000069 RID: 105
		// (get) Token: 0x06000113 RID: 275 RVA: 0x00003C38 File Offset: 0x00001E38
		[JsonIgnore]
		public List<InspectorScriptValue> InspectorValues
		{
			get
			{
				return this.inspectorValues;
			}
		}

		// Token: 0x1700006A RID: 106
		// (get) Token: 0x06000114 RID: 276 RVA: 0x00003C40 File Offset: 0x00001E40
		[JsonIgnore]
		public List<EndlessEventInfo> Receivers
		{
			get
			{
				return this.receivers;
			}
		}

		// Token: 0x1700006B RID: 107
		// (get) Token: 0x06000115 RID: 277 RVA: 0x00003C48 File Offset: 0x00001E48
		[JsonIgnore]
		public List<EndlessEventInfo> Events
		{
			get
			{
				return this.events;
			}
		}

		// Token: 0x1700006C RID: 108
		// (get) Token: 0x06000116 RID: 278 RVA: 0x00003C50 File Offset: 0x00001E50
		[JsonIgnore]
		public List<ScriptReference> ScriptReferences
		{
			get
			{
				return this.scriptReferences;
			}
		}

		// Token: 0x1700006D RID: 109
		// (get) Token: 0x06000117 RID: 279 RVA: 0x00003C58 File Offset: 0x00001E58
		[JsonIgnore]
		public List<InspectorOrganizationData> InspectorOrganizationData
		{
			get
			{
				return this.inspectorOrganizationData;
			}
		}

		// Token: 0x1700006E RID: 110
		// (get) Token: 0x06000118 RID: 280 RVA: 0x00003C60 File Offset: 0x00001E60
		[JsonIgnore]
		public List<WireOrganizationData> EventOrganizationData
		{
			get
			{
				return this.eventOrganizationData;
			}
		}

		// Token: 0x1700006F RID: 111
		// (get) Token: 0x06000119 RID: 281 RVA: 0x00003C68 File Offset: 0x00001E68
		[JsonIgnore]
		public List<WireOrganizationData> ReceiverOrganizationData
		{
			get
			{
				return this.receiverOrganizationData;
			}
		}

		// Token: 0x17000070 RID: 112
		// (get) Token: 0x0600011A RID: 282 RVA: 0x00003C70 File Offset: 0x00001E70
		[JsonIgnore]
		public string BaseTypeId
		{
			get
			{
				return this.baseTypeId;
			}
		}

		// Token: 0x17000071 RID: 113
		// (get) Token: 0x0600011B RID: 283 RVA: 0x00003C78 File Offset: 0x00001E78
		[JsonIgnore]
		public IReadOnlyList<string> ComponentIds
		{
			get
			{
				return this.componentIds;
			}
		}

		// Token: 0x17000072 RID: 114
		// (get) Token: 0x0600011C RID: 284 RVA: 0x00003C80 File Offset: 0x00001E80
		// (set) Token: 0x0600011D RID: 285 RVA: 0x00003C88 File Offset: 0x00001E88
		[JsonIgnore]
		public bool OpenSource
		{
			get
			{
				return this.openSource;
			}
			set
			{
				this.openSource = value;
			}
		}

		// Token: 0x0600011E RID: 286 RVA: 0x00003C91 File Offset: 0x00001E91
		public ValueTuple<string, List<string>> GetEvents()
		{
			return new ValueTuple<string, List<string>>("Events", this.events.Select((EndlessEventInfo e) => e.MemberName).ToList<string>());
		}

		// Token: 0x0600011F RID: 287 RVA: 0x00003CCC File Offset: 0x00001ECC
		public string GetEventSnippet()
		{
			ValueTuple<string, List<string>> valueTuple = this.GetEvents();
			string item = valueTuple.Item1;
			List<string> item2 = valueTuple.Item2;
			return Script.GetNamedStringsSnippet(item, item2);
		}

		// Token: 0x06000120 RID: 288 RVA: 0x00003CF3 File Offset: 0x00001EF3
		public static string GetNamedStringsSnippet(string snippetName, List<string> keys)
		{
			return Script.GetNamedStringsSnippet(snippetName, keys, keys);
		}

		// Token: 0x06000121 RID: 289 RVA: 0x00003D00 File Offset: 0x00001F00
		public static string GetNamedStringsSnippet(string snippetName, List<string> keys, List<string> values)
		{
			StringBuilder stringBuilder = new StringBuilder(snippetName);
			stringBuilder.Append(" = {");
			for (int i = 0; i < keys.Count; i++)
			{
				stringBuilder.Append(keys[i]);
				stringBuilder.Append("=\"");
				stringBuilder.Append(values[i]);
				stringBuilder.Append('"');
				if (i < keys.Count - 1)
				{
					stringBuilder.Append(", ");
				}
			}
			stringBuilder.Append("}\n");
			return stringBuilder.ToString();
		}

		// Token: 0x06000122 RID: 290 RVA: 0x00003D8C File Offset: 0x00001F8C
		[return: TupleElementNames(new string[] { "key", "names" })]
		public ValueTuple<string, List<string>> GetTransforms()
		{
			List<string> list = (from sr in this.scriptReferences
				where sr.Type == ScriptReference.ReferenceType.Transform
				select sr.NameInCode).ToList<string>();
			return new ValueTuple<string, List<string>>("Transforms", list);
		}

		// Token: 0x06000123 RID: 291 RVA: 0x00003DF8 File Offset: 0x00001FF8
		public string GetTransformReferenceSnippet(Prop prop)
		{
			IEnumerable<ScriptReference> enumerable = this.scriptReferences.Where((ScriptReference sr) => sr.Type == ScriptReference.ReferenceType.Transform);
			List<string> list = enumerable.Select((ScriptReference sr) => sr.NameInCode).ToList<string>();
			List<string> list2 = enumerable.Select((ScriptReference sr) => sr.ReferenceId).ToList<string>();
			prop.RemapTransforms(list2);
			return Script.GetNamedStringsSnippet("Transforms", list, list2);
		}

		// Token: 0x06000124 RID: 292 RVA: 0x00003E98 File Offset: 0x00002098
		public void SetComponentIds(string baseType, List<string> components)
		{
			this.baseTypeId = baseType;
			this.componentIds = components;
		}

		// Token: 0x06000125 RID: 293 RVA: 0x00003EA8 File Offset: 0x000020A8
		public Script Clone()
		{
			return JsonConvert.DeserializeObject<Script>(JsonConvert.SerializeObject(this));
		}

		// Token: 0x06000126 RID: 294 RVA: 0x00003EB8 File Offset: 0x000020B8
		public InspectorOrganizationData GetInspectorOrganizationData(int dataType, string memberName, int componentId)
		{
			if (componentId == 0)
			{
				Debug.LogError("Creating invalid inspector data! Please report this to the engineering team");
			}
			InspectorOrganizationData inspectorOrganizationData = this.inspectorOrganizationData.FirstOrDefault((InspectorOrganizationData data) => data.DataType == dataType && data.MemberName == memberName && data.ComponentId == componentId);
			if (inspectorOrganizationData == null)
			{
				inspectorOrganizationData = new InspectorOrganizationData(dataType, memberName, componentId, "", false, "");
				this.inspectorOrganizationData.Add(inspectorOrganizationData);
			}
			return inspectorOrganizationData;
		}

		// Token: 0x06000127 RID: 295 RVA: 0x00003F40 File Offset: 0x00002140
		public WireOrganizationData GetWireOrganizationEventData(string memberName, int componentId)
		{
			if (componentId == 0)
			{
				Debug.LogError("Creating invalid event data! Please report this to the engineering team");
			}
			WireOrganizationData wireOrganizationData = this.eventOrganizationData.FirstOrDefault((WireOrganizationData data) => data.MemberName == memberName && data.ComponentId == componentId);
			if (wireOrganizationData == null)
			{
				wireOrganizationData = new WireOrganizationData(memberName, componentId);
				this.eventOrganizationData.Add(wireOrganizationData);
			}
			return wireOrganizationData;
		}

		// Token: 0x06000128 RID: 296 RVA: 0x00003FB0 File Offset: 0x000021B0
		public WireOrganizationData GetWireOrganizationReceiverData(string memberName, int componentId)
		{
			if (componentId == 0)
			{
				Debug.LogError("Creating invalid receiver data! Please report this to the engineering team");
			}
			WireOrganizationData wireOrganizationData = this.receiverOrganizationData.FirstOrDefault((WireOrganizationData data) => data.MemberName == memberName && data.ComponentId == componentId);
			if (wireOrganizationData == null)
			{
				wireOrganizationData = new WireOrganizationData(memberName, componentId);
				this.receiverOrganizationData.Add(wireOrganizationData);
			}
			return wireOrganizationData;
		}

		// Token: 0x06000129 RID: 297 RVA: 0x00004020 File Offset: 0x00002220
		public void DebugInspectorOrgData()
		{
			Debug.Log("Debugging organization data");
			foreach (InspectorOrganizationData inspectorOrganizationData in this.inspectorOrganizationData)
			{
				Debug.Log(inspectorOrganizationData);
			}
		}

		// Token: 0x040000BF RID: 191
		public static readonly SemanticVersion INTERNAL_VERSION = new SemanticVersion(1, 0, 4);

		// Token: 0x040000C0 RID: 192
		public const int LUA_COMPONENT_ID = -1;

		// Token: 0x040000C1 RID: 193
		[TextArea(10, 40)]
		[SerializeField]
		private string body = "";

		// Token: 0x040000C2 RID: 194
		[SerializeField]
		private List<EnumEntry> enumTypes = new List<EnumEntry>();

		// Token: 0x040000C3 RID: 195
		[SerializeField]
		private List<InspectorScriptValue> inspectorValues = new List<InspectorScriptValue>();

		// Token: 0x040000C4 RID: 196
		[SerializeField]
		private List<EndlessEventInfo> receivers = new List<EndlessEventInfo>();

		// Token: 0x040000C5 RID: 197
		[SerializeField]
		private List<EndlessEventInfo> events = new List<EndlessEventInfo>();

		// Token: 0x040000C6 RID: 198
		[SerializeField]
		private List<ScriptReference> scriptReferences = new List<ScriptReference>();

		// Token: 0x040000C7 RID: 199
		[SerializeField]
		private List<InspectorOrganizationData> inspectorOrganizationData = new List<InspectorOrganizationData>();

		// Token: 0x040000C8 RID: 200
		[SerializeField]
		private List<WireOrganizationData> eventOrganizationData = new List<WireOrganizationData>();

		// Token: 0x040000C9 RID: 201
		[SerializeField]
		private List<WireOrganizationData> receiverOrganizationData = new List<WireOrganizationData>();

		// Token: 0x040000CA RID: 202
		[SerializeField]
		private string baseTypeId = SerializableGuid.Empty;

		// Token: 0x040000CB RID: 203
		[SerializeField]
		private List<string> componentIds = new List<string>();

		// Token: 0x040000CC RID: 204
		[SerializeField]
		[JsonProperty("has_errors")]
		private bool hasErrors;

		// Token: 0x040000CD RID: 205
		[SerializeField]
		private bool openSource;
	}
}
