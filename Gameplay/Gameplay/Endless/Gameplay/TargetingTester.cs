using System;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020002CC RID: 716
	public class TargetingTester : MonoBehaviour
	{
		// Token: 0x0600103F RID: 4159 RVA: 0x000523B0 File Offset: 0x000505B0
		private void Awake()
		{
			this.targeterComponent.OnTargetChanged += delegate(HittableComponent target)
			{
				Debug.Log("Target has changed to " + (target ? target.gameObject.name : "null"));
			};
			this.targeterComponent.OnTargetChanging += delegate(HittableComponent target)
			{
				Debug.Log("Target is changing from " + (target ? target.gameObject.name : "null"));
			};
		}

		// Token: 0x04000DEC RID: 3564
		[SerializeField]
		private TargeterComponent targeterComponent;
	}
}
