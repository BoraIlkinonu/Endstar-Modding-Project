using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Endless.Gameplay.Jobs;

[BurstCompile(FloatPrecision = FloatPrecision.Low, FloatMode = FloatMode.Fast)]
public struct GetUniqueKeysJob : IJob
{
	[ReadOnly]
	public NativeParallelMultiHashMap<int, int> RawMultiMap;

	[WriteOnly]
	public NativeList<int> UniqueKeys;

	public void Execute()
	{
		(NativeArray<int>, int) uniqueKeyArray = RawMultiMap.GetUniqueKeyArray(AllocatorManager.Temp);
		for (int i = 0; i < uniqueKeyArray.Item2; i++)
		{
			UniqueKeys.Add(uniqueKeyArray.Item1[i]);
		}
	}
}
