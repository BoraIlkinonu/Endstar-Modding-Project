using System;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020001A8 RID: 424
	public class NpcSource : MonoBehaviour, INpcSource
	{
		// Token: 0x170001CF RID: 463
		// (get) Token: 0x06000986 RID: 2438 RVA: 0x0002BDE6 File Offset: 0x00029FE6
		public NpcEntity Npc
		{
			get
			{
				return this.npcEntity;
			}
		}

		// Token: 0x040007BA RID: 1978
		[SerializeField]
		private NpcEntity npcEntity;
	}
}
