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

namespace Endless.Creator.LevelEditing.Runtime
{
	// Token: 0x0200037F RID: 895
	public class PropLocationMarker : MonoBehaviourSingleton<PropLocationMarker>
	{
		// Token: 0x0600113D RID: 4413 RVA: 0x0005382C File Offset: 0x00051A2C
		protected override void Awake()
		{
			base.Awake();
			this.propLocationPool = new SimpleObjectPool<Transform>(this.propLocationTemplate, base.transform, null);
		}

		// Token: 0x0600113E RID: 4414 RVA: 0x0005384C File Offset: 0x00051A4C
		public void MarkCellsForGhostProp(SerializableGuid assetId, Vector3 position, Quaternion rotation, PropLocationType propLocationType)
		{
			Vector3Int[] array = Array.Empty<Vector3Int>();
			PropLibrary.RuntimePropInfo runtimePropInfo;
			if (!MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(assetId, out runtimePropInfo))
			{
				array = Array.Empty<Vector3Int>();
			}
			else
			{
				if (runtimePropInfo.PropData.PropLocationOffsets.Count > 2000)
				{
					return;
				}
				array = (from x in MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetRotatedPropLocations(runtimePropInfo.PropData, position, rotation)
					select x.Offset).ToArray<Vector3Int>();
			}
			List<PropLocationMarkRecord> list;
			if (!this.idToLocationMarkRecords.TryGetValue(assetId, out list))
			{
				list = new List<PropLocationMarkRecord>();
				this.idToLocationMarkRecords.Add(assetId, list);
			}
			if (list.Any<PropLocationMarkRecord>())
			{
				PropLocationMarkRecord propLocationMarkRecord = list[list.Count<PropLocationMarkRecord>() - 1];
				if (propLocationMarkRecord.Type == propLocationType)
				{
					this.UpdateMarks(propLocationMarkRecord, array, true);
					return;
				}
			}
			PropLocationMarkRecord propLocationMarkRecord2 = null;
			for (int i = 0; i < list.Count<PropLocationMarkRecord>(); i++)
			{
				if (list[i].Type == propLocationType)
				{
					propLocationMarkRecord2 = list[i];
					break;
				}
			}
			if (propLocationMarkRecord2 != null)
			{
				this.UpdateMarks(propLocationMarkRecord2, array, false);
				return;
			}
			propLocationMarkRecord2 = new PropLocationMarkRecord
			{
				Id = assetId,
				Locations = array.ToList<Vector3Int>(),
				Type = propLocationType
			};
			this.Mark(propLocationMarkRecord2);
			list.Add(propLocationMarkRecord2);
		}

		// Token: 0x0600113F RID: 4415 RVA: 0x00053994 File Offset: 0x00051B94
		public void MarkCellsForProp(SerializableGuid instanceId, PropLocationType propLocationType)
		{
			SerializableGuid serializableGuid;
			if (!MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.TryGetAssetDefinitionFromInstanceId(instanceId, out serializableGuid))
			{
				return;
			}
			PropLibrary.RuntimePropInfo runtimePropInfo;
			if (!MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(serializableGuid, out runtimePropInfo))
			{
				return;
			}
			if (runtimePropInfo.PropData.PropLocationOffsets.Count > 2000)
			{
				return;
			}
			Vector3Int[] propLocations = this.GetPropLocations(runtimePropInfo, instanceId);
			List<PropLocationMarkRecord> list;
			if (!this.idToLocationMarkRecords.TryGetValue(instanceId, out list))
			{
				list = new List<PropLocationMarkRecord>();
				this.idToLocationMarkRecords.Add(instanceId, list);
			}
			if (list.Any<PropLocationMarkRecord>())
			{
				PropLocationMarkRecord propLocationMarkRecord = list[list.Count<PropLocationMarkRecord>() - 1];
				if (propLocationMarkRecord.Type == propLocationType)
				{
					this.UpdateMarks(propLocationMarkRecord, propLocations, true);
					return;
				}
			}
			PropLocationMarkRecord propLocationMarkRecord2 = null;
			for (int i = 0; i < list.Count<PropLocationMarkRecord>(); i++)
			{
				if (list[i].Type == propLocationType)
				{
					propLocationMarkRecord2 = list[i];
					break;
				}
			}
			if (propLocationMarkRecord2 != null)
			{
				this.UpdateMarks(propLocationMarkRecord2, propLocations, false);
				return;
			}
			propLocationMarkRecord2 = new PropLocationMarkRecord
			{
				Id = instanceId,
				Locations = propLocations.ToList<Vector3Int>(),
				Type = propLocationType
			};
			this.Mark(propLocationMarkRecord2);
			list.Add(propLocationMarkRecord2);
		}

		// Token: 0x06001140 RID: 4416 RVA: 0x00053AB0 File Offset: 0x00051CB0
		private Vector3Int[] GetPropLocations(PropLibrary.RuntimePropInfo metadata, SerializableGuid instanceId)
		{
			GameObject gameObjectFromInstanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetGameObjectFromInstanceId(instanceId);
			if (metadata.PropData.AssetID == SerializableGuid.Empty || !gameObjectFromInstanceId)
			{
				return Array.Empty<Vector3Int>();
			}
			return (from x in MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetRotatedPropLocations(metadata.PropData, gameObjectFromInstanceId.transform.position, gameObjectFromInstanceId.transform.rotation)
				select x.Offset).ToArray<Vector3Int>();
		}

		// Token: 0x06001141 RID: 4417 RVA: 0x00053B4C File Offset: 0x00051D4C
		private void UpdateMarks(PropLocationMarkRecord markRecord, Vector3Int[] locations, bool remark)
		{
			if (!markRecord.Locations.Except(locations).Any<Vector3Int>())
			{
				return;
			}
			markRecord.Locations = markRecord.Locations.Except(locations).ToList<Vector3Int>();
			this.Unmark(markRecord);
			markRecord.Locations = locations.ToList<Vector3Int>();
			if (remark)
			{
				this.Mark(markRecord);
			}
		}

		// Token: 0x06001142 RID: 4418 RVA: 0x00053BA4 File Offset: 0x00051DA4
		private void Unmark(PropLocationMarkRecord markRecord)
		{
			for (int i = 0; i < markRecord.Locations.Count<Vector3Int>(); i++)
			{
				Vector3Int vector3Int = markRecord.Locations[i];
				HashSet<SerializableGuid> hashSet;
				if (this.activePoolEntries.ContainsKey(vector3Int) && this.cellLocationClaimers.TryGetValue(vector3Int, out hashSet))
				{
					hashSet.Remove(markRecord.Id);
					if (hashSet.Count == 0)
					{
						this.propLocationPool.Pool.Release(this.activePoolEntries[vector3Int]);
						this.activePoolEntries.Remove(vector3Int);
					}
				}
			}
		}

		// Token: 0x06001143 RID: 4419 RVA: 0x00053C30 File Offset: 0x00051E30
		private void Mark(PropLocationMarkRecord markRecord)
		{
			for (int i = 0; i < markRecord.Locations.Count<Vector3Int>(); i++)
			{
				Vector3Int vector3Int = markRecord.Locations[i];
				HashSet<SerializableGuid> hashSet;
				if (this.cellLocationClaimers.TryGetValue(vector3Int, out hashSet))
				{
					hashSet.Add(markRecord.Id);
				}
				else
				{
					HashSet<SerializableGuid> hashSet2 = new HashSet<SerializableGuid> { markRecord.Id };
					this.cellLocationClaimers.Add(vector3Int, hashSet2);
				}
				if (!this.activePoolEntries.ContainsKey(vector3Int))
				{
					Transform transform;
					this.propLocationPool.Pool.Get(out transform);
					transform.position = vector3Int;
					this.activePoolEntries.Add(vector3Int, transform);
					BoundaryGrid[] array = transform.GetComponentsInChildren<BoundaryGrid>();
					for (int j = 0; j < array.Length; j++)
					{
						array[j].SetLineColor(this.GetColorForType(markRecord.Type));
					}
					transform.gameObject.SetActive(vector3Int != this.threeDCursorLocation);
				}
				else
				{
					Transform transform2 = this.activePoolEntries[vector3Int];
					BoundaryGrid[] array = transform2.GetComponentsInChildren<BoundaryGrid>();
					for (int j = 0; j < array.Length; j++)
					{
						array[j].SetLineColor(this.GetColorForType(markRecord.Type));
					}
					transform2.gameObject.SetActive(vector3Int != this.threeDCursorLocation);
				}
			}
		}

		// Token: 0x06001144 RID: 4420 RVA: 0x00053D88 File Offset: 0x00051F88
		private Color GetColorForType(PropLocationType type)
		{
			switch (type)
			{
			case PropLocationType.Emitter:
				return new Color(0.7f, 0.4f, 0.2f, 1f);
			case PropLocationType.Receiver:
				return new Color(0.2f, 0.4f, 0.7f, 1f);
			case PropLocationType.NoAction:
				return new Color(0.3f, 0.3f, 0.3f, 1f);
			}
			return Color.white;
		}

		// Token: 0x06001145 RID: 4421 RVA: 0x00053E04 File Offset: 0x00052004
		public void UnmarkPropAndType(SerializableGuid instanceId, PropLocationType type)
		{
			List<PropLocationMarkRecord> list;
			if (!this.idToLocationMarkRecords.TryGetValue(instanceId, out list))
			{
				return;
			}
			int num = -1;
			int i = 0;
			while (i < list.Count)
			{
				if (list[i].Type == type)
				{
					num = i;
					if (i != list.Count - 1)
					{
						break;
					}
					this.Unmark(list[i]);
					if (list.Count >= 2)
					{
						List<PropLocationMarkRecord> list2 = list;
						this.Mark(list2[list2.Count - 2]);
						break;
					}
					break;
				}
				else
				{
					i++;
				}
			}
			if (num >= 0)
			{
				list.RemoveAt(num);
			}
		}

		// Token: 0x06001146 RID: 4422 RVA: 0x00053E88 File Offset: 0x00052088
		public void Set3DCursorLocation(Vector3Int location)
		{
			if (this.threeDCursorLocation != location)
			{
				this.ToggleMarkedLocation(this.threeDCursorLocation, true);
				this.ToggleMarkedLocation(location, false);
				this.threeDCursorLocation = location;
			}
		}

		// Token: 0x06001147 RID: 4423 RVA: 0x00053EB4 File Offset: 0x000520B4
		private void ToggleMarkedLocation(Vector3Int location, bool state)
		{
			Transform transform;
			if (this.activePoolEntries.TryGetValue(location, out transform))
			{
				transform.gameObject.SetActive(state);
			}
		}

		// Token: 0x06001148 RID: 4424 RVA: 0x00053EE0 File Offset: 0x000520E0
		public void ClearAllMarkers()
		{
			foreach (Vector3Int vector3Int in this.activePoolEntries.Keys.ToList<Vector3Int>())
			{
				this.propLocationPool.Pool.Release(this.activePoolEntries[vector3Int]);
			}
			this.activePoolEntries.Clear();
			this.idToLocationMarkRecords.Clear();
			this.cellLocationClaimers.Clear();
		}

		// Token: 0x04000E1A RID: 3610
		private const int MAX_CLAIMABLE_CELL_LOCATIONS = 2000;

		// Token: 0x04000E1B RID: 3611
		private Vector3Int threeDCursorLocation = new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue);

		// Token: 0x04000E1C RID: 3612
		[SerializeField]
		private Transform propLocationTemplate;

		// Token: 0x04000E1D RID: 3613
		[SerializeField]
		private SimpleObjectPool<Transform> propLocationPool;

		// Token: 0x04000E1E RID: 3614
		private Dictionary<SerializableGuid, List<PropLocationMarkRecord>> idToLocationMarkRecords = new Dictionary<SerializableGuid, List<PropLocationMarkRecord>>();

		// Token: 0x04000E1F RID: 3615
		private Dictionary<Vector3Int, HashSet<SerializableGuid>> cellLocationClaimers = new Dictionary<Vector3Int, HashSet<SerializableGuid>>();

		// Token: 0x04000E20 RID: 3616
		private Dictionary<Vector3Int, Transform> activePoolEntries = new Dictionary<Vector3Int, Transform>();
	}
}
