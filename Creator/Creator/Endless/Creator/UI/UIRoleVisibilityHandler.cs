using System;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x0200015A RID: 346
	public class UIRoleVisibilityHandler : UIGameObject, IRoleInteractable
	{
		// Token: 0x06000536 RID: 1334 RVA: 0x0001C6DA File Offset: 0x0001A8DA
		public void SetLocalUserCanInteract(bool localUserCanInteract)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetLocalUserCanInteract", new object[] { localUserCanInteract });
			}
			base.gameObject.SetActive(localUserCanInteract);
		}

		// Token: 0x040004B8 RID: 1208
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
