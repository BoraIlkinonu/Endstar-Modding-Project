using System;
using Endless.Shared.DataTypes;
using UnityEngine;

namespace Endless.Props
{
	// Token: 0x02000007 RID: 7
	public class TransformIdentifier : MonoBehaviour
	{
		// Token: 0x06000015 RID: 21 RVA: 0x000024D4 File Offset: 0x000006D4
		[ContextMenu("Generate new id")]
		public void GenerateNewId()
		{
			this.UniqueId = SerializableGuid.NewGuid();
		}

		// Token: 0x06000016 RID: 22 RVA: 0x000024E1 File Offset: 0x000006E1
		[ContextMenu("Log id")]
		public void LogId()
		{
			Debug.Log(this.UniqueId);
		}

		// Token: 0x04000017 RID: 23
		public SerializableGuid UniqueId = SerializableGuid.Empty;
	}
}
