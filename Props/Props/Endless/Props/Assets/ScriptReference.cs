using System;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Serialization;

namespace Endless.Props.Assets
{
	// Token: 0x02000042 RID: 66
	[Serializable]
	public class ScriptReference
	{
		// Token: 0x0600012C RID: 300 RVA: 0x0000411C File Offset: 0x0000231C
		public ScriptReference(string nameInCode, string referenceId, ScriptReference.ReferenceType type)
		{
			this.nameInCode = nameInCode;
			this.referenceId = referenceId;
			this.type = type;
		}

		// Token: 0x17000073 RID: 115
		// (get) Token: 0x0600012D RID: 301 RVA: 0x00004139 File Offset: 0x00002339
		[JsonIgnore]
		public string NameInCode
		{
			get
			{
				return this.nameInCode;
			}
		}

		// Token: 0x17000074 RID: 116
		// (get) Token: 0x0600012E RID: 302 RVA: 0x00004141 File Offset: 0x00002341
		[JsonIgnore]
		public string ReferenceId
		{
			get
			{
				return this.referenceId;
			}
		}

		// Token: 0x17000075 RID: 117
		// (get) Token: 0x0600012F RID: 303 RVA: 0x00004149 File Offset: 0x00002349
		[JsonIgnore]
		public ScriptReference.ReferenceType Type
		{
			get
			{
				return this.type;
			}
		}

		// Token: 0x040000CE RID: 206
		[FormerlySerializedAs("NameInCode")]
		[SerializeField]
		private string nameInCode;

		// Token: 0x040000CF RID: 207
		[FormerlySerializedAs("ReferenceId")]
		[SerializeField]
		private string referenceId;

		// Token: 0x040000D0 RID: 208
		[FormerlySerializedAs("Type")]
		[SerializeField]
		private ScriptReference.ReferenceType type;

		// Token: 0x0200004F RID: 79
		public enum ReferenceType
		{
			// Token: 0x040000F7 RID: 247
			Transform
		}
	}
}
