using System;
using System.Collections.Generic;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x0200014E RID: 334
	public static class IUILayoutableExtensions
	{
		// Token: 0x06000827 RID: 2087 RVA: 0x00022298 File Offset: 0x00020498
		public static void RequestLayout(this IEnumerable<IUILayoutable> layoutables)
		{
			if (layoutables == null)
			{
				return;
			}
			if (!Application.isPlaying)
			{
				return;
			}
			foreach (IUILayoutable iuilayoutable in layoutables)
			{
				MonoBehaviourSingleton<UILayoutManager>.Instance.RequestLayout(iuilayoutable);
			}
		}

		// Token: 0x06000828 RID: 2088 RVA: 0x000222F0 File Offset: 0x000204F0
		public static void RequestLayout(this InterfaceReference<IUILayoutable>[] layoutables)
		{
			if (layoutables == null || layoutables.Length == 0)
			{
				return;
			}
			if (!Application.isPlaying)
			{
				return;
			}
			foreach (InterfaceReference<IUILayoutable> interfaceReference in layoutables)
			{
				if (interfaceReference.IsValid)
				{
					MonoBehaviourSingleton<UILayoutManager>.Instance.RequestLayout(interfaceReference.Interface);
				}
			}
		}

		// Token: 0x06000829 RID: 2089 RVA: 0x0002233C File Offset: 0x0002053C
		public static void CalculateLayout(this InterfaceReference<IUILayoutable>[] layoutables)
		{
			if (layoutables == null || layoutables.Length == 0)
			{
				return;
			}
			foreach (InterfaceReference<IUILayoutable> interfaceReference in layoutables)
			{
				if (interfaceReference.IsValid)
				{
					interfaceReference.Interface.CalculateLayout();
				}
			}
		}

		// Token: 0x0600082A RID: 2090 RVA: 0x00022378 File Offset: 0x00020578
		public static void ApplyLayout(this InterfaceReference<IUILayoutable>[] layoutables)
		{
			if (layoutables == null || layoutables.Length == 0)
			{
				return;
			}
			foreach (InterfaceReference<IUILayoutable> interfaceReference in layoutables)
			{
				if (interfaceReference.IsValid)
				{
					interfaceReference.Interface.ApplyLayout();
				}
			}
		}

		// Token: 0x0600082B RID: 2091 RVA: 0x000223B4 File Offset: 0x000205B4
		public static void Layout(this InterfaceReference<IUILayoutable>[] layoutables)
		{
			if (layoutables == null || layoutables.Length == 0)
			{
				return;
			}
			foreach (InterfaceReference<IUILayoutable> interfaceReference in layoutables)
			{
				if (interfaceReference.IsValid)
				{
					interfaceReference.Interface.Layout();
				}
			}
		}

		// Token: 0x0600082C RID: 2092 RVA: 0x000223F0 File Offset: 0x000205F0
		public static void CancelLayoutRequest(this InterfaceReference<IUILayoutable>[] layoutables)
		{
			if (layoutables == null || layoutables.Length == 0)
			{
				return;
			}
			if (!Application.isPlaying)
			{
				return;
			}
			foreach (InterfaceReference<IUILayoutable> interfaceReference in layoutables)
			{
				if (interfaceReference.IsValid)
				{
					MonoBehaviourSingleton<UILayoutManager>.Instance.CancelLayoutRequest(interfaceReference.Interface);
				}
			}
		}
	}
}
