using System;
using System.Collections.Generic;
using Endless.Gameplay.Serialization;
using Endless.Shared.DataTypes;
using Newtonsoft.Json;

namespace Endless.Gameplay.LevelEditing.Level
{
	// Token: 0x02000552 RID: 1362
	[Serializable]
	public class EventTargetEntry
	{
		// Token: 0x17000644 RID: 1604
		// (get) Token: 0x060020DA RID: 8410 RVA: 0x0009419D File Offset: 0x0009239D
		[JsonIgnore]
		public string AssemblyQualifiedTypeName
		{
			get
			{
				return EndlessTypeMapping.Instance.GetAssemblyQualifiedTypeName(this.TypeId);
			}
		}

		// Token: 0x04001A23 RID: 6691
		public SerializableGuid WiringId;

		// Token: 0x04001A24 RID: 6692
		public int TypeId;

		// Token: 0x04001A25 RID: 6693
		public SerializableGuid ReceiverInstanceId;

		// Token: 0x04001A26 RID: 6694
		public string ReceiverMemberName;

		// Token: 0x04001A27 RID: 6695
		public StoredParameter[] StaticParameter;

		// Token: 0x04001A28 RID: 6696
		public List<SerializableGuid> RerouteNodeIds = new List<SerializableGuid>();

		// Token: 0x04001A29 RID: 6697
		public WireColor WireColor;
	}
}
