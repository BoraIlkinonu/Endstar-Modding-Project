using System;
using System.Collections;
using System.Collections.Generic;
using Endless.Shared;
using Endless.Shared.DataTypes;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace Endless.Gameplay
{
	// Token: 0x02000147 RID: 327
	public class PathingComponent
	{
		// Token: 0x1400000D RID: 13
		// (add) Token: 0x060007B3 RID: 1971 RVA: 0x0002461C File Offset: 0x0002281C
		// (remove) Token: 0x060007B4 RID: 1972 RVA: 0x00024654 File Offset: 0x00022854
		public event Action OnEdgeExcluded;

		// Token: 0x060007B5 RID: 1973 RVA: 0x00024689 File Offset: 0x00022889
		public PathingComponent(NpcEntity entity, PathFollower follower, MonoBehaviorProxy proxy)
		{
			this.NpcEntity = entity;
			this.PathFollower = follower;
			this.proxy = proxy;
		}

		// Token: 0x1700016C RID: 364
		// (get) Token: 0x060007B6 RID: 1974 RVA: 0x000246BC File Offset: 0x000228BC
		private PathFollower PathFollower { get; }

		// Token: 0x1700016D RID: 365
		// (get) Token: 0x060007B7 RID: 1975 RVA: 0x000246C4 File Offset: 0x000228C4
		private NpcEntity NpcEntity { get; }

		// Token: 0x060007B8 RID: 1976 RVA: 0x000246CC File Offset: 0x000228CC
		public bool RequestPath(Vector3 destination, Action<Pathfinding.Response> pathfindingResponseHandler)
		{
			NavMeshHit navMeshHit;
			NavMeshHit navMeshHit2;
			Pathfinding.Request request;
			if (NavMesh.SamplePosition(destination, out navMeshHit, 0.5f, -1) && NavMesh.SamplePosition(this.NpcEntity.FootPosition, out navMeshHit2, 0.5f, -1) && Pathfinding.Request.TryCreateNewPathfindingRequest(out request, this.NpcEntity.WorldObject, navMeshHit2.position, navMeshHit.position, this.NpcEntity.PathfindingRange, pathfindingResponseHandler, this.excludedEdges))
			{
				MonoBehaviourSingleton<Pathfinding>.Instance.RequestPath(request);
				return true;
			}
			return false;
		}

		// Token: 0x060007B9 RID: 1977 RVA: 0x00024745 File Offset: 0x00022945
		public void Repath(HashSet<SerializableGuid> updatedProps)
		{
			if (this.PathFollower.Path != null && this.PathFollower.IsRepathNecessary(updatedProps))
			{
				Vector3 destination = this.PathFollower.Path.Destination;
				this.PathFollower.StopPath(false);
			}
		}

		// Token: 0x060007BA RID: 1978 RVA: 0x00024780 File Offset: 0x00022980
		public void ExcludeEdge(Lockable lockable, NavPath.Segment segment)
		{
			ExcludedEdge excludedEdge = new ExcludedEdge
			{
				OriginNodeKey = segment.StartSection,
				EndNodeKey = segment.EndSection
			};
			this.lockedEdges.TryAdd(lockable, new HashSet<ExcludedEdge>());
			this.lockedEdges[lockable].Add(excludedEdge);
			this.excludedEdges.Add(excludedEdge);
			lockable.Unlocked.AddListener(new UnityAction<Lockable>(this.HandleUnlock));
			this.proxy.StartMonoBehaviorRoutine(this.RemoveEdge(excludedEdge, 60f));
			this.Repath(new HashSet<SerializableGuid> { lockable.WorldObject.InstanceId });
			Action onEdgeExcluded = this.OnEdgeExcluded;
			if (onEdgeExcluded == null)
			{
				return;
			}
			onEdgeExcluded();
		}

		// Token: 0x060007BB RID: 1979 RVA: 0x00024840 File Offset: 0x00022A40
		private void HandleUnlock(Lockable lockable)
		{
			HashSet<ExcludedEdge> hashSet;
			if (!this.lockedEdges.TryGetValue(lockable, out hashSet))
			{
				return;
			}
			foreach (ExcludedEdge excludedEdge in hashSet)
			{
				this.excludedEdges.Remove(excludedEdge);
			}
			this.lockedEdges.Remove(lockable);
			lockable.Unlocked.RemoveListener(new UnityAction<Lockable>(this.HandleUnlock));
		}

		// Token: 0x060007BC RID: 1980 RVA: 0x000248CC File Offset: 0x00022ACC
		private IEnumerator RemoveEdge(ExcludedEdge edge, float delay)
		{
			yield return new WaitForSeconds(delay);
			this.excludedEdges.Remove(edge);
			yield break;
		}

		// Token: 0x04000629 RID: 1577
		private readonly List<ExcludedEdge> excludedEdges = new List<ExcludedEdge>();

		// Token: 0x0400062A RID: 1578
		private readonly Dictionary<Lockable, HashSet<ExcludedEdge>> lockedEdges = new Dictionary<Lockable, HashSet<ExcludedEdge>>();

		// Token: 0x0400062B RID: 1579
		private readonly MonoBehaviorProxy proxy;
	}
}
