using System;
using Endless.Gameplay.Scripting;

namespace Endless.Gameplay
{
	// Token: 0x020002F8 RID: 760
	public class BasicProp : EndlessBehaviour, IBaseType, IComponentBase
	{
		// Token: 0x17000366 RID: 870
		// (get) Token: 0x0600113E RID: 4414 RVA: 0x000567DA File Offset: 0x000549DA
		// (set) Token: 0x0600113F RID: 4415 RVA: 0x000567E2 File Offset: 0x000549E2
		public WorldObject WorldObject { get; private set; }

		// Token: 0x17000367 RID: 871
		// (get) Token: 0x06001140 RID: 4416 RVA: 0x000567EC File Offset: 0x000549EC
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

		// Token: 0x06001141 RID: 4417 RVA: 0x00056817 File Offset: 0x00054A17
		public void PrefabInitialize(WorldObject worldObject)
		{
			this.WorldObject = worldObject;
		}

		// Token: 0x04000EDC RID: 3804
		private Context context;
	}
}
