using System;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x0200014B RID: 331
	public class PositionReader
	{
		// Token: 0x060007DC RID: 2012 RVA: 0x00024F18 File Offset: 0x00023118
		public PositionReader(Transform transform, IndividualStateUpdater individualStateUpdater)
		{
			this.transform = transform;
			individualStateUpdater.OnStateInterpolated += this.HandleOnStateInterpolated;
		}

		// Token: 0x060007DD RID: 2013 RVA: 0x00024F39 File Offset: 0x00023139
		private void HandleOnStateInterpolated(NpcState state)
		{
			if (float.IsNaN(state.Position.x))
			{
				return;
			}
			this.transform.position = state.Position;
		}

		// Token: 0x0400063D RID: 1597
		private readonly Transform transform;
	}
}
