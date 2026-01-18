using System;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000246 RID: 582
	public class CombatPositionDebugger : MonoBehaviour, NetClock.ISimulateFrameLateSubscriber
	{
		// Token: 0x06000C07 RID: 3079 RVA: 0x0001CAAE File Offset: 0x0001ACAE
		public void Start()
		{
			NetClock.Register(this);
		}

		// Token: 0x06000C08 RID: 3080 RVA: 0x0001CAB6 File Offset: 0x0001ACB6
		public void OnDestroy()
		{
			NetClock.Unregister(this);
		}

		// Token: 0x06000C09 RID: 3081 RVA: 0x00041A44 File Offset: 0x0003FC44
		public void SimulateFrameLate(uint frame)
		{
			if (this.combatPositionGenerator == null || this.combatPositionGenerator.CombatPositions == null)
			{
				return;
			}
			foreach (Vector3 vector in this.combatPositionGenerator.CombatPositions.GetNearPositions())
			{
				Debug.DrawLine(vector, vector + Vector3.up, Color.cyan, NetClock.FixedDeltaTime);
			}
			foreach (Vector3 vector2 in this.combatPositionGenerator.CombatPositions.GetAroundPositions())
			{
				Debug.DrawLine(vector2, vector2 + Vector3.up, Color.magenta, NetClock.FixedDeltaTime);
			}
		}

		// Token: 0x04000B24 RID: 2852
		[SerializeField]
		private CombatPositionGenerator combatPositionGenerator;
	}
}
