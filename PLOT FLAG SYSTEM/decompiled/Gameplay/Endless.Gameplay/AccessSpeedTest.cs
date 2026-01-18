using System.Collections.Generic;
using System.Diagnostics;
using Unity.Collections;
using UnityEngine;

namespace Endless.Gameplay;

public class AccessSpeedTest : MonoBehaviour
{
	private const int numElements = 1000000;

	private Dictionary<int, WalkableOctantData> managedDict;

	private NativeParallelHashMap<int, WalkableOctantData> nativeHashMap;

	private void Start()
	{
		managedDict = new Dictionary<int, WalkableOctantData>(1000000);
		nativeHashMap = new NativeParallelHashMap<int, WalkableOctantData>(1000000, AllocatorManager.Persistent);
		for (int i = 0; i < 1000000; i++)
		{
			managedDict.Add(i, new WalkableOctantData
			{
				IslandKey = i,
				AreaKey = i,
				SectionKey = i,
				ZoneKey = i
			});
			nativeHashMap.TryAdd(i, new WalkableOctantData
			{
				IslandKey = i,
				AreaKey = i,
				SectionKey = i,
				ZoneKey = i
			});
		}
		TestAccessSpeed();
	}

	private void TestAccessSpeed()
	{
		long num = 0L;
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		for (int i = 0; i < 1000000; i++)
		{
			num += managedDict[i].AreaKey;
		}
		stopwatch.Stop();
		UnityEngine.Debug.Log("Dictionary access time: " + stopwatch.ElapsedMilliseconds + " ms");
		UnityEngine.Debug.Log("Dictionary sum (to avoid optimizations): " + num);
		num = 0L;
		stopwatch.Reset();
		stopwatch.Start();
		for (int j = 0; j < 1000000; j++)
		{
			num += nativeHashMap[j].AreaKey;
		}
		stopwatch.Stop();
		UnityEngine.Debug.Log("NativeParallelHashMap access time: " + stopwatch.ElapsedMilliseconds + " ms");
		UnityEngine.Debug.Log("NativeParallelHashMap sum: " + num);
	}

	private void OnDestroy()
	{
		nativeHashMap.Dispose();
	}
}
