using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Endless.Gameplay;

public class ValueSizeTest : MonoBehaviour
{
	private const int numElements = 1000000;

	private NativeParallelMultiHashMap<int, int> nativeParallelMultiHashMap;

	private NativeParallelHashMap<int, UnsafeList<int>> nativeParallelHashMap;

	[SerializeField]
	private bool testMulti;

	private void Start()
	{
		if (testMulti)
		{
			nativeParallelMultiHashMap = new NativeParallelMultiHashMap<int, int>(1000000, AllocatorManager.Persistent);
		}
		else
		{
			nativeParallelHashMap = new NativeParallelHashMap<int, UnsafeList<int>>(1000, AllocatorManager.Persistent);
			for (int i = 0; i < 1000; i++)
			{
				nativeParallelHashMap.Add(i, new UnsafeList<int>(1000, AllocatorManager.Persistent));
			}
		}
		for (int j = 0; j < 1000000; j++)
		{
			int key = j / 1000;
			int value = j % 1000;
			if (testMulti)
			{
				nativeParallelMultiHashMap.Add(key, value);
			}
			else
			{
				nativeParallelHashMap[key].Add(in value);
			}
		}
	}

	private void OnDestroy()
	{
		if (testMulti)
		{
			nativeParallelMultiHashMap.Dispose();
			return;
		}
		for (int i = 0; i < 1000; i++)
		{
			nativeParallelHashMap[i].Dispose();
		}
		nativeParallelHashMap.Dispose();
	}
}
