using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000221 RID: 545
	public class ValueSizeTest : MonoBehaviour
	{
		// Token: 0x06000B4F RID: 2895 RVA: 0x0003DE30 File Offset: 0x0003C030
		private void Start()
		{
			if (this.testMulti)
			{
				this.nativeParallelMultiHashMap = new NativeParallelMultiHashMap<int, int>(1000000, AllocatorManager.Persistent);
			}
			else
			{
				this.nativeParallelHashMap = new NativeParallelHashMap<int, UnsafeList<int>>(1000, AllocatorManager.Persistent);
				for (int i = 0; i < 1000; i++)
				{
					this.nativeParallelHashMap.Add(i, new UnsafeList<int>(1000, AllocatorManager.Persistent, NativeArrayOptions.UninitializedMemory));
				}
			}
			for (int j = 0; j < 1000000; j++)
			{
				int num = j / 1000;
				int num2 = j % 1000;
				if (this.testMulti)
				{
					this.nativeParallelMultiHashMap.Add(num, num2);
				}
				else
				{
					this.nativeParallelHashMap[num].Add(in num2);
				}
			}
		}

		// Token: 0x06000B50 RID: 2896 RVA: 0x0003DEEC File Offset: 0x0003C0EC
		private void OnDestroy()
		{
			if (this.testMulti)
			{
				this.nativeParallelMultiHashMap.Dispose();
				return;
			}
			for (int i = 0; i < 1000; i++)
			{
				this.nativeParallelHashMap[i].Dispose();
			}
			this.nativeParallelHashMap.Dispose();
		}

		// Token: 0x04000AAA RID: 2730
		private const int numElements = 1000000;

		// Token: 0x04000AAB RID: 2731
		private NativeParallelMultiHashMap<int, int> nativeParallelMultiHashMap;

		// Token: 0x04000AAC RID: 2732
		private NativeParallelHashMap<int, UnsafeList<int>> nativeParallelHashMap;

		// Token: 0x04000AAD RID: 2733
		[SerializeField]
		private bool testMulti;
	}
}
