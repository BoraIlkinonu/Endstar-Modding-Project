using System;
using System.Collections.Generic;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.Scripting;
using Endless.Shared;
using Endless.Shared.DataTypes;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020002A2 RID: 674
	public class GlobalContextsManager : EndlessBehaviourSingleton<GlobalContextsManager>, IBaseType, IComponentBase
	{
		// Token: 0x06000EDC RID: 3804 RVA: 0x0004EAF0 File Offset: 0x0004CCF0
		protected override void Awake()
		{
			base.Awake();
			this.gameWorldObject.Initialize(this, null, null);
		}

		// Token: 0x06000EDD RID: 3805 RVA: 0x0004EB08 File Offset: 0x0004CD08
		public void SaveState()
		{
			string text = Context.StaticLevelContext.ToJson();
			SerializableGuid activeLevelGuid = MonoBehaviourSingleton<StageManager>.Instance.ActiveLevelGuid;
			if (!this.levelContexts.TryAdd(activeLevelGuid, text))
			{
				this.levelContexts[activeLevelGuid] = text;
			}
		}

		// Token: 0x06000EDE RID: 3806 RVA: 0x0004EB48 File Offset: 0x0004CD48
		public void LoadState()
		{
			if (Context.StaticGameContext == null)
			{
				Context.StaticGameContext = new Context(this.gameWorldObject);
			}
			Context.StaticLevelContext = new Context(this.gameWorldObject);
			string text;
			if (this.levelContexts.TryGetValue(MonoBehaviourSingleton<StageManager>.Instance.ActiveLevelGuid, out text))
			{
				Context.StaticLevelContext.LoadFromJson(text);
			}
		}

		// Token: 0x06000EDF RID: 3807 RVA: 0x0004EBA0 File Offset: 0x0004CDA0
		public void ClearContexts()
		{
			Context.StaticGameContext = new Context(this.gameWorldObject);
			this.levelContexts = new Dictionary<SerializableGuid, string>();
		}

		// Token: 0x170002CF RID: 719
		// (get) Token: 0x06000EE0 RID: 3808 RVA: 0x0004EBBD File Offset: 0x0004CDBD
		public Context Context
		{
			get
			{
				return Context.StaticGameContext;
			}
		}

		// Token: 0x170002D0 RID: 720
		// (get) Token: 0x06000EE1 RID: 3809 RVA: 0x0004EBC4 File Offset: 0x0004CDC4
		public WorldObject WorldObject
		{
			get
			{
				return this.gameWorldObject;
			}
		}

		// Token: 0x06000EE2 RID: 3810 RVA: 0x00002DB0 File Offset: 0x00000FB0
		public void PrefabInitialize(WorldObject worldObject)
		{
		}

		// Token: 0x04000D45 RID: 3397
		[SerializeField]
		private WorldObject gameWorldObject;

		// Token: 0x04000D46 RID: 3398
		private Dictionary<SerializableGuid, string> levelContexts = new Dictionary<SerializableGuid, string>();
	}
}
