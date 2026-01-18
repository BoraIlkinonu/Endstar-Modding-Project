using System;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Shared.UI
{
	// Token: 0x020001E3 RID: 483
	public class UIColorDetailHdrView : UIBaseColorDetailView
	{
		// Token: 0x17000233 RID: 563
		// (get) Token: 0x06000BF6 RID: 3062 RVA: 0x00033E2B File Offset: 0x0003202B
		protected override int ColorMax
		{
			get
			{
				return 255;
			}
		}

		// Token: 0x06000BF7 RID: 3063 RVA: 0x00033E34 File Offset: 0x00032034
		public override void View(Color model)
		{
			if (this.isUpdatingFromIntensity)
			{
				this.ViewColorPreview(model);
				return;
			}
			int item = UIColorHdrUtility.DecomposeHDRColor(model).Item2;
			this.displayColor = UIColorHdrUtility.ToInspectorSwatch(model);
			this.currentIntensity = item;
			base.View(this.displayColor);
			this.intensity.SetModel(item, false);
		}

		// Token: 0x06000BF8 RID: 3064 RVA: 0x00033E89 File Offset: 0x00032089
		public override void SetInteractable(bool interactable)
		{
			base.SetInteractable(interactable);
			this.intensity.SetInteractable(interactable);
		}

		// Token: 0x06000BF9 RID: 3065 RVA: 0x00033E9E File Offset: 0x0003209E
		protected override void SetUpIntPresenters()
		{
			base.SetUpIntPresenters();
			base.SetUpIntPresenter(this.intensity, -10, 10, new Action<object>(this.SetIntensity));
		}

		// Token: 0x06000BFA RID: 3066 RVA: 0x00033EC2 File Offset: 0x000320C2
		protected override void ViewColorPreview(Color model)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "ViewColorPreview", "model", model), this);
			}
			this.colorPreviewImage.color = UIColorHdrUtility.ToInspectorSwatch(model);
		}

		// Token: 0x06000BFB RID: 3067 RVA: 0x00033F00 File Offset: 0x00032100
		protected override void SetRed(object newValueAsObject)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetRed", "newValueAsObject", newValueAsObject), this);
			}
			int num = (int)newValueAsObject;
			Color color = new Color((float)num / (float)this.ColorMax, this.displayColor.g, this.displayColor.b, this.displayColor.a);
			this.UpdateColorFromRGBChange(color);
		}

		// Token: 0x06000BFC RID: 3068 RVA: 0x00033F70 File Offset: 0x00032170
		protected override void SetGreen(object newValueAsObject)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetGreen", "newValueAsObject", newValueAsObject), this);
			}
			int num = (int)newValueAsObject;
			Color color = new Color(this.displayColor.r, (float)num / (float)this.ColorMax, this.displayColor.b, this.displayColor.a);
			this.UpdateColorFromRGBChange(color);
		}

		// Token: 0x06000BFD RID: 3069 RVA: 0x00033FE0 File Offset: 0x000321E0
		protected override void SetBlue(object newValueAsObject)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetBlue", "newValueAsObject", newValueAsObject), this);
			}
			int num = (int)newValueAsObject;
			Color color = new Color(this.displayColor.r, this.displayColor.g, (float)num / (float)this.ColorMax, this.displayColor.a);
			this.UpdateColorFromRGBChange(color);
		}

		// Token: 0x06000BFE RID: 3070 RVA: 0x00034050 File Offset: 0x00032250
		protected override void SetAlpha(object newValueAsObject)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetAlpha", "newValueAsObject", newValueAsObject), this);
			}
			int num = (int)newValueAsObject;
			Color color = new Color(this.displayColor.r, this.displayColor.g, this.displayColor.b, (float)num / (float)this.ColorMax);
			this.UpdateColorFromRGBChange(color);
		}

		// Token: 0x06000BFF RID: 3071 RVA: 0x000340C0 File Offset: 0x000322C0
		private void UpdateColorFromRGBChange(Color newDisplayColor)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "UpdateColorFromRGBChange", "newDisplayColor", newDisplayColor), this);
			}
			this.displayColor = newDisplayColor;
			Color color = ((this.currentIntensity == 0) ? this.displayColor : UIColorHdrUtility.CreateHdrColor(this.displayColor, this.currentIntensity));
			base.UpdateHsvFromColor(this.displayColor);
			this.isUpdatingFromIntensity = true;
			Action<Color> onColorChanged = this.OnColorChanged;
			if (onColorChanged != null)
			{
				onColorChanged(color);
			}
			this.isUpdatingFromIntensity = false;
			base.UpdateSaturationValueTexture();
			base.UpdateCursorPositions();
			this.UpdateHexFieldIfNotFocused(color);
		}

		// Token: 0x06000C00 RID: 3072 RVA: 0x00034160 File Offset: 0x00032360
		protected override void UpdateFromHSVChange(Color newColor)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "UpdateFromHSVChange", "newColor", newColor), this);
			}
			this.displayColor = newColor;
			Color color = ((this.currentIntensity == 0) ? newColor : UIColorHdrUtility.CreateHdrColor(newColor, this.currentIntensity));
			this.isUpdatingFromIntensity = true;
			Action<Color> onColorChanged = this.OnColorChanged;
			if (onColorChanged != null)
			{
				onColorChanged(color);
			}
			this.isUpdatingFromIntensity = false;
			base.UpdateCursorPositions();
			this.UpdateHexFieldIfNotFocused(color);
		}

		// Token: 0x06000C01 RID: 3073 RVA: 0x000341E4 File Offset: 0x000323E4
		protected override void UpdateHexFieldIfNotFocused(Color color)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "UpdateHexFieldIfNotFocused", "color", color), this);
			}
			if (this.hexadecimalInputField.isFocused)
			{
				return;
			}
			string text = UIColorUtility.ToHexString(color, true);
			this.hexadecimalInputField.SetTextWithoutNotify(text);
		}

		// Token: 0x06000C02 RID: 3074 RVA: 0x0003423C File Offset: 0x0003243C
		private void SetIntensity(object newValueAsObject)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetIntensity", "newValueAsObject", newValueAsObject), this);
			}
			this.currentIntensity = Mathf.Clamp((int)newValueAsObject, -10, 10);
			Color color = new Color((float)base.RedPresenterModel / (float)this.ColorMax, (float)base.GreenPresenterModel / (float)this.ColorMax, (float)base.BluePresenterModel / (float)this.ColorMax, (float)base.AlphaPresenterModel / (float)this.ColorMax);
			this.displayColor = color;
			Color color2;
			if (this.currentIntensity == 0)
			{
				color2 = color;
			}
			else
			{
				color2 = UIColorHdrUtility.CreateHdrColor(color, this.currentIntensity);
			}
			this.isUpdatingFromIntensity = true;
			Action<Color> onColorChanged = this.OnColorChanged;
			if (onColorChanged != null)
			{
				onColorChanged(color2);
			}
			this.isUpdatingFromIntensity = false;
			base.UpdateSaturationValueTexture();
			base.UpdateCursorPositions();
			this.UpdateHexFieldIfNotFocused(color2);
		}

		// Token: 0x040007BC RID: 1980
		[Header("UIColorDetailHdrView")]
		[SerializeField]
		private Image colorPreviewImage;

		// Token: 0x040007BD RID: 1981
		[SerializeField]
		private UIIntPresenter intensity;

		// Token: 0x040007BE RID: 1982
		private int currentIntensity;

		// Token: 0x040007BF RID: 1983
		private Color displayColor;

		// Token: 0x040007C0 RID: 1984
		private bool isUpdatingFromIntensity;
	}
}
