using System;
using System.Collections.Generic;
using Endless.Shared;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Endless.Gameplay;

public class Los : MonoBehaviourSingleton<Los>
{
	[Tooltip("Maximum distance from the Ai that cells will be sampled for being a potential los position.")]
	[SerializeField]
	private float losSampleDistance;

	[Tooltip("Value for optimizing batched raycasts")]
	[SerializeField]
	[Range(1f, 1000f)]
	private int raycastBatchSize;

	[SerializeField]
	private LayerMask raycastMask;

	public void RequestLosPositions(NpcEntity aiEntity, Action<List<Vector3>> callback)
	{
		List<Vector3> list = new List<Vector3>();
		if (!aiEntity.Target)
		{
			callback(list);
			return;
		}
		List<Vector3> list2 = MonoBehaviourSingleton<Pathfinding>.Instance.FindNavigationPositionsInRange(aiEntity.FootPosition, losSampleDistance);
		List<TargetDatum> targetableColliderData = aiEntity.Target.GetTargetableColliderData();
		int count = targetableColliderData.Count;
		int num = list2.Count * count;
		if (num == 0)
		{
			callback(list);
			return;
		}
		NativeArray<RaycastCommand> commands = new NativeArray<RaycastCommand>(num, Allocator.TempJob);
		QueryParameters queryParameters = new QueryParameters(raycastMask);
		for (int i = 0; i < list2.Count; i++)
		{
			Vector3 vector = list2[i] + Vector3.up;
			for (int j = 0; j < count; j++)
			{
				Vector3 vector2 = targetableColliderData[j].Position - vector;
				Vector3 normalized = vector2.normalized;
				int index = i * count + j;
				commands[index] = new RaycastCommand(vector, normalized, queryParameters, vector2.magnitude);
			}
		}
		NativeArray<RaycastHit> results = new NativeArray<RaycastHit>(num, Allocator.TempJob);
		RaycastCommand.ScheduleBatch(commands, results, raycastBatchSize).Complete();
		for (int k = 0; k < list2.Count; k++)
		{
			for (int l = 0; l < count; l++)
			{
				TargetDatum targetDatum = targetableColliderData[l];
				int index2 = k * count + l;
				if (results[index2].colliderInstanceID == targetDatum.ColliderId)
				{
					list.Add(list2[k]);
					break;
				}
			}
		}
		results.Dispose(default(JobHandle));
		commands.Dispose(default(JobHandle));
		if (list.Count == 0 && aiEntity.Target.CombatPositionGenerator.TryGetClosestAroundPosition(aiEntity.FootPosition, out var aroundPosition))
		{
			list.Add(aroundPosition);
		}
		list.Sort((Vector3 a, Vector3 b) => math.distance(a, aiEntity.FootPosition).CompareTo(math.distance(b, aiEntity.FootPosition)));
		callback(list);
	}
}
