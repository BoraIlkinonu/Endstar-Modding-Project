using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Endless.Gameplay.Jobs
{
	// Token: 0x0200042C RID: 1068
	[BurstCompile(FloatPrecision = FloatPrecision.Low, FloatMode = FloatMode.Fast)]
	public struct GetUniqueKeysJob : IJob
	{
		// Token: 0x06001A7C RID: 6780 RVA: 0x00079854 File Offset: 0x00077A54
		public void Execute()
		{
			ValueTuple<NativeArray<int>, int> uniqueKeyArray = this.RawMultiMap.GetUniqueKeyArray(AllocatorManager.Temp);
			for (int i = 0; i < uniqueKeyArray.Item2; i++)
			{
				int num = uniqueKeyArray.Item1[i];
				this.UniqueKeys.Add(in num);
			}
		}

		// Token: 0x04001530 RID: 5424
		[ReadOnly]
		public NativeParallelMultiHashMap<int, int> RawMultiMap;

		// Token: 0x04001531 RID: 5425
		[WriteOnly]
		public NativeList<int> UniqueKeys;
	}
}
