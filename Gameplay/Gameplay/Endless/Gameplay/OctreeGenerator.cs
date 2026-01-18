using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.LevelEditing.Tilesets;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.AI;

namespace Endless.Gameplay
{
	// Token: 0x020001E2 RID: 482
	public static class OctreeGenerator
	{
		// Token: 0x06000A0C RID: 2572 RVA: 0x000345E1 File Offset: 0x000327E1
		public static IEnumerator GenerateOctree(NavMeshQuery query, Action<OctreeGenerator.Results> getResults)
		{
			Octant octant = OctreeGenerator.BuildRootOctant();
			IReadOnlyList<Cell> cells = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.Cells;
			NativeReference<Octant> rootOctantReference = new NativeReference<Octant>(octant, AllocatorManager.Persistent);
			NativeList<NativeCellData> fullCells = new NativeList<NativeCellData>(cells.Count, AllocatorManager.Persistent);
			NativeList<NativeCellData> splitCells = new NativeList<NativeCellData>(cells.Count, AllocatorManager.Persistent);
			Dictionary<SerializableGuid, HashSet<int3>> dynamicCellMap = new Dictionary<SerializableGuid, HashSet<int3>>();
			yield return OctreeGenerator.ExtractCellDataForOctutree(cells, splitCells, fullCells, dynamicCellMap);
			NativeReference<int> numOctantsReference = new NativeReference<int>(0, AllocatorManager.Persistent);
			NativeReference<int> numWalkableOctantsReference = new NativeReference<int>(0, AllocatorManager.Persistent);
			NativeArray<Octant> rawOctants = new NativeArray<Octant>(fullCells.Length + splitCells.Length * 73 + 65536, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			NativeParallelHashMap<int, SerializableGuid> associatedPropMap = new NativeParallelHashMap<int, SerializableGuid>(1024, AllocatorManager.Persistent);
			NativeParallelHashMap<int, SlopeNeighbors> slopeMap = new NativeParallelHashMap<int, SlopeNeighbors>(splitCells.Length, AllocatorManager.Persistent);
			NativeQueue<OverlapBoxCommand> blockingOverlapCommands = new NativeQueue<OverlapBoxCommand>(AllocatorManager.Persistent);
			NativeQueue<OverlapBoxCommand> walkableOverlapCommands = new NativeQueue<OverlapBoxCommand>(AllocatorManager.Persistent);
			NativeQueue<OverlapBoxCommand> dynamicBlockingOctants = new NativeQueue<OverlapBoxCommand>(AllocatorManager.Persistent);
			NativeQueue<OctreeGenerator.PotentialOctantData> potentialBlockingOctants = new NativeQueue<OctreeGenerator.PotentialOctantData>(AllocatorManager.Persistent);
			NativeQueue<OctreeGenerator.PotentialOctantData> potentialWalkableOctants = new NativeQueue<OctreeGenerator.PotentialOctantData>(AllocatorManager.Persistent);
			NativeQueue<OctreeGenerator.PotentialOctantData> dynamicOctants = new NativeQueue<OctreeGenerator.PotentialOctantData>(AllocatorManager.Persistent);
			JobHandle jobHandle = new OctreeGenerator.OctreeGenerationJob
			{
				FullCellData = fullCells,
				SplitCellData = splitCells,
				Query = query,
				RootOctant = rootOctantReference,
				Octants = rawOctants,
				AssociatedPropMap = associatedPropMap,
				SlopeMap = slopeMap,
				NumOctants = numOctantsReference,
				NumWalkableOctants = numWalkableOctantsReference,
				octantDivisions = NavGraph.SplitCellDivisions,
				MinSplitCellAreaPercentage = NavGraph.MinSplitCellAreaPercentage,
				DynamicOctantBoxcastCommands = dynamicBlockingOctants,
				DynamicOctants = dynamicOctants,
				BlockingOctantBoxcastCommands = blockingOverlapCommands,
				PotentialBlockingOctants = potentialBlockingOctants,
				PotentialWalkableOctants = potentialWalkableOctants,
				WalkableOctantBoxcastCommands = walkableOverlapCommands,
				QueryParameters = new QueryParameters(NpcMovementValues.JumpSweepMask, false, QueryTriggerInteraction.UseGlobal, false)
			}.Schedule(default(JobHandle));
			fullCells.Dispose(jobHandle);
			splitCells.Dispose(jobHandle);
			rootOctantReference.Dispose(jobHandle);
			yield return JobUtilities.WaitForJobToComplete(jobHandle, false);
			NativeArray<OverlapBoxCommand> staticOverlapBoxArray = blockingOverlapCommands.ToArray(AllocatorManager.Persistent);
			blockingOverlapCommands.Dispose(default(JobHandle));
			NativeArray<OverlapBoxCommand> walkableOverlapArray = walkableOverlapCommands.ToArray(AllocatorManager.Persistent);
			NativeArray<ColliderHit> walkableResults = new NativeArray<ColliderHit>(walkableOverlapCommands.Count, Allocator.Persistent, NativeArrayOptions.ClearMemory);
			walkableOverlapCommands.Dispose(default(JobHandle));
			NativeArray<OctreeGenerator.PotentialOctantData> blockingOctantData = potentialBlockingOctants.ToArray(AllocatorManager.Persistent);
			potentialBlockingOctants.Dispose(default(JobHandle));
			NativeArray<OctreeGenerator.PotentialOctantData> nativeArray = potentialWalkableOctants.ToArray(AllocatorManager.Persistent);
			potentialWalkableOctants.Dispose(default(JobHandle));
			NativeArray<ColliderHit> blockingResults = new NativeArray<ColliderHit>(staticOverlapBoxArray.Length, Allocator.Persistent, NativeArrayOptions.ClearMemory);
			NativeArray<OverlapBoxCommand> dynamicBoxCommands = dynamicBlockingOctants.ToArray(AllocatorManager.Persistent);
			dynamicBlockingOctants.Dispose(default(JobHandle));
			NativeArray<OctreeGenerator.PotentialOctantData> dynamicOctantData = dynamicOctants.ToArray(AllocatorManager.Persistent);
			dynamicOctants.Dispose(default(JobHandle));
			NativeArray<ColliderHit> dynamicResults = new NativeArray<ColliderHit>(dynamicBoxCommands.Length, Allocator.Persistent, NativeArrayOptions.ClearMemory);
			OctreeGenerator.AddSplitBlockingOctants addSplitBlockingOctants = new OctreeGenerator.AddSplitBlockingOctants
			{
				PotentialBlockingOctants = blockingOctantData,
				PotentialWalkableOctants = nativeArray,
				DynamicOctants = dynamicOctantData,
				BlockingOverlapBoxResults = blockingResults,
				WalkableOverlapBoxResults = walkableResults,
				DynamicOverlapBoxResults = dynamicResults,
				SlopeMap = slopeMap,
				NumOctants = numOctantsReference,
				Octants = rawOctants,
				NumWalkableOctantsReference = numWalkableOctantsReference
			};
			JobHandle jobHandle2 = OverlapBoxCommand.ScheduleBatch(staticOverlapBoxArray, blockingResults, 30, 1, default(JobHandle));
			jobHandle2 = OverlapBoxCommand.ScheduleBatch(walkableOverlapArray, walkableResults, 30, 1, jobHandle2);
			jobHandle2 = OverlapBoxCommand.ScheduleBatch(dynamicBoxCommands, dynamicResults, 30, 1, jobHandle2);
			jobHandle2 = addSplitBlockingOctants.Schedule(jobHandle2);
			nativeArray.Dispose(jobHandle2);
			yield return JobUtilities.WaitForJobToComplete(jobHandle2, false);
			dynamicBoxCommands.Dispose(default(JobHandle));
			dynamicOctantData.Dispose(default(JobHandle));
			staticOverlapBoxArray.Dispose(default(JobHandle));
			blockingResults.Dispose(default(JobHandle));
			walkableResults.Dispose(default(JobHandle));
			walkableOverlapArray.Dispose(default(JobHandle));
			dynamicResults.Dispose(default(JobHandle));
			blockingOctantData.Dispose(default(JobHandle));
			int value = numOctantsReference.Value;
			numOctantsReference.Dispose(default(JobHandle));
			NativeArray<Octant> octants = new NativeArray<Octant>(value, Allocator.Persistent, NativeArrayOptions.ClearMemory);
			JobHandle jobHandle3 = new OctreeGenerator.PruneOctantsJob
			{
				RawOctants = rawOctants,
				Octants = octants,
				NumOctants = value
			}.Schedule(default(JobHandle));
			rawOctants.Dispose(jobHandle3);
			yield return JobUtilities.WaitForJobToComplete(jobHandle3, false);
			getResults(new OctreeGenerator.Results
			{
				Octants = octants,
				NumWalkableOctants = numWalkableOctantsReference.Value,
				DynamicCellMap = dynamicCellMap,
				AssociatedPropMap = associatedPropMap,
				SlopeMap = slopeMap
			});
			numWalkableOctantsReference.Dispose(default(JobHandle));
			yield break;
		}

		// Token: 0x06000A0D RID: 2573 RVA: 0x000345F8 File Offset: 0x000327F8
		private static Octant BuildRootOctant()
		{
			Vector3Int vector3Int = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.MinimumExtents - Vector3Int.one * 10;
			Vector3Int vector3Int2 = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.MaximumExtents + Vector3Int.one * 10;
			int num = vector3Int2.x - vector3Int.x;
			int num2 = vector3Int2.y - vector3Int.y;
			int num3 = math.max(vector3Int2.z - vector3Int.z, math.max(num, num2));
			num3 = math.ceilpow2(num3);
			float3 @float = new float3((float)num3, (float)num3, (float)num3);
			return Octant.Factory.BuildContainingOctant(vector3Int.ToInt3() + new float3(-0.5f) + @float / 2f, @float);
		}

		// Token: 0x06000A0E RID: 2574 RVA: 0x000346D0 File Offset: 0x000328D0
		private static HashSet<Door> GetNpcOpenableDoorProps(List<PropCell> propCells)
		{
			HashSet<Door> hashSet = new HashSet<Door>();
			foreach (PropCell propCell in propCells)
			{
				object obj;
				if (MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetGameObjectFromInstanceId(propCell.InstanceId).GetComponentInChildren<WorldObject>().TryGetUserComponent(typeof(Door), out obj))
				{
					Door door = (Door)obj;
					hashSet.Add(door);
				}
			}
			return hashSet;
		}

		// Token: 0x06000A0F RID: 2575 RVA: 0x0003475C File Offset: 0x0003295C
		private static IEnumerator ExtractCellDataForOctutree(IReadOnlyList<Cell> cells, NativeList<NativeCellData> splitCells, NativeList<NativeCellData> fullCells, Dictionary<SerializableGuid, HashSet<int3>> dynamicCellMap)
		{
			float num = Time.realtimeSinceStartup;
			Dictionary<int3, NativeCellData> cellMap = new Dictionary<int3, NativeCellData>(cells.Count);
			List<PropCell> propCells = new List<PropCell>(cells.Count);
			List<TerrainCell> terrainCells = new List<TerrainCell>(cells.Count);
			Dictionary<SerializableGuid, WorldObject> propWorldObjects = new Dictionary<SerializableGuid, WorldObject>(cells.Count);
			foreach (Cell cell in cells)
			{
				if (Time.realtimeSinceStartup - num > 0.05f)
				{
					yield return null;
					num = Time.realtimeSinceStartup;
				}
				PropCell propCell2 = cell as PropCell;
				if (propCell2 == null)
				{
					TerrainCell terrainCell2 = cell as TerrainCell;
					if (terrainCell2 != null)
					{
						terrainCells.Add(terrainCell2);
					}
				}
				else
				{
					propCells.Add(propCell2);
					GameObject gameObjectFromInstanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetGameObjectFromInstanceId(propCell2.InstanceId);
					WorldObject worldObject = (gameObjectFromInstanceId ? gameObjectFromInstanceId.GetComponentInChildren<WorldObject>() : null);
					propWorldObjects[propCell2.InstanceId] = worldObject;
				}
				cell = null;
			}
			IEnumerator<Cell> enumerator = null;
			foreach (TerrainCell terrainCell in terrainCells)
			{
				if (Time.realtimeSinceStartup - num > 0.05f)
				{
					yield return null;
					num = Time.realtimeSinceStartup;
				}
				Vector3 vector = terrainCell.Coordinate;
				bool flag;
				if (terrainCell.IsSlope())
				{
					SlopeNeighbors slopeNeighbors = terrainCell.GetSlope();
					flag = slopeNeighbors != SlopeNeighbors.TopNeighbor && slopeNeighbors != SlopeNeighbors.AllNeighbors;
				}
				else
				{
					flag = false;
				}
				bool flag2 = flag;
				NativeCellData nativeCellData = new NativeCellData
				{
					Position = vector,
					AssociatedProp = SerializableGuid.Empty,
					IsSplitCell = flag2,
					SlopeNeighbors = terrainCell.GetSlope(),
					IsTerrain = true
				};
				cellMap.Add(terrainCell.Coordinate.ToInt3(), nativeCellData);
				terrainCell = null;
			}
			List<TerrainCell>.Enumerator enumerator2 = default(List<TerrainCell>.Enumerator);
			HashSet<Door> doors = OctreeGenerator.GetNpcOpenableDoorProps(propCells);
			Dictionary<int3, SerializableGuid> capturedCells = new Dictionary<int3, SerializableGuid>();
			foreach (Door door in doors)
			{
				dynamicCellMap.Add(door.WorldObject.InstanceId, new HashSet<int3>());
				foreach (int3 cell2 in door.CaptureClosedCells())
				{
					if (Time.realtimeSinceStartup - num > 0.05f)
					{
						yield return null;
						num = Time.realtimeSinceStartup;
					}
					capturedCells.Add(cell2, door.WorldObject.InstanceId);
					dynamicCellMap[door.WorldObject.InstanceId].Add(cell2);
					cell2 = default(int3);
				}
				List<int3>.Enumerator enumerator4 = default(List<int3>.Enumerator);
				door = null;
			}
			HashSet<Door>.Enumerator enumerator3 = default(HashSet<Door>.Enumerator);
			foreach (Door door in doors)
			{
				foreach (int3 cell2 in door.OpenForwardAndReturnCells())
				{
					if (Time.realtimeSinceStartup - num > 0.05f)
					{
						yield return null;
						num = Time.realtimeSinceStartup;
					}
					if (capturedCells.TryAdd(cell2, door.WorldObject.InstanceId))
					{
						dynamicCellMap[door.WorldObject.InstanceId].Add(cell2);
					}
					cell2 = default(int3);
				}
				List<int3>.Enumerator enumerator4 = default(List<int3>.Enumerator);
				foreach (int3 cell2 in door.OpenBackwardAndReturnCells())
				{
					if (Time.realtimeSinceStartup - num > 0.05f)
					{
						yield return null;
						num = Time.realtimeSinceStartup;
					}
					if (capturedCells.TryAdd(cell2, door.WorldObject.InstanceId))
					{
						dynamicCellMap[door.WorldObject.InstanceId].Add(cell2);
					}
					cell2 = default(int3);
				}
				enumerator4 = default(List<int3>.Enumerator);
				door.Close();
				door = null;
			}
			enumerator3 = default(HashSet<Door>.Enumerator);
			foreach (KeyValuePair<int3, SerializableGuid> capturedCell in capturedCells)
			{
				if (Time.realtimeSinceStartup - num > 0.05f)
				{
					yield return null;
					num = Time.realtimeSinceStartup;
				}
				NativeCellData nativeCellData2 = new NativeCellData
				{
					Position = capturedCell.Key,
					IsSplitCell = true,
					IsConditionallyNavigable = true,
					AssociatedProp = capturedCell.Value
				};
				if (!cellMap.TryAdd(capturedCell.Key, nativeCellData2))
				{
					NativeCellData nativeCellData3 = cellMap[capturedCell.Key];
					if (nativeCellData3.IsSplitCell)
					{
						nativeCellData3.IsConditionallyNavigable = true;
						cellMap[capturedCell.Key] = nativeCellData3;
						capturedCell = default(KeyValuePair<int3, SerializableGuid>);
					}
				}
			}
			Dictionary<int3, SerializableGuid>.Enumerator enumerator5 = default(Dictionary<int3, SerializableGuid>.Enumerator);
			foreach (PropCell propCell in propCells)
			{
				if (Time.realtimeSinceStartup - num > 0.05f)
				{
					yield return null;
					num = Time.realtimeSinceStartup;
				}
				Vector3 vector2 = propCell.Coordinate;
				int3 @int = propCell.Coordinate.ToInt3();
				WorldObject worldObject2 = propWorldObjects[propCell.InstanceId];
				if (worldObject2.EndlessProp.NavValue != NavType.Intangible)
				{
					NativeCellData nativeCellData4 = new NativeCellData
					{
						Position = vector2,
						IsSplitCell = true,
						AssociatedProp = propCell.InstanceId,
						IsConditionallyNavigable = (worldObject2.EndlessProp.NavValue == NavType.Dynamic)
					};
					object obj;
					if (worldObject2.TryGetUserComponent(typeof(DynamicNavigationComponent), out obj))
					{
						dynamicCellMap.TryAdd(worldObject2.InstanceId, new HashSet<int3>());
						dynamicCellMap[worldObject2.InstanceId].Add(@int);
					}
					if (!cellMap.TryAdd(@int, nativeCellData4))
					{
						NativeCellData nativeCellData5 = cellMap[@int];
						nativeCellData5.AssociatedProp = propCell.InstanceId;
						cellMap[@int] = nativeCellData5;
						propCell = null;
					}
				}
			}
			List<PropCell>.Enumerator enumerator6 = default(List<PropCell>.Enumerator);
			foreach (PropCell propCell in propCells)
			{
				if (Time.realtimeSinceStartup - num > 0.05f)
				{
					yield return null;
					num = Time.realtimeSinceStartup;
				}
				Vector3 vector3 = propCell.Coordinate;
				WorldObject worldObject3 = propWorldObjects[propCell.InstanceId];
				if (worldObject3.EndlessProp.NavValue != NavType.Intangible)
				{
					NativeCellData nativeCellData6 = new NativeCellData
					{
						Position = vector3 + Vector3.down,
						IsSplitCell = true,
						IsConditionallyNavigable = (worldObject3.EndlessProp.NavValue == NavType.Dynamic)
					};
					int3 int2 = propCell.Coordinate.ToInt3();
					int3 int3 = int2 + new int3(0, -1, 0);
					if (cellMap.TryAdd(int3, nativeCellData6) && worldObject3.EndlessProp.NavValue == NavType.Dynamic)
					{
						dynamicCellMap.TryAdd(worldObject3.InstanceId, new HashSet<int3>());
						dynamicCellMap[worldObject3.InstanceId].Add(int3);
					}
					NativeCellData nativeCellData7 = new NativeCellData
					{
						Position = vector3 + Vector3.up,
						IsSplitCell = true,
						IsConditionallyNavigable = (worldObject3.EndlessProp.NavValue == NavType.Dynamic)
					};
					int3 int4 = int2 + new int3(0, 1, 0);
					if (cellMap.TryAdd(int4, nativeCellData7) && worldObject3.EndlessProp.NavValue == NavType.Dynamic)
					{
						dynamicCellMap.TryAdd(worldObject3.InstanceId, new HashSet<int3>());
						dynamicCellMap[worldObject3.InstanceId].Add(int4);
					}
					propCell = null;
				}
			}
			enumerator6 = default(List<PropCell>.Enumerator);
			foreach (TerrainCell terrainCell3 in terrainCells)
			{
				if (terrainCell3.IsSlope())
				{
					SlopeNeighbors slopeNeighbors = terrainCell3.GetSlope();
					if (slopeNeighbors != SlopeNeighbors.AllNeighbors && slopeNeighbors != SlopeNeighbors.TopNeighbor)
					{
						continue;
					}
				}
				int3 int5 = terrainCell3.Coordinate.ToInt3() + new int3(0, 1, 0);
				if (!cellMap.ContainsKey(int5))
				{
					Vector3 vector4 = terrainCell3.Coordinate;
					NativeCellData nativeCellData8 = new NativeCellData
					{
						Position = vector4 + Vector3.up,
						IsSplitCell = false,
						AssociatedProp = SerializableGuid.Empty
					};
					cellMap.Add(int5, nativeCellData8);
				}
			}
			using (Dictionary<int3, NativeCellData>.ValueCollection.Enumerator enumerator8 = cellMap.Values.GetEnumerator())
			{
				while (enumerator8.MoveNext())
				{
					NativeCellData nativeCellData9 = enumerator8.Current;
					if (nativeCellData9.IsSplitCell)
					{
						splitCells.Add(in nativeCellData9);
					}
					else
					{
						fullCells.Add(in nativeCellData9);
					}
				}
				yield break;
			}
			yield break;
			yield break;
		}

		// Token: 0x020001E3 RID: 483
		[BurstCompile(FloatPrecision = FloatPrecision.Low, FloatMode = FloatMode.Fast)]
		private struct OctreeGenerationJob : IJob
		{
			// Token: 0x06000A10 RID: 2576 RVA: 0x00034780 File Offset: 0x00032980
			public void Execute()
			{
				this.octantHead = 1;
				this.numWalkableOctants = 0;
				Octant value = this.RootOctant.Value;
				this.Octants[0] = value;
				float3 @float = new float3(0.125f, 0.125f, 0.125f);
				Vector3 vector = new Vector3(0.1f, 0.1f, 0.1f);
				Vector3 vector2 = new Vector3(0.001f, 0.001f, 0.001f);
				Vector3 vector3 = new Vector3(0.001f, 0.125f, 0.001f);
				Vector3 vector4 = new Vector3(0.5f, 0.5f, 0.5f);
				Vector3 vector5 = new Vector3(0f, -0.001f, 0f);
				int num = this.octantDivisions * this.octantDivisions;
				NativeList<Vector3> nativeList = new NativeList<Vector3>(this.octantDivisions * this.octantDivisions, AllocatorManager.Temp);
				foreach (NativeCellData nativeCellData in this.FullCellData)
				{
					float3 position = nativeCellData.Position;
					if (nativeCellData.IsTerrain)
					{
						Octant octant = Octant.Factory.BuildTerrainCellOctant(position);
						int num2;
						this.TryInsertCellOctantIntoOctree(ref octant, out num2);
					}
					else
					{
						NavMeshLocation navMeshLocation = this.Query.MapLocation(position, vector4, 0, -1);
						if (this.Query.IsValid(navMeshLocation))
						{
							Octant octant2 = Octant.Factory.BuildWalkableCellOctant(position);
							int num2;
							if (this.TryInsertCellOctantIntoOctree(ref octant2, out num2))
							{
								this.numWalkableOctants++;
							}
						}
					}
				}
				foreach (NativeCellData nativeCellData2 in this.SplitCellData)
				{
					if (nativeCellData2.IsConditionallyNavigable)
					{
						float3 position2 = nativeCellData2.Position;
						Octant octant3 = Octant.Factory.BuildSplitOctant(position2, true, nativeCellData2.IsTerrain);
						int num3;
						this.TryInsertCellOctantIntoOctree(ref octant3, out num3);
						this.AssociatedPropMap.Add(num3, nativeCellData2.AssociatedProp);
						if (nativeCellData2.IsTerrain)
						{
							this.SlopeMap.Add(num3, nativeCellData2.SlopeNeighbors);
						}
						ref Octant @ref = ref this.Octants.GetRef(num3);
						@ref.HasChildren = true;
						for (int i = 0; i < 8; i++)
						{
							float3 float2 = position2 + OctreeHelperMethods.GetOffsetPosition(i, 0.25f);
							this.Octants[this.octantHead] = Octant.Factory.BuildSplitOctantChild(float2, true, nativeCellData2.IsTerrain);
							@ref.SetChildIndex(i, this.octantHead);
							this.AssociatedPropMap.Add(this.octantHead, nativeCellData2.AssociatedProp);
							if (nativeCellData2.IsTerrain)
							{
								this.SlopeMap.Add(this.octantHead, nativeCellData2.SlopeNeighbors);
							}
							ref Octant ref2 = ref this.Octants.GetRef(this.octantHead);
							ref2.HasChildren = true;
							this.octantHead++;
							for (int j = 0; j < 8; j++)
							{
								float3 float3 = float2 + OctreeHelperMethods.GetOffsetPosition(j, 0.125f);
								this.Octants[this.octantHead] = Octant.Factory.BuildSplitOctantGrandchild(float3, false, false, true, nativeCellData2.IsTerrain);
								this.AssociatedPropMap.Add(this.octantHead, nativeCellData2.AssociatedProp);
								if (nativeCellData2.IsTerrain)
								{
									this.SlopeMap.Add(this.octantHead, nativeCellData2.SlopeNeighbors);
								}
								this.DynamicOctants.Enqueue(new OctreeGenerator.PotentialOctantData
								{
									center = float3,
									IsSlope = nativeCellData2.IsTerrain,
									Slope = nativeCellData2.SlopeNeighbors
								});
								this.DynamicOctantBoxcastCommands.Enqueue(new OverlapBoxCommand(float3, vector, Quaternion.identity, this.QueryParameters));
								ref2.SetChildIndex(j, this.octantHead);
								this.octantHead++;
							}
						}
					}
					else
					{
						float3 position3 = nativeCellData2.Position;
						for (int k = 0; k < 8; k++)
						{
							float3 float4 = position3 + OctreeHelperMethods.GetOffsetPosition(k, 0.25f);
							for (int l = 0; l < 8; l++)
							{
								float3 float5 = float4 + OctreeHelperMethods.GetOffsetPosition(l, 0.125f);
								int num4 = 0;
								Vector3 vector6 = Vector3.zero;
								OctreeHelperMethods.GetSamplePositions(nativeList, float5 - @float, float5 + @float, this.octantDivisions);
								foreach (Vector3 vector7 in nativeList)
								{
									NavMeshLocation navMeshLocation2 = this.Query.MapLocation(vector7 + vector5, vector3, 0, -1);
									if (this.Query.IsValid(navMeshLocation2))
									{
										num4++;
										vector6 = navMeshLocation2.position;
									}
								}
								if ((float)num4 / (float)num > this.MinSplitCellAreaPercentage)
								{
									this.PotentialWalkableOctants.Enqueue(new OctreeGenerator.PotentialOctantData
									{
										center = float5,
										IsSlope = nativeCellData2.IsTerrain,
										Slope = nativeCellData2.SlopeNeighbors
									});
									this.WalkableOctantBoxcastCommands.Enqueue(new OverlapBoxCommand(vector6 + Vector3.up, vector2, Quaternion.identity, this.QueryParameters));
								}
								else
								{
									this.PotentialBlockingOctants.Enqueue(new OctreeGenerator.PotentialOctantData
									{
										center = float5,
										IsSlope = nativeCellData2.IsTerrain,
										Slope = nativeCellData2.SlopeNeighbors
									});
									this.BlockingOctantBoxcastCommands.Enqueue(new OverlapBoxCommand(float5, vector, Quaternion.identity, this.QueryParameters));
								}
							}
						}
					}
				}
				this.NumWalkableOctants.Value = this.numWalkableOctants;
				this.NumOctants.Value = this.octantHead;
				this.RootOctant.Value = this.Octants[0];
			}

			// Token: 0x06000A11 RID: 2577 RVA: 0x00034DB0 File Offset: 0x00032FB0
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private bool TryInsertCellOctantIntoOctree(ref Octant octantToInsert, out int insertedOctantIndex)
			{
				ref Octant ptr = ref this.Octants.GetRef(0);
				int num;
				while (ptr.Size.x > 1f && ptr.TryGetChildOctantIndexFromPoint(octantToInsert.Center, out num))
				{
					ptr = this.Octants.GetRef(num);
				}
				if (ptr.Size.x < 2f)
				{
					insertedOctantIndex = -1;
					return false;
				}
				int num2;
				while (ptr.Size.x > 2f)
				{
					num2 = Octant.GetOctantRelativeOctantIndex(ptr.Center, octantToInsert.Center);
					float3 @float = ptr.Size / 2f;
					float num3 = @float.x / 2f;
					float3 float2 = ptr.Center + OctreeHelperMethods.GetOffsetPosition(num2, num3);
					ptr.HasChildren = true;
					ptr.SetChildIndex(num2, this.octantHead);
					this.Octants[this.octantHead] = Octant.Factory.BuildContainingOctant(float2, @float);
					ptr = this.Octants.GetRef(this.octantHead);
					this.octantHead++;
				}
				num2 = Octant.GetOctantRelativeOctantIndex(ptr.Center, octantToInsert.Center);
				ptr.SetChildIndex(num2, this.octantHead);
				ptr.HasChildren = true;
				this.Octants[this.octantHead] = octantToInsert;
				insertedOctantIndex = this.octantHead;
				this.octantHead++;
				return true;
			}

			// Token: 0x04000903 RID: 2307
			[ReadOnly]
			public NativeList<NativeCellData> FullCellData;

			// Token: 0x04000904 RID: 2308
			[ReadOnly]
			public NativeList<NativeCellData> SplitCellData;

			// Token: 0x04000905 RID: 2309
			public NavMeshQuery Query;

			// Token: 0x04000906 RID: 2310
			public NativeReference<Octant> RootOctant;

			// Token: 0x04000907 RID: 2311
			public NativeArray<Octant> Octants;

			// Token: 0x04000908 RID: 2312
			public int octantDivisions;

			// Token: 0x04000909 RID: 2313
			public float MinSplitCellAreaPercentage;

			// Token: 0x0400090A RID: 2314
			public QueryParameters QueryParameters;

			// Token: 0x0400090B RID: 2315
			[WriteOnly]
			public NativeParallelHashMap<int, SerializableGuid> AssociatedPropMap;

			// Token: 0x0400090C RID: 2316
			[WriteOnly]
			public NativeParallelHashMap<int, SlopeNeighbors> SlopeMap;

			// Token: 0x0400090D RID: 2317
			[WriteOnly]
			public NativeQueue<OverlapBoxCommand> DynamicOctantBoxcastCommands;

			// Token: 0x0400090E RID: 2318
			[WriteOnly]
			public NativeQueue<OctreeGenerator.PotentialOctantData> DynamicOctants;

			// Token: 0x0400090F RID: 2319
			[WriteOnly]
			public NativeQueue<OverlapBoxCommand> BlockingOctantBoxcastCommands;

			// Token: 0x04000910 RID: 2320
			[WriteOnly]
			public NativeQueue<OctreeGenerator.PotentialOctantData> PotentialBlockingOctants;

			// Token: 0x04000911 RID: 2321
			[WriteOnly]
			public NativeQueue<OverlapBoxCommand> WalkableOctantBoxcastCommands;

			// Token: 0x04000912 RID: 2322
			[WriteOnly]
			public NativeQueue<OctreeGenerator.PotentialOctantData> PotentialWalkableOctants;

			// Token: 0x04000913 RID: 2323
			public NativeReference<int> NumOctants;

			// Token: 0x04000914 RID: 2324
			public NativeReference<int> NumWalkableOctants;

			// Token: 0x04000915 RID: 2325
			private int octantHead;

			// Token: 0x04000916 RID: 2326
			private int numWalkableOctants;
		}

		// Token: 0x020001E4 RID: 484
		[BurstCompile]
		private struct AddSplitBlockingOctants : IJob
		{
			// Token: 0x06000A12 RID: 2578 RVA: 0x00034F14 File Offset: 0x00033114
			public void Execute()
			{
				this.octantHead = this.NumOctants.Value;
				for (int i = 0; i < this.DynamicOctants.Length; i++)
				{
					if (this.DynamicOverlapBoxResults[i].instanceID != 0)
					{
						OctreeHelperMethods.GetSmallestOctantForPoint(this.DynamicOctants[i].center, this.Octants, 0.25f).IsBlocking = true;
					}
				}
				for (int j = 0; j < this.PotentialBlockingOctants.Length; j++)
				{
					if (this.BlockingOverlapBoxResults[j].instanceID != 0)
					{
						OctreeGenerator.PotentialOctantData potentialOctantData = this.PotentialBlockingOctants[j];
						Octant octant = Octant.Factory.BuildSplitOctantGrandchild(potentialOctantData.center, false, true, false, potentialOctantData.IsSlope);
						if (this.TryInsertCellOctantIntoOctree(ref octant, potentialOctantData.Slope) && potentialOctantData.IsSlope)
						{
							this.SlopeMap.Add(this.octantHead - 1, potentialOctantData.Slope);
						}
						int num;
						OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(potentialOctantData.center, this.Octants, 1f, out num);
						this.Octants.GetRef(num).HasChildren = true;
					}
				}
				for (int k = 0; k < this.PotentialWalkableOctants.Length; k++)
				{
					if (this.WalkableOverlapBoxResults[k].instanceID == 0)
					{
						OctreeGenerator.PotentialOctantData potentialOctantData2 = this.PotentialWalkableOctants[k];
						Octant octant2 = Octant.Factory.BuildSplitOctantGrandchild(potentialOctantData2.center, true, false, false, potentialOctantData2.IsSlope);
						if (this.TryInsertCellOctantIntoOctree(ref octant2, potentialOctantData2.Slope) && potentialOctantData2.IsSlope)
						{
							this.SlopeMap.Add(this.octantHead - 1, potentialOctantData2.Slope);
						}
						int num2;
						OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(potentialOctantData2.center, this.Octants, 1f, out num2);
						ref Octant @ref = ref this.Octants.GetRef(num2);
						@ref.HasChildren = true;
						@ref.HasWalkableChildren = true;
						int num3;
						OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(potentialOctantData2.center, this.Octants, 1f, out num3);
						ref Octant ref2 = ref this.Octants.GetRef(num3);
						ref2.HasChildren = true;
						ref2.HasWalkableChildren = true;
						this.NumWalkableOctantsReference.Value = this.NumWalkableOctantsReference.Value + 1;
					}
				}
				this.NumOctants.Value = this.octantHead;
			}

			// Token: 0x06000A13 RID: 2579 RVA: 0x0003515C File Offset: 0x0003335C
			private bool TryInsertCellOctantIntoOctree(ref Octant octantToInsert, SlopeNeighbors slope)
			{
				ref Octant ptr = ref this.Octants.GetRef(0);
				while (ptr.Size.x > 0.5f)
				{
					int num;
					if (!ptr.TryGetChildOctantIndexFromPoint(octantToInsert.Center, out num))
					{
						IL_00D9:
						int num2;
						while (ptr.Size.x > 2f)
						{
							num2 = Octant.GetOctantRelativeOctantIndex(ptr.Center, octantToInsert.Center);
							float3 @float = ptr.Size / 2f;
							float num3 = @float.x / 2f;
							float3 float2 = ptr.Center + OctreeHelperMethods.GetOffsetPosition(num2, num3);
							ptr.HasChildren = true;
							ptr.SetChildIndex(num2, this.octantHead);
							this.Octants[this.octantHead] = Octant.Factory.BuildContainingOctant(float2, @float);
							ptr = this.Octants.GetRef(this.octantHead);
							this.octantHead++;
						}
						float num4 = ptr.Size.x;
						if (num4.Equals(2f))
						{
							num2 = Octant.GetOctantRelativeOctantIndex(ptr.Center, octantToInsert.Center);
							float num5 = ptr.Size.x / 4f;
							float3 float3 = ptr.Center + OctreeHelperMethods.GetOffsetPosition(num2, num5);
							this.Octants[this.octantHead] = Octant.Factory.BuildSplitOctant(float3, octantToInsert.IsConditionallyNavigable, octantToInsert.IsSlope);
							if (octantToInsert.IsSlope)
							{
								this.SlopeMap.Add(this.octantHead, slope);
							}
							ptr.HasChildren = true;
							ptr.SetChildIndex(num2, this.octantHead);
							ptr = this.Octants.GetRef(this.octantHead);
							this.octantHead++;
						}
						num4 = ptr.Size.x;
						if (num4.Equals(1f))
						{
							num2 = Octant.GetOctantRelativeOctantIndex(ptr.Center, octantToInsert.Center);
							float num6 = ptr.Size.x / 4f;
							float3 float4 = ptr.Center + OctreeHelperMethods.GetOffsetPosition(num2, num6);
							this.Octants[this.octantHead] = Octant.Factory.BuildSplitOctantChild(float4, octantToInsert.IsConditionallyNavigable, octantToInsert.IsSlope);
							if (octantToInsert.IsSlope)
							{
								this.SlopeMap.Add(this.octantHead, slope);
							}
							ptr.HasChildren = true;
							ptr.SetChildIndex(num2, this.octantHead);
							ptr = this.Octants.GetRef(this.octantHead);
							this.octantHead++;
						}
						num2 = Octant.GetOctantRelativeOctantIndex(ptr.Center, octantToInsert.Center);
						ptr.SetChildIndex(num2, this.octantHead);
						ptr.HasChildren = true;
						this.Octants[this.octantHead] = octantToInsert;
						this.octantHead++;
						return true;
					}
					ptr = this.Octants.GetRef(num);
				}
				goto IL_00D9;
			}

			// Token: 0x04000917 RID: 2327
			[ReadOnly]
			public NativeArray<OctreeGenerator.PotentialOctantData> PotentialBlockingOctants;

			// Token: 0x04000918 RID: 2328
			[ReadOnly]
			public NativeArray<OctreeGenerator.PotentialOctantData> PotentialWalkableOctants;

			// Token: 0x04000919 RID: 2329
			[ReadOnly]
			public NativeArray<OctreeGenerator.PotentialOctantData> DynamicOctants;

			// Token: 0x0400091A RID: 2330
			[ReadOnly]
			public NativeArray<ColliderHit> BlockingOverlapBoxResults;

			// Token: 0x0400091B RID: 2331
			[ReadOnly]
			public NativeArray<ColliderHit> WalkableOverlapBoxResults;

			// Token: 0x0400091C RID: 2332
			[ReadOnly]
			public NativeArray<ColliderHit> DynamicOverlapBoxResults;

			// Token: 0x0400091D RID: 2333
			public NativeReference<int> NumOctants;

			// Token: 0x0400091E RID: 2334
			public NativeArray<Octant> Octants;

			// Token: 0x0400091F RID: 2335
			public NativeReference<int> NumWalkableOctantsReference;

			// Token: 0x04000920 RID: 2336
			[WriteOnly]
			public NativeParallelHashMap<int, SlopeNeighbors> SlopeMap;

			// Token: 0x04000921 RID: 2337
			private int octantHead;
		}

		// Token: 0x020001E5 RID: 485
		[BurstCompile(FloatPrecision = FloatPrecision.Low, FloatMode = FloatMode.Fast)]
		private struct PruneOctantsJob : IJob
		{
			// Token: 0x06000A14 RID: 2580 RVA: 0x00035438 File Offset: 0x00033638
			public void Execute()
			{
				this.RawOctants.GetSubArray(0, this.NumOctants).CopyTo(this.Octants);
			}

			// Token: 0x04000922 RID: 2338
			[ReadOnly]
			public NativeArray<Octant> RawOctants;

			// Token: 0x04000923 RID: 2339
			[WriteOnly]
			public NativeArray<Octant> Octants;

			// Token: 0x04000924 RID: 2340
			public int NumOctants;
		}

		// Token: 0x020001E6 RID: 486
		public struct Results
		{
			// Token: 0x04000925 RID: 2341
			public NativeArray<Octant> Octants;

			// Token: 0x04000926 RID: 2342
			public NativeParallelHashMap<int, SerializableGuid> AssociatedPropMap;

			// Token: 0x04000927 RID: 2343
			public NativeParallelHashMap<int, SlopeNeighbors> SlopeMap;

			// Token: 0x04000928 RID: 2344
			public Dictionary<SerializableGuid, HashSet<int3>> DynamicCellMap;

			// Token: 0x04000929 RID: 2345
			public int NumWalkableOctants;
		}

		// Token: 0x020001E7 RID: 487
		private struct PotentialOctantData
		{
			// Token: 0x0400092A RID: 2346
			public float3 center;

			// Token: 0x0400092B RID: 2347
			public bool IsSlope;

			// Token: 0x0400092C RID: 2348
			public SlopeNeighbors Slope;
		}
	}
}
