using System;
using System.Collections.Generic;
using Endless.Gameplay.LuaInterfaces;
using Endless.Gameplay.Scripting;
using Endless.Shared;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000275 RID: 629
	public class PlayerLuaComponent : EndlessNetworkBehaviour, IBaseType, IComponentBase, IScriptInjector
	{
		// Token: 0x17000263 RID: 611
		// (get) Token: 0x06000D3E RID: 3390 RVA: 0x0004835C File Offset: 0x0004655C
		public GameplayPlayerReferenceManager References
		{
			get
			{
				return this.references;
			}
		}

		// Token: 0x06000D3F RID: 3391 RVA: 0x00048364 File Offset: 0x00046564
		private void Awake()
		{
			this.SetupContext();
			List<IComponentBase> list = new List<IComponentBase>
			{
				this.references.HealthComponent,
				this.references.HittableComponent,
				this.references.TeamComponent
			};
			this.references.WorldObject.Initialize(this, list, base.NetworkObject);
			foreach (IComponentBase componentBase in list)
			{
				componentBase.PrefabInitialize(this.references.WorldObject);
			}
		}

		// Token: 0x06000D40 RID: 3392 RVA: 0x00048414 File Offset: 0x00046614
		private async void SetupContext()
		{
			this.Context = new Context(this.references.WorldObject);
			int userId = NetworkBehaviourSingleton<UserIdManager>.Instance.GetUserId(this.references.OwnerClientId);
			Context context = this.Context;
			string text = await MonoBehaviourSingleton<RuntimeDatabase>.Instance.GetUserName(userId);
			context.InternalId = text;
			context = null;
		}

		// Token: 0x06000D41 RID: 3393 RVA: 0x0004844C File Offset: 0x0004664C
		public string GetUserName()
		{
			int userId = NetworkBehaviourSingleton<UserIdManager>.Instance.GetUserId(this.References.OwnerClientId);
			return MonoBehaviourSingleton<RuntimeDatabase>.Instance.GetUserNameSynchronous(userId);
		}

		// Token: 0x17000264 RID: 612
		// (get) Token: 0x06000D42 RID: 3394 RVA: 0x0004847A File Offset: 0x0004667A
		// (set) Token: 0x06000D43 RID: 3395 RVA: 0x00048482 File Offset: 0x00046682
		public Context Context { get; private set; }

		// Token: 0x17000265 RID: 613
		// (get) Token: 0x06000D44 RID: 3396 RVA: 0x0004848B File Offset: 0x0004668B
		public WorldObject WorldObject
		{
			get
			{
				return this.references.WorldObject;
			}
		}

		// Token: 0x06000D45 RID: 3397 RVA: 0x00002DB0 File Offset: 0x00000FB0
		public void PrefabInitialize(WorldObject worldObject)
		{
		}

		// Token: 0x17000266 RID: 614
		// (get) Token: 0x06000D46 RID: 3398 RVA: 0x00048498 File Offset: 0x00046698
		public Player Player
		{
			get
			{
				return this.luaInterface;
			}
		}

		// Token: 0x17000267 RID: 615
		// (get) Token: 0x06000D47 RID: 3399 RVA: 0x000484A0 File Offset: 0x000466A0
		public object LuaObject
		{
			get
			{
				if (this.luaInterface == null)
				{
					this.luaInterface = new Player(this);
				}
				return this.luaInterface;
			}
		}

		// Token: 0x17000268 RID: 616
		// (get) Token: 0x06000D48 RID: 3400 RVA: 0x000484BC File Offset: 0x000466BC
		public Type LuaObjectType
		{
			get
			{
				return typeof(Player);
			}
		}

		// Token: 0x06000D49 RID: 3401 RVA: 0x000484C8 File Offset: 0x000466C8
		public void ScriptInitialize(EndlessScriptComponent endlessScriptComponent)
		{
			this.scriptComponent = endlessScriptComponent;
		}

		// Token: 0x06000D4B RID: 3403 RVA: 0x000484D4 File Offset: 0x000466D4
		protected override void __initializeVariables()
		{
			base.__initializeVariables();
		}

		// Token: 0x06000D4C RID: 3404 RVA: 0x0001E813 File Offset: 0x0001CA13
		protected override void __initializeRpcs()
		{
			base.__initializeRpcs();
		}

		// Token: 0x06000D4D RID: 3405 RVA: 0x000484EA File Offset: 0x000466EA
		protected internal override string __getTypeName()
		{
			return "PlayerLuaComponent";
		}

		// Token: 0x04000C38 RID: 3128
		[SerializeField]
		internal GameplayPlayerReferenceManager references;

		// Token: 0x04000C3A RID: 3130
		private Player luaInterface;

		// Token: 0x04000C3B RID: 3131
		private EndlessScriptComponent scriptComponent;
	}
}
