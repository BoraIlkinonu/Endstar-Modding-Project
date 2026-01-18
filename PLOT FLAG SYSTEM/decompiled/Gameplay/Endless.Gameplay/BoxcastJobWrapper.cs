using System.Collections.Generic;
using Endless.Shared;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine;

namespace Endless.Gameplay;

public class BoxcastJobWrapper : IPerceptionJob
{
	private const int MAX_HITS = 5;

	private readonly List<PerceptionRequest> requests;

	private NativeArray<BoxcastCommand> boxcastCommands;

	private NativeArray<RaycastHit> boxcastHits;

	private NativeArray<OverlapBoxCommand> overlapCommands;

	private NativeArray<ColliderHit> overlapHits;

	private JobHandle boxcastHandle;

	private JobHandle overlapHandle;

	public bool IsComplete
	{
		get
		{
			if (boxcastHandle.IsCompleted)
			{
				return overlapHandle.IsCompleted;
			}
			return false;
		}
	}

	public BoxcastJobWrapper(List<PerceptionRequest> perceptionRequests)
	{
		requests = new List<PerceptionRequest>(perceptionRequests);
		boxcastCommands = new NativeArray<BoxcastCommand>(requests.Count, Allocator.TempJob);
		boxcastHits = new NativeArray<RaycastHit>(requests.Count * 5, Allocator.TempJob);
		overlapCommands = new NativeArray<OverlapBoxCommand>(requests.Count, Allocator.TempJob);
		overlapHits = new NativeArray<ColliderHit>(requests.Count * 5, Allocator.TempJob);
		for (int i = 0; i < requests.Count; i++)
		{
			PerceptionRequest perceptionRequest = requests[i];
			QueryParameters queryParameters = new QueryParameters
			{
				layerMask = (perceptionRequest.UseXray ? ((int)MonoBehaviourSingleton<PerceptionManager>.Instance.CharacterMask) : ((int)MonoBehaviourSingleton<PerceptionManager>.Instance.CharacterMask | (int)MonoBehaviourSingleton<PerceptionManager>.Instance.ObstacleMask))
			};
			Vector3 halfExtents = new Vector3(perceptionRequest.HorizontalValue, perceptionRequest.VerticalValue, perceptionRequest.HorizontalValue) * 0.5f;
			Quaternion quaternion = Quaternion.LookRotation(perceptionRequest.LookVector);
			Vector3 center = perceptionRequest.Position + quaternion * Vector3.forward * perceptionRequest.HorizontalValue;
			BoxcastCommand value = new BoxcastCommand(center, halfExtents, quaternion, perceptionRequest.LookVector, queryParameters, perceptionRequest.MaxDistance);
			OverlapBoxCommand value2 = new OverlapBoxCommand(center, new Vector3(0.5f, 0.5f, 0.2f), quaternion, queryParameters);
			boxcastCommands[i] = value;
			overlapCommands[i] = value2;
		}
		int minCommandsPerJob = Mathf.Max(5, Mathf.CeilToInt((float)boxcastCommands.Length / (float)JobsUtility.JobWorkerCount));
		overlapHandle = OverlapBoxCommand.ScheduleBatch(overlapCommands, overlapHits, minCommandsPerJob, 5);
		boxcastHandle = BoxcastCommand.ScheduleBatch(boxcastCommands, boxcastHits, minCommandsPerJob, 5);
	}

	public void RespondToRequests()
	{
		boxcastHandle.Complete();
		overlapHandle.Complete();
		if (!MonoBehaviourSingleton<GameplayManager>.Instance.IsPlaying)
		{
			DisposeCollections();
			return;
		}
		for (int i = 0; i < requests.Count; i++)
		{
			PerceptionRequest perceptionRequest = requests[i];
			List<PerceptionResult> perceptionResults = perceptionRequest.PerceptionResults;
			perceptionResults.Clear();
			NativeArray<RaycastHit> subArray = boxcastHits.GetSubArray(i * 5, 5);
			NativeSlice<ColliderHit> nativeSlice = overlapHits.GetSubArray(i * 5, 5);
			for (int j = 0; j < 5; j++)
			{
				HittableComponent hittableFromMap = MonoBehaviourSingleton<HittableMap>.Instance.GetHittableFromMap(subArray[j].colliderInstanceID);
				if (!hittableFromMap)
				{
					break;
				}
				PerceptionResult item = new PerceptionResult
				{
					HittableComponent = hittableFromMap,
					Awareness = 100f
				};
				perceptionResults.Add(item);
			}
			for (int k = 0; k < 5; k++)
			{
				HittableComponent hittableFromMap2 = MonoBehaviourSingleton<HittableMap>.Instance.GetHittableFromMap(nativeSlice[k].instanceID);
				if ((bool)hittableFromMap2)
				{
					PerceptionResult item2 = new PerceptionResult
					{
						HittableComponent = hittableFromMap2,
						Awareness = 100f
					};
					perceptionResults.Add(item2);
				}
			}
			perceptionRequest.PerceptionUpdatedCallback();
		}
		DisposeCollections();
	}

	private void DisposeCollections()
	{
		if (!boxcastHandle.IsCompleted)
		{
			boxcastHandle.Complete();
		}
		if (!overlapHandle.IsCompleted)
		{
			overlapHandle.Complete();
		}
		boxcastCommands.Dispose();
		boxcastHits.Dispose();
		overlapCommands.Dispose();
		overlapHits.Dispose();
	}
}
