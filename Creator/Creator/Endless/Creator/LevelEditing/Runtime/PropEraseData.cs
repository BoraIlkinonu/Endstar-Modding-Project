using System;
using Endless.Shared.DataTypes;
using UnityEngine;

namespace Endless.Creator.LevelEditing.Runtime
{
	// Token: 0x0200034E RID: 846
	[Serializable]
	public class PropEraseData
	{
		// Token: 0x17000271 RID: 625
		// (get) Token: 0x06000FEF RID: 4079 RVA: 0x0004A123 File Offset: 0x00048323
		// (set) Token: 0x06000FF0 RID: 4080 RVA: 0x0004A12B File Offset: 0x0004832B
		public Vector3 Position { get; set; }

		// Token: 0x17000272 RID: 626
		// (get) Token: 0x06000FF1 RID: 4081 RVA: 0x0004A134 File Offset: 0x00048334
		// (set) Token: 0x06000FF2 RID: 4082 RVA: 0x0004A13C File Offset: 0x0004833C
		public Quaternion Rotation { get; set; }

		// Token: 0x17000273 RID: 627
		// (get) Token: 0x06000FF3 RID: 4083 RVA: 0x0004A145 File Offset: 0x00048345
		// (set) Token: 0x06000FF4 RID: 4084 RVA: 0x0004A14D File Offset: 0x0004834D
		public SerializableGuid InstanceId { get; set; }

		// Token: 0x17000274 RID: 628
		// (get) Token: 0x06000FF5 RID: 4085 RVA: 0x0004A156 File Offset: 0x00048356
		// (set) Token: 0x06000FF6 RID: 4086 RVA: 0x0004A15E File Offset: 0x0004835E
		public SerializableGuid AssetId { get; set; }
	}
}
