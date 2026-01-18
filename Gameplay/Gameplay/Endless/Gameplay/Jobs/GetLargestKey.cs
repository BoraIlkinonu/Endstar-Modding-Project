using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Endless.Gameplay.Jobs
{
	// Token: 0x0200042D RID: 1069
	[BurstCompile(FloatPrecision = FloatPrecision.Low, FloatMode = FloatMode.Fast)]
	public struct GetLargestKey : IJob
	{
		// Token: 0x06001A7D RID: 6781 RVA: 0x000798A0 File Offset: 0x00077AA0
		public void Execute()
		{
			int num = 0;
			foreach (int num2 in this.MultiHashMap.GetKeyArray(AllocatorManager.Temp))
			{
				if (num2 > num)
				{
					num = num2;
				}
			}
			this.LargestKey.Value = num;
		}

		// Token: 0x04001532 RID: 5426
		[ReadOnly]
		public NativeParallelMultiHashMap<int, int> MultiHashMap;

		// Token: 0x04001533 RID: 5427
		[WriteOnly]
		public NativeReference<int> LargestKey;
	}
}
