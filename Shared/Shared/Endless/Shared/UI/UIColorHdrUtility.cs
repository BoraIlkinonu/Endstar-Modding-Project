using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Shared.UI
{
	// Token: 0x020001DE RID: 478
	public static class UIColorHdrUtility
	{
		// Token: 0x06000BB1 RID: 2993 RVA: 0x000326AD File Offset: 0x000308AD
		public static bool IsHdrColor(Color color)
		{
			return UIColorUtility.IsHdrColor(color);
		}

		// Token: 0x06000BB2 RID: 2994 RVA: 0x000326B8 File Offset: 0x000308B8
		[return: TupleElementNames(new string[] { "baseColor", "intensity" })]
		public static ValueTuple<Color, int> DecomposeHDRColor(Color hdrColor)
		{
			float num = Mathf.Max(new float[] { hdrColor.r, hdrColor.g, hdrColor.b });
			if (num <= 0.003f)
			{
				return new ValueTuple<Color, int>(new Color(0f, 0f, 0f, hdrColor.a), 0);
			}
			if (num <= 1f)
			{
				return new ValueTuple<Color, int>(hdrColor, 0);
			}
			int num2 = UIColorHdrUtility.CalculateIntensityFromMaxComponent(num);
			float num3 = Mathf.Pow(2f, (float)num2);
			Color color = new Color(Mathf.Min(1f, hdrColor.r / num3), Mathf.Min(1f, hdrColor.g / num3), Mathf.Min(1f, hdrColor.b / num3), hdrColor.a);
			if (UIColorHdrUtility.verboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2} ) | {3}: {4}, {5}: {6}", new object[] { "DecomposeHDRColor", "hdrColor", hdrColor, "baseColor", color, "intensity", num2 }));
			}
			return new ValueTuple<Color, int>(color, num2);
		}

		// Token: 0x06000BB3 RID: 2995 RVA: 0x000327DC File Offset: 0x000309DC
		private static int CalculateIntensityFromMaxComponent(float maxComponent)
		{
			float num = Mathf.Log(maxComponent, 2f);
			int num2 = Mathf.CeilToInt(Mathf.Clamp(num, -10f, 10f));
			if (maxComponent <= 1f)
			{
				num2 = 0;
			}
			if (UIColorHdrUtility.verboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2} ) | {3}: {4}, {5}: {6}", new object[] { "CalculateIntensityFromMaxComponent", "maxComponent", maxComponent, "intensity", num, "intensityAsInt", num2 }));
			}
			return num2;
		}

		// Token: 0x06000BB4 RID: 2996 RVA: 0x0003286D File Offset: 0x00030A6D
		private static Color ApplyUnityHDRTonemap(Color hdrColor)
		{
			return UIColorUtility.ClampColorForDisplay(hdrColor, false);
		}

		// Token: 0x06000BB5 RID: 2997 RVA: 0x00032878 File Offset: 0x00030A78
		public static Color CreateHdrColor(Color baseColor, int intensity)
		{
			intensity = Mathf.Clamp(intensity, -10, 10);
			float num = ((intensity == 0) ? 1f : Mathf.Pow(2f, (float)intensity));
			Color color = new Color(Mathf.Clamp01(baseColor.r), Mathf.Clamp01(baseColor.g), Mathf.Clamp01(baseColor.b), Mathf.Clamp01(baseColor.a));
			Color color2 = new Color(color.r * num, color.g * num, color.b * num, color.a);
			if (UIColorHdrUtility.verboseLogging)
			{
				UIColorHdrUtility.DecomposeHDRColor(color2);
				Debug.Log(string.Format("{0} ( {1}: {2}, {3}: {4} ) | {5}: {6}, {7}: {8}", new object[] { "CreateHdrColor", "baseColor", baseColor, "intensity", intensity, "multiplier", num, "hdrColor", color2 }));
			}
			return color2;
		}

		// Token: 0x06000BB6 RID: 2998 RVA: 0x00032974 File Offset: 0x00030B74
		public static Color ToInspectorSwatch(Color linearColor)
		{
			Color color = UIColorUtility.ClampColorForDisplay(linearColor, true);
			if (UIColorHdrUtility.IsHdrColor(linearColor))
			{
				color = UIColorUtility.ApplyToneMapping(color);
			}
			color = UIColorUtility.LinearToGamma(color);
			if (UIColorHdrUtility.verboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2} ) | result: {3}", new object[] { "ToInspectorSwatch", "linearColor", linearColor, color }));
			}
			return color;
		}

		// Token: 0x06000BB7 RID: 2999 RVA: 0x000329E0 File Offset: 0x00030BE0
		public static Texture2D ViewHdrColorPreview(Color model, RawImage colorPreviewRawImage, Texture2D hdrColorTexture)
		{
			if (UIColorHdrUtility.verboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2}, {3}: {4}, {5}: {6} )", new object[]
				{
					"ViewHdrColorPreview",
					"model",
					model,
					"colorPreviewRawImage",
					colorPreviewRawImage.gameObject.name,
					"hdrColorTexture",
					(!hdrColorTexture) ? "null" : "not null"
				}));
			}
			if (UIColorHdrUtility.IsHdrColor(model))
			{
				hdrColorTexture = UIColorHdrUtility.CreateHDRPreviewTexture(model, 64, 1, hdrColorTexture);
				colorPreviewRawImage.texture = hdrColorTexture;
				float num = Mathf.Clamp01(model.a);
				colorPreviewRawImage.color = new Color(1f, 1f, 1f, num);
			}
			else
			{
				colorPreviewRawImage.texture = null;
				colorPreviewRawImage.color = UIColorHdrUtility.ToInspectorSwatch(model);
			}
			return hdrColorTexture;
		}

		// Token: 0x06000BB8 RID: 3000 RVA: 0x00032AB0 File Offset: 0x00030CB0
		private static Texture2D CreateHDRPreviewTexture(Color hdrColor, int width = 64, int height = 1, Texture2D existingTexture = null)
		{
			if (UIColorHdrUtility.verboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2}, {3}: {4}, {5}: {6}, {7}: {8}", new object[]
				{
					"CreateHDRPreviewTexture",
					"hdrColor",
					hdrColor,
					"width",
					width,
					"height",
					height,
					"existingTexture",
					(!existingTexture) ? "null" : "not null"
				}));
			}
			Texture2D texture2D = UIColorHdrUtility.EnsureTextureSize(existingTexture, width, height, TextureFormat.RGBA32, true);
			ValueTuple<Color, int> valueTuple = UIColorHdrUtility.DecomposeHDRColor(hdrColor);
			Color item = valueTuple.Item1;
			int item2 = valueTuple.Item2;
			Color[] array = new Color[width * height];
			if (UIColorHdrUtility.IsHdrColor(hdrColor))
			{
				UIColorHdrUtility.FillHDRPreviewPixels(array, item, hdrColor.a, item2, width, height);
			}
			else
			{
				UIColorHdrUtility.FillSolidColorPixels(array, item);
			}
			texture2D.SetPixels(array);
			texture2D.Apply();
			return texture2D;
		}

		// Token: 0x06000BB9 RID: 3001 RVA: 0x00032B88 File Offset: 0x00030D88
		private static void FillSolidColorPixels(Color[] pixels, Color color)
		{
			Color color2 = UIColorHdrUtility.ApplyUnityHDRTonemap(color);
			for (int i = 0; i < pixels.Length; i++)
			{
				pixels[i] = color2;
			}
		}

		// Token: 0x06000BBA RID: 3002 RVA: 0x00032BB4 File Offset: 0x00030DB4
		private static void FillHDRPreviewPixels(Color[] pixels, Color baseColor, float alpha, int intensity, int width, int height)
		{
			float num = UIColorHdrUtility.CalculateMinIntensityForGradient(intensity);
			float num2 = (float)intensity;
			for (int i = 0; i < height; i++)
			{
				for (int j = 0; j < width; j++)
				{
					float num3 = ((width == 1) ? 0f : ((float)j / (float)(width - 1)));
					float num4 = Mathf.Lerp(num, num2, num3);
					float num5 = Mathf.Pow(2f, num4);
					Color color = new Color(baseColor.r * num5, baseColor.g * num5, baseColor.b * num5, alpha);
					pixels[i * width + j] = color;
				}
			}
		}

		// Token: 0x06000BBB RID: 3003 RVA: 0x00032C48 File Offset: 0x00030E48
		private static float CalculateMinIntensityForGradient(int currentIntensity)
		{
			if (currentIntensity > 8)
			{
				return Mathf.Max((float)currentIntensity - 8f, -4f);
			}
			if (currentIntensity > 2)
			{
				return Mathf.Max((float)currentIntensity - 4f, -2f);
			}
			return Mathf.Max((float)currentIntensity - 2f, -1f);
		}

		// Token: 0x06000BBC RID: 3004 RVA: 0x00032C95 File Offset: 0x00030E95
		public static void SafeDestroyTexture(ref Texture2D texture)
		{
			if (!texture)
			{
				return;
			}
			global::UnityEngine.Object.Destroy(texture);
			texture = null;
		}

		// Token: 0x06000BBD RID: 3005 RVA: 0x00032CAB File Offset: 0x00030EAB
		public static Texture2D EnsureTextureSize(Texture2D existingTexture, int width, int height, TextureFormat format = TextureFormat.RGBA32, bool linear = true)
		{
			if (existingTexture && existingTexture.width == width && existingTexture.height == height)
			{
				return existingTexture;
			}
			if (existingTexture)
			{
				UIColorHdrUtility.SafeDestroyTexture(ref existingTexture);
			}
			return new Texture2D(width, height, format, false, linear)
			{
				filterMode = FilterMode.Point
			};
		}

		// Token: 0x04000799 RID: 1945
		private const float MIN_HDR_THRESHOLD = 0.003f;

		// Token: 0x0400079A RID: 1946
		private const int DEFAULT_HDR_PREVIEW_WIDTH = 64;

		// Token: 0x0400079B RID: 1947
		private const int DEFAULT_HDR_PREVIEW_HEIGHT = 1;

		// Token: 0x0400079C RID: 1948
		private static readonly bool verboseLogging;
	}
}
