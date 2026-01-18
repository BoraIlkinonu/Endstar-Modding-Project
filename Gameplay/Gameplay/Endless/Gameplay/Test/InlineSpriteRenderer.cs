using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Gameplay.Test
{
	// Token: 0x02000425 RID: 1061
	public class InlineSpriteRenderer : MonoBehaviour
	{
		// Token: 0x06001A59 RID: 6745 RVA: 0x00078FD4 File Offset: 0x000771D4
		private void Awake()
		{
			if (this.parentRect == null)
			{
				this.parentRect = base.GetComponent<RectTransform>();
			}
			if (this.spanFinder == null)
			{
				this.spanFinder = base.GetComponent<InlineTextSpanFinder>();
			}
			this.spanFinder.SpansReady += this.ApplySpans;
			this.RebuildDictionary();
			this.PrewarmPool();
		}

		// Token: 0x06001A5A RID: 6746 RVA: 0x0007902C File Offset: 0x0007722C
		private void OnValidate()
		{
			this.RebuildDictionary();
		}

		// Token: 0x06001A5B RID: 6747 RVA: 0x00079034 File Offset: 0x00077234
		private void RebuildDictionary()
		{
			this.spriteDictionary.Clear();
			for (int i = 0; i < this.spriteEntries.Count; i++)
			{
				InlineSpriteRenderer.SpriteEntry spriteEntry = this.spriteEntries[i];
				if (!string.IsNullOrEmpty(spriteEntry.key) && spriteEntry.sprite != null)
				{
					this.spriteDictionary[spriteEntry.key] = spriteEntry.sprite;
				}
			}
		}

		// Token: 0x06001A5C RID: 6748 RVA: 0x000790A4 File Offset: 0x000772A4
		private void PrewarmPool()
		{
			if (this.imagePrefab == null || this.parentRect == null)
			{
				return;
			}
			for (int i = 0; i < this.initialPool; i++)
			{
				Image image = global::UnityEngine.Object.Instantiate<Image>(this.imagePrefab, this.parentRect);
				image.gameObject.SetActive(false);
				this.pooledImages.Push(image);
			}
		}

		// Token: 0x06001A5D RID: 6749 RVA: 0x0007910C File Offset: 0x0007730C
		private Image GetImage()
		{
			if (this.pooledImages.Count > 0)
			{
				Image image = this.pooledImages.Pop();
				image.gameObject.SetActive(true);
				this.activeImages.Add(image);
				return image;
			}
			Image image2 = global::UnityEngine.Object.Instantiate<Image>(this.imagePrefab, this.parentRect);
			image2.gameObject.SetActive(true);
			this.activeImages.Add(image2);
			return image2;
		}

		// Token: 0x06001A5E RID: 6750 RVA: 0x00079178 File Offset: 0x00077378
		private void ClearActive()
		{
			foreach (Image image in this.activeImages)
			{
				if (image != null)
				{
					image.gameObject.SetActive(false);
					this.pooledImages.Push(image);
				}
			}
			this.activeImages.Clear();
		}

		// Token: 0x06001A5F RID: 6751 RVA: 0x000791F0 File Offset: 0x000773F0
		public void ApplySpans(IReadOnlyList<InlineSpan> spans, TMP_Text source)
		{
			if (this.imagePrefab == null || this.parentRect == null)
			{
				return;
			}
			this.ClearActive();
			int num = 0;
			for (int i = 0; i < spans.Count; i++)
			{
				if (spans[i].scheme == this.scheme && this.spriteDictionary.ContainsKey(spans[i].key))
				{
					num++;
				}
			}
			while (this.pooledImages.Count + this.activeImages.Count < num)
			{
				Image image = global::UnityEngine.Object.Instantiate<Image>(this.imagePrefab, this.parentRect);
				image.gameObject.SetActive(false);
				this.pooledImages.Push(image);
			}
			Vector2 pivot = this.parentRect.pivot;
			for (int j = 0; j < spans.Count; j++)
			{
				InlineSpan inlineSpan = spans[j];
				Sprite sprite;
				if (!(inlineSpan.scheme != this.scheme) && this.spriteDictionary.TryGetValue(inlineSpan.key, out sprite) && !(sprite == null))
				{
					Image image2 = this.GetImage();
					image2.sprite = sprite;
					image2.preserveAspect = true;
					RectTransform rectTransform = (RectTransform)image2.transform;
					rectTransform.SetParent(this.parentRect, false);
					rectTransform.anchorMin = pivot;
					rectTransform.anchorMax = pivot;
					rectTransform.pivot = new Vector2(0.5f, 0.5f);
					rectTransform.anchoredPosition = inlineSpan.center;
					rectTransform.sizeDelta = new Vector2(inlineSpan.sideLength, inlineSpan.sideLength) * this.sizeMultiplier;
				}
			}
		}

		// Token: 0x06001A60 RID: 6752 RVA: 0x00079399 File Offset: 0x00077599
		public void SetSprite(string key, Sprite sprite)
		{
			if (string.IsNullOrEmpty(key))
			{
				return;
			}
			if (sprite == null)
			{
				this.spriteDictionary.Remove(key);
				return;
			}
			this.spriteDictionary[key] = sprite;
		}

		// Token: 0x04001512 RID: 5394
		[Header("Finder")]
		[Tooltip("Reference to the InlineTextSpanFinder that emits span events.")]
		[SerializeField]
		private InlineTextSpanFinder spanFinder;

		// Token: 0x04001513 RID: 5395
		[Header("Filtering")]
		[Tooltip("Only spans with this scheme are rendered (e.g., \"icon\").")]
		[SerializeField]
		private string scheme = "icon";

		// Token: 0x04001514 RID: 5396
		[Header("Setup")]
		[Tooltip("Parent RectTransform (usually the same object as the TMP). If null, uses this RectTransform.")]
		[SerializeField]
		private RectTransform parentRect;

		// Token: 0x04001515 RID: 5397
		[Tooltip("Prefab for the UI Image used to render a sprite.")]
		[SerializeField]
		private Image imagePrefab;

		// Token: 0x04001516 RID: 5398
		[Tooltip("Prewarm count for pooled Images.")]
		[SerializeField]
		private int initialPool = 8;

		// Token: 0x04001517 RID: 5399
		[Header("Sizing")]
		[Tooltip("Uniform scale applied on top of the finder’s suggested side length.")]
		[SerializeField]
		private float sizeMultiplier = 1f;

		// Token: 0x04001518 RID: 5400
		[Header("Sprite Dictionary")]
		[SerializeField]
		private List<InlineSpriteRenderer.SpriteEntry> spriteEntries = new List<InlineSpriteRenderer.SpriteEntry>();

		// Token: 0x04001519 RID: 5401
		private readonly Dictionary<string, Sprite> spriteDictionary = new Dictionary<string, Sprite>();

		// Token: 0x0400151A RID: 5402
		private readonly List<Image> activeImages = new List<Image>();

		// Token: 0x0400151B RID: 5403
		private readonly Stack<Image> pooledImages = new Stack<Image>();

		// Token: 0x02000426 RID: 1062
		[Serializable]
		private struct SpriteEntry
		{
			// Token: 0x0400151C RID: 5404
			public string key;

			// Token: 0x0400151D RID: 5405
			public Sprite sprite;
		}
	}
}
