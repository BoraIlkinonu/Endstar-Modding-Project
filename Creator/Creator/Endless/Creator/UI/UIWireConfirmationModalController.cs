using System;
using Endless.Gameplay;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x02000304 RID: 772
	[RequireComponent(typeof(UIWireConfirmationModalView))]
	public class UIWireConfirmationModalController : UIGameObject
	{
		// Token: 0x170001D9 RID: 473
		// (get) Token: 0x06000D84 RID: 3460 RVA: 0x0004061F File Offset: 0x0003E81F
		private UIWiringManager WiringManager
		{
			get
			{
				return MonoBehaviourSingleton<UIWiringManager>.Instance;
			}
		}

		// Token: 0x170001DA RID: 474
		// (get) Token: 0x06000D85 RID: 3461 RVA: 0x00040A29 File Offset: 0x0003EC29
		private UIWireConfirmationModalView View
		{
			get
			{
				if (!this.view)
				{
					base.TryGetComponent<UIWireConfirmationModalView>(out this.view);
				}
				return this.view;
			}
		}

		// Token: 0x06000D86 RID: 3462 RVA: 0x00040A4C File Offset: 0x0003EC4C
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.overrideEmitterContextualValueToggle.OnChange.AddListener(new UnityAction<bool>(this.View.SetWiringPropertyModifierVisibility));
			this.colorDropdown.OnColorChanged += this.SetColor;
		}

		// Token: 0x06000D87 RID: 3463 RVA: 0x00040AA9 File Offset: 0x0003ECA9
		private void OnDestroy()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDestroy", Array.Empty<object>());
			}
			this.colorDropdown.OnColorChanged -= this.SetColor;
		}

		// Token: 0x06000D88 RID: 3464 RVA: 0x00040ADC File Offset: 0x0003ECDC
		public void Confirm()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Confirm", Array.Empty<object>());
			}
			string[] array = ((!this.overrideEmitterContextualValueToggle.isActiveAndEnabled || this.overrideEmitterContextualValueToggle.IsOn) ? this.wiringPropertyModifier.StoredParameterValues : Array.Empty<string>());
			this.WiringManager.WireCreatorController.CreateWire(array, this.colorDropdown.Value[0].WireColor);
		}

		// Token: 0x06000D89 RID: 3465 RVA: 0x00040B58 File Offset: 0x0003ED58
		public void Cancel()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Cancel", Array.Empty<object>());
			}
			this.wiringReroute.HideRerouteSwitch();
			this.WiringManager.WireCreatorController.Restart(true);
		}

		// Token: 0x06000D8A RID: 3466 RVA: 0x00040B90 File Offset: 0x0003ED90
		private void SetColor(WireColor newColor)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetColor", new object[] { newColor });
			}
			if (this.View.Wire)
			{
				this.View.Wire.SetColor(newColor);
			}
		}

		// Token: 0x04000BA3 RID: 2979
		[SerializeField]
		private UIToggle overrideEmitterContextualValueToggle;

		// Token: 0x04000BA4 RID: 2980
		[SerializeField]
		private UIWirePropertyModifierView wiringPropertyModifier;

		// Token: 0x04000BA5 RID: 2981
		[SerializeField]
		private UIWireColorDropdown colorDropdown;

		// Token: 0x04000BA6 RID: 2982
		[SerializeField]
		private UIWiringRerouteView wiringReroute;

		// Token: 0x04000BA7 RID: 2983
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x04000BA8 RID: 2984
		private UIWireConfirmationModalView view;
	}
}
