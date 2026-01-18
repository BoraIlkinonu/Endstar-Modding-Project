using System;
using System.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000228 RID: 552
	public static class JobUtilities
	{
		// Token: 0x06000B71 RID: 2929 RVA: 0x0003EE03 File Offset: 0x0003D003
		public static IEnumerator WaitForJobToComplete(JobHandle handle, bool enforceTempCompletion = false)
		{
			JobUtilities.<>c__DisplayClass0_0 CS$<>8__locals1 = new JobUtilities.<>c__DisplayClass0_0();
			CS$<>8__locals1.handle = handle;
			if (enforceTempCompletion)
			{
				int num;
				for (int frames = 0; frames < 3; frames = num + 1)
				{
					if (CS$<>8__locals1.handle.IsCompleted)
					{
						CS$<>8__locals1.handle.Complete();
						yield break;
					}
					yield return null;
					num = frames;
				}
				CS$<>8__locals1.handle.Complete();
			}
			else
			{
				yield return new WaitUntil(new Func<bool>(CS$<>8__locals1.<WaitForJobToComplete>g__JobIsComplete|0));
				CS$<>8__locals1.handle.Complete();
			}
			yield break;
		}
	}
}
