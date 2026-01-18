using Endless.TerrainCosmetics;
using UnityEngine;

namespace Runtime.Gameplay.LevelEditing.Levels;

[CreateAssetMenu(menuName = "Level Editing/Collision Library")]
public class CollisionLibrary : ScriptableObject
{
	[SerializeField]
	private SlopeCollisionEntry[] slopeCollisionMeshes;

	public void ApplyCollisionToVisual(Transform visual, int index, TilesetType tilesetType)
	{
		switch (tilesetType)
		{
		case TilesetType.Slope:
		{
			int num = ((index >= 0 && index < slopeCollisionMeshes.Length) ? index : 0);
			GameObject gameObject = new GameObject("Collision Holder");
			gameObject.transform.SetParent(visual);
			gameObject.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
			for (int i = 0; i < slopeCollisionMeshes[num].CollisionMeshes.Length; i++)
			{
				MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
				meshCollider.sharedMesh = slopeCollisionMeshes[num].CollisionMeshes[i];
				meshCollider.convex = true;
			}
			break;
		}
		default:
		{
			BoxCollider boxCollider = visual.gameObject.AddComponent<BoxCollider>();
			boxCollider.center = Vector3.zero;
			boxCollider.size = Vector3.one;
			break;
		}
		}
	}
}
