using System;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x0200014F RID: 335
	public class RotationReader
	{
		// Token: 0x060007EE RID: 2030 RVA: 0x0002545F File Offset: 0x0002365F
		public RotationReader(Transform transform, NpcSettings settings, IndividualStateUpdater stateUpdater)
		{
			this.transform = transform;
			this.settings = settings;
			stateUpdater.OnStateInterpolated += this.HandleOnStateInterpolated;
		}

		// Token: 0x060007EF RID: 2031 RVA: 0x00025488 File Offset: 0x00023688
		private void HandleOnStateInterpolated(NpcState state)
		{
			this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, Quaternion.Euler(0f, state.Rotation, 0f), this.settings.RotationSpeed * Time.deltaTime);
		}

		// Token: 0x04000646 RID: 1606
		private readonly Transform transform;

		// Token: 0x04000647 RID: 1607
		private readonly NpcSettings settings;
	}
}
