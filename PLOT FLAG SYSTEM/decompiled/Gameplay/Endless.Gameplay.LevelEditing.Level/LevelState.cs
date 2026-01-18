using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Endless.Gameplay.Serialization;
using Endless.Props.Assets;
using Endless.Props.Scripting;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Newtonsoft.Json;
using Runtime.Gameplay.LevelEditing;
using Unity.Profiling;
using UnityEngine;

namespace Endless.Gameplay.LevelEditing.Level;

[Serializable]
public class LevelState : LevelAsset
{
	public const string ASSET_TYPE = "level";

	public static readonly SemanticVersion INTERNAL_VERSION = new SemanticVersion(1, 0, 6);

	[JsonIgnore]
	private ProfilerMarker nonJsonCopyProfileMarker = new ProfilerMarker("Level State - NonJsonCopy");

	[JsonProperty]
	private List<PropEntry> propEntries = new List<PropEntry>();

	[JsonProperty]
	private List<TerrainEntry> terrainEntries = new List<TerrainEntry>();

	[JsonProperty]
	private List<WireBundle> wireBundles = new List<WireBundle>();

	[JsonProperty]
	private List<SerializableGuid> spawnPointIds = new List<SerializableGuid>();

	[JsonProperty]
	private List<SerializableGuid> selectedSpawnPointIds = new List<SerializableGuid>();

	[JsonProperty]
	public List<int> ScreenshotFileInstanceIds = new List<int>();

	[JsonProperty]
	private SerializableGuid defaultEnvironmentInstanceId = SerializableGuid.Empty;

	[JsonIgnore]
	private Dictionary<Vector3Int, TerrainEntry> terrainEntryLookup;

	[JsonIgnore]
	private Dictionary<SerializableGuid, PropEntry> propEntryLookup;

	[JsonIgnore]
	private WireBundle defaultWireBundle = new WireBundle();

	[JsonProperty("asset_need_update_parent_version")]
	public bool UpdateParentVersion => true;

	[JsonIgnore]
	private Dictionary<Vector3Int, TerrainEntry> TerrainEntryLookup
	{
		get
		{
			if (terrainEntryLookup == null)
			{
				terrainEntryLookup = new Dictionary<Vector3Int, TerrainEntry>();
				foreach (TerrainEntry terrainEntry in terrainEntries)
				{
					terrainEntryLookup.Add(terrainEntry.Position, terrainEntry);
				}
			}
			return terrainEntryLookup;
		}
	}

	[JsonIgnore]
	private Dictionary<SerializableGuid, PropEntry> PropEntryLookup
	{
		get
		{
			if (propEntryLookup == null)
			{
				propEntryLookup = new Dictionary<SerializableGuid, PropEntry>();
				foreach (PropEntry propEntry in propEntries)
				{
					propEntryLookup.Add(propEntry.InstanceId, propEntry);
				}
			}
			return propEntryLookup;
		}
	}

	[JsonIgnore]
	public IReadOnlyList<WireBundle> WireBundles => wireBundles;

	[JsonIgnore]
	public IReadOnlyList<PropEntry> PropEntries => propEntries;

	[JsonIgnore]
	public IReadOnlyList<TerrainEntry> TerrainEntries => terrainEntries;

	[JsonIgnore]
	public IReadOnlyList<SerializableGuid> SpawnPointIds => spawnPointIds;

	[JsonIgnore]
	public IReadOnlyList<SerializableGuid> SelectedSpawnPointIds => selectedSpawnPointIds;

	[JsonIgnore]
	public bool HasDefaultEnvironmentSet
	{
		get
		{
			bool flag = defaultEnvironmentInstanceId != SerializableGuid.Empty;
			if (flag)
			{
				flag = propEntries.FirstOrDefault((PropEntry p) => p.InstanceId == defaultEnvironmentInstanceId) != null;
			}
			return flag;
		}
	}

	public LevelState()
	{
		Name = "Level One";
		InternalVersion = INTERNAL_VERSION.ToString();
	}

	public bool IsDefaultEnvironment(SerializableGuid instanceId)
	{
		if (instanceId == SerializableGuid.Empty)
		{
			return false;
		}
		return defaultEnvironmentInstanceId == instanceId;
	}

	public void SetDefaultEnvironment(SerializableGuid instanceId)
	{
		defaultEnvironmentInstanceId = instanceId;
	}

	public void ClearDefaultEnvironment()
	{
		SetDefaultEnvironment(SerializableGuid.Empty);
	}

	public void AddTerrainCell(Cell cell)
	{
		if (cell is TerrainCell terrainCell && !TerrainEntryLookup.ContainsKey(cell.Coordinate))
		{
			TerrainEntry terrainEntry = new TerrainEntry
			{
				Position = terrainCell.Coordinate,
				TilesetId = terrainCell.TilesetIndex
			};
			TerrainEntryLookup.Add(cell.Coordinate, terrainEntry);
			terrainEntries.Add(terrainEntry);
		}
	}

	public void AddTerrainCell(Vector3Int positon, int tilesetIndex)
	{
		if (!TerrainEntryLookup.ContainsKey(positon))
		{
			TerrainEntry terrainEntry = new TerrainEntry
			{
				Position = positon,
				TilesetId = tilesetIndex
			};
			TerrainEntryLookup.Add(positon, terrainEntry);
			terrainEntries.Add(terrainEntry);
		}
	}

	public void RemoveTerrainCell(Cell cell)
	{
		if (cell is TerrainCell)
		{
			TerrainEntry item = terrainEntries.Find((TerrainEntry e) => e.Position == cell.Coordinate);
			terrainEntries.Remove(item);
			TerrainEntryLookup.Remove(cell.Coordinate);
		}
	}

	public void AddProp(GameObject prop, PropLibrary.RuntimePropInfo runtimePropInfo, SerializableGuid instanceId)
	{
		if (PropEntryLookup.ContainsKey(instanceId))
		{
			Debug.LogException(new Exception("Adding a duplicate prop (" + runtimePropInfo.PropData.Name + ") to LevelState!"), prop);
			return;
		}
		PropEntry propEntry = new PropEntry
		{
			Label = runtimePropInfo.PropData.Name,
			AssetId = runtimePropInfo.PropData.AssetID,
			InstanceId = instanceId,
			Position = prop.transform.position,
			Rotation = prop.transform.rotation
		};
		AddProp(propEntry, runtimePropInfo);
	}

	public void AddProp(PropEntry propEntry, PropLibrary.RuntimePropInfo runtimePropInfo)
	{
		bool isSpawnPoint = false;
		if (MonoBehaviourSingleton<StageManager>.Instance.BaseTypeList.TryGetDefinition(runtimePropInfo.PropData.BaseTypeId, out var componentDefinition) && componentDefinition.IsSpawnPoint)
		{
			isSpawnPoint = true;
		}
		AddProp(propEntry, isSpawnPoint, runtimePropInfo);
	}

	public void AddProp(PropEntry propEntry, bool isSpawnPoint, PropLibrary.RuntimePropInfo runtimePropInfo)
	{
		if (PropEntryLookup.ContainsKey(propEntry.InstanceId))
		{
			Debug.LogException(new Exception("Adding a duplicate prop (" + propEntry.Label + ") to LevelState!"));
			return;
		}
		if (isSpawnPoint)
		{
			spawnPointIds.Add(propEntry.InstanceId);
		}
		propEntries.Add(propEntry);
		PropEntryLookup.Add(propEntry.InstanceId, propEntry);
		if (runtimePropInfo == null)
		{
			return;
		}
		foreach (InspectorOrganizationData inspectorOrganizationDatum in runtimePropInfo.EndlessProp.ScriptComponent.Script.InspectorOrganizationData)
		{
			if (!string.IsNullOrWhiteSpace(inspectorOrganizationDatum.DefaultValueOverride))
			{
				string componentTypeName = ((inspectorOrganizationDatum.ComponentId == -1) ? string.Empty : EndlessTypeMapping.Instance.GetAssemblyQualifiedTypeName(inspectorOrganizationDatum.ComponentId));
				ApplyMemberChange(propEntry.InstanceId, new MemberChange(inspectorOrganizationDatum.MemberName, inspectorOrganizationDatum.DataType, inspectorOrganizationDatum.DefaultValueOverride), componentTypeName);
			}
		}
	}

	public void AddOrUpdatePropWithInstance(PropEntry updatedPropEntry)
	{
		if (PropEntryLookup.ContainsKey(updatedPropEntry.InstanceId))
		{
			PropEntryLookup[updatedPropEntry.InstanceId] = updatedPropEntry;
			for (int i = 0; i < propEntries.Count; i++)
			{
				if (!(propEntries[i].InstanceId != updatedPropEntry.InstanceId))
				{
					propEntries[i] = updatedPropEntry;
					break;
				}
			}
		}
		else
		{
			PropEntryLookup[updatedPropEntry.InstanceId] = updatedPropEntry;
			propEntries.Add(updatedPropEntry);
			Debug.LogError("I added a prop in an improper way");
		}
	}

	public void UpdatePropLabel(SerializableGuid instanceId, string newPropLabel)
	{
		PropEntryLookup[instanceId].Label = newPropLabel;
	}

	public void RemoveProp(SerializableGuid instanceId)
	{
		if (PropEntryLookup.TryGetValue(instanceId, out var value))
		{
			propEntries.Remove(value);
			PropEntryLookup.Remove(instanceId);
		}
		spawnPointIds.Remove(instanceId);
		selectedSpawnPointIds.Remove(instanceId);
	}

	public void AddWiring(SerializableGuid bundleId, SerializableGuid wireId, SerializableGuid emitterInstanceId, EndlessEventInfo emitterEndlessEventInfo, string emitterAssemblyQualifiedTypeName, SerializableGuid receiverInstanceId, EndlessEventInfo receiverEndlessEventInfo, string receiverAssemblyQualifiedTypeName, string[] storedParameterValues, IEnumerable<SerializableGuid> rerouteNodeIds, WireColor wireColor)
	{
		WireBundle wireBundle = null;
		for (int i = 0; i < wireBundles.Count; i++)
		{
			if (wireBundles[i].EmitterInstanceId == emitterInstanceId && wireBundles[i].ReceiverInstanceId == receiverInstanceId)
			{
				wireBundle = wireBundles[i];
				break;
			}
		}
		if (wireBundle == null)
		{
			wireBundle = new WireBundle
			{
				BundleId = bundleId,
				EmitterInstanceId = emitterInstanceId,
				ReceiverInstanceId = receiverInstanceId,
				WireColor = wireColor
			};
			wireBundles.Add(wireBundle);
		}
		WireEntry wireEntry = null;
		for (int j = 0; j < wireBundle.Wires.Count; j++)
		{
			if (wireBundle.Wires[j].WireId == wireId)
			{
				wireEntry = wireBundle.Wires[j];
			}
		}
		if (wireEntry == null)
		{
			wireEntry = new WireEntry();
			wireEntry.WireId = wireId;
			wireBundle.Wires.Add(wireEntry);
		}
		wireBundle.RerouteNodeIds = rerouteNodeIds.ToList();
		wireBundle.WireColor = wireColor;
		wireEntry.EmitterMemberName = emitterEndlessEventInfo.MemberName;
		wireEntry.ReceiverMemberName = receiverEndlessEventInfo.MemberName;
		wireEntry.EmitterComponentTypeId = (string.IsNullOrEmpty(emitterAssemblyQualifiedTypeName) ? (-1) : EndlessTypeMapping.Instance.GetTypeId(emitterAssemblyQualifiedTypeName));
		wireEntry.ReceiverComponentTypeId = (string.IsNullOrEmpty(receiverAssemblyQualifiedTypeName) ? (-1) : EndlessTypeMapping.Instance.GetTypeId(receiverAssemblyQualifiedTypeName));
		wireEntry.WireColor = (int)wireColor;
		StoredParameter[] array = new StoredParameter[storedParameterValues.Length];
		for (int k = 0; k < array.Length; k++)
		{
			array[k] = new StoredParameter
			{
				JsonData = storedParameterValues[k],
				DataType = receiverEndlessEventInfo.ParamList[k].DataType
			};
		}
		wireEntry.StaticParameters = array;
	}

	public SerializableGuid RemoveWiring(SerializableGuid wiringId)
	{
		for (int num = wireBundles.Count - 1; num >= 0; num--)
		{
			if (wireBundles[num].BundleId == wiringId)
			{
				SerializableGuid bundleId = wireBundles[num].BundleId;
				wireBundles.RemoveAt(num);
				return bundleId;
			}
			List<WireEntry> wires = wireBundles[num].Wires;
			for (int num2 = wires.Count - 1; num2 >= 0; num2--)
			{
				if (wires[num2].WireId == wiringId)
				{
					wireBundles[num].Wires.RemoveAt(num2);
					break;
				}
			}
			if (wireBundles[num].Wires.Count == 0)
			{
				SerializableGuid bundleId2 = wireBundles[num].BundleId;
				wireBundles.RemoveAt(num);
				return bundleId2;
			}
		}
		return SerializableGuid.Empty;
	}

	public void Clear()
	{
		terrainEntries.Clear();
		TerrainEntryLookup.Clear();
		propEntries.Clear();
	}

	public PropEntry GetPropEntry(SerializableGuid instanceId)
	{
		return PropEntryLookup[instanceId];
	}

	public bool TryGetPropEntry(SerializableGuid instanceId, out PropEntry propEntry)
	{
		return PropEntryLookup.TryGetValue(instanceId, out propEntry);
	}

	public void ApplyMemberChange(SerializableGuid instanceId, MemberChange newChange, string componentTypeName)
	{
		PropEntry propEntry = PropEntryLookup[instanceId];
		if (string.IsNullOrEmpty(componentTypeName))
		{
			MemberChange memberChange = propEntry.LuaMemberChanges.FirstOrDefault((MemberChange c) => c.MemberName == newChange.MemberName);
			if (memberChange == null)
			{
				memberChange = newChange.Copy();
				propEntry.LuaMemberChanges.Add(memberChange);
			}
			else
			{
				memberChange.DataType = newChange.DataType;
				memberChange.JsonData = newChange.JsonData;
			}
			return;
		}
		ComponentEntry componentEntry = propEntry.ComponentEntries.FirstOrDefault((ComponentEntry e) => e.AssemblyQualifiedName == componentTypeName);
		if (componentEntry == null)
		{
			componentEntry = new ComponentEntry
			{
				TypeId = EndlessTypeMapping.Instance.GetTypeId(componentTypeName),
				Changes = new List<MemberChange> { newChange.Copy() }
			};
			propEntry.ComponentEntries.Add(componentEntry);
			return;
		}
		MemberChange memberChange2 = componentEntry.Changes.FirstOrDefault((MemberChange c) => c.MemberName == newChange.MemberName);
		if (memberChange2 == null)
		{
			memberChange2 = newChange.Copy();
			componentEntry.Changes.Add(memberChange2);
		}
		else
		{
			memberChange2.JsonData = newChange.JsonData;
			memberChange2.DataType = newChange.DataType;
		}
	}

	public void ApplyLabelChange(SerializableGuid instanceId, string newLabel)
	{
		PropEntryLookup[instanceId].Label = newLabel;
	}

	public bool UpdateSpawnPointOrder(SerializableGuid[] newOrder)
	{
		if (newOrder.Length != spawnPointIds.Count)
		{
			return false;
		}
		if (spawnPointIds.All(((IEnumerable<SerializableGuid>)newOrder).Contains<SerializableGuid>))
		{
			spawnPointIds = newOrder.ToList();
			return true;
		}
		return false;
	}

	public bool UpdateSelectedSpawnPoints(SerializableGuid[] selectedIds)
	{
		if (selectedIds.All(spawnPointIds.Contains))
		{
			selectedSpawnPointIds = selectedIds.ToList();
			return true;
		}
		return false;
	}

	public void GetSpawnId(int userSlot, List<SerializableGuid> spawnPointList, out SerializableGuid spawnPointId, out int spawnPositionIndex)
	{
		List<SerializableGuid> list = null;
		if (spawnPointList != null)
		{
			list = spawnPointList.Where(IsSpawnPointValid).ToList();
		}
		if (list == null || list.Count == 0)
		{
			list = selectedSpawnPointIds.Where(IsSpawnPointValid).ToList();
		}
		if (list == null || list.Count == 0)
		{
			list = spawnPointIds.Where(IsSpawnPointValid).ToList();
		}
		if (list == null || list.Count == 0)
		{
			spawnPointId = SerializableGuid.Empty;
			spawnPositionIndex = 0;
		}
		else
		{
			userSlot = Mathf.Max(userSlot, 0);
			spawnPointId = list[userSlot % list.Count];
			spawnPositionIndex = userSlot / list.Count;
		}
	}

	public bool IsSpawnPointValid(SerializableGuid spawnPointId)
	{
		if (spawnPointIds.Contains(spawnPointId))
		{
			return GetPropEntry(spawnPointId).Position.y > MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.StageFallOffHeight;
		}
		return false;
	}

	public bool PropIsWired(SerializableGuid instanceId)
	{
		foreach (WireBundle wireBundle in wireBundles)
		{
			if (wireBundle.EmitterInstanceId == instanceId || wireBundle.ReceiverInstanceId == instanceId)
			{
				return true;
			}
		}
		return false;
	}

	public void ModifyWireReroutes(SerializableGuid bundleId, SerializableGuid instanceId, bool add = true)
	{
		for (int i = 0; i < wireBundles.Count; i++)
		{
			if (wireBundles[i].BundleId == bundleId)
			{
				if (add)
				{
					wireBundles[i].RerouteNodeIds.Add(instanceId);
				}
				else
				{
					wireBundles[i].RerouteNodeIds.Remove(instanceId);
				}
			}
		}
	}

	public void EraseReroute(SerializableGuid instanceId)
	{
		for (int i = 0; i < wireBundles.Count && !wireBundles[i].RerouteNodeIds.Remove(instanceId); i++)
		{
		}
	}

	public void UpdateWireColor(SerializableGuid wireId, WireColor newColor)
	{
		foreach (WireBundle wireBundle in wireBundles)
		{
			if (wireBundle.BundleId == wireId)
			{
				wireBundle.WireColor = newColor;
				break;
			}
		}
	}

	public List<SerializableGuid> GetRerouteNodesFromWire(SerializableGuid bundleId)
	{
		List<SerializableGuid> list = null;
		for (int i = 0; i < wireBundles.Count; i++)
		{
			if (wireBundles[i].BundleId == bundleId)
			{
				list = wireBundles[i].RerouteNodeIds;
				break;
			}
		}
		return list ?? new List<SerializableGuid>();
	}

	public WireBundle GetBundleUsingInstances(SerializableGuid emitterInstanceId, SerializableGuid receiverInstanceId)
	{
		for (int i = 0; i < wireBundles.Count; i++)
		{
			if (wireBundles[i].EmitterInstanceId == emitterInstanceId && wireBundles[i].ReceiverInstanceId == receiverInstanceId)
			{
				return wireBundles[i];
			}
		}
		return null;
	}

	public void CopyInstanceToInstance(SerializableGuid existingInstanceId, SerializableGuid newInstanceId, Vector3 position, Vector3 eulerAngles)
	{
		if (!PropEntryLookup.TryGetValue(existingInstanceId, out var _))
		{
			Debug.LogException(new Exception($"Attempting to copy an instance Id that didn't exist! Instance Id: {existingInstanceId}"));
		}
	}

	private void CopyWiresFromSourceToTarget(SerializableGuid existingInstanceId, SerializableGuid newInstanceId)
	{
		List<WireBundle> list = new List<WireBundle>();
		foreach (WireBundle wireBundle3 in wireBundles)
		{
			if (wireBundle3.EmitterInstanceId == existingInstanceId)
			{
				WireBundle wireBundle = wireBundle3.Copy(generateNewUniqueIds: true);
				wireBundle.EmitterInstanceId = newInstanceId;
				list.Add(wireBundle);
			}
			else if (wireBundle3.ReceiverInstanceId == existingInstanceId)
			{
				WireBundle wireBundle2 = wireBundle3.Copy(generateNewUniqueIds: true);
				wireBundle2.ReceiverInstanceId = newInstanceId;
				list.Add(wireBundle2);
			}
		}
		wireBundles.AddRange(list);
	}

	public void UpdatePropPositionAndRotation(SerializableGuid instanceId, Vector3 position, Quaternion rotation)
	{
		PropEntry propEntry = propEntries.FirstOrDefault((PropEntry entry) => entry.InstanceId == instanceId);
		if (propEntry != null)
		{
			propEntry.Position = position;
			propEntry.Rotation = rotation;
		}
	}

	public PropEntry CopyProp(SerializableGuid instanceId)
	{
		return propEntries.FirstOrDefault((PropEntry entry) => entry.InstanceId == instanceId)?.CopyWithNewInstanceId(SerializableGuid.Empty);
	}

	public (List<WireBundle>, List<WireBundle>) GetBundlesWiredToInstance(SerializableGuid instanceId)
	{
		List<WireBundle> list = new List<WireBundle>();
		List<WireBundle> list2 = new List<WireBundle>();
		foreach (WireBundle wireBundle3 in wireBundles)
		{
			if (wireBundle3.EmitterInstanceId == instanceId)
			{
				WireBundle wireBundle = wireBundle3.Copy(generateNewUniqueIds: true);
				wireBundle.EmitterInstanceId = SerializableGuid.Empty;
				list.Add(wireBundle);
			}
			else if (wireBundle3.ReceiverInstanceId == instanceId)
			{
				WireBundle wireBundle2 = wireBundle3.Copy(generateNewUniqueIds: true);
				wireBundle2.ReceiverInstanceId = SerializableGuid.Empty;
				list2.Add(wireBundle2);
			}
		}
		return (list, list2);
	}

	public void InsertWireBundles(IEnumerable<WireBundle> wireBundlesToInsert)
	{
		wireBundles.AddRange(wireBundlesToInsert);
	}

	[OnDeserialized]
	private void OnDeserialized(StreamingContext context)
	{
	}

	public static LevelState Upgrade(LevelState_0_0 oldLevelState)
	{
		List<PropEntry> list = new List<PropEntry>(oldLevelState.propEntries.Count);
		foreach (PropEntry propEntry in oldLevelState.propEntries)
		{
			propEntry.ComponentEntries = new List<ComponentEntry>();
			list.Add(propEntry);
		}
		return new LevelState
		{
			AssetID = oldLevelState.AssetID,
			AssetType = oldLevelState.AssetType,
			AssetVersion = oldLevelState.AssetVersion,
			Description = oldLevelState.Description,
			Name = oldLevelState.Name,
			RevisionMetaData = oldLevelState.RevisionMetaData,
			Screenshots = oldLevelState.Screenshots,
			terrainEntries = oldLevelState.terrainEntries,
			spawnPointIds = oldLevelState.spawnPointIds,
			selectedSpawnPointIds = oldLevelState.selectedSpawnPointIds,
			ScreenshotFileInstanceIds = oldLevelState.ScreenshotFileInstanceIds,
			defaultEnvironmentInstanceId = oldLevelState.defaultEnvironmentInstanceId,
			propEntries = list,
			wireBundles = new List<WireBundle>(),
			InternalVersion = new SemanticVersion(1, 0, 0).ToString()
		};
	}

	public void RemoveBundle(WireBundle wireBundle)
	{
		wireBundles.Remove(wireBundle);
	}

	public async Task<string> SerializeLevelAsync()
	{
		LevelState levelState = Copy();
		return await Task.Run(() => JsonConvert.SerializeObject(levelState));
	}

	public LevelState Copy()
	{
		LevelState levelState = new LevelState
		{
			Name = Name,
			AssetID = AssetID,
			AssetVersion = AssetVersion,
			AssetType = AssetType,
			Description = Description,
			InternalVersion = InternalVersion
		};
		levelState.RevisionMetaData = RevisionMetaData.Copy();
		for (int i = 0; i < base.Screenshots.Count; i++)
		{
			levelState.Screenshots.Add(base.Screenshots[i].Copy());
		}
		levelState.Archived = base.Archived;
		for (int j = 0; j < propEntries.Count; j++)
		{
			levelState.propEntries.Add(propEntries[j].Copy());
		}
		for (int k = 0; k < terrainEntries.Count; k++)
		{
			levelState.terrainEntries.Add(terrainEntries[k].Copy());
		}
		for (int l = 0; l < wireBundles.Count; l++)
		{
			levelState.wireBundles.Add(wireBundles[l].Copy(generateNewUniqueIds: false));
		}
		levelState.spawnPointIds = spawnPointIds.ToList();
		levelState.selectedSpawnPointIds = selectedSpawnPointIds.ToList();
		levelState.ScreenshotFileInstanceIds = ScreenshotFileInstanceIds.ToList();
		levelState.defaultEnvironmentInstanceId = defaultEnvironmentInstanceId;
		return levelState;
	}

	public HashSet<int> GetUsedTileSetIds(GameLibrary gameLibrary)
	{
		HashSet<int> hashSet = new HashSet<int>();
		foreach (TerrainEntry terrainEntry in TerrainEntries)
		{
			hashSet.Add(terrainEntry.TilesetId);
		}
		HashSet<int> hashSet2 = new HashSet<int>();
		for (int i = 0; i < hashSet.Count(); i++)
		{
			int num = hashSet.ElementAt(i);
			if (gameLibrary.TerrainEntries.Count <= num)
			{
				continue;
			}
			TerrainUsage terrainUsage = gameLibrary.TerrainEntries[num];
			if (terrainUsage.IsActive)
			{
				hashSet2.Add(hashSet.ElementAt(i));
				continue;
			}
			HashSet<int> hashSet3 = new HashSet<int>();
			int num2 = -1;
			do
			{
				num2 = terrainUsage.RedirectIndex;
				if (num2 > gameLibrary.TerrainEntries.Count)
				{
					num2 = 0;
					Debug.LogException(new Exception("Had improper index in terrain redirects"));
				}
				else if (hashSet3.Contains(num2))
				{
					Debug.LogException(new Exception($"Redirect Indexes looped. Can't validate terrain usage at index: {i}"));
					break;
				}
				hashSet3.Add(num2);
				terrainUsage = gameLibrary.TerrainEntries[num2];
			}
			while (!terrainUsage.IsActive);
			hashSet2.Add(num2);
		}
		return hashSet2;
	}

	public HashSet<SerializableGuid> GetUsedPropIds()
	{
		HashSet<SerializableGuid> hashSet = new HashSet<SerializableGuid>();
		foreach (PropEntry propEntry in PropEntries)
		{
			hashSet.Add(propEntry.AssetId);
		}
		return hashSet;
	}
}
