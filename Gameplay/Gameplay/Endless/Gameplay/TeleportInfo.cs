using System;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.Scripting;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020002C6 RID: 710
	public abstract class TeleportInfo : ScriptableObject
	{
		// Token: 0x17000331 RID: 817
		// (get) Token: 0x06001029 RID: 4137 RVA: 0x000521EF File Offset: 0x000503EF
		// (set) Token: 0x0600102A RID: 4138 RVA: 0x000521F7 File Offset: 0x000503F7
		public virtual TeleportType TeleportType { get; private set; }

		// Token: 0x17000332 RID: 818
		// (get) Token: 0x0600102B RID: 4139 RVA: 0x00052200 File Offset: 0x00050400
		// (set) Token: 0x0600102C RID: 4140 RVA: 0x00052208 File Offset: 0x00050408
		public virtual uint FramesToTeleport { get; private set; }

		// Token: 0x0600102D RID: 4141
		public abstract void TeleportStart(EndlessVisuals endlessVisuals, Animator animator, global::UnityEngine.Vector3 position);

		// Token: 0x0600102E RID: 4142
		public abstract void TeleportEnd(EndlessVisuals endlessVisuals, Animator animator, global::UnityEngine.Vector3 position);
	}
}
