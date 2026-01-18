using System;
using Endless.TerrainCosmetics;
using UnityEngine;

namespace Runtime.Gameplay.LevelEditing.Levels
{
	// Token: 0x02000027 RID: 39
	[CreateAssetMenu(menuName = "Level Editing/Collision Library")]
	public class CollisionLibrary : ScriptableObject
	{
		// Token: 0x06000092 RID: 146 RVA: 0x00003C20 File Offset: 0x00001E20
		public void ApplyCollisionToVisual(Transform visual, int index, TilesetType tilesetType)
		{
			switch (tilesetType)
			{
			case TilesetType.Slope:
			{
				int num = ((index >= 0 && index < this.slopeCollisionMeshes.Length) ? index : 0);
				GameObject gameObject = new GameObject("Collision Holder");
				gameObject.transform.SetParent(visual);
				gameObject.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
				for (int i = 0; i < this.slopeCollisionMeshes[num].CollisionMeshes.Length; i++)
				{
					MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
					meshCollider.sharedMesh = this.slopeCollisionMeshes[num].CollisionMeshes[i];
					meshCollider.convex = true;
				}
				return;
			}
			}
			BoxCollider boxCollider = visual.gameObject.AddComponent<BoxCollider>();
			boxCollider.center = Vector3.zero;
			boxCollider.size = Vector3.one;
		}

		// Token: 0x04000058 RID: 88
		[SerializeField]
		private SlopeCollisionEntry[] slopeCollisionMeshes;
	}
}
