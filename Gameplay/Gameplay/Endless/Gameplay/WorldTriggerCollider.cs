using System;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x0200037C RID: 892
	public class WorldTriggerCollider : MonoBehaviour
	{
		// Token: 0x170004CC RID: 1228
		// (get) Token: 0x060016DE RID: 5854 RVA: 0x0006AF3C File Offset: 0x0006913C
		// (set) Token: 0x060016DF RID: 5855 RVA: 0x0006AF44 File Offset: 0x00069144
		public WorldTrigger WorldTrigger { get; private set; }

		// Token: 0x060016E0 RID: 5856 RVA: 0x0006AF4D File Offset: 0x0006914D
		public void Initialize(WorldTrigger worldTrigger)
		{
			this.WorldTrigger = worldTrigger;
		}
	}
}
