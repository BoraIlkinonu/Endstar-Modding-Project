using System.Collections.Generic;
using System.Linq;
using Endless.Shared;
using Endless.TempUI;
using UnityEngine;

namespace Endless.Creator.LevelEditing.Runtime;

public class MarkerManager : MonoBehaviourSingleton<MarkerManager>
{
	[SerializeField]
	private Transform createMarkerTemplate;

	[SerializeField]
	private Transform eraseMarkerTemplate;

	[SerializeField]
	private Transform ignoredMarkerTemplate;

	[SerializeField]
	private Transform cellHighlightTemplate;

	[SerializeField]
	private Transform propLocationTemplate;

	private Dictionary<MarkerType, SimpleObjectPool<Transform>> poolsByMarkerType = new Dictionary<MarkerType, SimpleObjectPool<Transform>>();

	private Dictionary<MarkerType, Dictionary<Vector3Int, Transform>> activeMarkerDictionaryByType = new Dictionary<MarkerType, Dictionary<Vector3Int, Transform>>();

	protected override void Awake()
	{
		base.Awake();
		if (MonoBehaviourSingleton<MarkerManager>.Instance == this)
		{
			for (int i = 0; i < 5; i++)
			{
				activeMarkerDictionaryByType.TryAdd((MarkerType)i, new Dictionary<Vector3Int, Transform>());
			}
			SimpleObjectPool<Transform> value = new SimpleObjectPool<Transform>(createMarkerTemplate, base.transform);
			SimpleObjectPool<Transform> value2 = new SimpleObjectPool<Transform>(eraseMarkerTemplate, base.transform);
			SimpleObjectPool<Transform> value3 = new SimpleObjectPool<Transform>(ignoredMarkerTemplate, base.transform);
			SimpleObjectPool<Transform> value4 = new SimpleObjectPool<Transform>(cellHighlightTemplate, base.transform);
			SimpleObjectPool<Transform> value5 = new SimpleObjectPool<Transform>(propLocationTemplate, base.transform);
			poolsByMarkerType.Add(MarkerType.Ignore, value3);
			poolsByMarkerType.Add(MarkerType.Create, value);
			poolsByMarkerType.Add(MarkerType.Erase, value2);
			poolsByMarkerType.Add(MarkerType.CellHighlight, value4);
			poolsByMarkerType.Add(MarkerType.PropLocation, value5);
		}
	}

	public void MarkCoordinate(Vector3Int coordinate, MarkerType markerType, float? rotation = null)
	{
		Dictionary<Vector3Int, Transform> dictionary = activeMarkerDictionaryByType[markerType];
		SimpleObjectPool<Transform> simpleObjectPool = poolsByMarkerType[markerType];
		Transform v;
		if (!dictionary.ContainsKey(coordinate))
		{
			simpleObjectPool.Pool.Get(out v);
			dictionary.Add(coordinate, v);
		}
		else
		{
			v = dictionary[coordinate];
		}
		if (markerType == MarkerType.CellHighlight)
		{
			v.GetComponent<CellReferenceIndicator>().SetRotationArrowEnabled(rotation.HasValue);
		}
		v.position = coordinate;
		v.rotation = Quaternion.Euler(0f, rotation.GetValueOrDefault(), 0f);
	}

	public void MarkCoordinates(IEnumerable<Vector3Int> coordinates, MarkerType markerType)
	{
		Dictionary<Vector3Int, Transform> activeMarkers = activeMarkerDictionaryByType[markerType];
		SimpleObjectPool<Transform> pool = poolsByMarkerType[markerType];
		UpdateMarkers(coordinates, activeMarkers, pool);
	}

	private void UpdateMarkers(IEnumerable<Vector3Int> coordinates, Dictionary<Vector3Int, Transform> activeMarkers, SimpleObjectPool<Transform> pool)
	{
		foreach (Vector3Int coordinate in coordinates)
		{
			if (!activeMarkers.ContainsKey(coordinate))
			{
				pool.Pool.Get(out var v);
				v.gameObject.SetActive(value: true);
				v.position = coordinate;
				activeMarkers.Add(coordinate, v);
			}
		}
		foreach (Vector3Int item in (IEnumerable<Vector3Int>)activeMarkers.Keys.Except(coordinates).ToArray())
		{
			activeMarkers[item].gameObject.SetActive(value: false);
			pool.Pool.Release(activeMarkers[item]);
			activeMarkers.Remove(item);
		}
	}

	public void ReleaseAllMarkers()
	{
		ClearMarkersOfType(MarkerType.Ignore);
		ClearMarkersOfType(MarkerType.Create);
		ClearMarkersOfType(MarkerType.Erase);
		ClearMarkersOfType(MarkerType.CellHighlight);
		ClearMarkersOfType(MarkerType.PropLocation);
	}

	public void ClearMarkersOfType(MarkerType markerType)
	{
		Dictionary<Vector3Int, Transform> dictionary = activeMarkerDictionaryByType[markerType];
		SimpleObjectPool<Transform> simpleObjectPool = poolsByMarkerType[markerType];
		foreach (Transform value in dictionary.Values)
		{
			simpleObjectPool.Pool.Release(value);
		}
		dictionary.Clear();
	}
}
