using System;
using System.Collections.Generic;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared.DataTypes;

namespace Endless.Creator.UI
{
	// Token: 0x02000184 RID: 388
	public static class UISpawnPointUtility
	{
		// Token: 0x060005B6 RID: 1462 RVA: 0x0001DC74 File Offset: 0x0001BE74
		public static List<UISpawnPoint> GetSpawnPointsFrom(IReadOnlyList<SerializableGuid> spawnPointIds, IReadOnlyList<PropEntry> propEntries)
		{
			List<UISpawnPoint> list = new List<UISpawnPoint>();
			for (int i = 0; i < spawnPointIds.Count; i++)
			{
				SerializableGuid serializableGuid = spawnPointIds[i];
				string spawnPointLabel = UISpawnPointUtility.GetSpawnPointLabel(serializableGuid, propEntries);
				UISpawnPoint uispawnPoint = new UISpawnPoint(serializableGuid, spawnPointLabel);
				list.Add(uispawnPoint);
			}
			return list;
		}

		// Token: 0x060005B7 RID: 1463 RVA: 0x0001DCBC File Offset: 0x0001BEBC
		public static string GetSpawnPointLabel(SerializableGuid spawnPointId, IReadOnlyList<PropEntry> propEntries)
		{
			foreach (PropEntry propEntry in propEntries)
			{
				if (propEntry.InstanceId == spawnPointId)
				{
					return propEntry.Label;
				}
			}
			return "Spawn Point Removed";
		}

		// Token: 0x04000508 RID: 1288
		private const bool VERBOSE_LOGGING = false;
	}
}
