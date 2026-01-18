using System;
using System.Collections.Generic;
using Endless.Shared.DataTypes;

namespace Endless.Gameplay.LevelEditing.Level
{
	// Token: 0x0200054F RID: 1359
	[Serializable]
	public class WiringEntry
	{
		// Token: 0x04001A1D RID: 6685
		public SerializableGuid EmitterInstanceId;

		// Token: 0x04001A1E RID: 6686
		public List<WiredComponentEntry> WiredComponents = new List<WiredComponentEntry>();
	}
}
