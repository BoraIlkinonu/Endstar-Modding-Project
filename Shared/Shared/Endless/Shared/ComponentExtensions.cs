using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Endless.Shared
{
	// Token: 0x0200005A RID: 90
	public static class ComponentExtensions
	{
		// Token: 0x060002D2 RID: 722 RVA: 0x0000E218 File Offset: 0x0000C418
		public static IEnumerable<T> GetComponentsInChildrenAndExcludeSelf<T>(this Component component, bool includeInactive) where T : Component
		{
			return from c in component.GetComponentsInChildren<T>(includeInactive)
				where c != component
				select c;
		}

		// Token: 0x060002D3 RID: 723 RVA: 0x0000E250 File Offset: 0x0000C450
		public static T GetTopMostComponentInParent<T>(this Component child) where T : Component
		{
			T t = default(T);
			Transform transform = child.transform.parent;
			while (transform != null)
			{
				T component = transform.GetComponent<T>();
				if (component != null)
				{
					t = component;
				}
				transform = transform.parent;
			}
			return t;
		}

		// Token: 0x060002D4 RID: 724 RVA: 0x0000E29C File Offset: 0x0000C49C
		public static string GetFullHierarchy(this Component component)
		{
			Stack<string> stack = new Stack<string>();
			Transform transform = component.transform;
			while (transform != null)
			{
				stack.Push(transform.name);
				transform = transform.parent;
			}
			return string.Join('/', stack.ToArray());
		}
	}
}
