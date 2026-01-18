using System;
using Endless.Shared.Debugging;
using Runtime.Shared;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Endless.Shared.UI
{
	// Token: 0x020001E1 RID: 481
	public class UIColorDefaultView : UIBaseColorView, IUIInteractable
	{
		// Token: 0x14000034 RID: 52
		// (add) Token: 0x06000BCA RID: 3018 RVA: 0x00032F5C File Offset: 0x0003115C
		// (remove) Token: 0x06000BCB RID: 3019 RVA: 0x00032F94 File Offset: 0x00031194
		public event Action<bool> OpenColorWindow;

		// Token: 0x06000BCC RID: 3020 RVA: 0x00032FC9 File Offset: 0x000311C9
		private void Start()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("Start", this);
			}
			this.openColorWindowButton.onClick.AddListener(new UnityAction(this.InvokeOpenColorWindow));
		}

		// Token: 0x06000BCD RID: 3021 RVA: 0x00032FFA File Offset: 0x000311FA
		public override void View(Color model)
		{
			base.View(model);
			this.colorPreviewAlphaSlider.value = model.a;
		}

		// Token: 0x06000BCE RID: 3022 RVA: 0x00033014 File Offset: 0x00031214
		public override void SetInteractable(bool interactable)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetInteractable", "interactable", interactable), this);
			}
			this.interactable = interactable;
		}

		// Token: 0x06000BCF RID: 3023 RVA: 0x00033045 File Offset: 0x00031245
		protected override void ViewColorPreview(Color model)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "ViewColorPreview", "model", model), this);
			}
			this.colorPreviewImage.color = UIColorUtility.NormalizeColorForDisplay(model);
		}

		// Token: 0x06000BD0 RID: 3024 RVA: 0x00033080 File Offset: 0x00031280
		private void InvokeOpenColorWindow()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("InvokeOpenColorWindow", this);
			}
			Action<bool> openColorWindow = this.OpenColorWindow;
			if (openColorWindow == null)
			{
				return;
			}
			openColorWindow(this.interactable);
		}

		// Token: 0x040007A0 RID: 1952
		[Header("UIColorDefaultView")]
		[SerializeField]
		private Image colorPreviewImage;

		// Token: 0x040007A1 RID: 1953
		[SerializeField]
		private UIButton openColorWindowButton;

		// Token: 0x040007A2 RID: 1954
		[SerializeField]
		private UISlider colorPreviewAlphaSlider;

		// Token: 0x040007A3 RID: 1955
		private bool interactable;
	}
}
