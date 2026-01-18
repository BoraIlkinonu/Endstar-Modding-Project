using System;
using Endless.Gameplay.LuaInterfaces;
using Endless.Gameplay.Scripting;
using Endless.Props.ReferenceComponents;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020002B3 RID: 691
	public class JetpackItem : Item, IScriptInjector
	{
		// Token: 0x170002FB RID: 763
		// (get) Token: 0x06000F6D RID: 3949 RVA: 0x0005052A File Offset: 0x0004E72A
		protected override Item.VisualsInfo GroundVisualsInfo
		{
			get
			{
				return this.tempVisualsInfoGround;
			}
		}

		// Token: 0x170002FC RID: 764
		// (get) Token: 0x06000F6E RID: 3950 RVA: 0x00050532 File Offset: 0x0004E732
		protected override Item.VisualsInfo EquippedVisualsInfo
		{
			get
			{
				return this.tempVisualsInfoEqupped;
			}
		}

		// Token: 0x06000F6F RID: 3951 RVA: 0x0005053A File Offset: 0x0004E73A
		protected override void HandleVisualReferenceInitialized(ComponentReferences references)
		{
			this.jetpackVisualReferences = references as JetpackVisualReferences;
		}

		// Token: 0x06000F70 RID: 3952 RVA: 0x00050548 File Offset: 0x0004E748
		protected override void HandleInUseChanged(bool inUse)
		{
			if (this.jetpackVisualReferences)
			{
				if (inUse)
				{
					this.jetpackVisualReferences.InUseParticleSystem.RuntimeParticleSystem.Play();
					return;
				}
				this.jetpackVisualReferences.InUseParticleSystem.RuntimeParticleSystem.Stop();
			}
		}

		// Token: 0x170002FD RID: 765
		// (get) Token: 0x06000F71 RID: 3953 RVA: 0x00050585 File Offset: 0x0004E785
		public object LuaObject
		{
			get
			{
				if (this.luaInterface == null)
				{
					this.luaInterface = new Jetpack(this);
				}
				return this.luaInterface;
			}
		}

		// Token: 0x170002FE RID: 766
		// (get) Token: 0x06000F72 RID: 3954 RVA: 0x000505A1 File Offset: 0x0004E7A1
		Type IScriptInjector.LuaObjectType
		{
			get
			{
				return typeof(Jetpack);
			}
		}

		// Token: 0x06000F73 RID: 3955 RVA: 0x0004EE71 File Offset: 0x0004D071
		void IScriptInjector.ScriptInitialize(EndlessScriptComponent endlessScriptComponent)
		{
			this.scriptComponent = endlessScriptComponent;
		}

		// Token: 0x06000F75 RID: 3957 RVA: 0x000505B0 File Offset: 0x0004E7B0
		protected override void __initializeVariables()
		{
			base.__initializeVariables();
		}

		// Token: 0x06000F76 RID: 3958 RVA: 0x0004F016 File Offset: 0x0004D216
		protected override void __initializeRpcs()
		{
			base.__initializeRpcs();
		}

		// Token: 0x06000F77 RID: 3959 RVA: 0x000505C6 File Offset: 0x0004E7C6
		protected internal override string __getTypeName()
		{
			return "JetpackItem";
		}

		// Token: 0x04000D91 RID: 3473
		[SerializeField]
		private Item.VisualsInfo tempVisualsInfoGround;

		// Token: 0x04000D92 RID: 3474
		[SerializeField]
		private Item.VisualsInfo tempVisualsInfoEqupped;

		// Token: 0x04000D93 RID: 3475
		[SerializeField]
		[HideInInspector]
		private JetpackVisualReferences jetpackVisualReferences;

		// Token: 0x04000D94 RID: 3476
		private Jetpack luaInterface;
	}
}
