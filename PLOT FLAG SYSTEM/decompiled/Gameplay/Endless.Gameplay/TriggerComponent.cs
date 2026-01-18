using System;
using System.Collections.Generic;
using Endless.Gameplay.Scripting;
using Endless.Props.ReferenceComponents;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay;

public class TriggerComponent : EndlessBehaviour, IStartSubscriber, IComponentBase
{
	[Serializable]
	public class TriggerEventIndexer
	{
		[SerializeField]
		private WorldTrigger worldTrigger;

		[SerializeField]
		private int index;

		private UnityAction<int, WorldCollidable, bool> callback;

		private void HandleTriggerEnter(WorldCollidable worldCollidable, bool rollback)
		{
			callback(index, worldCollidable, arg2: true);
		}

		private void HandleTriggerExit(WorldCollidable worldCollidable, bool rollback)
		{
			callback(index, worldCollidable, arg2: false);
		}

		public void InitCallback(UnityAction<int, WorldCollidable, bool> callback, Func<WorldCollidable, bool> overlapValidator)
		{
			this.callback = callback;
			worldTrigger.OnTriggerEnter.AddListener(HandleTriggerEnter);
			worldTrigger.OnTriggerExit.AddListener(HandleTriggerExit);
			worldTrigger.AllowInteractionChecker = overlapValidator;
		}

		public TriggerEventIndexer(WorldTrigger worldTrigger, int index)
		{
			this.worldTrigger = worldTrigger;
			this.index = index;
		}
	}

	[SerializeField]
	[HideInInspector]
	private EndlessScriptComponent scriptComponent;

	[SerializeField]
	[HideInInspector]
	private List<TriggerEventIndexer> allTriggerEventIndexers = new List<TriggerEventIndexer>();

	[SerializeField]
	[HideInInspector]
	private TriggerComponentReferences references;

	[field: SerializeField]
	public WorldObject WorldObject { get; private set; }

	public Type ComponentReferenceType => typeof(TriggerComponentReferences);

	private void HandleTriggerEvent(int colliderIndex, WorldCollidable worldCollidable, bool enter)
	{
		scriptComponent.TryExecuteFunction(enter ? "OnTriggerEnter" : "OnTriggerExit", out var _, worldCollidable.WorldObject.Context, colliderIndex);
	}

	public bool ValidateOverlap(WorldCollidable worldCollidable)
	{
		return worldCollidable.WorldObject != null;
	}

	public void EndlessStart()
	{
		scriptComponent = base.transform.parent.GetComponentInParent<EndlessScriptComponent>();
		foreach (TriggerEventIndexer allTriggerEventIndexer in allTriggerEventIndexers)
		{
			allTriggerEventIndexer.InitCallback(HandleTriggerEvent, ValidateOverlap);
		}
	}

	public void ComponentInitialize(ReferenceBase referenceBase, EndlessProp endlessProp)
	{
		references = (TriggerComponentReferences)referenceBase;
		for (int i = 0; i < references.TriggerColliders.Length; i++)
		{
			if (references.TriggerColliders[i] != null)
			{
				WorldTrigger worldTrigger = references.TriggerColliders[i].gameObject.AddComponent<WorldTrigger>();
				allTriggerEventIndexers.Add(new TriggerEventIndexer(worldTrigger, i));
				Collider[] cachedColliders = references.TriggerColliders[i].CachedColliders;
				foreach (Collider obj in cachedColliders)
				{
					obj.isTrigger = true;
					obj.gameObject.AddComponent<WorldTriggerCollider>().Initialize(worldTrigger);
				}
			}
		}
	}

	public void PrefabInitialize(WorldObject worldObject)
	{
		WorldObject = worldObject;
	}
}
