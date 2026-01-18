using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Gameplay.Test;

public class InlineSpriteRenderer : MonoBehaviour
{
	[Serializable]
	private struct SpriteEntry
	{
		public string key;

		public Sprite sprite;
	}

	[Header("Finder")]
	[Tooltip("Reference to the InlineTextSpanFinder that emits span events.")]
	[SerializeField]
	private InlineTextSpanFinder spanFinder;

	[Header("Filtering")]
	[Tooltip("Only spans with this scheme are rendered (e.g., \"icon\").")]
	[SerializeField]
	private string scheme = "icon";

	[Header("Setup")]
	[Tooltip("Parent RectTransform (usually the same object as the TMP). If null, uses this RectTransform.")]
	[SerializeField]
	private RectTransform parentRect;

	[Tooltip("Prefab for the UI Image used to render a sprite.")]
	[SerializeField]
	private Image imagePrefab;

	[Tooltip("Prewarm count for pooled Images.")]
	[SerializeField]
	private int initialPool = 8;

	[Header("Sizing")]
	[Tooltip("Uniform scale applied on top of the finder’s suggested side length.")]
	[SerializeField]
	private float sizeMultiplier = 1f;

	[Header("Sprite Dictionary")]
	[SerializeField]
	private List<SpriteEntry> spriteEntries = new List<SpriteEntry>();

	private readonly Dictionary<string, Sprite> spriteDictionary = new Dictionary<string, Sprite>();

	private readonly List<Image> activeImages = new List<Image>();

	private readonly Stack<Image> pooledImages = new Stack<Image>();

	private void Awake()
	{
		if ((object)parentRect == null)
		{
			parentRect = GetComponent<RectTransform>();
		}
		if ((object)spanFinder == null)
		{
			spanFinder = GetComponent<InlineTextSpanFinder>();
		}
		spanFinder.SpansReady += ApplySpans;
		RebuildDictionary();
		PrewarmPool();
	}

	private void OnValidate()
	{
		RebuildDictionary();
	}

	private void RebuildDictionary()
	{
		spriteDictionary.Clear();
		for (int i = 0; i < spriteEntries.Count; i++)
		{
			SpriteEntry spriteEntry = spriteEntries[i];
			if (!string.IsNullOrEmpty(spriteEntry.key) && spriteEntry.sprite != null)
			{
				spriteDictionary[spriteEntry.key] = spriteEntry.sprite;
			}
		}
	}

	private void PrewarmPool()
	{
		if (!(imagePrefab == null) && !(parentRect == null))
		{
			for (int i = 0; i < initialPool; i++)
			{
				Image image = UnityEngine.Object.Instantiate(imagePrefab, parentRect);
				image.gameObject.SetActive(value: false);
				pooledImages.Push(image);
			}
		}
	}

	private Image GetImage()
	{
		if (pooledImages.Count > 0)
		{
			Image image = pooledImages.Pop();
			image.gameObject.SetActive(value: true);
			activeImages.Add(image);
			return image;
		}
		Image image2 = UnityEngine.Object.Instantiate(imagePrefab, parentRect);
		image2.gameObject.SetActive(value: true);
		activeImages.Add(image2);
		return image2;
	}

	private void ClearActive()
	{
		foreach (Image activeImage in activeImages)
		{
			if (activeImage != null)
			{
				activeImage.gameObject.SetActive(value: false);
				pooledImages.Push(activeImage);
			}
		}
		activeImages.Clear();
	}

	public void ApplySpans(IReadOnlyList<InlineSpan> spans, TMP_Text source)
	{
		if (imagePrefab == null || parentRect == null)
		{
			return;
		}
		ClearActive();
		int num = 0;
		for (int i = 0; i < spans.Count; i++)
		{
			if (spans[i].scheme == scheme && spriteDictionary.ContainsKey(spans[i].key))
			{
				num++;
			}
		}
		while (pooledImages.Count + activeImages.Count < num)
		{
			Image image = UnityEngine.Object.Instantiate(imagePrefab, parentRect);
			image.gameObject.SetActive(value: false);
			pooledImages.Push(image);
		}
		Vector2 pivot = parentRect.pivot;
		for (int j = 0; j < spans.Count; j++)
		{
			InlineSpan inlineSpan = spans[j];
			if (!(inlineSpan.scheme != scheme) && spriteDictionary.TryGetValue(inlineSpan.key, out var value) && !(value == null))
			{
				Image image2 = GetImage();
				image2.sprite = value;
				image2.preserveAspect = true;
				RectTransform obj = (RectTransform)image2.transform;
				obj.SetParent(parentRect, worldPositionStays: false);
				obj.anchorMin = pivot;
				obj.anchorMax = pivot;
				obj.pivot = new Vector2(0.5f, 0.5f);
				obj.anchoredPosition = inlineSpan.center;
				obj.sizeDelta = new Vector2(inlineSpan.sideLength, inlineSpan.sideLength) * sizeMultiplier;
			}
		}
	}

	public void SetSprite(string key, Sprite sprite)
	{
		if (!string.IsNullOrEmpty(key))
		{
			if (sprite == null)
			{
				spriteDictionary.Remove(key);
			}
			else
			{
				spriteDictionary[key] = sprite;
			}
		}
	}
}
