using System;
using Endless.Gameplay.LuaInterfaces;
using Endless.Props.ReferenceComponents;

namespace Endless.Gameplay
{
	// Token: 0x02000346 RID: 838
	public class DestroyableComponent : EndlessBehaviour, IComponentBase, IScriptInjector
	{
		// Token: 0x06001485 RID: 5253 RVA: 0x00002DB0 File Offset: 0x00000FB0
		public void Destroy()
		{
		}

		// Token: 0x17000435 RID: 1077
		// (get) Token: 0x06001486 RID: 5254 RVA: 0x000625A5 File Offset: 0x000607A5
		// (set) Token: 0x06001487 RID: 5255 RVA: 0x000625AD File Offset: 0x000607AD
		public WorldObject WorldObject { get; private set; }

		// Token: 0x17000436 RID: 1078
		// (get) Token: 0x06001488 RID: 5256 RVA: 0x000625B6 File Offset: 0x000607B6
		public Type ComponentReferenceType
		{
			get
			{
				return typeof(DestroyableReferences);
			}
		}

		// Token: 0x06001489 RID: 5257 RVA: 0x000625C2 File Offset: 0x000607C2
		public void PrefabInitialize(WorldObject worldObject)
		{
			this.WorldObject = worldObject;
		}

		// Token: 0x17000437 RID: 1079
		// (get) Token: 0x0600148A RID: 5258 RVA: 0x000625CB File Offset: 0x000607CB
		public object LuaObject
		{
			get
			{
				return this.luaInterface ?? new Destroyable(this);
			}
		}

		// Token: 0x17000438 RID: 1080
		// (get) Token: 0x0600148B RID: 5259 RVA: 0x000625DD File Offset: 0x000607DD
		public Type LuaObjectType
		{
			get
			{
				return typeof(Destroyable);
			}
		}

		// Token: 0x04001105 RID: 4357
		private Destroyable luaInterface;
	}
}
