using System;
using Endless.Gameplay.LuaInterfaces;
using Endless.Gameplay.Scripting;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020002A9 RID: 681
	public class ConsumableHealingItem : StackableItem, IScriptInjector
	{
		// Token: 0x170002DB RID: 731
		// (get) Token: 0x06000EFB RID: 3835 RVA: 0x0004EE0D File Offset: 0x0004D00D
		protected override Item.VisualsInfo GroundVisualsInfo
		{
			get
			{
				return this.tempVisualsInfoGround;
			}
		}

		// Token: 0x170002DC RID: 732
		// (get) Token: 0x06000EFC RID: 3836 RVA: 0x0004EE15 File Offset: 0x0004D015
		protected override Item.VisualsInfo EquippedVisualsInfo
		{
			get
			{
				return this.tempVisualsInfoEqupped;
			}
		}

		// Token: 0x170002DD RID: 733
		// (get) Token: 0x06000EFD RID: 3837 RVA: 0x0004EE1D File Offset: 0x0004D01D
		// (set) Token: 0x06000EFE RID: 3838 RVA: 0x0004EE25 File Offset: 0x0004D025
		public int HealAmount { get; internal set; } = 4;

		// Token: 0x06000EFF RID: 3839 RVA: 0x0004EE2E File Offset: 0x0004D02E
		protected override object SaveData()
		{
			return this.HealAmount;
		}

		// Token: 0x06000F00 RID: 3840 RVA: 0x0004EE3B File Offset: 0x0004D03B
		protected override void LoadData(object data)
		{
			this.HealAmount = (int)data;
		}

		// Token: 0x170002DE RID: 734
		// (get) Token: 0x06000F01 RID: 3841 RVA: 0x0004EE49 File Offset: 0x0004D049
		public object LuaObject
		{
			get
			{
				if (this.item == null)
				{
					this.item = new ConsumableHealing(this);
				}
				return this.item;
			}
		}

		// Token: 0x170002DF RID: 735
		// (get) Token: 0x06000F02 RID: 3842 RVA: 0x0004EE65 File Offset: 0x0004D065
		Type IScriptInjector.LuaObjectType
		{
			get
			{
				return typeof(ConsumableHealing);
			}
		}

		// Token: 0x06000F03 RID: 3843 RVA: 0x0004EE71 File Offset: 0x0004D071
		void IScriptInjector.ScriptInitialize(EndlessScriptComponent endlessScriptComponent)
		{
			this.scriptComponent = endlessScriptComponent;
		}

		// Token: 0x06000F05 RID: 3845 RVA: 0x0004EE8C File Offset: 0x0004D08C
		protected override void __initializeVariables()
		{
			base.__initializeVariables();
		}

		// Token: 0x06000F06 RID: 3846 RVA: 0x0004EEA2 File Offset: 0x0004D0A2
		protected override void __initializeRpcs()
		{
			base.__initializeRpcs();
		}

		// Token: 0x06000F07 RID: 3847 RVA: 0x0004EEAC File Offset: 0x0004D0AC
		protected internal override string __getTypeName()
		{
			return "ConsumableHealingItem";
		}

		// Token: 0x04000D56 RID: 3414
		[SerializeField]
		private Item.VisualsInfo tempVisualsInfoGround;

		// Token: 0x04000D57 RID: 3415
		[SerializeField]
		private Item.VisualsInfo tempVisualsInfoEqupped;

		// Token: 0x04000D59 RID: 3417
		private ConsumableHealing item;
	}
}
