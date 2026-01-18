using System;
using System.Collections;
using System.Collections.Generic;
using Endless.Shared;
using Unity.Collections;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000260 RID: 608
	public class PerceptionManager : MonoBehaviourSingleton<PerceptionManager>
	{
		// Token: 0x17000242 RID: 578
		// (get) Token: 0x06000C90 RID: 3216 RVA: 0x00044275 File Offset: 0x00042475
		// (set) Token: 0x06000C91 RID: 3217 RVA: 0x0004427D File Offset: 0x0004247D
		public LayerMask CharacterMask { get; private set; }

		// Token: 0x17000243 RID: 579
		// (get) Token: 0x06000C92 RID: 3218 RVA: 0x00044286 File Offset: 0x00042486
		// (set) Token: 0x06000C93 RID: 3219 RVA: 0x0004428E File Offset: 0x0004248E
		public LayerMask ObstacleMask { get; private set; }

		// Token: 0x17000244 RID: 580
		// (get) Token: 0x06000C94 RID: 3220 RVA: 0x00044297 File Offset: 0x00042497
		public HashSet<HittableComponent> Perceivables { get; } = new HashSet<HittableComponent>();

		// Token: 0x06000C95 RID: 3221 RVA: 0x0004429F File Offset: 0x0004249F
		private void Start()
		{
			UnifiedStateUpdater.OnProcessRequests += this.ProcessPerceptionRequests;
			UnifiedStateUpdater.OnProcessRequests += this.ProcessBoxcastRequests;
		}

		// Token: 0x06000C96 RID: 3222 RVA: 0x000442C4 File Offset: 0x000424C4
		private void ProcessBoxcastRequests()
		{
			if (this.boxcastRequests.Count == 0)
			{
				return;
			}
			BoxcastJobWrapper boxcastJobWrapper = new BoxcastJobWrapper(this.boxcastRequests);
			base.StartCoroutine(PerceptionManager.JobCompletionChecker(boxcastJobWrapper));
			this.boxcastRequests.Clear();
		}

		// Token: 0x06000C97 RID: 3223 RVA: 0x00044303 File Offset: 0x00042503
		public void RequestPerception(PerceptionRequest request)
		{
			if (request.IsBoxcast)
			{
				this.boxcastRequests.Add(request);
				return;
			}
			this.perceptionRequests.Add(request);
		}

		// Token: 0x06000C98 RID: 3224 RVA: 0x00044328 File Offset: 0x00042528
		private void ProcessPerceptionRequests()
		{
			if (this.perceptionRequests.Count == 0)
			{
				return;
			}
			this.UpdateTargetData();
			if (this.targetData.Count == 0)
			{
				this.perceptionRequests.Clear();
				return;
			}
			PerceptionJobWrapper perceptionJobWrapper = new PerceptionJobWrapper(this.perceptionRequests, this.targetData);
			base.StartCoroutine(PerceptionManager.JobCompletionChecker(perceptionJobWrapper));
			this.perceptionRequests.Clear();
		}

		// Token: 0x06000C99 RID: 3225 RVA: 0x0004438C File Offset: 0x0004258C
		private void UpdateTargetData()
		{
			this.targetData.Clear();
			foreach (HittableComponent hittableComponent in this.Perceivables)
			{
				this.targetData.AddRange(hittableComponent.GetTargetableColliderData());
			}
		}

		// Token: 0x06000C9A RID: 3226 RVA: 0x000443F4 File Offset: 0x000425F4
		private static IEnumerator JobCompletionChecker(IPerceptionJob job)
		{
			int frameCount = 0;
			while (!job.IsComplete)
			{
				yield return null;
				int num = frameCount + 1;
				frameCount = num;
				if (num > 4)
				{
					job.RespondToRequests();
					yield break;
				}
			}
			job.RespondToRequests();
			yield break;
		}

		// Token: 0x04000B9A RID: 2970
		private readonly List<PerceptionRequest> perceptionRequests = new List<PerceptionRequest>();

		// Token: 0x04000B9B RID: 2971
		private readonly List<PerceptionRequest> boxcastRequests = new List<PerceptionRequest>();

		// Token: 0x04000B9C RID: 2972
		private NativeArray<BoxcastCommand> boxcastCommands;

		// Token: 0x04000B9D RID: 2973
		private readonly List<TargetDatum> targetData = new List<TargetDatum>();

		// Token: 0x04000B9E RID: 2974
		private Coroutine boxcastCompletionChecker;

		// Token: 0x04000B9F RID: 2975
		private Coroutine perceptionCompletionChecker;
	}
}
