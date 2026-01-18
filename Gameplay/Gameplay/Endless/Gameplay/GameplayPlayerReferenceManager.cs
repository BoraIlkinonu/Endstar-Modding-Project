using System;
using Endless.Gameplay.PlayerInventory;
using Endless.Gameplay.Scripting;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x0200026B RID: 619
	public class GameplayPlayerReferenceManager : PlayerReferenceManager
	{
		// Token: 0x1700024C RID: 588
		// (get) Token: 0x06000CC9 RID: 3273 RVA: 0x00044FF7 File Offset: 0x000431F7
		public override PlayerEquipmentManager PlayerEquipmentManager
		{
			get
			{
				return this.playerEquipmentManager;
			}
		}

		// Token: 0x1700024D RID: 589
		// (get) Token: 0x06000CCA RID: 3274 RVA: 0x00044FFF File Offset: 0x000431FF
		public override Inventory Inventory
		{
			get
			{
				return this.inventory;
			}
		}

		// Token: 0x1700024E RID: 590
		// (get) Token: 0x06000CCB RID: 3275 RVA: 0x00045007 File Offset: 0x00043207
		public override HealthComponent HealthComponent
		{
			get
			{
				return this.healthComponent;
			}
		}

		// Token: 0x1700024F RID: 591
		// (get) Token: 0x06000CCC RID: 3276 RVA: 0x0004500F File Offset: 0x0004320F
		public override PlayerDownedComponent PlayerDownedComponent
		{
			get
			{
				return this.playerDownedComponent;
			}
		}

		// Token: 0x17000250 RID: 592
		// (get) Token: 0x06000CCD RID: 3277 RVA: 0x00045017 File Offset: 0x00043217
		public override WorldCollidable WorldCollidable
		{
			get
			{
				return this.worldCollidable;
			}
		}

		// Token: 0x17000251 RID: 593
		// (get) Token: 0x06000CCE RID: 3278 RVA: 0x0004501F File Offset: 0x0004321F
		public override Transform LosProbe
		{
			get
			{
				return this.losProbe.transform;
			}
		}

		// Token: 0x17000252 RID: 594
		// (get) Token: 0x06000CCF RID: 3279 RVA: 0x0004502C File Offset: 0x0004322C
		public override DamageReaction DamageReaction
		{
			get
			{
				return this.damageReaction;
			}
		}

		// Token: 0x17000253 RID: 595
		// (get) Token: 0x06000CD0 RID: 3280 RVA: 0x00045034 File Offset: 0x00043234
		public Context PlayerContext
		{
			get
			{
				return this.LuaComponent.Context;
			}
		}

		// Token: 0x17000254 RID: 596
		// (get) Token: 0x06000CD1 RID: 3281 RVA: 0x00045041 File Offset: 0x00043241
		public PlayerLuaComponent LuaComponent
		{
			get
			{
				return this.luaComponent;
			}
		}

		// Token: 0x06000CD2 RID: 3282 RVA: 0x0004504C File Offset: 0x0004324C
		protected override void OnValidate()
		{
			base.OnValidate();
			if (this.playerDownedComponent == null)
			{
				this.playerDownedComponent = base.GetComponent<PlayerDownedComponent>();
			}
			if (this.healthComponent == null)
			{
				this.healthComponent = base.GetComponent<HealthComponent>();
			}
			if (this.playerEquipmentManager == null)
			{
				this.playerEquipmentManager = base.GetComponent<PlayerEquipmentManager>();
			}
			if (this.inventory == null)
			{
				this.inventory = base.GetComponent<Inventory>();
			}
			if (this.worldCollidable == null)
			{
				this.worldCollidable = base.GetComponent<WorldCollidable>();
			}
		}

		// Token: 0x06000CD4 RID: 3284 RVA: 0x000450EC File Offset: 0x000432EC
		protected override void __initializeVariables()
		{
			base.__initializeVariables();
		}

		// Token: 0x06000CD5 RID: 3285 RVA: 0x00045102 File Offset: 0x00043302
		protected override void __initializeRpcs()
		{
			base.__initializeRpcs();
		}

		// Token: 0x06000CD6 RID: 3286 RVA: 0x0004510C File Offset: 0x0004330C
		protected internal override string __getTypeName()
		{
			return "GameplayPlayerReferenceManager";
		}

		// Token: 0x04000BD9 RID: 3033
		[SerializeField]
		private PlayerEquipmentManager playerEquipmentManager;

		// Token: 0x04000BDA RID: 3034
		[SerializeField]
		private Inventory inventory;

		// Token: 0x04000BDB RID: 3035
		[SerializeField]
		private HealthComponent healthComponent;

		// Token: 0x04000BDC RID: 3036
		[SerializeField]
		private PlayerDownedComponent playerDownedComponent;

		// Token: 0x04000BDD RID: 3037
		[SerializeField]
		private WorldCollidable worldCollidable;

		// Token: 0x04000BDE RID: 3038
		[SerializeField]
		private GameObject losProbe;

		// Token: 0x04000BDF RID: 3039
		[SerializeField]
		private DamageReaction damageReaction;

		// Token: 0x04000BE0 RID: 3040
		[SerializeField]
		private PlayerLuaComponent luaComponent;
	}
}
