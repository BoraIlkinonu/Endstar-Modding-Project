using System;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using UnityEngine;

namespace Endless.Creator.LevelEditing
{
	// Token: 0x02000344 RID: 836
	public class WorldBoundaryMarker : MonoBehaviourSingleton<WorldBoundaryMarker>
	{
		// Token: 0x06000F79 RID: 3961 RVA: 0x000478F3 File Offset: 0x00045AF3
		protected override void Awake()
		{
			base.Awake();
			this.boundaryGrids = base.GetComponentsInChildren<BoundaryGrid>(true);
		}

		// Token: 0x06000F7A RID: 3962 RVA: 0x00047908 File Offset: 0x00045B08
		private void Start()
		{
			this.SetActiveState(false);
		}

		// Token: 0x06000F7B RID: 3963 RVA: 0x00047914 File Offset: 0x00045B14
		public void UpdateTo(Vector3Int minExtents, Vector3Int maxExtents)
		{
			Vector3 vector = minExtents - new Vector3(0.5f, 0.5f, 0.5f);
			Vector3 vector2 = maxExtents + new Vector3(0.5f, 0.5f, 0.5f);
			Vector3Int vector3Int = maxExtents - minExtents;
			Vector3Int vector3Int2 = new Vector3Int(100, 100, 100) - vector3Int;
			vector -= vector3Int2;
			vector2 += vector3Int2;
			Vector3Int vector3Int3 = new Vector3Int(100, 100, 100) + vector3Int2 + Vector3Int.one;
			this.minimumXGrid.position = vector;
			this.minimumXGrid.localScale = vector3Int3;
			this.minimumYGrid.position = vector;
			this.minimumYGrid.localScale = vector3Int3;
			this.minimumZGrid.position = vector;
			this.minimumZGrid.localScale = vector3Int3;
			this.maximumXGrid.position = vector2;
			this.maximumXGrid.localScale = vector3Int3;
			this.maximumYGrid.position = vector2;
			this.maximumYGrid.localScale = vector3Int3;
			this.maximumZGrid.position = vector2;
			this.maximumZGrid.localScale = vector3Int3;
		}

		// Token: 0x06000F7C RID: 3964 RVA: 0x00047A68 File Offset: 0x00045C68
		private void Update()
		{
			if (MonoBehaviourSingleton<StageManager>.Instance.ActiveStage == null)
			{
				return;
			}
			bool flag = false;
			if (this.lastMinExtents != MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.MinimumExtents)
			{
				this.lastMinExtents = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.MinimumExtents;
				flag = true;
			}
			if (this.lastMaxExtents != MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.MaximumExtents)
			{
				this.lastMaxExtents = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.MaximumExtents;
				flag = true;
			}
			if (flag)
			{
				this.UpdateTo(this.lastMinExtents, this.lastMaxExtents);
			}
		}

		// Token: 0x06000F7D RID: 3965 RVA: 0x00047B08 File Offset: 0x00045D08
		public void Track(Transform trackedTransform)
		{
			BoundaryGrid[] array = this.boundaryGrids;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Track(trackedTransform);
			}
		}

		// Token: 0x06000F7E RID: 3966 RVA: 0x00047B34 File Offset: 0x00045D34
		public void Untrack(Transform untrackedTransform)
		{
			BoundaryGrid[] array = this.boundaryGrids;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Untrack(untrackedTransform);
			}
		}

		// Token: 0x06000F7F RID: 3967 RVA: 0x00047B60 File Offset: 0x00045D60
		public void SetFadeDistance(float fadeDistance)
		{
			foreach (BoundaryGrid boundaryGrid in this.boundaryGrids)
			{
				float num = Mathf.Clamp(fadeDistance, boundaryGrid.FadeDistance, 25f);
				boundaryGrid.SetFadeDistance(num);
			}
		}

		// Token: 0x06000F80 RID: 3968 RVA: 0x00047B9F File Offset: 0x00045D9F
		public void SetActiveState(bool active)
		{
			this.rootContainer.SetActive(active);
		}

		// Token: 0x04000CE0 RID: 3296
		[SerializeField]
		private GameObject rootContainer;

		// Token: 0x04000CE1 RID: 3297
		[SerializeField]
		private Transform minimumXGrid;

		// Token: 0x04000CE2 RID: 3298
		[SerializeField]
		private Transform maximumXGrid;

		// Token: 0x04000CE3 RID: 3299
		[SerializeField]
		private Transform minimumYGrid;

		// Token: 0x04000CE4 RID: 3300
		[SerializeField]
		private Transform maximumYGrid;

		// Token: 0x04000CE5 RID: 3301
		[SerializeField]
		private Transform minimumZGrid;

		// Token: 0x04000CE6 RID: 3302
		[SerializeField]
		private Transform maximumZGrid;

		// Token: 0x04000CE7 RID: 3303
		private Vector3Int lastMinExtents = new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue);

		// Token: 0x04000CE8 RID: 3304
		private Vector3Int lastMaxExtents = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);

		// Token: 0x04000CE9 RID: 3305
		private BoundaryGrid[] boundaryGrids;
	}
}
