using System.Collections.Generic;
using Endless.Shared;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace Endless.Gameplay;

public class PerceptionJobWrapper : IPerceptionJob
{
	private readonly List<PerceptionRequest> requests;

	private NativeArray<TargeterDatum> targeterData;

	private NativeArray<TargetDatum> targetData;

	private NativeArray<TargetDatum> awarenessResults;

	private NativeArray<int> validTargets;

	private NativeArray<RaycastCommand> characterCommands;

	private NativeArray<RaycastCommand> obstacleCommands;

	private NativeArray<RaycastHit> characterRaycastHits;

	private NativeArray<RaycastHit> obstacleRaycastHits;

	private JobHandle jobHandle;

	private readonly int numTargets;

	private readonly int numRequests;

	private const int MAX_HITS = 3;

	public bool IsComplete => jobHandle.IsCompleted;

	public PerceptionJobWrapper(List<PerceptionRequest> perceptionRequests, List<TargetDatum> targetDatums)
	{
		requests = new List<PerceptionRequest>(perceptionRequests);
		numRequests = requests.Count;
		numTargets = targetDatums.Count;
		targeterData = new NativeArray<TargeterDatum>(numRequests, Allocator.TempJob);
		targetData = new NativeArray<TargetDatum>(numTargets, Allocator.TempJob);
		awarenessResults = new NativeArray<TargetDatum>(numRequests * numTargets, Allocator.TempJob);
		characterCommands = new NativeArray<RaycastCommand>(numRequests * numTargets, Allocator.TempJob);
		obstacleCommands = new NativeArray<RaycastCommand>(numRequests * numTargets, Allocator.TempJob);
		characterRaycastHits = new NativeArray<RaycastHit>(numRequests * numTargets * 3, Allocator.TempJob);
		obstacleRaycastHits = new NativeArray<RaycastHit>(numRequests * numTargets, Allocator.TempJob);
		validTargets = new NativeArray<int>(numRequests, Allocator.TempJob);
		for (int i = 0; i < numRequests; i++)
		{
			PerceptionRequest perceptionRequest = requests[i];
			TargeterDatum targeterDatum = perceptionRequest.GetTargeterDatum();
			targeterData[i] = targeterDatum;
			QueryParameters queryParameters = new QueryParameters(MonoBehaviourSingleton<PerceptionManager>.Instance.CharacterMask);
			QueryParameters queryParameters2 = new QueryParameters(MonoBehaviourSingleton<PerceptionManager>.Instance.ObstacleMask);
			for (int j = 0; j < numTargets; j++)
			{
				TargetDatum targetDatum = targetDatums[j];
				bool flag = targeterDatum.Position.Approximately(targetDatum.Position, 0.1f);
				Vector3 vector = (flag ? Vector3.up : (targetDatum.Position - targeterDatum.Position));
				vector = math.select(vector, new float3(0f, 1f, 0f), flag);
				Vector3 normalized = vector.normalized;
				RaycastCommand value = new RaycastCommand(perceptionRequest.Position, normalized, queryParameters, vector.magnitude);
				RaycastCommand value2 = new RaycastCommand(perceptionRequest.Position, normalized, queryParameters2, vector.magnitude);
				characterCommands[i * numTargets + j] = value;
				obstacleCommands[i * numTargets + j] = value2;
			}
		}
		for (int k = 0; k < numTargets; k++)
		{
			targetData[k] = targetDatums[k];
		}
		int minCommandsPerJob = Mathf.Max(10, Mathf.CeilToInt((float)characterCommands.Length / (float)JobsUtility.JobWorkerCount));
		JobHandle job = RaycastCommand.ScheduleBatch(characterCommands, characterRaycastHits, minCommandsPerJob, 3);
		JobHandle job2 = RaycastCommand.ScheduleBatch(obstacleCommands, obstacleRaycastHits, minCommandsPerJob);
		JobHandle dependsOn = JobHandle.CombineDependencies(job, job2);
		PerceptionJob jobData = new PerceptionJob
		{
			MaxHits = 3,
			TargeterData = targeterData,
			TargetData = targetData,
			CharacterRaycastHits = characterRaycastHits,
			ObstacleRaycastHits = obstacleRaycastHits,
			Targets = awarenessResults,
			ValidTargets = validTargets
		};
		int innerloopBatchCount = numRequests / MonoBehaviourSingleton<NavGraph>.Instance.MaxNumBatches;
		jobHandle = IJobParallelForExtensions.Schedule(jobData, numRequests, innerloopBatchCount, dependsOn);
	}

	public void RespondToRequests()
	{
		jobHandle.Complete();
		if (!MonoBehaviourSingleton<GameplayManager>.Instance.IsPlaying)
		{
			DisposeCollections();
			return;
		}
		for (int i = 0; i < numRequests; i++)
		{
			List<PerceptionResult> perceptionResults = requests[i].PerceptionResults;
			perceptionResults.Clear();
			foreach (TargetDatum item2 in awarenessResults.GetSubArray(i * numTargets, validTargets[i]))
			{
				HittableComponent hittableFromMap = MonoBehaviourSingleton<HittableMap>.Instance.GetHittableFromMap(item2.ColliderId);
				PerceptionResult item = new PerceptionResult
				{
					HittableComponent = hittableFromMap,
					Awareness = item2.Awareness
				};
				perceptionResults.Add(item);
			}
			requests[i].PerceptionUpdatedCallback();
		}
		DisposeCollections();
	}

	private void DisposeCollections()
	{
		if (!jobHandle.IsCompleted)
		{
			jobHandle.Complete();
		}
		targeterData.Dispose();
		targetData.Dispose();
		awarenessResults.Dispose();
		validTargets.Dispose();
		characterCommands.Dispose();
		obstacleCommands.Dispose();
		characterRaycastHits.Dispose();
		obstacleRaycastHits.Dispose();
	}
}
