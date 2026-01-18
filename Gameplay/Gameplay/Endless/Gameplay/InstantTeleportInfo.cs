using System;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.Scripting;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020002C4 RID: 708
	public class InstantTeleportInfo : TeleportInfo
	{
		// Token: 0x1700032D RID: 813
		// (get) Token: 0x0600101F RID: 4127 RVA: 0x0001965C File Offset: 0x0001785C
		public override TeleportType TeleportType
		{
			get
			{
				return TeleportType.Instant;
			}
		}

		// Token: 0x1700032E RID: 814
		// (get) Token: 0x06001020 RID: 4128 RVA: 0x000521A0 File Offset: 0x000503A0
		public override uint FramesToTeleport
		{
			get
			{
				return this.frames;
			}
		}

		// Token: 0x06001021 RID: 4129 RVA: 0x00002DB0 File Offset: 0x00000FB0
		public override void TeleportStart(EndlessVisuals endlessVisuals, Animator animator, global::UnityEngine.Vector3 position)
		{
		}

		// Token: 0x06001022 RID: 4130 RVA: 0x00002DB0 File Offset: 0x00000FB0
		public override void TeleportEnd(EndlessVisuals endlessVisuals, Animator animator, global::UnityEngine.Vector3 position)
		{
		}

		// Token: 0x04000DD5 RID: 3541
		[SerializeField]
		private uint frames = 8U;
	}
}
