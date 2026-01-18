using System;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000197 RID: 407
	[CreateAssetMenu(menuName = "ScriptableObject/GoalBuilders/RegainHealthBuilder", fileName = "RegainHealthBuilder")]
	public class RegainHealthBuilderSo : GoalBuilderSo
	{
		// Token: 0x0600093B RID: 2363 RVA: 0x0002AD24 File Offset: 0x00028F24
		protected override float Priority(NpcEntity entity)
		{
			return this.basePriority * this.priorityCurve.Evaluate((float)entity.Health / (float)entity.Components.Health.MaxHealth);
		}

		// Token: 0x0400078C RID: 1932
		[SerializeField]
		private AnimationCurve priorityCurve;
	}
}
