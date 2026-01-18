using System;
using Endless.Gameplay.Scripting;
using Endless.Props.ReferenceComponents;

namespace Endless.Gameplay
{
	// Token: 0x020002A5 RID: 677
	public interface IComponentBase
	{
		// Token: 0x170002D2 RID: 722
		// (get) Token: 0x06000EE5 RID: 3813
		WorldObject WorldObject { get; }

		// Token: 0x170002D3 RID: 723
		// (get) Token: 0x06000EE6 RID: 3814 RVA: 0x00002D9F File Offset: 0x00000F9F
		Type ComponentReferenceType
		{
			get
			{
				return null;
			}
		}

		// Token: 0x170002D4 RID: 724
		// (get) Token: 0x06000EE7 RID: 3815 RVA: 0x00017586 File Offset: 0x00015786
		ReferenceFilter Filter
		{
			get
			{
				return ReferenceFilter.NonStatic;
			}
		}

		// Token: 0x170002D5 RID: 725
		// (get) Token: 0x06000EE8 RID: 3816 RVA: 0x0001965C File Offset: 0x0001785C
		NavType NavValue
		{
			get
			{
				return NavType.Static;
			}
		}

		// Token: 0x06000EE9 RID: 3817 RVA: 0x00002DB0 File Offset: 0x00000FB0
		void ComponentInitialize(ReferenceBase referenceBase, EndlessProp endlessProp)
		{
		}

		// Token: 0x06000EEA RID: 3818
		void PrefabInitialize(WorldObject worldObject);
	}
}
