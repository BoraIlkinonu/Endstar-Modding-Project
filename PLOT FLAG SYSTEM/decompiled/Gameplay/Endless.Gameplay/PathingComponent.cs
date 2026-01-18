using System;
using System.Collections;
using System.Collections.Generic;
using Endless.Shared;
using Endless.Shared.DataTypes;
using UnityEngine;
using UnityEngine.AI;

namespace Endless.Gameplay;

public class PathingComponent
{
	private readonly List<ExcludedEdge> excludedEdges = new List<ExcludedEdge>();

	private readonly Dictionary<Lockable, HashSet<ExcludedEdge>> lockedEdges = new Dictionary<Lockable, HashSet<ExcludedEdge>>();

	private readonly MonoBehaviorProxy proxy;

	private PathFollower PathFollower { get; }

	private NpcEntity NpcEntity { get; }

	public event Action OnEdgeExcluded;

	public PathingComponent(NpcEntity entity, PathFollower follower, MonoBehaviorProxy proxy)
	{
		NpcEntity = entity;
		PathFollower = follower;
		this.proxy = proxy;
	}

	public bool RequestPath(Vector3 destination, Action<Pathfinding.Response> pathfindingResponseHandler)
	{
		if (NavMesh.SamplePosition(destination, out var hit, 0.5f, -1) && NavMesh.SamplePosition(NpcEntity.FootPosition, out var hit2, 0.5f, -1) && Pathfinding.Request.TryCreateNewPathfindingRequest(out var request, NpcEntity.WorldObject, hit2.position, hit.position, NpcEntity.PathfindingRange, pathfindingResponseHandler, excludedEdges))
		{
			MonoBehaviourSingleton<Pathfinding>.Instance.RequestPath(request);
			return true;
		}
		return false;
	}

	public void Repath(HashSet<SerializableGuid> updatedProps)
	{
		if (PathFollower.Path != null && PathFollower.IsRepathNecessary(updatedProps))
		{
			_ = PathFollower.Path.Destination;
			PathFollower.StopPath();
		}
	}

	public void ExcludeEdge(Lockable lockable, NavPath.Segment segment)
	{
		ExcludedEdge excludedEdge = new ExcludedEdge
		{
			OriginNodeKey = segment.StartSection,
			EndNodeKey = segment.EndSection
		};
		lockedEdges.TryAdd(lockable, new HashSet<ExcludedEdge>());
		lockedEdges[lockable].Add(excludedEdge);
		excludedEdges.Add(excludedEdge);
		lockable.Unlocked.AddListener(HandleUnlock);
		proxy.StartMonoBehaviorRoutine(RemoveEdge(excludedEdge, 60f));
		Repath(new HashSet<SerializableGuid> { lockable.WorldObject.InstanceId });
		this.OnEdgeExcluded?.Invoke();
	}

	private void HandleUnlock(Lockable lockable)
	{
		if (!lockedEdges.TryGetValue(lockable, out var value))
		{
			return;
		}
		foreach (ExcludedEdge item in value)
		{
			excludedEdges.Remove(item);
		}
		lockedEdges.Remove(lockable);
		lockable.Unlocked.RemoveListener(HandleUnlock);
	}

	private IEnumerator RemoveEdge(ExcludedEdge edge, float delay)
	{
		yield return new WaitForSeconds(delay);
		excludedEdges.Remove(edge);
	}
}
