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

namespace Endless.Gameplay.LevelEditing.Level
{
	// Token: 0x02000547 RID: 1351
	[Serializable]
	public class LevelState : LevelAsset
	{
		// Token: 0x0600208E RID: 8334 RVA: 0x00092810 File Offset: 0x00090A10
		public LevelState()
		{
			this.Name = "Level One";
			this.InternalVersion = LevelState.INTERNAL_VERSION.ToString();
		}

		// Token: 0x1700063A RID: 1594
		// (get) Token: 0x0600208F RID: 8335 RVA: 0x00017586 File Offset: 0x00015786
		[JsonProperty("asset_need_update_parent_version")]
		public bool UpdateParentVersion
		{
			get
			{
				return true;
			}
		}

		// Token: 0x1700063B RID: 1595
		// (get) Token: 0x06002090 RID: 8336 RVA: 0x000928A8 File Offset: 0x00090AA8
		[JsonIgnore]
		private Dictionary<Vector3Int, TerrainEntry> TerrainEntryLookup
		{
			get
			{
				if (this.terrainEntryLookup == null)
				{
					this.terrainEntryLookup = new Dictionary<Vector3Int, TerrainEntry>();
					foreach (TerrainEntry terrainEntry in this.terrainEntries)
					{
						this.terrainEntryLookup.Add(terrainEntry.Position, terrainEntry);
					}
				}
				return this.terrainEntryLookup;
			}
		}

		// Token: 0x1700063C RID: 1596
		// (get) Token: 0x06002091 RID: 8337 RVA: 0x00092920 File Offset: 0x00090B20
		[JsonIgnore]
		private Dictionary<SerializableGuid, PropEntry> PropEntryLookup
		{
			get
			{
				if (this.propEntryLookup == null)
				{
					this.propEntryLookup = new Dictionary<SerializableGuid, PropEntry>();
					foreach (PropEntry propEntry in this.propEntries)
					{
						this.propEntryLookup.Add(propEntry.InstanceId, propEntry);
					}
				}
				return this.propEntryLookup;
			}
		}

		// Token: 0x1700063D RID: 1597
		// (get) Token: 0x06002092 RID: 8338 RVA: 0x00092998 File Offset: 0x00090B98
		[JsonIgnore]
		public IReadOnlyList<WireBundle> WireBundles
		{
			get
			{
				return this.wireBundles;
			}
		}

		// Token: 0x1700063E RID: 1598
		// (get) Token: 0x06002093 RID: 8339 RVA: 0x000929A0 File Offset: 0x00090BA0
		[JsonIgnore]
		public IReadOnlyList<PropEntry> PropEntries
		{
			get
			{
				return this.propEntries;
			}
		}

		// Token: 0x1700063F RID: 1599
		// (get) Token: 0x06002094 RID: 8340 RVA: 0x000929A8 File Offset: 0x00090BA8
		[JsonIgnore]
		public IReadOnlyList<TerrainEntry> TerrainEntries
		{
			get
			{
				return this.terrainEntries;
			}
		}

		// Token: 0x17000640 RID: 1600
		// (get) Token: 0x06002095 RID: 8341 RVA: 0x000929B0 File Offset: 0x00090BB0
		[JsonIgnore]
		public IReadOnlyList<SerializableGuid> SpawnPointIds
		{
			get
			{
				return this.spawnPointIds;
			}
		}

		// Token: 0x17000641 RID: 1601
		// (get) Token: 0x06002096 RID: 8342 RVA: 0x000929B8 File Offset: 0x00090BB8
		[JsonIgnore]
		public IReadOnlyList<SerializableGuid> SelectedSpawnPointIds
		{
			get
			{
				return this.selectedSpawnPointIds;
			}
		}

		// Token: 0x17000642 RID: 1602
		// (get) Token: 0x06002097 RID: 8343 RVA: 0x000929C0 File Offset: 0x00090BC0
		[JsonIgnore]
		public bool HasDefaultEnvironmentSet
		{
			get
			{
				bool flag = this.defaultEnvironmentInstanceId != SerializableGuid.Empty;
				if (flag)
				{
					flag = this.propEntries.FirstOrDefault((PropEntry p) => p.InstanceId == this.defaultEnvironmentInstanceId) != null;
				}
				return flag;
			}
		}

		// Token: 0x06002098 RID: 8344 RVA: 0x000929FD File Offset: 0x00090BFD
		public bool IsDefaultEnvironment(SerializableGuid instanceId)
		{
			return !(instanceId == SerializableGuid.Empty) && this.defaultEnvironmentInstanceId == instanceId;
		}

		// Token: 0x06002099 RID: 8345 RVA: 0x00092A1A File Offset: 0x00090C1A
		public void SetDefaultEnvironment(SerializableGuid instanceId)
		{
			this.defaultEnvironmentInstanceId = instanceId;
		}

		// Token: 0x0600209A RID: 8346 RVA: 0x00092A23 File Offset: 0x00090C23
		public void ClearDefaultEnvironment()
		{
			this.SetDefaultEnvironment(SerializableGuid.Empty);
		}

		// Token: 0x0600209B RID: 8347 RVA: 0x00092A30 File Offset: 0x00090C30
		public void AddTerrainCell(Cell cell)
		{
			TerrainCell terrainCell = cell as TerrainCell;
			if (terrainCell != null && !this.TerrainEntryLookup.ContainsKey(cell.Coordinate))
			{
				TerrainEntry terrainEntry = new TerrainEntry
				{
					Position = terrainCell.Coordinate,
					TilesetId = terrainCell.TilesetIndex
				};
				this.TerrainEntryLookup.Add(cell.Coordinate, terrainEntry);
				this.terrainEntries.Add(terrainEntry);
			}
		}

		// Token: 0x0600209C RID: 8348 RVA: 0x00092A98 File Offset: 0x00090C98
		public void AddTerrainCell(Vector3Int positon, int tilesetIndex)
		{
			if (!this.TerrainEntryLookup.ContainsKey(positon))
			{
				TerrainEntry terrainEntry = new TerrainEntry
				{
					Position = positon,
					TilesetId = tilesetIndex
				};
				this.TerrainEntryLookup.Add(positon, terrainEntry);
				this.terrainEntries.Add(terrainEntry);
			}
		}

		// Token: 0x0600209D RID: 8349 RVA: 0x00092AE0 File Offset: 0x00090CE0
		public void RemoveTerrainCell(Cell cell)
		{
			if (cell is TerrainCell)
			{
				TerrainEntry terrainEntry = this.terrainEntries.Find((TerrainEntry e) => e.Position == cell.Coordinate);
				this.terrainEntries.Remove(terrainEntry);
				this.TerrainEntryLookup.Remove(cell.Coordinate);
			}
		}

		// Token: 0x0600209E RID: 8350 RVA: 0x00092B44 File Offset: 0x00090D44
		public void AddProp(GameObject prop, PropLibrary.RuntimePropInfo runtimePropInfo, SerializableGuid instanceId)
		{
			if (this.PropEntryLookup.ContainsKey(instanceId))
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
			this.AddProp(propEntry, runtimePropInfo);
		}

		// Token: 0x0600209F RID: 8351 RVA: 0x00092BE4 File Offset: 0x00090DE4
		public void AddProp(PropEntry propEntry, PropLibrary.RuntimePropInfo runtimePropInfo)
		{
			bool flag = false;
			BaseTypeDefinition baseTypeDefinition;
			if (MonoBehaviourSingleton<StageManager>.Instance.BaseTypeList.TryGetDefinition(runtimePropInfo.PropData.BaseTypeId, out baseTypeDefinition) && baseTypeDefinition.IsSpawnPoint)
			{
				flag = true;
			}
			this.AddProp(propEntry, flag, runtimePropInfo);
		}

		// Token: 0x060020A0 RID: 8352 RVA: 0x00092C2C File Offset: 0x00090E2C
		public void AddProp(PropEntry propEntry, bool isSpawnPoint, PropLibrary.RuntimePropInfo runtimePropInfo)
		{
			if (this.PropEntryLookup.ContainsKey(propEntry.InstanceId))
			{
				Debug.LogException(new Exception("Adding a duplicate prop (" + propEntry.Label + ") to LevelState!"));
				return;
			}
			if (isSpawnPoint)
			{
				this.spawnPointIds.Add(propEntry.InstanceId);
			}
			this.propEntries.Add(propEntry);
			this.PropEntryLookup.Add(propEntry.InstanceId, propEntry);
			if (runtimePropInfo != null)
			{
				foreach (InspectorOrganizationData inspectorOrganizationData in runtimePropInfo.EndlessProp.ScriptComponent.Script.InspectorOrganizationData)
				{
					if (!string.IsNullOrWhiteSpace(inspectorOrganizationData.DefaultValueOverride))
					{
						string text = ((inspectorOrganizationData.ComponentId == -1) ? string.Empty : EndlessTypeMapping.Instance.GetAssemblyQualifiedTypeName(inspectorOrganizationData.ComponentId));
						this.ApplyMemberChange(propEntry.InstanceId, new MemberChange(inspectorOrganizationData.MemberName, inspectorOrganizationData.DataType, inspectorOrganizationData.DefaultValueOverride), text);
					}
				}
			}
		}

		// Token: 0x060020A1 RID: 8353 RVA: 0x00092D44 File Offset: 0x00090F44
		public void AddOrUpdatePropWithInstance(PropEntry updatedPropEntry)
		{
			if (this.PropEntryLookup.ContainsKey(updatedPropEntry.InstanceId))
			{
				this.PropEntryLookup[updatedPropEntry.InstanceId] = updatedPropEntry;
				for (int i = 0; i < this.propEntries.Count; i++)
				{
					if (!(this.propEntries[i].InstanceId != updatedPropEntry.InstanceId))
					{
						this.propEntries[i] = updatedPropEntry;
						return;
					}
				}
				return;
			}
			this.PropEntryLookup[updatedPropEntry.InstanceId] = updatedPropEntry;
			this.propEntries.Add(updatedPropEntry);
			Debug.LogError("I added a prop in an improper way");
		}

		// Token: 0x060020A2 RID: 8354 RVA: 0x00092DE1 File Offset: 0x00090FE1
		public void UpdatePropLabel(SerializableGuid instanceId, string newPropLabel)
		{
			this.PropEntryLookup[instanceId].Label = newPropLabel;
		}

		// Token: 0x060020A3 RID: 8355 RVA: 0x00092DF8 File Offset: 0x00090FF8
		public void RemoveProp(SerializableGuid instanceId)
		{
			PropEntry propEntry;
			if (this.PropEntryLookup.TryGetValue(instanceId, out propEntry))
			{
				this.propEntries.Remove(propEntry);
				this.PropEntryLookup.Remove(instanceId);
			}
			this.spawnPointIds.Remove(instanceId);
			this.selectedSpawnPointIds.Remove(instanceId);
		}

		// Token: 0x060020A4 RID: 8356 RVA: 0x00092E4C File Offset: 0x0009104C
		public void AddWiring(SerializableGuid bundleId, SerializableGuid wireId, SerializableGuid emitterInstanceId, EndlessEventInfo emitterEndlessEventInfo, string emitterAssemblyQualifiedTypeName, SerializableGuid receiverInstanceId, EndlessEventInfo receiverEndlessEventInfo, string receiverAssemblyQualifiedTypeName, string[] storedParameterValues, IEnumerable<SerializableGuid> rerouteNodeIds, WireColor wireColor)
		{
			WireBundle wireBundle = null;
			for (int i = 0; i < this.wireBundles.Count; i++)
			{
				if (this.wireBundles[i].EmitterInstanceId == emitterInstanceId && this.wireBundles[i].ReceiverInstanceId == receiverInstanceId)
				{
					wireBundle = this.wireBundles[i];
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
				this.wireBundles.Add(wireBundle);
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
			wireBundle.RerouteNodeIds = rerouteNodeIds.ToList<SerializableGuid>();
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

		// Token: 0x060020A5 RID: 8357 RVA: 0x0009300C File Offset: 0x0009120C
		public SerializableGuid RemoveWiring(SerializableGuid wiringId)
		{
			for (int i = this.wireBundles.Count - 1; i >= 0; i--)
			{
				if (this.wireBundles[i].BundleId == wiringId)
				{
					SerializableGuid bundleId = this.wireBundles[i].BundleId;
					this.wireBundles.RemoveAt(i);
					return bundleId;
				}
				List<WireEntry> wires = this.wireBundles[i].Wires;
				for (int j = wires.Count - 1; j >= 0; j--)
				{
					if (wires[j].WireId == wiringId)
					{
						this.wireBundles[i].Wires.RemoveAt(j);
						break;
					}
				}
				if (this.wireBundles[i].Wires.Count == 0)
				{
					SerializableGuid bundleId2 = this.wireBundles[i].BundleId;
					this.wireBundles.RemoveAt(i);
					return bundleId2;
				}
			}
			return SerializableGuid.Empty;
		}

		// Token: 0x060020A6 RID: 8358 RVA: 0x000930FB File Offset: 0x000912FB
		public void Clear()
		{
			this.terrainEntries.Clear();
			this.TerrainEntryLookup.Clear();
			this.propEntries.Clear();
		}

		// Token: 0x060020A7 RID: 8359 RVA: 0x0009311E File Offset: 0x0009131E
		public PropEntry GetPropEntry(SerializableGuid instanceId)
		{
			return this.PropEntryLookup[instanceId];
		}

		// Token: 0x060020A8 RID: 8360 RVA: 0x0009312C File Offset: 0x0009132C
		public bool TryGetPropEntry(SerializableGuid instanceId, out PropEntry propEntry)
		{
			return this.PropEntryLookup.TryGetValue(instanceId, out propEntry);
		}

		// Token: 0x060020A9 RID: 8361 RVA: 0x0009313C File Offset: 0x0009133C
		public void ApplyMemberChange(SerializableGuid instanceId, MemberChange newChange, string componentTypeName)
		{
			PropEntry propEntry = this.PropEntryLookup[instanceId];
			if (string.IsNullOrEmpty(componentTypeName))
			{
				MemberChange memberChange = propEntry.LuaMemberChanges.FirstOrDefault((MemberChange c) => c.MemberName == newChange.MemberName);
				if (memberChange == null)
				{
					memberChange = newChange.Copy();
					propEntry.LuaMemberChanges.Add(memberChange);
					return;
				}
				memberChange.DataType = newChange.DataType;
				memberChange.JsonData = newChange.JsonData;
				return;
			}
			else
			{
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
					return;
				}
				memberChange2.JsonData = newChange.JsonData;
				memberChange2.DataType = newChange.DataType;
				return;
			}
		}

		// Token: 0x060020AA RID: 8362 RVA: 0x00092DE1 File Offset: 0x00090FE1
		public void ApplyLabelChange(SerializableGuid instanceId, string newLabel)
		{
			this.PropEntryLookup[instanceId].Label = newLabel;
		}

		// Token: 0x060020AB RID: 8363 RVA: 0x0009328A File Offset: 0x0009148A
		public bool UpdateSpawnPointOrder(SerializableGuid[] newOrder)
		{
			if (newOrder.Length != this.spawnPointIds.Count)
			{
				return false;
			}
			if (this.spawnPointIds.All(new Func<SerializableGuid, bool>(newOrder.Contains<SerializableGuid>)))
			{
				this.spawnPointIds = newOrder.ToList<SerializableGuid>();
				return true;
			}
			return false;
		}

		// Token: 0x060020AC RID: 8364 RVA: 0x000932C6 File Offset: 0x000914C6
		public bool UpdateSelectedSpawnPoints(SerializableGuid[] selectedIds)
		{
			if (selectedIds.All(new Func<SerializableGuid, bool>(this.spawnPointIds.Contains)))
			{
				this.selectedSpawnPointIds = selectedIds.ToList<SerializableGuid>();
				return true;
			}
			return false;
		}

		// Token: 0x060020AD RID: 8365 RVA: 0x000932F4 File Offset: 0x000914F4
		public void GetSpawnId(int userSlot, List<SerializableGuid> spawnPointList, out SerializableGuid spawnPointId, out int spawnPositionIndex)
		{
			List<SerializableGuid> list = null;
			if (spawnPointList != null)
			{
				list = spawnPointList.Where(new Func<SerializableGuid, bool>(this.IsSpawnPointValid)).ToList<SerializableGuid>();
			}
			if (list == null || list.Count == 0)
			{
				list = this.selectedSpawnPointIds.Where(new Func<SerializableGuid, bool>(this.IsSpawnPointValid)).ToList<SerializableGuid>();
			}
			if (list == null || list.Count == 0)
			{
				list = this.spawnPointIds.Where(new Func<SerializableGuid, bool>(this.IsSpawnPointValid)).ToList<SerializableGuid>();
			}
			if (list == null || list.Count == 0)
			{
				spawnPointId = SerializableGuid.Empty;
				spawnPositionIndex = 0;
				return;
			}
			userSlot = Mathf.Max(userSlot, 0);
			spawnPointId = list[userSlot % list.Count];
			spawnPositionIndex = userSlot / list.Count;
		}

		// Token: 0x060020AE RID: 8366 RVA: 0x000933B1 File Offset: 0x000915B1
		public bool IsSpawnPointValid(SerializableGuid spawnPointId)
		{
			return this.spawnPointIds.Contains(spawnPointId) && this.GetPropEntry(spawnPointId).Position.y > MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.StageFallOffHeight;
		}

		// Token: 0x060020AF RID: 8367 RVA: 0x000933E8 File Offset: 0x000915E8
		public bool PropIsWired(SerializableGuid instanceId)
		{
			foreach (WireBundle wireBundle in this.wireBundles)
			{
				if (wireBundle.EmitterInstanceId == instanceId || wireBundle.ReceiverInstanceId == instanceId)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060020B0 RID: 8368 RVA: 0x00093458 File Offset: 0x00091658
		public void ModifyWireReroutes(SerializableGuid bundleId, SerializableGuid instanceId, bool add = true)
		{
			for (int i = 0; i < this.wireBundles.Count; i++)
			{
				if (this.wireBundles[i].BundleId == bundleId)
				{
					if (add)
					{
						this.wireBundles[i].RerouteNodeIds.Add(instanceId);
					}
					else
					{
						this.wireBundles[i].RerouteNodeIds.Remove(instanceId);
					}
				}
			}
		}

		// Token: 0x060020B1 RID: 8369 RVA: 0x000934C8 File Offset: 0x000916C8
		public void EraseReroute(SerializableGuid instanceId)
		{
			int num = 0;
			while (num < this.wireBundles.Count && !this.wireBundles[num].RerouteNodeIds.Remove(instanceId))
			{
				num++;
			}
		}

		// Token: 0x060020B2 RID: 8370 RVA: 0x00093504 File Offset: 0x00091704
		public void UpdateWireColor(SerializableGuid wireId, WireColor newColor)
		{
			foreach (WireBundle wireBundle in this.wireBundles)
			{
				if (wireBundle.BundleId == wireId)
				{
					wireBundle.WireColor = newColor;
					break;
				}
			}
		}

		// Token: 0x060020B3 RID: 8371 RVA: 0x00093568 File Offset: 0x00091768
		public List<SerializableGuid> GetRerouteNodesFromWire(SerializableGuid bundleId)
		{
			List<SerializableGuid> list = null;
			for (int i = 0; i < this.wireBundles.Count; i++)
			{
				if (this.wireBundles[i].BundleId == bundleId)
				{
					list = this.wireBundles[i].RerouteNodeIds;
					break;
				}
			}
			return list ?? new List<SerializableGuid>();
		}

		// Token: 0x060020B4 RID: 8372 RVA: 0x000935C4 File Offset: 0x000917C4
		public WireBundle GetBundleUsingInstances(SerializableGuid emitterInstanceId, SerializableGuid receiverInstanceId)
		{
			for (int i = 0; i < this.wireBundles.Count; i++)
			{
				if (this.wireBundles[i].EmitterInstanceId == emitterInstanceId && this.wireBundles[i].ReceiverInstanceId == receiverInstanceId)
				{
					return this.wireBundles[i];
				}
			}
			return null;
		}

		// Token: 0x060020B5 RID: 8373 RVA: 0x00093628 File Offset: 0x00091828
		public void CopyInstanceToInstance(SerializableGuid existingInstanceId, SerializableGuid newInstanceId, Vector3 position, Vector3 eulerAngles)
		{
			PropEntry propEntry;
			if (!this.PropEntryLookup.TryGetValue(existingInstanceId, out propEntry))
			{
				Debug.LogException(new Exception(string.Format("Attempting to copy an instance Id that didn't exist! Instance Id: {0}", existingInstanceId)));
				return;
			}
		}

		// Token: 0x060020B6 RID: 8374 RVA: 0x00093660 File Offset: 0x00091860
		private void CopyWiresFromSourceToTarget(SerializableGuid existingInstanceId, SerializableGuid newInstanceId)
		{
			List<WireBundle> list = new List<WireBundle>();
			foreach (WireBundle wireBundle in this.wireBundles)
			{
				if (wireBundle.EmitterInstanceId == existingInstanceId)
				{
					WireBundle wireBundle2 = wireBundle.Copy(true);
					wireBundle2.EmitterInstanceId = newInstanceId;
					list.Add(wireBundle2);
				}
				else if (wireBundle.ReceiverInstanceId == existingInstanceId)
				{
					WireBundle wireBundle3 = wireBundle.Copy(true);
					wireBundle3.ReceiverInstanceId = newInstanceId;
					list.Add(wireBundle3);
				}
			}
			this.wireBundles.AddRange(list);
		}

		// Token: 0x060020B7 RID: 8375 RVA: 0x0009370C File Offset: 0x0009190C
		public void UpdatePropPositionAndRotation(SerializableGuid instanceId, Vector3 position, Quaternion rotation)
		{
			PropEntry propEntry = this.propEntries.FirstOrDefault((PropEntry entry) => entry.InstanceId == instanceId);
			if (propEntry == null)
			{
				return;
			}
			propEntry.Position = position;
			propEntry.Rotation = rotation;
		}

		// Token: 0x060020B8 RID: 8376 RVA: 0x00093750 File Offset: 0x00091950
		public PropEntry CopyProp(SerializableGuid instanceId)
		{
			PropEntry propEntry = this.propEntries.FirstOrDefault((PropEntry entry) => entry.InstanceId == instanceId);
			if (propEntry == null)
			{
				return null;
			}
			return propEntry.CopyWithNewInstanceId(SerializableGuid.Empty);
		}

		// Token: 0x060020B9 RID: 8377 RVA: 0x00093794 File Offset: 0x00091994
		public ValueTuple<List<WireBundle>, List<WireBundle>> GetBundlesWiredToInstance(SerializableGuid instanceId)
		{
			List<WireBundle> list = new List<WireBundle>();
			List<WireBundle> list2 = new List<WireBundle>();
			foreach (WireBundle wireBundle in this.wireBundles)
			{
				if (wireBundle.EmitterInstanceId == instanceId)
				{
					WireBundle wireBundle2 = wireBundle.Copy(true);
					wireBundle2.EmitterInstanceId = SerializableGuid.Empty;
					list.Add(wireBundle2);
				}
				else if (wireBundle.ReceiverInstanceId == instanceId)
				{
					WireBundle wireBundle3 = wireBundle.Copy(true);
					wireBundle3.ReceiverInstanceId = SerializableGuid.Empty;
					list2.Add(wireBundle3);
				}
			}
			return new ValueTuple<List<WireBundle>, List<WireBundle>>(list, list2);
		}

		// Token: 0x060020BA RID: 8378 RVA: 0x0009384C File Offset: 0x00091A4C
		public void InsertWireBundles(IEnumerable<WireBundle> wireBundlesToInsert)
		{
			this.wireBundles.AddRange(wireBundlesToInsert);
		}

		// Token: 0x060020BB RID: 8379 RVA: 0x00002DB0 File Offset: 0x00000FB0
		[OnDeserialized]
		private void OnDeserialized(StreamingContext context)
		{
		}

		// Token: 0x060020BC RID: 8380 RVA: 0x0009385C File Offset: 0x00091A5C
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

		// Token: 0x060020BD RID: 8381 RVA: 0x00093988 File Offset: 0x00091B88
		public void RemoveBundle(WireBundle wireBundle)
		{
			this.wireBundles.Remove(wireBundle);
		}

		// Token: 0x060020BE RID: 8382 RVA: 0x00093998 File Offset: 0x00091B98
		public async Task<string> SerializeLevelAsync()
		{
			LevelState levelState = this.Copy();
			return await Task.Run<string>(() => JsonConvert.SerializeObject(levelState));
		}

		// Token: 0x060020BF RID: 8383 RVA: 0x000939DC File Offset: 0x00091BDC
		public LevelState Copy()
		{
			LevelState levelState = new LevelState
			{
				Name = this.Name,
				AssetID = this.AssetID,
				AssetVersion = this.AssetVersion,
				AssetType = this.AssetType,
				Description = this.Description,
				InternalVersion = this.InternalVersion
			};
			levelState.RevisionMetaData = this.RevisionMetaData.Copy();
			for (int i = 0; i < base.Screenshots.Count; i++)
			{
				levelState.Screenshots.Add(base.Screenshots[i].Copy());
			}
			levelState.Archived = base.Archived;
			for (int j = 0; j < this.propEntries.Count; j++)
			{
				levelState.propEntries.Add(this.propEntries[j].Copy());
			}
			for (int k = 0; k < this.terrainEntries.Count; k++)
			{
				levelState.terrainEntries.Add(this.terrainEntries[k].Copy());
			}
			for (int l = 0; l < this.wireBundles.Count; l++)
			{
				levelState.wireBundles.Add(this.wireBundles[l].Copy(false));
			}
			levelState.spawnPointIds = this.spawnPointIds.ToList<SerializableGuid>();
			levelState.selectedSpawnPointIds = this.selectedSpawnPointIds.ToList<SerializableGuid>();
			levelState.ScreenshotFileInstanceIds = this.ScreenshotFileInstanceIds.ToList<int>();
			levelState.defaultEnvironmentInstanceId = this.defaultEnvironmentInstanceId;
			return levelState;
		}

		// Token: 0x060020C0 RID: 8384 RVA: 0x00093B64 File Offset: 0x00091D64
		public HashSet<int> GetUsedTileSetIds(GameLibrary gameLibrary)
		{
			HashSet<int> hashSet = new HashSet<int>();
			foreach (TerrainEntry terrainEntry in this.TerrainEntries)
			{
				hashSet.Add(terrainEntry.TilesetId);
			}
			HashSet<int> hashSet2 = new HashSet<int>();
			for (int i = 0; i < hashSet.Count<int>(); i++)
			{
				int num = hashSet.ElementAt(i);
				if (gameLibrary.TerrainEntries.Count > num)
				{
					TerrainUsage terrainUsage = gameLibrary.TerrainEntries[num];
					if (!terrainUsage.IsActive)
					{
						HashSet<int> hashSet3 = new HashSet<int>();
						int num2;
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
								goto Block_5;
							}
							hashSet3.Add(num2);
							terrainUsage = gameLibrary.TerrainEntries[num2];
						}
						while (!terrainUsage.IsActive);
						IL_0112:
						hashSet2.Add(num2);
						goto IL_011B;
						Block_5:
						Debug.LogException(new Exception(string.Format("Redirect Indexes looped. Can't validate terrain usage at index: {0}", i)));
						goto IL_0112;
					}
					hashSet2.Add(hashSet.ElementAt(i));
				}
				IL_011B:;
			}
			return hashSet2;
		}

		// Token: 0x060020C1 RID: 8385 RVA: 0x00093CB0 File Offset: 0x00091EB0
		public HashSet<SerializableGuid> GetUsedPropIds()
		{
			HashSet<SerializableGuid> hashSet = new HashSet<SerializableGuid>();
			foreach (PropEntry propEntry in this.PropEntries)
			{
				hashSet.Add(propEntry.AssetId);
			}
			return hashSet;
		}

		// Token: 0x040019FF RID: 6655
		public const string ASSET_TYPE = "level";

		// Token: 0x04001A00 RID: 6656
		public static readonly SemanticVersion INTERNAL_VERSION = new SemanticVersion(1, 0, 6);

		// Token: 0x04001A01 RID: 6657
		[JsonIgnore]
		private ProfilerMarker nonJsonCopyProfileMarker = new ProfilerMarker("Level State - NonJsonCopy");

		// Token: 0x04001A02 RID: 6658
		[JsonProperty]
		private List<PropEntry> propEntries = new List<PropEntry>();

		// Token: 0x04001A03 RID: 6659
		[JsonProperty]
		private List<TerrainEntry> terrainEntries = new List<TerrainEntry>();

		// Token: 0x04001A04 RID: 6660
		[JsonProperty]
		private List<WireBundle> wireBundles = new List<WireBundle>();

		// Token: 0x04001A05 RID: 6661
		[JsonProperty]
		private List<SerializableGuid> spawnPointIds = new List<SerializableGuid>();

		// Token: 0x04001A06 RID: 6662
		[JsonProperty]
		private List<SerializableGuid> selectedSpawnPointIds = new List<SerializableGuid>();

		// Token: 0x04001A07 RID: 6663
		[JsonProperty]
		public List<int> ScreenshotFileInstanceIds = new List<int>();

		// Token: 0x04001A08 RID: 6664
		[JsonProperty]
		private SerializableGuid defaultEnvironmentInstanceId = SerializableGuid.Empty;

		// Token: 0x04001A09 RID: 6665
		[JsonIgnore]
		private Dictionary<Vector3Int, TerrainEntry> terrainEntryLookup;

		// Token: 0x04001A0A RID: 6666
		[JsonIgnore]
		private Dictionary<SerializableGuid, PropEntry> propEntryLookup;

		// Token: 0x04001A0B RID: 6667
		[JsonIgnore]
		private WireBundle defaultWireBundle = new WireBundle();
	}
}
