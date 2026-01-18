using System;
using Endless.Gameplay.LuaInterfaces;
using Endless.Gameplay.Scripting;
using Endless.Props.ReferenceComponents;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020002AA RID: 682
	public class DashPackItem : Item, IScriptInjector
	{
		// Token: 0x170002E0 RID: 736
		// (get) Token: 0x06000F08 RID: 3848 RVA: 0x0004EEB3 File Offset: 0x0004D0B3
		protected override Item.VisualsInfo GroundVisualsInfo
		{
			get
			{
				return this.tempVisualsInfoGround;
			}
		}

		// Token: 0x170002E1 RID: 737
		// (get) Token: 0x06000F09 RID: 3849 RVA: 0x0004EEBB File Offset: 0x0004D0BB
		protected override Item.VisualsInfo EquippedVisualsInfo
		{
			get
			{
				return this.tempVisualsInfoEqupped;
			}
		}

		// Token: 0x06000F0A RID: 3850 RVA: 0x0004EEC3 File Offset: 0x0004D0C3
		protected override void HandleVisualReferenceInitialized(ComponentReferences references)
		{
			this.dashPackVisualReferences = references as DashPackVisualReferences;
		}

		// Token: 0x06000F0B RID: 3851 RVA: 0x0004EED4 File Offset: 0x0004D0D4
		protected override void HandleEquipmentUseStateChanged(UsableDefinition.UseState eus)
		{
			if (this.dashPackVisualReferences == null)
			{
				return;
			}
			if (eus != null)
			{
				GroundDashUsableDefinition groundDashUsableDefinition = this.inventoryUsableDefinition as GroundDashUsableDefinition;
				GroundDashUsableDefinition.GroundDashEquipmentUseState groundDashEquipmentUseState = (GroundDashUsableDefinition.GroundDashEquipmentUseState)eus;
				if (!this.startedEffect && groundDashEquipmentUseState.DashFrame >= groundDashUsableDefinition.DashDirectionDelayFramesCount)
				{
					this.startedEffect = true;
					int num = Mathf.RoundToInt(groundDashEquipmentUseState.DashAngleRelative * 4f);
					if (num == 1)
					{
						this.dashPackVisualReferences.LeftSideParticleSystem.RuntimeParticleSystem.Play();
						return;
					}
					if (num != 3)
					{
						this.dashPackVisualReferences.LeftSideParticleSystem.RuntimeParticleSystem.Play();
						this.dashPackVisualReferences.RightSideParticleSystem.RuntimeParticleSystem.Play();
						return;
					}
					this.dashPackVisualReferences.RightSideParticleSystem.RuntimeParticleSystem.Play();
					return;
				}
			}
			else
			{
				this.startedEffect = false;
				this.dashPackVisualReferences.LeftSideParticleSystem.RuntimeParticleSystem.Stop();
				this.dashPackVisualReferences.RightSideParticleSystem.RuntimeParticleSystem.Stop();
			}
		}

		// Token: 0x170002E2 RID: 738
		// (get) Token: 0x06000F0C RID: 3852 RVA: 0x0004EFD0 File Offset: 0x0004D1D0
		public object LuaObject
		{
			get
			{
				if (this.luaInterface == null)
				{
					this.luaInterface = new DashPack(this);
				}
				return this.luaInterface;
			}
		}

		// Token: 0x170002E3 RID: 739
		// (get) Token: 0x06000F0D RID: 3853 RVA: 0x0004EFEC File Offset: 0x0004D1EC
		Type IScriptInjector.LuaObjectType
		{
			get
			{
				return typeof(DashPack);
			}
		}

		// Token: 0x06000F0E RID: 3854 RVA: 0x0004EE71 File Offset: 0x0004D071
		void IScriptInjector.ScriptInitialize(EndlessScriptComponent endlessScriptComponent)
		{
			this.scriptComponent = endlessScriptComponent;
		}

		// Token: 0x06000F10 RID: 3856 RVA: 0x0004F000 File Offset: 0x0004D200
		protected override void __initializeVariables()
		{
			base.__initializeVariables();
		}

		// Token: 0x06000F11 RID: 3857 RVA: 0x0004F016 File Offset: 0x0004D216
		protected override void __initializeRpcs()
		{
			base.__initializeRpcs();
		}

		// Token: 0x06000F12 RID: 3858 RVA: 0x0004F020 File Offset: 0x0004D220
		protected internal override string __getTypeName()
		{
			return "DashPackItem";
		}

		// Token: 0x04000D5A RID: 3418
		[SerializeField]
		private Item.VisualsInfo tempVisualsInfoGround;

		// Token: 0x04000D5B RID: 3419
		[SerializeField]
		private Item.VisualsInfo tempVisualsInfoEqupped;

		// Token: 0x04000D5C RID: 3420
		[SerializeField]
		[HideInInspector]
		private DashPackVisualReferences dashPackVisualReferences;

		// Token: 0x04000D5D RID: 3421
		private bool startedEffect;

		// Token: 0x04000D5E RID: 3422
		private DashPack luaInterface;
	}
}
