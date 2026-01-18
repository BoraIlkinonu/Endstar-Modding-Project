using System;
using System.Collections.Generic;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared.DataTypes;

namespace Endless.Creator.LevelEditing.Runtime
{
	// Token: 0x02000346 RID: 838
	public class ServerCopyHistoryEntry
	{
		// Token: 0x1700025D RID: 605
		// (get) Token: 0x06000F8B RID: 3979 RVA: 0x00047C2D File Offset: 0x00045E2D
		// (set) Token: 0x06000F8C RID: 3980 RVA: 0x00047C35 File Offset: 0x00045E35
		public PropEntry Prop { get; set; }

		// Token: 0x1700025E RID: 606
		// (get) Token: 0x06000F8D RID: 3981 RVA: 0x00047C3E File Offset: 0x00045E3E
		// (set) Token: 0x06000F8E RID: 3982 RVA: 0x00047C46 File Offset: 0x00045E46
		public List<WireBundle> EmitterWireBundles { get; set; }

		// Token: 0x1700025F RID: 607
		// (get) Token: 0x06000F8F RID: 3983 RVA: 0x00047C4F File Offset: 0x00045E4F
		// (set) Token: 0x06000F90 RID: 3984 RVA: 0x00047C57 File Offset: 0x00045E57
		public List<WireBundle> ReceiverWireBundles { get; set; }

		// Token: 0x06000F91 RID: 3985 RVA: 0x00047C60 File Offset: 0x00045E60
		public WireBundle[] CopyEmitterBundles(SerializableGuid newInstanceId)
		{
			WireBundle[] array = new WireBundle[this.EmitterWireBundles.Count];
			for (int i = 0; i < this.EmitterWireBundles.Count; i++)
			{
				WireBundle wireBundle = this.EmitterWireBundles[i].Copy(true);
				wireBundle.EmitterInstanceId = newInstanceId;
				array[i] = wireBundle;
			}
			return array;
		}

		// Token: 0x06000F92 RID: 3986 RVA: 0x00047CB4 File Offset: 0x00045EB4
		public WireBundle[] CopyReceiverBundles(SerializableGuid newInstanceId)
		{
			WireBundle[] array = new WireBundle[this.ReceiverWireBundles.Count];
			for (int i = 0; i < this.ReceiverWireBundles.Count; i++)
			{
				WireBundle wireBundle = this.ReceiverWireBundles[i].Copy(true);
				wireBundle.ReceiverInstanceId = newInstanceId;
				array[i] = wireBundle;
			}
			return array;
		}
	}
}
