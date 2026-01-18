using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Endless.Gameplay.Jobs;

[BurstCompile(FloatPrecision = FloatPrecision.Low, FloatMode = FloatMode.Fast)]
public struct GetLargestKey : IJob
{
	[ReadOnly]
	public NativeParallelMultiHashMap<int, int> MultiHashMap;

	[WriteOnly]
	public NativeReference<int> LargestKey;

	public void Execute()
	{
		int num = 0;
		foreach (int item in MultiHashMap.GetKeyArray(AllocatorManager.Temp))
		{
			if (item > num)
			{
				num = item;
			}
		}
		LargestKey.Value = num;
	}
}
