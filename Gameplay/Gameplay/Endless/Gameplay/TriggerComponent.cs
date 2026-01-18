using System;
using System.Collections.Generic;
using Endless.Gameplay.Scripting;
using Endless.Props.ReferenceComponents;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay
{
	// Token: 0x02000361 RID: 865
	public class TriggerComponent : EndlessBehaviour, IStartSubscriber, IComponentBase
	{
		// Token: 0x06001640 RID: 5696 RVA: 0x00068E48 File Offset: 0x00067048
		private void HandleTriggerEvent(int colliderIndex, WorldCollidable worldCollidable, bool enter)
		{
			object[] array;
			this.scriptComponent.TryExecuteFunction(enter ? "OnTriggerEnter" : "OnTriggerExit", out array, new object[]
			{
				worldCollidable.WorldObject.Context,
				colliderIndex
			});
		}

		// Token: 0x06001641 RID: 5697 RVA: 0x00068E8F File Offset: 0x0006708F
		public bool ValidateOverlap(WorldCollidable worldCollidable)
		{
			return worldCollidable.WorldObject != null;
		}

		// Token: 0x06001642 RID: 5698 RVA: 0x00068EA0 File Offset: 0x000670A0
		public void EndlessStart()
		{
			this.scriptComponent = base.transform.parent.GetComponentInParent<EndlessScriptComponent>();
			foreach (TriggerComponent.TriggerEventIndexer triggerEventIndexer in this.allTriggerEventIndexers)
			{
				triggerEventIndexer.InitCallback(new UnityAction<int, WorldCollidable, bool>(this.HandleTriggerEvent), new Func<WorldCollidable, bool>(this.ValidateOverlap));
			}
		}

		// Token: 0x170004B7 RID: 1207
		// (get) Token: 0x06001643 RID: 5699 RVA: 0x00068F20 File Offset: 0x00067120
		// (set) Token: 0x06001644 RID: 5700 RVA: 0x00068F28 File Offset: 0x00067128
		public WorldObject WorldObject { get; private set; }

		// Token: 0x170004B8 RID: 1208
		// (get) Token: 0x06001645 RID: 5701 RVA: 0x00068F31 File Offset: 0x00067131
		public Type ComponentReferenceType
		{
			get
			{
				return typeof(TriggerComponentReferences);
			}
		}

		// Token: 0x06001646 RID: 5702 RVA: 0x00068F40 File Offset: 0x00067140
		public void ComponentInitialize(ReferenceBase referenceBase, EndlessProp endlessProp)
		{
			this.references = (TriggerComponentReferences)referenceBase;
			for (int i = 0; i < this.references.TriggerColliders.Length; i++)
			{
				if (this.references.TriggerColliders[i] != null)
				{
					WorldTrigger worldTrigger = this.references.TriggerColliders[i].gameObject.AddComponent<WorldTrigger>();
					this.allTriggerEventIndexers.Add(new TriggerComponent.TriggerEventIndexer(worldTrigger, i));
					foreach (Collider collider in this.references.TriggerColliders[i].CachedColliders)
					{
						collider.isTrigger = true;
						collider.gameObject.AddComponent<WorldTriggerCollider>().Initialize(worldTrigger);
					}
				}
			}
		}

		// Token: 0x06001647 RID: 5703 RVA: 0x00068FEE File Offset: 0x000671EE
		public void PrefabInitialize(WorldObject worldObject)
		{
			this.WorldObject = worldObject;
		}

		// Token: 0x04001205 RID: 4613
		[SerializeField]
		[HideInInspector]
		private EndlessScriptComponent scriptComponent;

		// Token: 0x04001206 RID: 4614
		[SerializeField]
		[HideInInspector]
		private List<TriggerComponent.TriggerEventIndexer> allTriggerEventIndexers = new List<TriggerComponent.TriggerEventIndexer>();

		// Token: 0x04001208 RID: 4616
		[SerializeField]
		[HideInInspector]
		private TriggerComponentReferences references;

		// Token: 0x02000362 RID: 866
		[Serializable]
		public class TriggerEventIndexer
		{
			// Token: 0x06001649 RID: 5705 RVA: 0x0006900A File Offset: 0x0006720A
			private void HandleTriggerEnter(WorldCollidable worldCollidable, bool rollback)
			{
				this.callback(this.index, worldCollidable, true);
			}

			// Token: 0x0600164A RID: 5706 RVA: 0x0006901F File Offset: 0x0006721F
			private void HandleTriggerExit(WorldCollidable worldCollidable, bool rollback)
			{
				this.callback(this.index, worldCollidable, false);
			}

			// Token: 0x0600164B RID: 5707 RVA: 0x00069034 File Offset: 0x00067234
			public void InitCallback(UnityAction<int, WorldCollidable, bool> callback, Func<WorldCollidable, bool> overlapValidator)
			{
				this.callback = callback;
				this.worldTrigger.OnTriggerEnter.AddListener(new UnityAction<WorldCollidable, bool>(this.HandleTriggerEnter));
				this.worldTrigger.OnTriggerExit.AddListener(new UnityAction<WorldCollidable, bool>(this.HandleTriggerExit));
				this.worldTrigger.AllowInteractionChecker = overlapValidator;
			}

			// Token: 0x0600164C RID: 5708 RVA: 0x0006908C File Offset: 0x0006728C
			public TriggerEventIndexer(WorldTrigger worldTrigger, int index)
			{
				this.worldTrigger = worldTrigger;
				this.index = index;
			}

			// Token: 0x04001209 RID: 4617
			[SerializeField]
			private WorldTrigger worldTrigger;

			// Token: 0x0400120A RID: 4618
			[SerializeField]
			private int index;

			// Token: 0x0400120B RID: 4619
			private UnityAction<int, WorldCollidable, bool> callback;
		}
	}
}
