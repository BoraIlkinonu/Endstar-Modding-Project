using System;
using Endless.Gameplay.LevelEditing.Level;
using UnityEngine;

namespace Endless.Gameplay.LevelEditing.Tilesets
{
	// Token: 0x02000513 RID: 1299
	public abstract class FringeSet
	{
		// Token: 0x06001F73 RID: 8051
		public abstract void AddFringe(Transform parent, Vector3Int coordinate, TileSpawnContext context, Stage stage);

		// Token: 0x06001F74 RID: 8052 RVA: 0x0008B60A File Offset: 0x0008980A
		protected void SpawnFringeWithRotationIndex(GameObject fringe, int index, Transform parent)
		{
			if (fringe == null)
			{
				return;
			}
			Transform transform = global::UnityEngine.Object.Instantiate<Transform>(fringe.transform, parent, false);
			transform.localPosition = Vector3.zero;
			transform.localRotation = Quaternion.Euler(0f, (float)index * 90f, 0f);
		}
	}
}
