using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Endless.Shared.DataTypes;

namespace Endless.Gameplay.LevelEditing.Level
{
	// Token: 0x02000545 RID: 1349
	[Serializable]
	public class LevelData
	{
		// Token: 0x06002089 RID: 8329 RVA: 0x0009266D File Offset: 0x0009086D
		public LevelData()
		{
		}

		// Token: 0x0600208A RID: 8330 RVA: 0x00092698 File Offset: 0x00090898
		public LevelData(LevelData reference)
		{
			this.Name = reference.Name;
			this.Description = reference.Description;
			this.Archived = reference.Archived;
			this.SpawnPoints = reference.SpawnPoints;
		}

		// Token: 0x0600208B RID: 8331 RVA: 0x000926FC File Offset: 0x000908FC
		public LevelData(LevelState levelState)
		{
			this.Name = levelState.Name;
			this.Description = levelState.Description;
			HashSet<SerializableGuid> hashSet = new HashSet<SerializableGuid>();
			foreach (SerializableGuid serializableGuid in levelState.SpawnPointIds)
			{
				hashSet.Add(serializableGuid);
			}
			foreach (PropEntry propEntry in levelState.PropEntries)
			{
				if (hashSet.Contains(propEntry.InstanceId))
				{
					this.SpawnPoints.Add(new ValueTuple<SerializableGuid, string>(propEntry.InstanceId, propEntry.Label));
				}
			}
		}

		// Token: 0x040019FB RID: 6651
		public string Name = string.Empty;

		// Token: 0x040019FC RID: 6652
		public string Description = string.Empty;

		// Token: 0x040019FD RID: 6653
		public bool Archived;

		// Token: 0x040019FE RID: 6654
		[TupleElementNames(new string[] { "InstanceId", "Label" })]
		public List<ValueTuple<SerializableGuid, string>> SpawnPoints = new List<ValueTuple<SerializableGuid, string>>();
	}
}
