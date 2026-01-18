using System;
using System.Collections.Generic;
using Endless.Gameplay.LuaEnums;
using UnityEngine;

namespace Endless.Gameplay;

public class DefaultBehaviorList : EndlessBehaviour
{
	[Serializable]
	public struct IdleBehaviorKvp
	{
		public IdleBehavior IdleBehavior;

		public BehaviorNode BehaviorNode;
	}

	[SerializeField]
	private List<IdleBehaviorKvp> behaviorNodes;

	private readonly Dictionary<IdleBehavior, BehaviorNode> behaviorNodesByBehaviorEnum = new Dictionary<IdleBehavior, BehaviorNode>();

	private void InitializeDictionary()
	{
		behaviorNodesByBehaviorEnum.Clear();
		foreach (IdleBehaviorKvp behaviorNode in behaviorNodes)
		{
			if (!behaviorNodesByBehaviorEnum.TryAdd(behaviorNode.IdleBehavior, behaviorNode.BehaviorNode))
			{
				Debug.LogWarning($"Multiple entries in behavior nodes with key {behaviorNode.IdleBehavior}, ignoring all but the first");
			}
		}
	}

	public bool TryGetBehaviorNode(IdleBehavior idleBehavior, out BehaviorNode behaviorNode)
	{
		if (behaviorNodesByBehaviorEnum.Count != behaviorNodes.Count)
		{
			InitializeDictionary();
		}
		if (behaviorNodesByBehaviorEnum.TryGetValue(idleBehavior, out behaviorNode) && (bool)behaviorNode)
		{
			return true;
		}
		Debug.LogWarning($"No Behavior node was found associated with {idleBehavior}");
		return false;
	}
}
