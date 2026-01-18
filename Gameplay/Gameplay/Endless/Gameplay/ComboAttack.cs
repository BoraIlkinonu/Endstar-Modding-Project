using System;
using System.Collections.Generic;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x0200023B RID: 571
	[CreateAssetMenu(menuName = "ScriptableObject/ComboAttack", fileName = "NewComboAttack")]
	public class ComboAttack : ScriptableObject
	{
		// Token: 0x04000AFF RID: 2815
		public float Cost;

		// Token: 0x04000B00 RID: 2816
		public List<ComboStep> ComboSteps;
	}
}
