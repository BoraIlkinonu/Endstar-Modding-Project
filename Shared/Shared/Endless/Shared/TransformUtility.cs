using System;
using System.Collections.Generic;
using UnityEngine;

namespace Endless.Shared
{
	// Token: 0x020000A7 RID: 167
	public static class TransformUtility
	{
		// Token: 0x0600048E RID: 1166 RVA: 0x0001444C File Offset: 0x0001264C
		public static GameObject FindChildWithName(Transform transform, string targetName, bool recursive = true)
		{
			foreach (object obj in transform)
			{
				Transform transform2 = (Transform)obj;
				if (transform2.name == targetName)
				{
					return transform2.gameObject;
				}
				if (recursive)
				{
					GameObject gameObject = TransformUtility.FindChildWithName(transform2, targetName, true);
					if (gameObject)
					{
						return gameObject;
					}
				}
			}
			return null;
		}

		// Token: 0x0600048F RID: 1167 RVA: 0x000144D0 File Offset: 0x000126D0
		public static GameObject BreadthFirstSearchForName(Transform transform, string targetName)
		{
			Queue<Transform> queue = new Queue<Transform>();
			queue.Enqueue(transform);
			while (queue.Count > 0)
			{
				Transform transform2 = queue.Dequeue();
				if (transform2.name == targetName)
				{
					return transform2.gameObject;
				}
				foreach (object obj in transform2)
				{
					Transform transform3 = (Transform)obj;
					queue.Enqueue(transform3);
				}
			}
			return null;
		}
	}
}
