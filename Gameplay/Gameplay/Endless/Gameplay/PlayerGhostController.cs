using System;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000274 RID: 628
	public class PlayerGhostController : MonoBehaviour
	{
		// Token: 0x06000D3C RID: 3388 RVA: 0x00048310 File Offset: 0x00046510
		public void SetGhostMode(bool ghostToggleTo)
		{
			if (ghostToggleTo != this.ghostEnabled)
			{
				if (ghostToggleTo)
				{
					base.gameObject.layer = LayerMask.NameToLayer("GhostCharacter");
				}
				else
				{
					base.gameObject.layer = LayerMask.NameToLayer("Character");
				}
				this.ghostEnabled = ghostToggleTo;
			}
		}

		// Token: 0x04000C36 RID: 3126
		[SerializeField]
		private PlayerReferenceManager playerReferenceManager;

		// Token: 0x04000C37 RID: 3127
		private bool ghostEnabled;
	}
}
