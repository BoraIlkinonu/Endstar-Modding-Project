using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Shared;
using Endless.TempUI;
using UnityEngine;

namespace Endless.Creator.LevelEditing.Runtime
{
	// Token: 0x0200036B RID: 875
	public class MarkerManager : MonoBehaviourSingleton<MarkerManager>
	{
		// Token: 0x0600109A RID: 4250 RVA: 0x0004F978 File Offset: 0x0004DB78
		protected override void Awake()
		{
			base.Awake();
			if (MonoBehaviourSingleton<MarkerManager>.Instance == this)
			{
				for (int i = 0; i < 5; i++)
				{
					this.activeMarkerDictionaryByType.TryAdd((MarkerType)i, new Dictionary<Vector3Int, Transform>());
				}
				SimpleObjectPool<Transform> simpleObjectPool = new SimpleObjectPool<Transform>(this.createMarkerTemplate, base.transform, null);
				SimpleObjectPool<Transform> simpleObjectPool2 = new SimpleObjectPool<Transform>(this.eraseMarkerTemplate, base.transform, null);
				SimpleObjectPool<Transform> simpleObjectPool3 = new SimpleObjectPool<Transform>(this.ignoredMarkerTemplate, base.transform, null);
				SimpleObjectPool<Transform> simpleObjectPool4 = new SimpleObjectPool<Transform>(this.cellHighlightTemplate, base.transform, null);
				SimpleObjectPool<Transform> simpleObjectPool5 = new SimpleObjectPool<Transform>(this.propLocationTemplate, base.transform, null);
				this.poolsByMarkerType.Add(MarkerType.Ignore, simpleObjectPool3);
				this.poolsByMarkerType.Add(MarkerType.Create, simpleObjectPool);
				this.poolsByMarkerType.Add(MarkerType.Erase, simpleObjectPool2);
				this.poolsByMarkerType.Add(MarkerType.CellHighlight, simpleObjectPool4);
				this.poolsByMarkerType.Add(MarkerType.PropLocation, simpleObjectPool5);
			}
		}

		// Token: 0x0600109B RID: 4251 RVA: 0x0004FA60 File Offset: 0x0004DC60
		public void MarkCoordinate(Vector3Int coordinate, MarkerType markerType, float? rotation = null)
		{
			Dictionary<Vector3Int, Transform> dictionary = this.activeMarkerDictionaryByType[markerType];
			SimpleObjectPool<Transform> simpleObjectPool = this.poolsByMarkerType[markerType];
			Transform transform;
			if (!dictionary.ContainsKey(coordinate))
			{
				simpleObjectPool.Pool.Get(out transform);
				dictionary.Add(coordinate, transform);
			}
			else
			{
				transform = dictionary[coordinate];
			}
			if (markerType == MarkerType.CellHighlight)
			{
				transform.GetComponent<CellReferenceIndicator>().SetRotationArrowEnabled(rotation != null);
			}
			transform.position = coordinate;
			transform.rotation = Quaternion.Euler(0f, rotation.GetValueOrDefault(), 0f);
		}

		// Token: 0x0600109C RID: 4252 RVA: 0x0004FAF0 File Offset: 0x0004DCF0
		public void MarkCoordinates(IEnumerable<Vector3Int> coordinates, MarkerType markerType)
		{
			Dictionary<Vector3Int, Transform> dictionary = this.activeMarkerDictionaryByType[markerType];
			SimpleObjectPool<Transform> simpleObjectPool = this.poolsByMarkerType[markerType];
			this.UpdateMarkers(coordinates, dictionary, simpleObjectPool);
		}

		// Token: 0x0600109D RID: 4253 RVA: 0x0004FB20 File Offset: 0x0004DD20
		private void UpdateMarkers(IEnumerable<Vector3Int> coordinates, Dictionary<Vector3Int, Transform> activeMarkers, SimpleObjectPool<Transform> pool)
		{
			foreach (Vector3Int vector3Int in coordinates)
			{
				if (!activeMarkers.ContainsKey(vector3Int))
				{
					Transform transform;
					pool.Pool.Get(out transform);
					transform.gameObject.SetActive(true);
					transform.position = vector3Int;
					activeMarkers.Add(vector3Int, transform);
				}
			}
			foreach (Vector3Int vector3Int2 in ((IEnumerable<Vector3Int>)activeMarkers.Keys.Except(coordinates).ToArray<Vector3Int>()))
			{
				activeMarkers[vector3Int2].gameObject.SetActive(false);
				pool.Pool.Release(activeMarkers[vector3Int2]);
				activeMarkers.Remove(vector3Int2);
			}
		}

		// Token: 0x0600109E RID: 4254 RVA: 0x0004FC04 File Offset: 0x0004DE04
		public void ReleaseAllMarkers()
		{
			this.ClearMarkersOfType(MarkerType.Ignore);
			this.ClearMarkersOfType(MarkerType.Create);
			this.ClearMarkersOfType(MarkerType.Erase);
			this.ClearMarkersOfType(MarkerType.CellHighlight);
			this.ClearMarkersOfType(MarkerType.PropLocation);
		}

		// Token: 0x0600109F RID: 4255 RVA: 0x0004FC2C File Offset: 0x0004DE2C
		public void ClearMarkersOfType(MarkerType markerType)
		{
			Dictionary<Vector3Int, Transform> dictionary = this.activeMarkerDictionaryByType[markerType];
			SimpleObjectPool<Transform> simpleObjectPool = this.poolsByMarkerType[markerType];
			foreach (Transform transform in dictionary.Values)
			{
				simpleObjectPool.Pool.Release(transform);
			}
			dictionary.Clear();
		}

		// Token: 0x04000DBC RID: 3516
		[SerializeField]
		private Transform createMarkerTemplate;

		// Token: 0x04000DBD RID: 3517
		[SerializeField]
		private Transform eraseMarkerTemplate;

		// Token: 0x04000DBE RID: 3518
		[SerializeField]
		private Transform ignoredMarkerTemplate;

		// Token: 0x04000DBF RID: 3519
		[SerializeField]
		private Transform cellHighlightTemplate;

		// Token: 0x04000DC0 RID: 3520
		[SerializeField]
		private Transform propLocationTemplate;

		// Token: 0x04000DC1 RID: 3521
		private Dictionary<MarkerType, SimpleObjectPool<Transform>> poolsByMarkerType = new Dictionary<MarkerType, SimpleObjectPool<Transform>>();

		// Token: 0x04000DC2 RID: 3522
		private Dictionary<MarkerType, Dictionary<Vector3Int, Transform>> activeMarkerDictionaryByType = new Dictionary<MarkerType, Dictionary<Vector3Int, Transform>>();
	}
}
