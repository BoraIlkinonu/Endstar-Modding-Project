using System;
using Endless.Gameplay.LevelEditing;
using Endless.Shared;
using Endless.Shared.DataTypes;
using UnityEngine;

namespace Endless.Creator.DynamicPropCreation
{
	// Token: 0x020003AF RID: 943
	public static class AbstractPropIconUtility
	{
		// Token: 0x06001272 RID: 4722 RVA: 0x0005F23E File Offset: 0x0005D43E
		public static Texture2D MergeIcon(Texture2D background, SerializableGuid abstractIconId)
		{
			return AbstractPropIconUtility.MergeIcon(background, MonoBehaviourSingleton<DefaultContentManager>.Instance.DefaultIconList[abstractIconId]);
		}

		// Token: 0x06001273 RID: 4723 RVA: 0x0005F258 File Offset: 0x0005D458
		public static Texture2D MergeIcon(Texture2D background, Texture2D abstractIcon)
		{
			Texture2D texture2D = new Texture2D(background.width, background.height, TextureFormat.RGBA32, false);
			texture2D.SetPixels(background.GetPixels());
			Texture2D texture2D2 = new Texture2D(abstractIcon.width, abstractIcon.height, TextureFormat.RGBA32, false);
			texture2D2.SetPixels(abstractIcon.GetPixels());
			texture2D2.Apply();
			AbstractPropIconUtility.GPUTextureScaler.Scale(texture2D2, 64, 64, FilterMode.Bilinear);
			Color[] pixels = texture2D.GetPixels(AbstractPropIconUtility.pixelOffset.x, AbstractPropIconUtility.pixelOffset.y, 64, 64);
			Color[] pixels2 = texture2D2.GetPixels();
			for (int i = 0; i < pixels2.Length; i++)
			{
				if (pixels2[i].a > 0.5f)
				{
					pixels[i] = pixels2[i];
				}
			}
			texture2D.SetPixels(AbstractPropIconUtility.pixelOffset.x, AbstractPropIconUtility.pixelOffset.y, 64, 64, pixels);
			texture2D.Apply();
			return texture2D;
		}

		// Token: 0x04000F3B RID: 3899
		private const int ABSTRACT_ICON_SIZE = 64;

		// Token: 0x04000F3C RID: 3900
		private static readonly Vector2Int pixelOffset = new Vector2Int(32, 32);

		// Token: 0x020003B0 RID: 944
		private class GPUTextureScaler
		{
			// Token: 0x06001275 RID: 4725 RVA: 0x0005F354 File Offset: 0x0005D554
			public static Texture2D Scaled(Texture2D src, int width, int height, FilterMode mode = FilterMode.Trilinear)
			{
				Rect rect = new Rect(0f, 0f, (float)width, (float)height);
				AbstractPropIconUtility.GPUTextureScaler._gpu_scale(src, width, height, mode);
				Texture2D texture2D = new Texture2D(width, height, TextureFormat.ARGB32, true);
				texture2D.Reinitialize(width, height);
				texture2D.ReadPixels(rect, 0, 0, true);
				return texture2D;
			}

			// Token: 0x06001276 RID: 4726 RVA: 0x0005F39C File Offset: 0x0005D59C
			public static void Scale(Texture2D tex, int width, int height, FilterMode mode = FilterMode.Trilinear)
			{
				Rect rect = new Rect(0f, 0f, (float)width, (float)height);
				AbstractPropIconUtility.GPUTextureScaler._gpu_scale(tex, width, height, mode);
				tex.Reinitialize(width, height);
				tex.ReadPixels(rect, 0, 0, true);
				tex.Apply(true);
			}

			// Token: 0x06001277 RID: 4727 RVA: 0x0005F3E4 File Offset: 0x0005D5E4
			private static void _gpu_scale(Texture2D src, int width, int height, FilterMode fmode)
			{
				src.filterMode = fmode;
				src.Apply(true);
				Graphics.SetRenderTarget(new RenderTexture(width, height, 32));
				GL.LoadPixelMatrix(0f, 1f, 1f, 0f);
				GL.Clear(true, true, new Color(0f, 0f, 0f, 0f));
				Graphics.DrawTexture(new Rect(0f, 0f, 1f, 1f), src);
			}
		}
	}
}
