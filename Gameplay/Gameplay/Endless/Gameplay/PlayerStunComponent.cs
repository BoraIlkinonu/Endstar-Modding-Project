using System;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x0200027E RID: 638
	public class PlayerStunComponent : MonoBehaviour
	{
		// Token: 0x06000DB9 RID: 3513 RVA: 0x0004A2E0 File Offset: 0x000484E0
		public void ProcessStun(uint frame, ref NetState currentState)
		{
			if (currentState.StunFrame > 0U)
			{
				currentState.StunFrame = (uint)Mathf.Max(currentState.StunFrame - 1U, 0f);
			}
		}

		// Token: 0x06000DBA RID: 3514 RVA: 0x0004A306 File Offset: 0x00048506
		public void ApplyStun(uint frame, uint stunLengthFrames)
		{
			this.incomingStunThisFrame = (uint)Mathf.Max(stunLengthFrames, this.incomingStunThisFrame);
		}

		// Token: 0x06000DBB RID: 3515 RVA: 0x0004A31F File Offset: 0x0004851F
		public void EndFrame(ref NetState currentState)
		{
			currentState.StunFrame = (uint)Mathf.Max(currentState.StunFrame, this.incomingStunThisFrame);
			this.incomingStunThisFrame = 0U;
		}

		// Token: 0x04000C97 RID: 3223
		private uint incomingStunThisFrame;
	}
}
