using System;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Collections;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x0200021F RID: 543
	public class AccessSpeedTest : MonoBehaviour
	{
		// Token: 0x06000B47 RID: 2887 RVA: 0x0003DA2C File Offset: 0x0003BC2C
		private void Start()
		{
			this.managedDict = new Dictionary<int, WalkableOctantData>(1000000);
			this.nativeHashMap = new NativeParallelHashMap<int, WalkableOctantData>(1000000, AllocatorManager.Persistent);
			for (int i = 0; i < 1000000; i++)
			{
				this.managedDict.Add(i, new WalkableOctantData
				{
					IslandKey = i,
					AreaKey = i,
					SectionKey = i,
					ZoneKey = i
				});
				this.nativeHashMap.TryAdd(i, new WalkableOctantData
				{
					IslandKey = i,
					AreaKey = i,
					SectionKey = i,
					ZoneKey = i
				});
			}
			this.TestAccessSpeed();
		}

		// Token: 0x06000B48 RID: 2888 RVA: 0x0003DAE0 File Offset: 0x0003BCE0
		private void TestAccessSpeed()
		{
			long num = 0L;
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			for (int i = 0; i < 1000000; i++)
			{
				num += (long)this.managedDict[i].AreaKey;
			}
			stopwatch.Stop();
			global::UnityEngine.Debug.Log("Dictionary access time: " + stopwatch.ElapsedMilliseconds.ToString() + " ms");
			global::UnityEngine.Debug.Log("Dictionary sum (to avoid optimizations): " + num.ToString());
			num = 0L;
			stopwatch.Reset();
			stopwatch.Start();
			for (int j = 0; j < 1000000; j++)
			{
				num += (long)this.nativeHashMap[j].AreaKey;
			}
			stopwatch.Stop();
			global::UnityEngine.Debug.Log("NativeParallelHashMap access time: " + stopwatch.ElapsedMilliseconds.ToString() + " ms");
			global::UnityEngine.Debug.Log("NativeParallelHashMap sum: " + num.ToString());
		}

		// Token: 0x06000B49 RID: 2889 RVA: 0x0003DBD6 File Offset: 0x0003BDD6
		private void OnDestroy()
		{
			this.nativeHashMap.Dispose();
		}

		// Token: 0x04000AA5 RID: 2725
		private const int numElements = 1000000;

		// Token: 0x04000AA6 RID: 2726
		private Dictionary<int, WalkableOctantData> managedDict;

		// Token: 0x04000AA7 RID: 2727
		private NativeParallelHashMap<int, WalkableOctantData> nativeHashMap;
	}
}
