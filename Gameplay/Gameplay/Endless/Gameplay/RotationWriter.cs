using System;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000150 RID: 336
	public class RotationWriter
	{
		// Token: 0x060007F0 RID: 2032 RVA: 0x000254D6 File Offset: 0x000236D6
		public RotationWriter(Transform transform, IndividualStateUpdater individualStateUpdater)
		{
			this.transform = transform;
			individualStateUpdater.OnWriteState += this.HandleOnWriteState;
		}

		// Token: 0x060007F1 RID: 2033 RVA: 0x000254F8 File Offset: 0x000236F8
		private void HandleOnWriteState(ref NpcState currentState)
		{
			currentState.Rotation = this.transform.rotation.eulerAngles.y;
		}

		// Token: 0x04000648 RID: 1608
		private readonly Transform transform;
	}
}
