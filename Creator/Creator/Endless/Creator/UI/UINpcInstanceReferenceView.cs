using System;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x02000229 RID: 553
	public class UINpcInstanceReferenceView : UIBaseInstanceReferenceView<NpcInstanceReference, UINpcInstanceReferenceView.Styles>
	{
		// Token: 0x1700011A RID: 282
		// (get) Token: 0x06000901 RID: 2305 RVA: 0x0002B0C1 File Offset: 0x000292C1
		// (set) Token: 0x06000902 RID: 2306 RVA: 0x0002B0C9 File Offset: 0x000292C9
		public override UINpcInstanceReferenceView.Styles Style { get; protected set; }

		// Token: 0x1700011B RID: 283
		// (get) Token: 0x06000903 RID: 2307 RVA: 0x00029A30 File Offset: 0x00027C30
		protected override ReferenceFilter ReferenceFilter
		{
			get
			{
				return ReferenceFilter.Npc;
			}
		}

		// Token: 0x06000904 RID: 2308 RVA: 0x0002B0D2 File Offset: 0x000292D2
		protected override Vector3 GetPinOffset(PropLibrary.RuntimePropInfo endlessDefinition)
		{
			return base.GetPinOffset(endlessDefinition) - Vector3.up * 1f;
		}

		// Token: 0x0200022A RID: 554
		public enum Styles
		{
			// Token: 0x04000791 RID: 1937
			Default,
			// Token: 0x04000792 RID: 1938
			NoneOrContext
		}
	}
}
