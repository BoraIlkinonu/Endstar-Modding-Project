using System;
using Endless.Gameplay.LuaInterfaces;
using Endless.Gameplay.Scripting;
using Endless.ParticleSystems.Components;
using Endless.Props.ReferenceComponents;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000321 RID: 801
	public class InstantPickup : InstantPickupBase, IBaseType, IComponentBase, IScriptInjector
	{
		// Token: 0x06001287 RID: 4743 RVA: 0x0005BAA4 File Offset: 0x00059CA4
		protected override void ShowCollectedFX()
		{
			if (this.collectedParticleSystem && this.collectedParticleSystem.RuntimeParticleSystem)
			{
				this.collectedParticleSystem.Spawn(false);
			}
		}

		// Token: 0x06001288 RID: 4744 RVA: 0x0005BAD4 File Offset: 0x00059CD4
		protected override bool ExternalAttemptPickup(Context context)
		{
			bool flag;
			return this.scriptComponent.TryExecuteFunction<bool>("AttemptPickup", out flag, new object[] { context }) && flag;
		}

		// Token: 0x06001289 RID: 4745 RVA: 0x0005BB02 File Offset: 0x00059D02
		protected override void SetActive(bool active)
		{
			this.references.gameObject.SetActive(active);
			this.references.WorldTriggerArea.SetCollidersEnabled(active);
		}

		// Token: 0x170003AD RID: 941
		// (get) Token: 0x0600128A RID: 4746 RVA: 0x0005BB26 File Offset: 0x00059D26
		// (set) Token: 0x0600128B RID: 4747 RVA: 0x0005BB2E File Offset: 0x00059D2E
		public WorldObject WorldObject { get; private set; }

		// Token: 0x170003AE RID: 942
		// (get) Token: 0x0600128C RID: 4748 RVA: 0x0005BB38 File Offset: 0x00059D38
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

		// Token: 0x170003AF RID: 943
		// (get) Token: 0x0600128D RID: 4749 RVA: 0x0005BB63 File Offset: 0x00059D63
		public Type ComponentReferenceType
		{
			get
			{
				return typeof(InstantPickupReferences);
			}
		}

		// Token: 0x170003B0 RID: 944
		// (get) Token: 0x0600128E RID: 4750 RVA: 0x0001BD04 File Offset: 0x00019F04
		public NavType NavValue
		{
			get
			{
				return NavType.Intangible;
			}
		}

		// Token: 0x0600128F RID: 4751 RVA: 0x0005BB6F File Offset: 0x00059D6F
		public void PrefabInitialize(WorldObject worldObject)
		{
			this.WorldObject = worldObject;
		}

		// Token: 0x06001290 RID: 4752 RVA: 0x0005BB78 File Offset: 0x00059D78
		public void ComponentInitialize(ReferenceBase referenceBase, EndlessProp endlessProp)
		{
			this.references = referenceBase as InstantPickupReferences;
			this.collectedParticleSystem = this.references.CollectedParticleSystem;
			Collider[] cachedColliders = this.references.WorldTriggerArea.CachedColliders;
			for (int i = 0; i < cachedColliders.Length; i++)
			{
				cachedColliders[i].gameObject.AddComponent<WorldTriggerCollider>().Initialize(this.worldTrigger);
			}
		}

		// Token: 0x170003B1 RID: 945
		// (get) Token: 0x06001291 RID: 4753 RVA: 0x0005BBDC File Offset: 0x00059DDC
		public object LuaObject
		{
			get
			{
				InstantPickup instantPickup;
				if ((instantPickup = this.luaInterface) == null)
				{
					instantPickup = (this.luaInterface = new InstantPickup(this));
				}
				return instantPickup;
			}
		}

		// Token: 0x170003B2 RID: 946
		// (get) Token: 0x06001292 RID: 4754 RVA: 0x0005BC02 File Offset: 0x00059E02
		public Type LuaObjectType
		{
			get
			{
				return typeof(InstantPickup);
			}
		}

		// Token: 0x06001293 RID: 4755 RVA: 0x0005BC0E File Offset: 0x00059E0E
		public void ScriptInitialize(EndlessScriptComponent endlessScriptComponent)
		{
			this.scriptComponent = endlessScriptComponent;
		}

		// Token: 0x06001295 RID: 4757 RVA: 0x0005BC20 File Offset: 0x00059E20
		protected override void __initializeVariables()
		{
			base.__initializeVariables();
		}

		// Token: 0x06001296 RID: 4758 RVA: 0x0005BC36 File Offset: 0x00059E36
		protected override void __initializeRpcs()
		{
			base.__initializeRpcs();
		}

		// Token: 0x06001297 RID: 4759 RVA: 0x0005BC40 File Offset: 0x00059E40
		protected internal override string __getTypeName()
		{
			return "InstantPickup";
		}

		// Token: 0x04000FE6 RID: 4070
		private Context context;

		// Token: 0x04000FE7 RID: 4071
		[HideInInspector]
		[SerializeField]
		private InstantPickupReferences references;

		// Token: 0x04000FE9 RID: 4073
		[HideInInspector]
		[SerializeField]
		private SwappableParticleSystem collectedParticleSystem;

		// Token: 0x04000FEA RID: 4074
		private EndlessScriptComponent scriptComponent;

		// Token: 0x04000FEB RID: 4075
		private InstantPickup luaInterface;
	}
}
