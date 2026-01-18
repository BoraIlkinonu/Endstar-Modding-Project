using System;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000365 RID: 869
	public class DynamicObjectResetter : EndlessBehaviour, IStartSubscriber, IGameEndSubscriber
	{
		// Token: 0x06001656 RID: 5718 RVA: 0x00069222 File Offset: 0x00067422
		public void EndlessStart()
		{
			this.startPosition = base.transform.position;
			this.startRotation = base.transform.rotation;
		}

		// Token: 0x06001657 RID: 5719 RVA: 0x00069246 File Offset: 0x00067446
		public void EndlessGameEnd()
		{
			base.transform.position = this.startPosition;
			base.transform.rotation = this.startRotation;
		}

		// Token: 0x04001211 RID: 4625
		private Vector3 startPosition;

		// Token: 0x04001212 RID: 4626
		private Quaternion startRotation;
	}
}
