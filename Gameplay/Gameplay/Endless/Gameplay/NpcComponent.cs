using System;

namespace Endless.Gameplay
{
	// Token: 0x02000140 RID: 320
	public class NpcComponent : EndlessBehaviour
	{
		// Token: 0x17000165 RID: 357
		// (get) Token: 0x06000781 RID: 1921 RVA: 0x0002362C File Offset: 0x0002182C
		protected NpcEntity NpcEntity
		{
			get
			{
				NpcEntity npcEntity;
				if ((npcEntity = this.entity) == null)
				{
					npcEntity = (this.entity = base.GetComponentInParent<NpcEntity>());
				}
				return npcEntity;
			}
		}

		// Token: 0x17000166 RID: 358
		// (get) Token: 0x06000782 RID: 1922 RVA: 0x00023654 File Offset: 0x00021854
		protected Components Components
		{
			get
			{
				Components components;
				if ((components = this.components) == null)
				{
					components = (this.components = base.GetComponentInParent<Components>());
				}
				return components;
			}
		}

		// Token: 0x040005FF RID: 1535
		private NpcEntity entity;

		// Token: 0x04000600 RID: 1536
		private Components components;
	}
}
