using System;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.Scripting;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020002C5 RID: 709
	public class MaterializeTeleportInfo : TeleportInfo
	{
		// Token: 0x1700032F RID: 815
		// (get) Token: 0x06001024 RID: 4132 RVA: 0x00017586 File Offset: 0x00015786
		public override TeleportType TeleportType
		{
			get
			{
				return TeleportType.Materialize;
			}
		}

		// Token: 0x17000330 RID: 816
		// (get) Token: 0x06001025 RID: 4133 RVA: 0x000521B7 File Offset: 0x000503B7
		public override uint FramesToTeleport
		{
			get
			{
				return this.frames;
			}
		}

		// Token: 0x06001026 RID: 4134 RVA: 0x000521BF File Offset: 0x000503BF
		public override void TeleportStart(EndlessVisuals endlessVisuals, Animator animator, global::UnityEngine.Vector3 position)
		{
			if (endlessVisuals)
			{
				endlessVisuals.FadeOut();
			}
		}

		// Token: 0x06001027 RID: 4135 RVA: 0x000521CF File Offset: 0x000503CF
		public override void TeleportEnd(EndlessVisuals endlessVisuals, Animator animator, global::UnityEngine.Vector3 position)
		{
			if (endlessVisuals)
			{
				endlessVisuals.FadeIn();
			}
		}

		// Token: 0x04000DD6 RID: 3542
		[SerializeField]
		private uint frames = 20U;
	}
}
