using System.Collections.Generic;
using Endless.Shared;
using UnityEngine;

namespace Endless.Gameplay.Scripting;

public class NodeMap : MonoBehaviourSingleton<NodeMap>
{
	public Dictionary<Vector3Int, IInstructionNode> InstructionNodesByCellPosition { get; } = new Dictionary<Vector3Int, IInstructionNode>();

	private void Start()
	{
		MonoBehaviourSingleton<GameplayManager>.Instance.OnGameplayCleanup.AddListener(CleanupNodeMap);
	}

	private void CleanupNodeMap()
	{
		InstructionNodesByCellPosition.Clear();
	}
}
