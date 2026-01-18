using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Endless.Gameplay
{
	// Token: 0x02000248 RID: 584
	public class CombatPositions
	{
		// Token: 0x06000C13 RID: 3091 RVA: 0x00041BD4 File Offset: 0x0003FDD4
		public CombatPositions(HittableComponent targetableComponent)
		{
			this.targetable = targetableComponent;
			this.generatedPosition = new Vector3(-5000f, -5000f, -5000f);
		}

		// Token: 0x06000C14 RID: 3092 RVA: 0x00041C13 File Offset: 0x0003FE13
		public bool TryGetClosestNearPosition(Vector3 originPosition, out Vector3 closestPosition)
		{
			this.CheckCombatPositions();
			return CombatPositions.TryGetClosestPosition(this.nearPositions, originPosition, out closestPosition);
		}

		// Token: 0x06000C15 RID: 3093 RVA: 0x00041C28 File Offset: 0x0003FE28
		public bool TryGetClosestAroundPosition(Vector3 originPosition, out Vector3 closestPosition)
		{
			this.CheckCombatPositions();
			return CombatPositions.TryGetClosestPosition(this.aroundPositions, originPosition, out closestPosition);
		}

		// Token: 0x06000C16 RID: 3094 RVA: 0x00041C3D File Offset: 0x0003FE3D
		public List<Vector3> GetNearPositions()
		{
			this.CheckCombatPositions();
			return new List<Vector3>(this.nearPositions);
		}

		// Token: 0x06000C17 RID: 3095 RVA: 0x00041C50 File Offset: 0x0003FE50
		public List<Vector3> GetAroundPositions()
		{
			this.CheckCombatPositions();
			return new List<Vector3>(this.aroundPositions);
		}

		// Token: 0x06000C18 RID: 3096 RVA: 0x00041C63 File Offset: 0x0003FE63
		private void CheckCombatPositions()
		{
			if (!this.hasGeneratedPositions || Vector3.Distance(this.generatedPosition, this.targetable.Position) < 0.3f)
			{
				this.BuildPositions();
			}
		}

		// Token: 0x06000C19 RID: 3097 RVA: 0x00041C90 File Offset: 0x0003FE90
		private void BuildPositions()
		{
			this.generatedPosition = this.targetable.Position;
			this.nearPositions.Clear();
			this.aroundPositions.Clear();
			List<Vector3> list = this.GeneratePotentialPositions(2f, 6);
			List<Vector3> list2 = this.GeneratePotentialPositions(4f, 12);
			using (List<Vector3>.Enumerator enumerator = list.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Vector3 vector;
					if (CombatPositions.SamplePositions(enumerator.Current, out vector))
					{
						this.nearPositions.Add(vector);
					}
				}
			}
			using (List<Vector3>.Enumerator enumerator = list2.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Vector3 vector2;
					if (CombatPositions.SamplePositions(enumerator.Current, out vector2))
					{
						this.aroundPositions.Add(vector2);
					}
				}
			}
		}

		// Token: 0x06000C1A RID: 3098 RVA: 0x00041D78 File Offset: 0x0003FF78
		private List<Vector3> GeneratePotentialPositions(float radius, int numPositions)
		{
			List<Vector3> list = new List<Vector3>();
			float num = 0f;
			float num2 = 360f / (float)numPositions;
			for (int i = 0; i < numPositions; i++)
			{
				float num3 = num * 0.017453292f;
				Vector3 vector = this.generatedPosition + new Vector3(Mathf.Sin(num3), 0f, Mathf.Cos(num3)) * radius;
				list.Add(vector);
				num += num2;
			}
			return list;
		}

		// Token: 0x06000C1B RID: 3099 RVA: 0x00041DE8 File Offset: 0x0003FFE8
		private static bool SamplePositions(Vector3 candidatePosition, out Vector3 sampledPosition)
		{
			NavMeshHit navMeshHit;
			if (NavMesh.SamplePosition(candidatePosition, out navMeshHit, 0.5f, -1))
			{
				sampledPosition = navMeshHit.position;
				return true;
			}
			if (NavMesh.SamplePosition(candidatePosition + Vector3.up, out navMeshHit, 0.5f, -1))
			{
				sampledPosition = navMeshHit.position;
				return true;
			}
			if (NavMesh.SamplePosition(candidatePosition + Vector3.down, out navMeshHit, 0.5f, -1))
			{
				sampledPosition = navMeshHit.position;
				return true;
			}
			sampledPosition = Vector3.zero;
			return false;
		}

		// Token: 0x06000C1C RID: 3100 RVA: 0x00041E74 File Offset: 0x00040074
		private static bool TryGetClosestPosition(List<Vector3> positions, Vector3 originPosition, out Vector3 closestPosition)
		{
			closestPosition = Vector3.zero;
			if (positions.Count == 0)
			{
				return false;
			}
			float num = float.PositiveInfinity;
			foreach (Vector3 vector in positions)
			{
				float num2 = Vector3.Distance(originPosition, vector);
				if (num2 < num)
				{
					closestPosition = vector;
					num = num2;
				}
			}
			return true;
		}

		// Token: 0x04000B27 RID: 2855
		private readonly HittableComponent targetable;

		// Token: 0x04000B28 RID: 2856
		private Vector3 generatedPosition;

		// Token: 0x04000B29 RID: 2857
		public bool hasGeneratedPositions;

		// Token: 0x04000B2A RID: 2858
		private readonly List<Vector3> nearPositions = new List<Vector3>();

		// Token: 0x04000B2B RID: 2859
		private readonly List<Vector3> aroundPositions = new List<Vector3>();
	}
}
