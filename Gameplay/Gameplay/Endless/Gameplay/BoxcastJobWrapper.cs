using System;
using System.Collections.Generic;
using Endless.Shared;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x0200025C RID: 604
	public class BoxcastJobWrapper : IPerceptionJob
	{
		// Token: 0x06000C85 RID: 3205 RVA: 0x00043650 File Offset: 0x00041850
		public BoxcastJobWrapper(List<PerceptionRequest> perceptionRequests)
		{
			this.requests = new List<PerceptionRequest>(perceptionRequests);
			this.boxcastCommands = new NativeArray<BoxcastCommand>(this.requests.Count, Allocator.TempJob, NativeArrayOptions.ClearMemory);
			this.boxcastHits = new NativeArray<RaycastHit>(this.requests.Count * 5, Allocator.TempJob, NativeArrayOptions.ClearMemory);
			this.overlapCommands = new NativeArray<OverlapBoxCommand>(this.requests.Count, Allocator.TempJob, NativeArrayOptions.ClearMemory);
			this.overlapHits = new NativeArray<ColliderHit>(this.requests.Count * 5, Allocator.TempJob, NativeArrayOptions.ClearMemory);
			for (int i = 0; i < this.requests.Count; i++)
			{
				PerceptionRequest perceptionRequest = this.requests[i];
				QueryParameters queryParameters = new QueryParameters
				{
					layerMask = (perceptionRequest.UseXray ? MonoBehaviourSingleton<PerceptionManager>.Instance.CharacterMask : (MonoBehaviourSingleton<PerceptionManager>.Instance.CharacterMask | MonoBehaviourSingleton<PerceptionManager>.Instance.ObstacleMask))
				};
				Vector3 vector = new Vector3(perceptionRequest.HorizontalValue, perceptionRequest.VerticalValue, perceptionRequest.HorizontalValue) * 0.5f;
				Quaternion quaternion = Quaternion.LookRotation(perceptionRequest.LookVector);
				Vector3 vector2 = perceptionRequest.Position + quaternion * Vector3.forward * perceptionRequest.HorizontalValue;
				BoxcastCommand boxcastCommand = new BoxcastCommand(vector2, vector, quaternion, perceptionRequest.LookVector, queryParameters, perceptionRequest.MaxDistance);
				OverlapBoxCommand overlapBoxCommand = new OverlapBoxCommand(vector2, new Vector3(0.5f, 0.5f, 0.2f), quaternion, queryParameters);
				this.boxcastCommands[i] = boxcastCommand;
				this.overlapCommands[i] = overlapBoxCommand;
			}
			int num = Mathf.Max(5, Mathf.CeilToInt((float)this.boxcastCommands.Length / (float)JobsUtility.JobWorkerCount));
			this.overlapHandle = OverlapBoxCommand.ScheduleBatch(this.overlapCommands, this.overlapHits, num, 5, default(JobHandle));
			this.boxcastHandle = BoxcastCommand.ScheduleBatch(this.boxcastCommands, this.boxcastHits, num, 5, default(JobHandle));
		}

		// Token: 0x1700023F RID: 575
		// (get) Token: 0x06000C86 RID: 3206 RVA: 0x00043855 File Offset: 0x00041A55
		public bool IsComplete
		{
			get
			{
				return this.boxcastHandle.IsCompleted && this.overlapHandle.IsCompleted;
			}
		}

		// Token: 0x06000C87 RID: 3207 RVA: 0x00043874 File Offset: 0x00041A74
		public void RespondToRequests()
		{
			this.boxcastHandle.Complete();
			this.overlapHandle.Complete();
			if (!MonoBehaviourSingleton<GameplayManager>.Instance.IsPlaying)
			{
				this.DisposeCollections();
				return;
			}
			for (int i = 0; i < this.requests.Count; i++)
			{
				PerceptionRequest perceptionRequest = this.requests[i];
				List<PerceptionResult> perceptionResults = perceptionRequest.PerceptionResults;
				perceptionResults.Clear();
				NativeArray<RaycastHit> subArray = this.boxcastHits.GetSubArray(i * 5, 5);
				NativeSlice<ColliderHit> nativeSlice = this.overlapHits.GetSubArray(i * 5, 5);
				for (int j = 0; j < 5; j++)
				{
					HittableComponent hittableFromMap = MonoBehaviourSingleton<HittableMap>.Instance.GetHittableFromMap(subArray[j].colliderInstanceID);
					if (!hittableFromMap)
					{
						break;
					}
					PerceptionResult perceptionResult = new PerceptionResult
					{
						HittableComponent = hittableFromMap,
						Awareness = 100f
					};
					perceptionResults.Add(perceptionResult);
				}
				for (int k = 0; k < 5; k++)
				{
					HittableComponent hittableFromMap2 = MonoBehaviourSingleton<HittableMap>.Instance.GetHittableFromMap(nativeSlice[k].instanceID);
					if (hittableFromMap2)
					{
						PerceptionResult perceptionResult2 = new PerceptionResult
						{
							HittableComponent = hittableFromMap2,
							Awareness = 100f
						};
						perceptionResults.Add(perceptionResult2);
					}
				}
				perceptionRequest.PerceptionUpdatedCallback();
			}
			this.DisposeCollections();
		}

		// Token: 0x06000C88 RID: 3208 RVA: 0x000439D8 File Offset: 0x00041BD8
		private void DisposeCollections()
		{
			if (!this.boxcastHandle.IsCompleted)
			{
				this.boxcastHandle.Complete();
			}
			if (!this.overlapHandle.IsCompleted)
			{
				this.overlapHandle.Complete();
			}
			this.boxcastCommands.Dispose();
			this.boxcastHits.Dispose();
			this.overlapCommands.Dispose();
			this.overlapHits.Dispose();
		}

		// Token: 0x04000B7C RID: 2940
		private const int MAX_HITS = 5;

		// Token: 0x04000B7D RID: 2941
		private readonly List<PerceptionRequest> requests;

		// Token: 0x04000B7E RID: 2942
		private NativeArray<BoxcastCommand> boxcastCommands;

		// Token: 0x04000B7F RID: 2943
		private NativeArray<RaycastHit> boxcastHits;

		// Token: 0x04000B80 RID: 2944
		private NativeArray<OverlapBoxCommand> overlapCommands;

		// Token: 0x04000B81 RID: 2945
		private NativeArray<ColliderHit> overlapHits;

		// Token: 0x04000B82 RID: 2946
		private JobHandle boxcastHandle;

		// Token: 0x04000B83 RID: 2947
		private JobHandle overlapHandle;
	}
}
