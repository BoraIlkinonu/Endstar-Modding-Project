using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.Scripting;
using Endless.Shared;
using Endless.Shared.DataTypes;

namespace Endless.Gameplay
{
	// Token: 0x020002BD RID: 701
	[Serializable]
	public class LevelDestination
	{
		// Token: 0x06000FF8 RID: 4088 RVA: 0x00051CA9 File Offset: 0x0004FEA9
		public void ChangeLevel(Context context)
		{
			MonoBehaviourSingleton<GameplayManager>.Instance.ChangeLevel(this);
		}

		// Token: 0x06000FF9 RID: 4089 RVA: 0x00051CB6 File Offset: 0x0004FEB6
		public bool IsValidLevel()
		{
			return !this.TargetLevelId.IsEmpty && MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame.levels.Any((LevelReference level) => level.AssetID == this.TargetLevelId);
		}

		// Token: 0x06000FFA RID: 4090 RVA: 0x00051CE7 File Offset: 0x0004FEE7
		public override string ToString()
		{
			return string.Format("Targeting level {0} and {1} spawn points", this.TargetLevelId, this.TargetSpawnPointIds.Count);
		}

		// Token: 0x04000DC2 RID: 3522
		public SerializableGuid TargetLevelId = SerializableGuid.Empty;

		// Token: 0x04000DC3 RID: 3523
		public List<SerializableGuid> TargetSpawnPointIds = new List<SerializableGuid>();
	}
}
