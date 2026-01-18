using System;
using System.Collections.Generic;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Unity.AI.Navigation;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

namespace Endless.Gameplay
{
	// Token: 0x02000205 RID: 517
	public class NavMeshBaker : MonoBehaviour
	{
		// Token: 0x17000202 RID: 514
		// (get) Token: 0x06000AB7 RID: 2743 RVA: 0x0003A49C File Offset: 0x0003869C
		private NavMeshSurface NavMeshSurface
		{
			get
			{
				NavMeshSurface navMeshSurface;
				if ((navMeshSurface = this.navMeshSurface) == null)
				{
					navMeshSurface = (this.navMeshSurface = global::UnityEngine.Object.FindObjectOfType<NavMeshSurface>());
				}
				return navMeshSurface;
			}
		}

		// Token: 0x06000AB8 RID: 2744 RVA: 0x0003A4C1 File Offset: 0x000386C1
		private void Start()
		{
			MonoBehaviourSingleton<NavGraph>.Instance.NavMeshBaker = this;
		}

		// Token: 0x06000AB9 RID: 2745 RVA: 0x0003A4D0 File Offset: 0x000386D0
		public AsyncOperation BuildInitialNavMesh()
		{
			this.navMeshData = new NavMeshData();
			NavMesh.AddNavMeshData(this.navMeshData);
			Vector3Int minimumExtents = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.MinimumExtents;
			Vector3Int maximumExtents = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.MaximumExtents;
			this.navMeshBounds = default(Bounds);
			this.navMeshBounds.Encapsulate(minimumExtents + new Vector3(-4f, -4f, -4f));
			this.navMeshBounds.Encapsulate(maximumExtents + new Vector3(4f, 4f, 4f));
			return this.BuildNavMesh();
		}

		// Token: 0x06000ABA RID: 2746 RVA: 0x0003A57A File Offset: 0x0003877A
		[ContextMenu("UpdateNavMesh")]
		private void ContextUpdateNavMesh()
		{
			this.UpdateNavMesh(null);
		}

		// Token: 0x06000ABB RID: 2747 RVA: 0x0003A584 File Offset: 0x00038784
		public void UpdateNavMesh(Action<AsyncOperation> updateCompletionAction)
		{
			AsyncOperation asyncOperation = this.currentBuildOperation;
			if (asyncOperation != null && !asyncOperation.isDone)
			{
				this.rebakeAfterCompletion = true;
				this.enqueuedActions.Add(updateCompletionAction);
				return;
			}
			this.InitializeNavMeshBuild(updateCompletionAction);
		}

		// Token: 0x06000ABC RID: 2748 RVA: 0x0003A5BE File Offset: 0x000387BE
		private void InitializeNavMeshBuild(Action<AsyncOperation> updateCompletionAction)
		{
			this.currentBuildOperation = this.BuildNavMesh();
			this.currentBuildOperation.completed += this.HandleCurrentBuildOperationCompleted;
			if (updateCompletionAction != null)
			{
				this.currentBuildOperation.completed += updateCompletionAction;
			}
		}

		// Token: 0x06000ABD RID: 2749 RVA: 0x0003A5F4 File Offset: 0x000387F4
		private void HandleCurrentBuildOperationCompleted(AsyncOperation obj)
		{
			this.currentBuildOperation = null;
			if (this.rebakeAfterCompletion)
			{
				this.rebakeAfterCompletion = false;
				this.InitializeNavMeshBuild(null);
				foreach (Action<AsyncOperation> action in this.enqueuedActions)
				{
					this.currentBuildOperation.completed += action;
				}
			}
		}

		// Token: 0x06000ABE RID: 2750 RVA: 0x0003A66C File Offset: 0x0003886C
		private AsyncOperation BuildNavMesh()
		{
			List<NavMeshBuildSource> navMeshBuildSources = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.BuildSourceTracker.GetNavMeshBuildSources();
			NavMeshModifierVolume falloffVolume = MonoBehaviourSingleton<NavGraph>.Instance.FalloffVolume;
			Vector3 vector = falloffVolume.transform.TransformPoint(falloffVolume.center);
			Vector3 lossyScale = falloffVolume.transform.lossyScale;
			Vector3 vector2 = new Vector3(falloffVolume.size.x * math.abs(lossyScale.x), falloffVolume.size.y * math.abs(lossyScale.y), falloffVolume.size.z * math.abs(lossyScale.z));
			navMeshBuildSources.Add(new NavMeshBuildSource
			{
				shape = NavMeshBuildSourceShape.ModifierBox,
				area = falloffVolume.area,
				transform = Matrix4x4.TRS(vector, falloffVolume.transform.rotation, Vector3.one),
				size = vector2
			});
			return NavMeshBuilder.UpdateNavMeshDataAsync(this.navMeshData, this.NavMeshSurface.GetBuildSettings(), navMeshBuildSources, this.navMeshBounds);
		}

		// Token: 0x06000ABF RID: 2751 RVA: 0x0003A76C File Offset: 0x0003896C
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
			foreach (NavMeshBuildSource navMeshBuildSource in sourcesA)
			{
				bool flag = false;
				for (int i = 0; i < list.Count; i++)
				{
					if (NavMeshBaker.AreSourcesEquivalent(navMeshBuildSource, list[i]))
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

		// Token: 0x06000AC0 RID: 2752 RVA: 0x0003A820 File Offset: 0x00038A20
		private static bool AreSourcesEquivalent(NavMeshBuildSource a, NavMeshBuildSource b)
		{
			return a.shape == b.shape && a.transform == b.transform && a.size == b.size && a.area == b.area && a.component == b.component && a.sourceObject == b.sourceObject;
		}

		// Token: 0x04000A19 RID: 2585
		private NavMeshSurface navMeshSurface;

		// Token: 0x04000A1A RID: 2586
		private NavMeshData navMeshData;

		// Token: 0x04000A1B RID: 2587
		private Bounds navMeshBounds;

		// Token: 0x04000A1C RID: 2588
		private AsyncOperation currentBuildOperation;

		// Token: 0x04000A1D RID: 2589
		private bool rebakeAfterCompletion;

		// Token: 0x04000A1E RID: 2590
		private readonly List<Action<AsyncOperation>> enqueuedActions = new List<Action<AsyncOperation>>();
	}
}
