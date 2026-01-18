using Endless.Gameplay.LevelEditing.Level;
using UnityEngine;

namespace Endless.Gameplay.LevelEditing.Tilesets;

public abstract class FringeSet
{
	public abstract void AddFringe(Transform parent, Vector3Int coordinate, TileSpawnContext context, Stage stage);

	protected void SpawnFringeWithRotationIndex(GameObject fringe, int index, Transform parent)
	{
		if (!(fringe == null))
		{
			Transform transform = Object.Instantiate(fringe.transform, parent, worldPositionStays: false);
			transform.localPosition = Vector3.zero;
			transform.localRotation = Quaternion.Euler(0f, (float)index * 90f, 0f);
		}
	}
}
