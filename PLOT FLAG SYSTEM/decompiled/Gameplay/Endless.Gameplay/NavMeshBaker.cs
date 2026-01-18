using System;
using System.Collections.Generic;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Unity.AI.Navigation;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

namespace Endless.Gameplay;

public class NavMeshBaker : MonoBehaviour
{
	private NavMeshSurface navMeshSurface;

	private NavMeshData navMeshData;

	private Bounds navMeshBounds;

	private AsyncOperation currentBuildOperation;

	private bool rebakeAfterCompletion;

	private readonly List<Action<AsyncOperation>> enqueuedActions = new List<Action<AsyncOperation>>();

	private NavMeshSurface NavMeshSurface => navMeshSurface ?? (navMeshSurface = UnityEngine.Object.FindObjectOfType<NavMeshSurface>());

	private void Start()
	{
		MonoBehaviourSingleton<NavGraph>.Instance.NavMeshBaker = this;
	}

	public AsyncOperation BuildInitialNavMesh()
	{
		navMeshData = new NavMeshData();
		NavMesh.AddNavMeshData(navMeshData);
		Vector3Int minimumExtents = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.MinimumExtents;
		Vector3Int maximumExtents = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.MaximumExtents;
		navMeshBounds = default(Bounds);
		navMeshBounds.Encapsulate(minimumExtents + new Vector3(-4f, -4f, -4f));
		navMeshBounds.Encapsulate(maximumExtents + new Vector3(4f, 4f, 4f));
		return BuildNavMesh();
	}

	[ContextMenu("UpdateNavMesh")]
	private void ContextUpdateNavMesh()
	{
		UpdateNavMesh(null);
	}

	public void UpdateNavMesh(Action<AsyncOperation> updateCompletionAction)
	{
		AsyncOperation asyncOperation = currentBuildOperation;
		if (asyncOperation != null && !asyncOperation.isDone)
		{
			rebakeAfterCompletion = true;
			enqueuedActions.Add(updateCompletionAction);
		}
		else
		{
			InitializeNavMeshBuild(updateCompletionAction);
		}
	}

	private void InitializeNavMeshBuild(Action<AsyncOperation> updateCompletionAction)
	{
		currentBuildOperation = BuildNavMesh();
		currentBuildOperation.completed += HandleCurrentBuildOperationCompleted;
		if (updateCompletionAction != null)
		{
			currentBuildOperation.completed += updateCompletionAction;
		}
	}

	private void HandleCurrentBuildOperationCompleted(AsyncOperation obj)
	{
		currentBuildOperation = null;
		if (!rebakeAfterCompletion)
		{
			return;
		}
		rebakeAfterCompletion = false;
		InitializeNavMeshBuild(null);
		foreach (Action<AsyncOperation> enqueuedAction in enqueuedActions)
		{
			currentBuildOperation.completed += enqueuedAction;
		}
	}

	private AsyncOperation BuildNavMesh()
	{
		List<NavMeshBuildSource> navMeshBuildSources = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.BuildSourceTracker.GetNavMeshBuildSources();
		NavMeshModifierVolume falloffVolume = MonoBehaviourSingleton<NavGraph>.Instance.FalloffVolume;
		Vector3 pos = falloffVolume.transform.TransformPoint(falloffVolume.center);
		Vector3 lossyScale = falloffVolume.transform.lossyScale;
		Vector3 size = new Vector3(falloffVolume.size.x * math.abs(lossyScale.x), falloffVolume.size.y * math.abs(lossyScale.y), falloffVolume.size.z * math.abs(lossyScale.z));
		navMeshBuildSources.Add(new NavMeshBuildSource
		{
			shape = NavMeshBuildSourceShape.ModifierBox,
			area = falloffVolume.area,
			transform = Matrix4x4.TRS(pos, falloffVolume.transform.rotation, Vector3.one),
			size = size
		});
		return NavMeshBuilder.UpdateNavMeshDataAsync(navMeshData, NavMeshSurface.GetBuildSettings(), navMeshBuildSources, navMeshBounds);
	}

	private static bool AreSourcesEqual(List<NavMeshBuildSource> sourcesA, List<NavMeshBuildSource> sourcesB)
	{
		if (sourcesA == sourcesB)
		{
			return true;
		}
		if (sourcesA == null || sourcesB == null)
		{
			return false;
		}
		if (sourcesA.Count != sourcesB.Count)
		{
			return false;
		}
		List<NavMeshBuildSource> list = new List<NavMeshBuildSource>(sourcesB);
		foreach (NavMeshBuildSource item in sourcesA)
		{
			bool flag = false;
			for (int i = 0; i < list.Count; i++)
			{
				if (AreSourcesEquivalent(item, list[i]))
				{
					list.RemoveAt(i);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		return list.Count == 0;
	}

	private static bool AreSourcesEquivalent(NavMeshBuildSource a, NavMeshBuildSource b)
	{
		if (a.shape == b.shape && a.transform == b.transform && a.size == b.size && a.area == b.area && a.component == b.component)
		{
			return a.sourceObject == b.sourceObject;
		}
		return false;
	}
}
