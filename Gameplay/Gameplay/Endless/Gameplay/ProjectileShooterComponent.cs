using System;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.LuaInterfaces;
using Endless.Gameplay.Scripting;
using Endless.Props.ReferenceComponents;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000355 RID: 853
	public class ProjectileShooterComponent : EndlessNetworkBehaviour, NetClock.ISimulateFrameEnvironmentSubscriber, IComponentBase, IScriptInjector
	{
		// Token: 0x0600155F RID: 5471 RVA: 0x00066157 File Offset: 0x00064357
		public void Shoot()
		{
			this.shootThisFrame = true;
		}

		// Token: 0x06001560 RID: 5472 RVA: 0x00066160 File Offset: 0x00064360
		private void OnProjectileHitCallback(HealthChangeResult result, Context context)
		{
			object[] array;
			this.scriptComponent.TryExecuteFunction("OnProjectileHit", out array, new object[] { context });
		}

		// Token: 0x06001561 RID: 5473 RVA: 0x0006070D File Offset: 0x0005E90D
		protected override void Start()
		{
			base.Start();
			NetClock.Register(this);
		}

		// Token: 0x06001562 RID: 5474 RVA: 0x0006071B File Offset: 0x0005E91B
		public override void OnDestroy()
		{
			base.OnDestroy();
			NetClock.Unregister(this);
		}

		// Token: 0x06001563 RID: 5475 RVA: 0x0006618C File Offset: 0x0006438C
		public void SimulateFrameEnvironment(uint frame)
		{
			if (base.IsServer && this.shootThisFrame)
			{
				this.projectileShooter.ShootProjectileLocal(this.references.FirePoint.position, this.references.FirePoint.eulerAngles, frame, new Action<HealthChangeResult, Context>(this.OnProjectileHitCallback), null);
				this.shootThisFrame = false;
			}
		}

		// Token: 0x17000473 RID: 1139
		// (get) Token: 0x06001564 RID: 5476 RVA: 0x000661E9 File Offset: 0x000643E9
		// (set) Token: 0x06001565 RID: 5477 RVA: 0x000661F1 File Offset: 0x000643F1
		public WorldObject WorldObject { get; private set; }

		// Token: 0x17000474 RID: 1140
		// (get) Token: 0x06001566 RID: 5478 RVA: 0x000661FA File Offset: 0x000643FA
		Type IComponentBase.ComponentReferenceType
		{
			get
			{
				return typeof(ProjectileShooterReferences);
			}
		}

		// Token: 0x06001567 RID: 5479 RVA: 0x00066206 File Offset: 0x00064406
		void IComponentBase.ComponentInitialize(ReferenceBase referenceBase, EndlessProp endlessProp)
		{
			this.references = (ProjectileShooterReferences)referenceBase;
			this.projectileShooter.SetupProjectileShooterReferences(this.references);
		}

		// Token: 0x06001568 RID: 5480 RVA: 0x00066225 File Offset: 0x00064425
		public void PrefabInitialize(WorldObject worldObject)
		{
			this.WorldObject = worldObject;
		}

		// Token: 0x17000475 RID: 1141
		// (get) Token: 0x06001569 RID: 5481 RVA: 0x00066230 File Offset: 0x00064430
		object IScriptInjector.LuaObject
		{
			get
			{
				ProjectileShooter projectileShooter;
				if ((projectileShooter = this.luaInterface) == null)
				{
					projectileShooter = (this.luaInterface = new ProjectileShooter(this));
				}
				return projectileShooter;
			}
		}

		// Token: 0x17000476 RID: 1142
		// (get) Token: 0x0600156A RID: 5482 RVA: 0x00066256 File Offset: 0x00064456
		Type IScriptInjector.LuaObjectType
		{
			get
			{
				return typeof(ProjectileShooter);
			}
		}

		// Token: 0x0600156B RID: 5483 RVA: 0x00066262 File Offset: 0x00064462
		void IScriptInjector.ScriptInitialize(EndlessScriptComponent endlessScriptComponent)
		{
			this.scriptComponent = endlessScriptComponent;
		}

		// Token: 0x0600156D RID: 5485 RVA: 0x0006626C File Offset: 0x0006446C
		protected override void __initializeVariables()
		{
			base.__initializeVariables();
		}

		// Token: 0x0600156E RID: 5486 RVA: 0x0001E813 File Offset: 0x0001CA13
		protected override void __initializeRpcs()
		{
			base.__initializeRpcs();
		}

		// Token: 0x0600156F RID: 5487 RVA: 0x00066282 File Offset: 0x00064482
		protected internal override string __getTypeName()
		{
			return "ProjectileShooterComponent";
		}

		// Token: 0x04001184 RID: 4484
		[SerializeField]
		private ProjectileShooter projectileShooter;

		// Token: 0x04001185 RID: 4485
		private bool shootThisFrame;

		// Token: 0x04001186 RID: 4486
		[SerializeField]
		private ProjectileShooterReferences references;

		// Token: 0x04001188 RID: 4488
		private ProjectileShooter luaInterface;

		// Token: 0x04001189 RID: 4489
		private EndlessScriptComponent scriptComponent;
	}
}
