using System;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x02000248 RID: 584
	public class UIRolesRadioButton : UIBaseRadioButton<Roles>
	{
		// Token: 0x06000EE6 RID: 3814 RVA: 0x000401B6 File Offset: 0x0003E3B6
		public override void SetInteractable(bool interactable)
		{
			base.SetInteractable(interactable);
			this.disabledVisual.SetActive(!interactable);
		}

		// Token: 0x0400095A RID: 2394
		[Header("UIRolesRadioButton")]
		[SerializeField]
		private GameObject disabledVisual;
	}
}
