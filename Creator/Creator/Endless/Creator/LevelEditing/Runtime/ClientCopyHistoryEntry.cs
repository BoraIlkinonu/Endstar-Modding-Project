using System;
using Endless.Shared.DataTypes;
using UnityEngine;

namespace Endless.Creator.LevelEditing.Runtime
{
	// Token: 0x02000345 RID: 837
	public class ClientCopyHistoryEntry
	{
		// Token: 0x17000259 RID: 601
		// (get) Token: 0x06000F82 RID: 3970 RVA: 0x00047BE9 File Offset: 0x00045DE9
		// (set) Token: 0x06000F83 RID: 3971 RVA: 0x00047BF1 File Offset: 0x00045DF1
		public string Label { get; set; }

		// Token: 0x1700025A RID: 602
		// (get) Token: 0x06000F84 RID: 3972 RVA: 0x00047BFA File Offset: 0x00045DFA
		// (set) Token: 0x06000F85 RID: 3973 RVA: 0x00047C02 File Offset: 0x00045E02
		public SerializableGuid InstanceId { get; set; }

		// Token: 0x1700025B RID: 603
		// (get) Token: 0x06000F86 RID: 3974 RVA: 0x00047C0B File Offset: 0x00045E0B
		// (set) Token: 0x06000F87 RID: 3975 RVA: 0x00047C13 File Offset: 0x00045E13
		public SerializableGuid AssetId { get; set; }

		// Token: 0x1700025C RID: 604
		// (get) Token: 0x06000F88 RID: 3976 RVA: 0x00047C1C File Offset: 0x00045E1C
		// (set) Token: 0x06000F89 RID: 3977 RVA: 0x00047C24 File Offset: 0x00045E24
		public Quaternion Rotation { get; set; }
	}
}
