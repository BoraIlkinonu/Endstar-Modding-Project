using System;
using System.Collections.Generic;
using Endless.Shared.DataTypes;
using UnityEngine;

namespace Endless.Gameplay.LevelEditing.Level
{
	// Token: 0x02000563 RID: 1379
	[Serializable]
	public class SerializedLevel
	{
		// Token: 0x17000654 RID: 1620
		// (get) Token: 0x06002124 RID: 8484 RVA: 0x00095122 File Offset: 0x00093322
		public IReadOnlyList<SerializedTerrainCell> SerializedTerrainCells
		{
			get
			{
				return this.serializedTerrainCells;
			}
		}

		// Token: 0x17000655 RID: 1621
		// (get) Token: 0x06002125 RID: 8485 RVA: 0x0009512A File Offset: 0x0009332A
		public IReadOnlyList<SerializedProp> SerializedProps
		{
			get
			{
				return this.serializedProps;
			}
		}

		// Token: 0x06002126 RID: 8486 RVA: 0x00095132 File Offset: 0x00093332
		public SerializedLevel()
		{
			this.serializedTerrainCells = new List<SerializedTerrainCell>();
			this.serializedProps = new List<SerializedProp>();
		}

		// Token: 0x06002127 RID: 8487 RVA: 0x00095150 File Offset: 0x00093350
		public void AddTerrainCell(Vector3Int coordinate, int tilesetIndex)
		{
			this.serializedTerrainCells.Add(new SerializedTerrainCell
			{
				Coordinate = coordinate,
				TilesetIndex = tilesetIndex
			});
		}

		// Token: 0x06002128 RID: 8488 RVA: 0x00095170 File Offset: 0x00093370
		public void AddProp(Vector3 position, float rotation, SerializableGuid masterGuid)
		{
			this.serializedProps.Add(new SerializedProp
			{
				Position = position,
				Rotation = rotation,
				MasterReference = masterGuid
			});
		}

		// Token: 0x04001A5E RID: 6750
		[SerializeField]
		private List<SerializedTerrainCell> serializedTerrainCells;

		// Token: 0x04001A5F RID: 6751
		[SerializeField]
		private List<SerializedProp> serializedProps;
	}
}
