using Endless.Gameplay.LevelEditing;
using Endless.Shared;
using Endless.Shared.DataTypes;
using UnityEngine;

namespace Endless.Creator.DynamicPropCreation;

public static class AbstractPropIconUtility
{
	private class GPUTextureScaler
	{
		public static Texture2D Scaled(Texture2D src, int width, int height, FilterMode mode = FilterMode.Trilinear)
		{
			Rect source = new Rect(0f, 0f, width, height);
			_gpu_scale(src, width, height, mode);
			Texture2D texture2D = new Texture2D(width, height, TextureFormat.ARGB32, mipChain: true);
			texture2D.Reinitialize(width, height);
			texture2D.ReadPixels(source, 0, 0, recalculateMipMaps: true);
			return texture2D;
		}

		public static void Scale(Texture2D tex, int width, int height, FilterMode mode = FilterMode.Trilinear)
		{
			Rect source = new Rect(0f, 0f, width, height);
			_gpu_scale(tex, width, height, mode);
			tex.Reinitialize(width, height);
			tex.ReadPixels(source, 0, 0, recalculateMipMaps: true);
			tex.Apply(updateMipmaps: true);
		}

		private static void _gpu_scale(Texture2D src, int width, int height, FilterMode fmode)
		{
			src.filterMode = fmode;
			src.Apply(updateMipmaps: true);
			Graphics.SetRenderTarget(new RenderTexture(width, height, 32));
			GL.LoadPixelMatrix(0f, 1f, 1f, 0f);
			GL.Clear(clearDepth: true, clearColor: true, new Color(0f, 0f, 0f, 0f));
			Graphics.DrawTexture(new Rect(0f, 0f, 1f, 1f), src);
		}
	}

	private const int ABSTRACT_ICON_SIZE = 64;

	private static readonly Vector2Int pixelOffset = new Vector2Int(32, 32);

	public static Texture2D MergeIcon(Texture2D background, SerializableGuid abstractIconId)
	{
		return MergeIcon(background, MonoBehaviourSingleton<DefaultContentManager>.Instance.DefaultIconList[abstractIconId]);
	}

	public static Texture2D MergeIcon(Texture2D background, Texture2D abstractIcon)
	{
		Texture2D texture2D = new Texture2D(background.width, background.height, TextureFormat.RGBA32, mipChain: false);
		texture2D.SetPixels(background.GetPixels());
		Texture2D texture2D2 = new Texture2D(abstractIcon.width, abstractIcon.height, TextureFormat.RGBA32, mipChain: false);
		texture2D2.SetPixels(abstractIcon.GetPixels());
		texture2D2.Apply();
		GPUTextureScaler.Scale(texture2D2, 64, 64, FilterMode.Bilinear);
		Color[] pixels = texture2D.GetPixels(pixelOffset.x, pixelOffset.y, 64, 64);
		Color[] pixels2 = texture2D2.GetPixels();
		for (int i = 0; i < pixels2.Length; i++)
		{
			if (pixels2[i].a > 0.5f)
			{
				pixels[i] = pixels2[i];
			}
		}
		texture2D.SetPixels(pixelOffset.x, pixelOffset.y, 64, 64, pixels);
		texture2D.Apply();
		return texture2D;
	}
}
