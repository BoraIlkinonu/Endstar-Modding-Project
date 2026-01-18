using System;
using Endless.Gameplay.LuaInterfaces;
using Endless.Gameplay.Scripting;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020002B4 RID: 692
	public class MeleeWeaponItem : Item, IScriptInjector
	{
		// Token: 0x170002FF RID: 767
		// (get) Token: 0x06000F78 RID: 3960 RVA: 0x000505CD File Offset: 0x0004E7CD
		protected override Item.VisualsInfo GroundVisualsInfo
		{
			get
			{
				return this.tempVisualsInfoGround;
			}
		}

		// Token: 0x17000300 RID: 768
		// (get) Token: 0x06000F79 RID: 3961 RVA: 0x000505D5 File Offset: 0x0004E7D5
		protected override Item.VisualsInfo EquippedVisualsInfo
		{
			get
			{
				return this.tempVisualsInfoEqupped;
			}
		}

		// Token: 0x17000301 RID: 769
		// (get) Token: 0x06000F7A RID: 3962 RVA: 0x00002D9F File Offset: 0x00000F9F
		public override Type ComponentReferenceType
		{
			get
			{
				return null;
			}
		}

		// Token: 0x17000302 RID: 770
		// (get) Token: 0x06000F7B RID: 3963 RVA: 0x000505DD File Offset: 0x0004E7DD
		// (set) Token: 0x06000F7C RID: 3964 RVA: 0x000505E5 File Offset: 0x0004E7E5
		public int DamageOnHit { get; internal set; } = -1;

		// Token: 0x06000F7D RID: 3965 RVA: 0x000505EE File Offset: 0x0004E7EE
		public void Hit(HittableComponent hittableComponent)
		{
			if (base.IsServer)
			{
				this.scriptComponent.ExecuteFunction("OnHit", new object[] { hittableComponent.WorldObject.Context });
			}
		}

		// Token: 0x06000F7E RID: 3966 RVA: 0x0005061D File Offset: 0x0004E81D
		protected override object SaveData()
		{
			return this.DamageOnHit;
		}

		// Token: 0x06000F7F RID: 3967 RVA: 0x0005062A File Offset: 0x0004E82A
		protected override void LoadData(object data)
		{
			this.DamageOnHit = (int)data;
		}

		// Token: 0x17000303 RID: 771
		// (get) Token: 0x06000F80 RID: 3968 RVA: 0x00050638 File Offset: 0x0004E838
		public object LuaObject
		{
			get
			{
				if (this.luaInterface == null)
				{
					this.luaInterface = new MeleeWeapon(this);
				}
				return this.luaInterface;
			}
		}

		// Token: 0x17000304 RID: 772
		// (get) Token: 0x06000F81 RID: 3969 RVA: 0x00050654 File Offset: 0x0004E854
		Type IScriptInjector.LuaObjectType
		{
			get
			{
				return typeof(MeleeWeapon);
			}
		}

		// Token: 0x06000F82 RID: 3970 RVA: 0x0004EE71 File Offset: 0x0004D071
		void IScriptInjector.ScriptInitialize(EndlessScriptComponent endlessScriptComponent)
		{
			this.scriptComponent = endlessScriptComponent;
		}

		// Token: 0x06000F84 RID: 3972 RVA: 0x00050670 File Offset: 0x0004E870
		protected override void __initializeVariables()
		{
			base.__initializeVariables();
		}

		// Token: 0x06000F85 RID: 3973 RVA: 0x0004F016 File Offset: 0x0004D216
		protected override void __initializeRpcs()
		{
			base.__initializeRpcs();
		}

		// Token: 0x06000F86 RID: 3974 RVA: 0x00050686 File Offset: 0x0004E886
		protected internal override string __getTypeName()
		{
			return "MeleeWeaponItem";
		}

		// Token: 0x04000D95 RID: 3477
		[SerializeField]
		private Item.VisualsInfo tempVisualsInfoGround;

		// Token: 0x04000D96 RID: 3478
		[SerializeField]
		private Item.VisualsInfo tempVisualsInfoEqupped;

		// Token: 0x04000D98 RID: 3480
		private MeleeWeapon luaInterface;
	}
}
