using System;
using System.Collections.Generic;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x0200014D RID: 333
	public static class IUIChildLayoutableExtensions
	{
		// Token: 0x0600081E RID: 2078 RVA: 0x00022028 File Offset: 0x00020228
		public static void CollectLayoutGroupChildren(this InterfaceReference<IUIChildLayoutable>[] layoutables)
		{
			if (layoutables == null || layoutables.Length == 0)
			{
				return;
			}
			foreach (InterfaceReference<IUIChildLayoutable> interfaceReference in layoutables)
			{
				if (interfaceReference.IsValid)
				{
					interfaceReference.Interface.CollectChildLayoutItems();
				}
			}
		}

		// Token: 0x0600081F RID: 2079 RVA: 0x00022064 File Offset: 0x00020264
		public static void RequestLayout(this IEnumerable<IUIChildLayoutable> layoutables)
		{
			if (layoutables == null)
			{
				return;
			}
			if (!Application.isPlaying)
			{
				return;
			}
			foreach (IUIChildLayoutable iuichildLayoutable in layoutables)
			{
				MonoBehaviourSingleton<UILayoutManager>.Instance.RequestLayout(iuichildLayoutable);
			}
		}

		// Token: 0x06000820 RID: 2080 RVA: 0x000220BC File Offset: 0x000202BC
		public static void RequestLayout(this InterfaceReference<IUIChildLayoutable>[] layoutables)
		{
			if (layoutables == null || layoutables.Length == 0)
			{
				return;
			}
			if (!Application.isPlaying)
			{
				return;
			}
			foreach (InterfaceReference<IUIChildLayoutable> interfaceReference in layoutables)
			{
				if (interfaceReference.IsValid)
				{
					MonoBehaviourSingleton<UILayoutManager>.Instance.RequestLayout(interfaceReference.Interface);
				}
			}
		}

		// Token: 0x06000821 RID: 2081 RVA: 0x00022108 File Offset: 0x00020308
		public static void CalculateLayout(this InterfaceReference<IUIChildLayoutable>[] layoutables)
		{
			if (layoutables == null || layoutables.Length == 0)
			{
				return;
			}
			foreach (InterfaceReference<IUIChildLayoutable> interfaceReference in layoutables)
			{
				if (interfaceReference.IsValid)
				{
					interfaceReference.Interface.CalculateLayout();
				}
			}
		}

		// Token: 0x06000822 RID: 2082 RVA: 0x00022144 File Offset: 0x00020344
		public static void ApplyLayout(this InterfaceReference<IUIChildLayoutable>[] layoutables)
		{
			if (layoutables == null || layoutables.Length == 0)
			{
				return;
			}
			foreach (InterfaceReference<IUIChildLayoutable> interfaceReference in layoutables)
			{
				if (interfaceReference.IsValid)
				{
					interfaceReference.Interface.ApplyLayout();
				}
			}
		}

		// Token: 0x06000823 RID: 2083 RVA: 0x00022180 File Offset: 0x00020380
		public static void Layout(this InterfaceReference<IUIChildLayoutable>[] layoutables)
		{
			if (layoutables == null || layoutables.Length == 0)
			{
				return;
			}
			foreach (InterfaceReference<IUIChildLayoutable> interfaceReference in layoutables)
			{
				if (interfaceReference.IsValid)
				{
					interfaceReference.Interface.Layout();
				}
			}
		}

		// Token: 0x06000824 RID: 2084 RVA: 0x000221BC File Offset: 0x000203BC
		public static void CancelLayoutRequest(this InterfaceReference<IUIChildLayoutable>[] layoutables)
		{
			if (layoutables == null || layoutables.Length == 0)
			{
				return;
			}
			if (!Application.isPlaying)
			{
				return;
			}
			foreach (InterfaceReference<IUIChildLayoutable> interfaceReference in layoutables)
			{
				if (interfaceReference.IsValid)
				{
					MonoBehaviourSingleton<UILayoutManager>.Instance.CancelLayoutRequest(interfaceReference.Interface);
				}
			}
		}

		// Token: 0x06000825 RID: 2085 RVA: 0x00022208 File Offset: 0x00020408
		public static void AddChildLayoutItem(this InterfaceReference<IUIChildLayoutable>[] layoutables, RectTransform newChildLayoutItem, int? siblingIndex = null)
		{
			if (layoutables == null || layoutables.Length == 0)
			{
				return;
			}
			if (!Application.isPlaying)
			{
				return;
			}
			foreach (InterfaceReference<IUIChildLayoutable> interfaceReference in layoutables)
			{
				if (interfaceReference.IsValid)
				{
					interfaceReference.Interface.AddChildLayoutItem(newChildLayoutItem, siblingIndex);
				}
			}
		}

		// Token: 0x06000826 RID: 2086 RVA: 0x00022250 File Offset: 0x00020450
		public static void RemoveChildLayoutItem(this InterfaceReference<IUIChildLayoutable>[] layoutables, RectTransform childLayoutItem)
		{
			if (layoutables == null || layoutables.Length == 0)
			{
				return;
			}
			if (!Application.isPlaying)
			{
				return;
			}
			foreach (InterfaceReference<IUIChildLayoutable> interfaceReference in layoutables)
			{
				if (interfaceReference.IsValid)
				{
					interfaceReference.Interface.RemoveChildLayoutItem(childLayoutItem);
				}
			}
		}
	}
}
