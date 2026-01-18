using System;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;

namespace Endless.Gameplay.UI
{
	// Token: 0x020003D7 RID: 983
	public class UIOnScreenButtonHandler : OnScreenControl
	{
		// Token: 0x17000517 RID: 1303
		// (get) Token: 0x060018D9 RID: 6361 RVA: 0x00073826 File Offset: 0x00071A26
		// (set) Token: 0x060018DA RID: 6362 RVA: 0x0007382E File Offset: 0x00071A2E
		protected override string controlPathInternal
		{
			get
			{
				return this.controlPathValue;
			}
			set
			{
				this.controlPathValue = value;
			}
		}

		// Token: 0x060018DB RID: 6363 RVA: 0x00073837 File Offset: 0x00071A37
		public void SetButtonState(bool down)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetButtonState", new object[] { down });
			}
			base.SendValueToControl<float>(down ? 1f : 0f);
		}

		// Token: 0x040013FC RID: 5116
		[InputControl(layout = "Button")]
		[SerializeField]
		private string controlPathValue;

		// Token: 0x040013FD RID: 5117
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
