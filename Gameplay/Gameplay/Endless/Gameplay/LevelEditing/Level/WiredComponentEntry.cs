using System;
using System.Collections.Generic;
using Endless.Gameplay.Serialization;
using Newtonsoft.Json;

namespace Endless.Gameplay.LevelEditing.Level
{
	// Token: 0x02000550 RID: 1360
	[Serializable]
	public class WiredComponentEntry
	{
		// Token: 0x17000643 RID: 1603
		// (get) Token: 0x060020D7 RID: 8407 RVA: 0x00094165 File Offset: 0x00092365
		[JsonIgnore]
		public string AssemblyQualifiedTypeName
		{
			get
			{
				return EndlessTypeMapping.Instance.GetAssemblyQualifiedTypeName(this.TypeId);
			}
		}

		// Token: 0x04001A1F RID: 6687
		public int TypeId;

		// Token: 0x04001A20 RID: 6688
		public List<EventEntry> EventEntries = new List<EventEntry>();
	}
}
