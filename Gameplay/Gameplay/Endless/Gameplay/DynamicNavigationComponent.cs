using System;
using Endless.Gameplay.LuaInterfaces;
using Endless.Gameplay.Scripting;
using Endless.Props.ReferenceComponents;
using Endless.Shared;

namespace Endless.Gameplay
{
	// Token: 0x02000347 RID: 839
	public class DynamicNavigationComponent : EndlessBehaviour, IScriptInjector, IComponentBase
	{
		// Token: 0x17000439 RID: 1081
		// (get) Token: 0x0600148D RID: 5261 RVA: 0x000625E9 File Offset: 0x000607E9
		// (set) Token: 0x0600148E RID: 5262 RVA: 0x000625F1 File Offset: 0x000607F1
		private EndlessProp Prop { get; set; }

		// Token: 0x1700043A RID: 1082
		// (get) Token: 0x0600148F RID: 5263 RVA: 0x000625FA File Offset: 0x000607FA
		// (set) Token: 0x06001490 RID: 5264 RVA: 0x00062602 File Offset: 0x00060802
		public bool StartsBlocking { get; private set; }

		// Token: 0x1700043B RID: 1083
		// (get) Token: 0x06001491 RID: 5265 RVA: 0x00017586 File Offset: 0x00015786
		public NavType NavValue
		{
			get
			{
				return NavType.Dynamic;
			}
		}

		// Token: 0x06001492 RID: 5266 RVA: 0x0006260B File Offset: 0x0006080B
		internal void SetBlockingBehavior(Context instigator, bool isBlocking)
		{
			MonoBehaviourSingleton<NavGraph>.Instance.PropStateChanged(this.WorldObject, isBlocking);
		}

		// Token: 0x1700043C RID: 1084
		// (get) Token: 0x06001493 RID: 5267 RVA: 0x00062620 File Offset: 0x00060820
		public object LuaObject
		{
			get
			{
				Navigation navigation;
				if ((navigation = this.luaObject) == null)
				{
					navigation = (this.luaObject = new Navigation(this));
				}
				return navigation;
			}
		}

		// Token: 0x1700043D RID: 1085
		// (get) Token: 0x06001494 RID: 5268 RVA: 0x00062646 File Offset: 0x00060846
		public Type LuaObjectType
		{
			get
			{
				return typeof(Navigation);
			}
		}

		// Token: 0x1700043E RID: 1086
		// (get) Token: 0x06001495 RID: 5269 RVA: 0x00062652 File Offset: 0x00060852
		// (set) Token: 0x06001496 RID: 5270 RVA: 0x0006265A File Offset: 0x0006085A
		public WorldObject WorldObject { get; private set; }

		// Token: 0x06001497 RID: 5271 RVA: 0x00062663 File Offset: 0x00060863
		public void PrefabInitialize(WorldObject worldObject)
		{
			this.WorldObject = worldObject;
		}

		// Token: 0x06001498 RID: 5272 RVA: 0x0006266C File Offset: 0x0006086C
		public void ComponentInitialize(ReferenceBase referenceBase, EndlessProp endlessProp)
		{
			this.Prop = endlessProp;
		}

		// Token: 0x04001108 RID: 4360
		private Navigation luaObject;
	}
}
