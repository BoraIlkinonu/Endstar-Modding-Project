using System.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Endless.Gameplay;

public static class JobUtilities
{
	public static IEnumerator WaitForJobToComplete(JobHandle handle, bool enforceTempCompletion = false)
	{
		if (enforceTempCompletion)
		{
			for (int frames = 0; frames < 3; frames++)
			{
				if (handle.IsCompleted)
				{
					handle.Complete();
					yield break;
				}
				yield return null;
			}
			handle.Complete();
		}
		else
		{
			yield return new WaitUntil(JobIsComplete);
			handle.Complete();
		}
		bool JobIsComplete()
		{
			return handle.IsCompleted;
		}
	}
}
