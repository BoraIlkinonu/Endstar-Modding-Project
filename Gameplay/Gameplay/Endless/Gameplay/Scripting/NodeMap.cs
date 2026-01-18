using System;
using System.Collections.Generic;
using Endless.Shared;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay.Scripting
{
	// Token: 0x020004BE RID: 1214
	public class NodeMap : MonoBehaviourSingleton<NodeMap>
	{
		// Token: 0x170005E8 RID: 1512
		// (get) Token: 0x06001E44 RID: 7748 RVA: 0x00084062 File Offset: 0x00082262
		public Dictionary<Vector3Int, IInstructionNode> InstructionNodesByCellPosition { get; } = new Dictionary<Vector3Int, IInstructionNode>();

		// Token: 0x06001E45 RID: 7749 RVA: 0x0008406A File Offset: 0x0008226A
		private void Start()
		{
			MonoBehaviourSingleton<GameplayManager>.Instance.OnGameplayCleanup.AddListener(new UnityAction(this.CleanupNodeMap));
		}

		// Token: 0x06001E46 RID: 7750 RVA: 0x00084087 File Offset: 0x00082287
		private void CleanupNodeMap()
		{
			this.InstructionNodesByCellPosition.Clear();
		}
	}
}
