using System;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Serialization;

namespace Endless.Props.Scripting
{
	// Token: 0x0200000D RID: 13
	[Serializable]
	public class InspectorScriptValue
	{
		// Token: 0x0600002E RID: 46 RVA: 0x000028C4 File Offset: 0x00000AC4
		public InspectorScriptValue(string name, string enumName, int dataType, string description, string defaultValue, ClampValue[] clampValues)
		{
			this.name = name;
			this.enumName = enumName;
			this.dataType = dataType;
			this.description = description;
			this.defaultValue = defaultValue;
			this.clampValues = clampValues;
		}

		// Token: 0x1700000A RID: 10
		// (get) Token: 0x0600002F RID: 47 RVA: 0x000028F9 File Offset: 0x00000AF9
		[JsonIgnore]
		public string Name
		{
			get
			{
				return this.name;
			}
		}

		// Token: 0x1700000B RID: 11
		// (get) Token: 0x06000030 RID: 48 RVA: 0x00002901 File Offset: 0x00000B01
		[JsonIgnore]
		public string EnumName
		{
			get
			{
				return this.enumName;
			}
		}

		// Token: 0x1700000C RID: 12
		// (get) Token: 0x06000031 RID: 49 RVA: 0x00002909 File Offset: 0x00000B09
		// (set) Token: 0x06000032 RID: 50 RVA: 0x00002911 File Offset: 0x00000B11
		[JsonIgnore]
		public int DataType
		{
			get
			{
				return this.dataType;
			}
			set
			{
				this.dataType = value;
			}
		}

		// Token: 0x1700000D RID: 13
		// (get) Token: 0x06000033 RID: 51 RVA: 0x0000291A File Offset: 0x00000B1A
		[JsonIgnore]
		public string Description
		{
			get
			{
				return this.description;
			}
		}

		// Token: 0x1700000E RID: 14
		// (get) Token: 0x06000034 RID: 52 RVA: 0x00002922 File Offset: 0x00000B22
		// (set) Token: 0x06000035 RID: 53 RVA: 0x0000292A File Offset: 0x00000B2A
		[JsonIgnore]
		public string DefaultValue
		{
			get
			{
				return this.defaultValue;
			}
			set
			{
				this.defaultValue = value;
			}
		}

		// Token: 0x1700000F RID: 15
		// (get) Token: 0x06000036 RID: 54 RVA: 0x00002933 File Offset: 0x00000B33
		[JsonIgnore]
		public ClampValue[] ClampValues
		{
			get
			{
				return this.clampValues;
			}
		}

		// Token: 0x06000037 RID: 55 RVA: 0x0000293B File Offset: 0x00000B3B
		public object GetDefaultObject(Type type)
		{
			return JsonConvert.DeserializeObject(this.DefaultValue, type);
		}

		// Token: 0x04000026 RID: 38
		[FormerlySerializedAs("Name")]
		[SerializeField]
		private string name;

		// Token: 0x04000027 RID: 39
		[FormerlySerializedAs("EnumName")]
		[SerializeField]
		private string enumName;

		// Token: 0x04000028 RID: 40
		[FormerlySerializedAs("DataType")]
		[SerializeField]
		private int dataType;

		// Token: 0x04000029 RID: 41
		[FormerlySerializedAs("Description")]
		[SerializeField]
		private string description;

		// Token: 0x0400002A RID: 42
		[FormerlySerializedAs("DefaultValue")]
		[SerializeField]
		private string defaultValue;

		// Token: 0x0400002B RID: 43
		[SerializeField]
		private ClampValue[] clampValues;
	}
}
