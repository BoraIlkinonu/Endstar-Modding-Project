using System;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x020001DF RID: 479
	public static class UIColorUtility
	{
		// Token: 0x06000BBE RID: 3006 RVA: 0x00032CEC File Offset: 0x00030EEC
		public static bool TryParseHexColor(string hexString, out Color color)
		{
			color = Color.white;
			if (string.IsNullOrEmpty(hexString))
			{
				return false;
			}
			string text = hexString.Replace("#", "").ToUpper();
			return ColorUtility.TryParseHtmlString("#" + text, out color);
		}

		// Token: 0x06000BBF RID: 3007 RVA: 0x00032D38 File Offset: 0x00030F38
		public static string ToHexString(Color color, bool includeAlpha = true)
		{
			Color color2 = UIColorUtility.NormalizeColorForDisplay(color);
			if (!includeAlpha)
			{
				return ColorUtility.ToHtmlStringRGB(color2);
			}
			return ColorUtility.ToHtmlStringRGBA(color2);
		}

		// Token: 0x06000BC0 RID: 3008 RVA: 0x00032D5C File Offset: 0x00030F5C
		public static Color NormalizeColorForDisplay(Color color)
		{
			if (!UIColorUtility.IsHdrColor(color))
			{
				return color;
			}
			return UIColorUtility.ApplyToneMappingAndGammaCorrection(color);
		}

		// Token: 0x06000BC1 RID: 3009 RVA: 0x00032D70 File Offset: 0x00030F70
		public static bool IsHdrColor(Color color)
		{
			return Mathf.Max(new float[] { color.r, color.g, color.b }) > 1f || Mathf.Min(new float[] { color.r, color.g, color.b }) < 0f;
		}

		// Token: 0x06000BC2 RID: 3010 RVA: 0x00032DD9 File Offset: 0x00030FD9
		public static Color LinearToGamma(Color linear)
		{
			return new Color(Mathf.LinearToGammaSpace(linear.r), Mathf.LinearToGammaSpace(linear.g), Mathf.LinearToGammaSpace(linear.b), linear.a);
		}

		// Token: 0x06000BC3 RID: 3011 RVA: 0x00032E07 File Offset: 0x00031007
		public static Color ApplyToneMapping(Color hdrColor)
		{
			return new Color(UIColorUtility.ToneMapComponent(hdrColor.r), UIColorUtility.ToneMapComponent(hdrColor.g), UIColorUtility.ToneMapComponent(hdrColor.b), hdrColor.a);
		}

		// Token: 0x06000BC4 RID: 3012 RVA: 0x00032E35 File Offset: 0x00031035
		private static float ToneMapComponent(float component)
		{
			if (component <= 1f)
			{
				return component;
			}
			return component / (component + 1f);
		}

		// Token: 0x06000BC5 RID: 3013 RVA: 0x00032E4C File Offset: 0x0003104C
		public static Color ClampColorForDisplay(Color color, bool isHdr = false)
		{
			if (!isHdr)
			{
				return new Color(Mathf.Clamp01(color.r), Mathf.Clamp01(color.g), Mathf.Clamp01(color.b), Mathf.Clamp01(color.a));
			}
			return new Color(Mathf.Max(0f, color.r), Mathf.Max(0f, color.g), Mathf.Max(0f, color.b), Mathf.Clamp01(color.a));
		}

		// Token: 0x06000BC6 RID: 3014 RVA: 0x00032ECE File Offset: 0x000310CE
		private static Color ApplyToneMappingAndGammaCorrection(Color linearColor)
		{
			return UIColorUtility.LinearToGamma(UIColorUtility.ApplyToneMapping(UIColorUtility.ClampColorForDisplay(linearColor, true)));
		}
	}
}
