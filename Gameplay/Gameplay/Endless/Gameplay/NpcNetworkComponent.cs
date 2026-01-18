using System;

namespace Endless.Gameplay
{
	// Token: 0x02000141 RID: 321
	public abstract class NpcNetworkComponent : EndlessNetworkBehaviour
	{
		// Token: 0x17000167 RID: 359
		// (get) Token: 0x06000784 RID: 1924 RVA: 0x0002367C File Offset: 0x0002187C
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

		// Token: 0x17000168 RID: 360
		// (get) Token: 0x06000785 RID: 1925 RVA: 0x000236A4 File Offset: 0x000218A4
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

		// Token: 0x06000787 RID: 1927 RVA: 0x000236D4 File Offset: 0x000218D4
		protected override void __initializeVariables()
		{
			base.__initializeVariables();
		}

		// Token: 0x06000788 RID: 1928 RVA: 0x0001E813 File Offset: 0x0001CA13
		protected override void __initializeRpcs()
		{
			base.__initializeRpcs();
		}

		// Token: 0x06000789 RID: 1929 RVA: 0x000236EA File Offset: 0x000218EA
		protected internal override string __getTypeName()
		{
			return "NpcNetworkComponent";
		}

		// Token: 0x04000601 RID: 1537
		private NpcEntity entity;

		// Token: 0x04000602 RID: 1538
		private Components components;
	}
}
