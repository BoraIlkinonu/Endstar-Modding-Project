using System;
using Endless.Gameplay.Scripting;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000340 RID: 832
	public class StaticProp : MonoBehaviour, IBaseType, IComponentBase
	{
		// Token: 0x17000419 RID: 1049
		// (get) Token: 0x06001412 RID: 5138 RVA: 0x00060DA0 File Offset: 0x0005EFA0
		public Context Context
		{
			get
			{
				Context context;
				if ((context = this.context) == null)
				{
					context = (this.context = new Context(this.WorldObject));
				}
				return context;
			}
		}

		// Token: 0x1700041A RID: 1050
		// (get) Token: 0x06001413 RID: 5139 RVA: 0x00060DCB File Offset: 0x0005EFCB
		// (set) Token: 0x06001414 RID: 5140 RVA: 0x00060DD3 File Offset: 0x0005EFD3
		public WorldObject WorldObject { get; private set; }

		// Token: 0x1700041B RID: 1051
		// (get) Token: 0x06001415 RID: 5141 RVA: 0x0001965C File Offset: 0x0001785C
		public ReferenceFilter Filter
		{
			get
			{
				return ReferenceFilter.None;
			}
		}

		// Token: 0x06001416 RID: 5142 RVA: 0x00060DDC File Offset: 0x0005EFDC
		public void PrefabInitialize(WorldObject worldObject)
		{
			this.WorldObject = worldObject;
		}

		// Token: 0x040010D2 RID: 4306
		private Context context;
	}
}
