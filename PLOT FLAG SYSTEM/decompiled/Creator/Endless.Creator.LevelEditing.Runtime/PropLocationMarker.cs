using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Props.Assets;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.TempUI;
using UnityEngine;

namespace Endless.Creator.LevelEditing.Runtime;

public class PropLocationMarker : MonoBehaviourSingleton<PropLocationMarker>
{
	private const int MAX_CLAIMABLE_CELL_LOCATIONS = 2000;

	private Vector3Int threeDCursorLocation = new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue);

	[SerializeField]
	private Transform propLocationTemplate;

	[SerializeField]
	private SimpleObjectPool<Transform> propLocationPool;

	private Dictionary<SerializableGuid, List<PropLocationMarkRecord>> idToLocationMarkRecords = new Dictionary<SerializableGuid, List<PropLocationMarkRecord>>();

	private Dictionary<Vector3Int, HashSet<SerializableGuid>> cellLocationClaimers = new Dictionary<Vector3Int, HashSet<SerializableGuid>>();

	private Dictionary<Vector3Int, Transform> activePoolEntries = new Dictionary<Vector3Int, Transform>();

	protected override void Awake()
	{
		base.Awake();
		propLocationPool = new SimpleObjectPool<Transform>(propLocationTemplate, base.transform);
	}

	public void MarkCellsForGhostProp(SerializableGuid assetId, Vector3 position, Quaternion rotation, PropLocationType propLocationType)
	{
		Vector3Int[] array = Array.Empty<Vector3Int>();
		if (!MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(assetId, out var metadata))
		{
			array = Array.Empty<Vector3Int>();
		}
		else
		{
			if (metadata.PropData.PropLocationOffsets.Count > 2000)
			{
				return;
			}
			array = (from x in MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetRotatedPropLocations(metadata.PropData, position, rotation)
				select x.Offset).ToArray();
		}
		if (!idToLocationMarkRecords.TryGetValue(assetId, out var value))
		{
			value = new List<PropLocationMarkRecord>();
			idToLocationMarkRecords.Add(assetId, value);
		}
		if (value.Any())
		{
			PropLocationMarkRecord propLocationMarkRecord = value[value.Count() - 1];
			if (propLocationMarkRecord.Type == propLocationType)
			{
				UpdateMarks(propLocationMarkRecord, array, remark: true);
				return;
			}
		}
		PropLocationMarkRecord propLocationMarkRecord2 = null;
		for (int num = 0; num < value.Count(); num++)
		{
			if (value[num].Type == propLocationType)
			{
				propLocationMarkRecord2 = value[num];
				break;
			}
		}
		if (propLocationMarkRecord2 != null)
		{
			UpdateMarks(propLocationMarkRecord2, array, remark: false);
			return;
		}
		propLocationMarkRecord2 = new PropLocationMarkRecord
		{
			Id = assetId,
			Locations = array.ToList(),
			Type = propLocationType
		};
		Mark(propLocationMarkRecord2);
		value.Add(propLocationMarkRecord2);
	}

	public void MarkCellsForProp(SerializableGuid instanceId, PropLocationType propLocationType)
	{
		if (!MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.TryGetAssetDefinitionFromInstanceId(instanceId, out var assetId) || !MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(assetId, out var metadata) || metadata.PropData.PropLocationOffsets.Count > 2000)
		{
			return;
		}
		Vector3Int[] propLocations = GetPropLocations(metadata, instanceId);
		if (!idToLocationMarkRecords.TryGetValue(instanceId, out var value))
		{
			value = new List<PropLocationMarkRecord>();
			idToLocationMarkRecords.Add(instanceId, value);
		}
		if (value.Any())
		{
			PropLocationMarkRecord propLocationMarkRecord = value[value.Count() - 1];
			if (propLocationMarkRecord.Type == propLocationType)
			{
				UpdateMarks(propLocationMarkRecord, propLocations, remark: true);
				return;
			}
		}
		PropLocationMarkRecord propLocationMarkRecord2 = null;
		for (int i = 0; i < value.Count(); i++)
		{
			if (value[i].Type == propLocationType)
			{
				propLocationMarkRecord2 = value[i];
				break;
			}
		}
		if (propLocationMarkRecord2 != null)
		{
			UpdateMarks(propLocationMarkRecord2, propLocations, remark: false);
			return;
		}
		propLocationMarkRecord2 = new PropLocationMarkRecord
		{
			Id = instanceId,
			Locations = propLocations.ToList(),
			Type = propLocationType
		};
		Mark(propLocationMarkRecord2);
		value.Add(propLocationMarkRecord2);
	}

	private Vector3Int[] GetPropLocations(PropLibrary.RuntimePropInfo metadata, SerializableGuid instanceId)
	{
		GameObject gameObjectFromInstanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetGameObjectFromInstanceId(instanceId);
		if ((SerializableGuid)metadata.PropData.AssetID == SerializableGuid.Empty || !gameObjectFromInstanceId)
		{
			return Array.Empty<Vector3Int>();
		}
		return (from x in MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetRotatedPropLocations(metadata.PropData, gameObjectFromInstanceId.transform.position, gameObjectFromInstanceId.transform.rotation)
			select x.Offset).ToArray();
	}

	private void UpdateMarks(PropLocationMarkRecord markRecord, Vector3Int[] locations, bool remark)
	{
		if (markRecord.Locations.Except(locations).Any())
		{
			markRecord.Locations = markRecord.Locations.Except(locations).ToList();
			Unmark(markRecord);
			markRecord.Locations = locations.ToList();
			if (remark)
			{
				Mark(markRecord);
			}
		}
	}

	private void Unmark(PropLocationMarkRecord markRecord)
	{
		for (int i = 0; i < markRecord.Locations.Count(); i++)
		{
			Vector3Int key = markRecord.Locations[i];
			if (activePoolEntries.ContainsKey(key) && cellLocationClaimers.TryGetValue(key, out var value))
			{
				value.Remove(markRecord.Id);
				if (value.Count == 0)
				{
					propLocationPool.Pool.Release(activePoolEntries[key]);
					activePoolEntries.Remove(key);
				}
			}
		}
	}

	private void Mark(PropLocationMarkRecord markRecord)
	{
		for (int i = 0; i < markRecord.Locations.Count(); i++)
		{
			Vector3Int vector3Int = markRecord.Locations[i];
			if (cellLocationClaimers.TryGetValue(vector3Int, out var value))
			{
				value.Add(markRecord.Id);
			}
			else
			{
				HashSet<SerializableGuid> value2 = new HashSet<SerializableGuid> { markRecord.Id };
				cellLocationClaimers.Add(vector3Int, value2);
			}
			if (!activePoolEntries.ContainsKey(vector3Int))
			{
				propLocationPool.Pool.Get(out var v);
				v.position = vector3Int;
				activePoolEntries.Add(vector3Int, v);
				BoundaryGrid[] componentsInChildren = v.GetComponentsInChildren<BoundaryGrid>();
				for (int j = 0; j < componentsInChildren.Length; j++)
				{
					componentsInChildren[j].SetLineColor(GetColorForType(markRecord.Type));
				}
				v.gameObject.SetActive(vector3Int != threeDCursorLocation);
			}
			else
			{
				Transform transform = activePoolEntries[vector3Int];
				BoundaryGrid[] componentsInChildren = transform.GetComponentsInChildren<BoundaryGrid>();
				for (int j = 0; j < componentsInChildren.Length; j++)
				{
					componentsInChildren[j].SetLineColor(GetColorForType(markRecord.Type));
				}
				transform.gameObject.SetActive(vector3Int != threeDCursorLocation);
			}
		}
	}

	private Color GetColorForType(PropLocationType type)
	{
		return type switch
		{
			PropLocationType.Emitter => new Color(0.7f, 0.4f, 0.2f, 1f), 
			PropLocationType.Receiver => new Color(0.2f, 0.4f, 0.7f, 1f), 
			PropLocationType.NoAction => new Color(0.3f, 0.3f, 0.3f, 1f), 
			_ => Color.white, 
		};
	}

	public void UnmarkPropAndType(SerializableGuid instanceId, PropLocationType type)
	{
		if (!idToLocationMarkRecords.TryGetValue(instanceId, out var value))
		{
			return;
		}
		int num = -1;
		for (int i = 0; i < value.Count; i++)
		{
			if (value[i].Type != type)
			{
				continue;
			}
			num = i;
			if (i == value.Count - 1)
			{
				Unmark(value[i]);
				if (value.Count >= 2)
				{
					List<PropLocationMarkRecord> list = value;
					Mark(list[list.Count - 2]);
				}
			}
			break;
		}
		if (num >= 0)
		{
			value.RemoveAt(num);
		}
	}

	public void Set3DCursorLocation(Vector3Int location)
	{
		if (threeDCursorLocation != location)
		{
			ToggleMarkedLocation(threeDCursorLocation, state: true);
			ToggleMarkedLocation(location, state: false);
			threeDCursorLocation = location;
		}
	}

	private void ToggleMarkedLocation(Vector3Int location, bool state)
	{
		if (activePoolEntries.TryGetValue(location, out var value))
		{
			value.gameObject.SetActive(state);
		}
	}

	public void ClearAllMarkers()
	{
		foreach (Vector3Int item in activePoolEntries.Keys.ToList())
		{
			propLocationPool.Pool.Release(activePoolEntries[item]);
		}
		activePoolEntries.Clear();
		idToLocationMarkRecords.Clear();
		cellLocationClaimers.Clear();
	}
}
