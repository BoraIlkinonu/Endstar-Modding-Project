using System;
using Endless.Shared.DataTypes;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020000D3 RID: 211
	[Serializable]
	public abstract class InspectorReference
	{
		// Token: 0x0600042F RID: 1071 RVA: 0x00016AE4 File Offset: 0x00014CE4
		public bool IsReferenceEmpty()
		{
			return this.Id == SerializableGuid.Empty;
		}

		// Token: 0x06000430 RID: 1072 RVA: 0x00016AF6 File Offset: 0x00014CF6
		public bool IsReferenceSet()
		{
			return !this.IsReferenceEmpty();
		}

		// Token: 0x06000431 RID: 1073 RVA: 0x00016B01 File Offset: 0x00014D01
		public override string ToString()
		{
			return string.Format("{0}: {1}", "Id", this.Id);
		}

		// Token: 0x040003B3 RID: 947
		[SerializeField]
		internal SerializableGuid Id = SerializableGuid.Empty;
	}
}
