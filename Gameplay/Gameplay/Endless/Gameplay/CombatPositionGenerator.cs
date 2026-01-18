using System;
using UnityEngine;
using UnityEngine.AI;

namespace Endless.Gameplay
{
	// Token: 0x02000247 RID: 583
	public class CombatPositionGenerator : MonoBehaviour
	{
		// Token: 0x06000C0B RID: 3083 RVA: 0x00041B28 File Offset: 0x0003FD28
		private void Awake()
		{
			this.CombatPositions = new CombatPositions(this.targetable);
		}

		// Token: 0x17000239 RID: 569
		// (get) Token: 0x06000C0C RID: 3084 RVA: 0x00041B3B File Offset: 0x0003FD3B
		// (set) Token: 0x06000C0D RID: 3085 RVA: 0x00041B43 File Offset: 0x0003FD43
		public CombatPositions CombatPositions { get; private set; }

		// Token: 0x06000C0E RID: 3086 RVA: 0x00041B4C File Offset: 0x0003FD4C
		public bool TryGetClosestMeleePosition(Vector3 originPosition, float attackDistance, out Vector3 meleePosition)
		{
			return CombatPositionGenerator.TryGetClosestMeleePosition(originPosition, this.targetable.Position, attackDistance, out meleePosition);
		}

		// Token: 0x06000C0F RID: 3087 RVA: 0x00041B64 File Offset: 0x0003FD64
		public static bool TryGetClosestMeleePosition(Vector3 originPosition, Vector3 targetPosition, float attackDistance, out Vector3 meleePosition)
		{
			meleePosition = Vector3.zero;
			NavMeshHit navMeshHit;
			if (!NavMesh.SamplePosition(targetPosition + (originPosition - targetPosition).normalized * attackDistance, out navMeshHit, 1f, -1))
			{
				return false;
			}
			meleePosition = navMeshHit.position;
			return true;
		}

		// Token: 0x06000C10 RID: 3088 RVA: 0x00041BB6 File Offset: 0x0003FDB6
		public bool TryGetClosestNearPosition(Vector3 originPosition, out Vector3 nearPosition)
		{
			return this.CombatPositions.TryGetClosestNearPosition(originPosition, out nearPosition);
		}

		// Token: 0x06000C11 RID: 3089 RVA: 0x00041BC5 File Offset: 0x0003FDC5
		public bool TryGetClosestAroundPosition(Vector3 originPosition, out Vector3 aroundPosition)
		{
			return this.CombatPositions.TryGetClosestAroundPosition(originPosition, out aroundPosition);
		}

		// Token: 0x04000B25 RID: 2853
		[SerializeField]
		private HittableComponent targetable;
	}
}
