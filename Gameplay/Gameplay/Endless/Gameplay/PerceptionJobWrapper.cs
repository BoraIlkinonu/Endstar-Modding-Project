using System;
using System.Collections.Generic;
using Endless.Shared;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x0200025F RID: 607
	public class PerceptionJobWrapper : IPerceptionJob
	{
		// Token: 0x06000C8C RID: 3212 RVA: 0x00043D64 File Offset: 0x00041F64
		public PerceptionJobWrapper(List<PerceptionRequest> perceptionRequests, List<TargetDatum> targetDatums)
		{
			this.requests = new List<PerceptionRequest>(perceptionRequests);
			this.numRequests = this.requests.Count;
			this.numTargets = targetDatums.Count;
			this.targeterData = new NativeArray<TargeterDatum>(this.numRequests, Allocator.TempJob, NativeArrayOptions.ClearMemory);
			this.targetData = new NativeArray<TargetDatum>(this.numTargets, Allocator.TempJob, NativeArrayOptions.ClearMemory);
			this.awarenessResults = new NativeArray<TargetDatum>(this.numRequests * this.numTargets, Allocator.TempJob, NativeArrayOptions.ClearMemory);
			this.characterCommands = new NativeArray<RaycastCommand>(this.numRequests * this.numTargets, Allocator.TempJob, NativeArrayOptions.ClearMemory);
			this.obstacleCommands = new NativeArray<RaycastCommand>(this.numRequests * this.numTargets, Allocator.TempJob, NativeArrayOptions.ClearMemory);
			this.characterRaycastHits = new NativeArray<RaycastHit>(this.numRequests * this.numTargets * 3, Allocator.TempJob, NativeArrayOptions.ClearMemory);
			this.obstacleRaycastHits = new NativeArray<RaycastHit>(this.numRequests * this.numTargets, Allocator.TempJob, NativeArrayOptions.ClearMemory);
			this.validTargets = new NativeArray<int>(this.numRequests, Allocator.TempJob, NativeArrayOptions.ClearMemory);
			for (int i = 0; i < this.numRequests; i++)
			{
				PerceptionRequest perceptionRequest = this.requests[i];
				TargeterDatum targeterDatum = perceptionRequest.GetTargeterDatum();
				this.targeterData[i] = targeterDatum;
				QueryParameters queryParameters = new QueryParameters(MonoBehaviourSingleton<PerceptionManager>.Instance.CharacterMask, false, QueryTriggerInteraction.UseGlobal, false);
				QueryParameters queryParameters2 = new QueryParameters(MonoBehaviourSingleton<PerceptionManager>.Instance.ObstacleMask, false, QueryTriggerInteraction.UseGlobal, false);
				for (int j = 0; j < this.numTargets; j++)
				{
					TargetDatum targetDatum = targetDatums[j];
					bool flag = targeterDatum.Position.Approximately(targetDatum.Position, 0.1f);
					Vector3 vector = (flag ? Vector3.up : (targetDatum.Position - targeterDatum.Position));
					vector = math.select(vector, new float3(0f, 1f, 0f), flag);
					Vector3 normalized = vector.normalized;
					RaycastCommand raycastCommand = new RaycastCommand(perceptionRequest.Position, normalized, queryParameters, vector.magnitude);
					RaycastCommand raycastCommand2 = new RaycastCommand(perceptionRequest.Position, normalized, queryParameters2, vector.magnitude);
					this.characterCommands[i * this.numTargets + j] = raycastCommand;
					this.obstacleCommands[i * this.numTargets + j] = raycastCommand2;
				}
			}
			for (int k = 0; k < this.numTargets; k++)
			{
				this.targetData[k] = targetDatums[k];
			}
			int num = Mathf.Max(10, Mathf.CeilToInt((float)this.characterCommands.Length / (float)JobsUtility.JobWorkerCount));
			JobHandle jobHandle = RaycastCommand.ScheduleBatch(this.characterCommands, this.characterRaycastHits, num, 3, default(JobHandle));
			JobHandle jobHandle2 = RaycastCommand.ScheduleBatch(this.obstacleCommands, this.obstacleRaycastHits, num, default(JobHandle));
			JobHandle jobHandle3 = JobHandle.CombineDependencies(jobHandle, jobHandle2);
			PerceptionJob perceptionJob = new PerceptionJob
			{
				MaxHits = 3,
				TargeterData = this.targeterData,
				TargetData = this.targetData,
				CharacterRaycastHits = this.characterRaycastHits,
				ObstacleRaycastHits = this.obstacleRaycastHits,
				Targets = this.awarenessResults,
				ValidTargets = this.validTargets
			};
			int num2 = this.numRequests / MonoBehaviourSingleton<NavGraph>.Instance.MaxNumBatches;
			this.jobHandle = perceptionJob.Schedule(this.numRequests, num2, jobHandle3);
		}

		// Token: 0x17000241 RID: 577
		// (get) Token: 0x06000C8D RID: 3213 RVA: 0x000440DA File Offset: 0x000422DA
		public bool IsComplete
		{
			get
			{
				return this.jobHandle.IsCompleted;
			}
		}

		// Token: 0x06000C8E RID: 3214 RVA: 0x000440E8 File Offset: 0x000422E8
		public void RespondToRequests()
		{
			this.jobHandle.Complete();
			if (!MonoBehaviourSingleton<GameplayManager>.Instance.IsPlaying)
			{
				this.DisposeCollections();
				return;
			}
			for (int i = 0; i < this.numRequests; i++)
			{
				List<PerceptionResult> perceptionResults = this.requests[i].PerceptionResults;
				perceptionResults.Clear();
				foreach (TargetDatum targetDatum in this.awarenessResults.GetSubArray(i * this.numTargets, this.validTargets[i]))
				{
					HittableComponent hittableFromMap = MonoBehaviourSingleton<HittableMap>.Instance.GetHittableFromMap(targetDatum.ColliderId);
					PerceptionResult perceptionResult = new PerceptionResult
					{
						HittableComponent = hittableFromMap,
						Awareness = targetDatum.Awareness
					};
					perceptionResults.Add(perceptionResult);
				}
				this.requests[i].PerceptionUpdatedCallback();
			}
			this.DisposeCollections();
		}

		// Token: 0x06000C8F RID: 3215 RVA: 0x000441F8 File Offset: 0x000423F8
		private void DisposeCollections()
		{
			if (!this.jobHandle.IsCompleted)
			{
				this.jobHandle.Complete();
			}
			this.targeterData.Dispose();
			this.targetData.Dispose();
			this.awarenessResults.Dispose();
			this.validTargets.Dispose();
			this.characterCommands.Dispose();
			this.obstacleCommands.Dispose();
			this.characterRaycastHits.Dispose();
			this.obstacleRaycastHits.Dispose();
		}

		// Token: 0x04000B8B RID: 2955
		private readonly List<PerceptionRequest> requests;

		// Token: 0x04000B8C RID: 2956
		private NativeArray<TargeterDatum> targeterData;

		// Token: 0x04000B8D RID: 2957
		private NativeArray<TargetDatum> targetData;

		// Token: 0x04000B8E RID: 2958
		private NativeArray<TargetDatum> awarenessResults;

		// Token: 0x04000B8F RID: 2959
		private NativeArray<int> validTargets;

		// Token: 0x04000B90 RID: 2960
		private NativeArray<RaycastCommand> characterCommands;

		// Token: 0x04000B91 RID: 2961
		private NativeArray<RaycastCommand> obstacleCommands;

		// Token: 0x04000B92 RID: 2962
		private NativeArray<RaycastHit> characterRaycastHits;

		// Token: 0x04000B93 RID: 2963
		private NativeArray<RaycastHit> obstacleRaycastHits;

		// Token: 0x04000B94 RID: 2964
		private JobHandle jobHandle;

		// Token: 0x04000B95 RID: 2965
		private readonly int numTargets;

		// Token: 0x04000B96 RID: 2966
		private readonly int numRequests;

		// Token: 0x04000B97 RID: 2967
		private const int MAX_HITS = 3;
	}
}
