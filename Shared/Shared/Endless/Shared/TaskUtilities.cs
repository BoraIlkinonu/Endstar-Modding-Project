using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Endless.Shared
{
	// Token: 0x020000A4 RID: 164
	public static class TaskUtilities
	{
		// Token: 0x06000488 RID: 1160 RVA: 0x00013F78 File Offset: 0x00012178
		public static async Task ProcessTasksWithSimultaneousCap(int totalTaskCount, int maxInFlight, Func<int, Task> getTaskByIndex, CancellationToken cancelToken, bool debug = false)
		{
			List<Task> activeTasks = new List<Task>();
			int index = 0;
			while (index < totalTaskCount || activeTasks.Count > 0)
			{
				for (int i = activeTasks.Count - 1; i >= 0; i--)
				{
					if (activeTasks[i].IsCompleted)
					{
						if (activeTasks[i].Exception != null)
						{
							Debug.LogException(activeTasks[i].Exception);
						}
						activeTasks.RemoveAt(i);
						if (debug)
						{
							Debug.Log("Finished task");
						}
					}
				}
				while (activeTasks.Count < maxInFlight && index < totalTaskCount)
				{
					if (cancelToken.IsCancellationRequested)
					{
						index = totalTaskCount;
						if (debug)
						{
							Debug.Log("Cancelled task group!");
							break;
						}
						break;
					}
					else
					{
						if (debug)
						{
							Debug.Log("Started new task");
						}
						Task task = getTaskByIndex(index);
						int num = index;
						index = num + 1;
						activeTasks.Add(task);
					}
				}
				if (activeTasks.Count > 0)
				{
					await Task.Yield();
				}
			}
			if (debug)
			{
				Debug.Log("Finished all tasks");
			}
		}

		// Token: 0x06000489 RID: 1161 RVA: 0x00013FDC File Offset: 0x000121DC
		public static async Task ProcessTasksWithSimultaneousCap<T>(int totalTaskCount, int maxInFlight, Func<int, Task<T>> getTaskByIndex, CancellationToken cancelToken)
		{
			List<Task<T>> activeTasks = new List<Task<T>>();
			int index = 0;
			while (index < totalTaskCount || activeTasks.Count > 0)
			{
				for (int i = activeTasks.Count - 1; i >= 0; i--)
				{
					if (activeTasks[i].IsCompleted)
					{
						activeTasks.RemoveAt(i);
					}
				}
				while (activeTasks.Count < maxInFlight && index < totalTaskCount)
				{
					if (cancelToken.IsCancellationRequested)
					{
						index = totalTaskCount;
						break;
					}
					Task<T> task = getTaskByIndex(index);
					int num = index;
					index = num + 1;
					activeTasks.Add(task);
				}
				if (activeTasks.Count > 0)
				{
					await Task.Yield();
				}
			}
		}
	}
}
