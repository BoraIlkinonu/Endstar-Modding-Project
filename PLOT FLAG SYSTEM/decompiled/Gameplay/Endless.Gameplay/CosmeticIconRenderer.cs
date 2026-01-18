using Endless.Shared;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Gameplay;

public class CosmeticIconRenderer : CosmeticIconRendererBase<PoolableInventoryIcon>
{
	[Header("UI Prefab")]
	[SerializeField]
	private PoolableInventoryIcon poolableInventoryIconPrefab;

	private new void Start()
	{
		MonoBehaviourSingleton<PoolManagerT>.Instance.PrewarmPool(poolableInventoryIconPrefab, 4);
	}

	protected override PoolableInventoryIcon SpawnIcon(InlineSpan span, TMP_Text source, int displayIndex)
	{
		PoolableInventoryIcon poolableInventoryIcon = MonoBehaviourSingleton<PoolManagerT>.Instance.Spawn(poolableInventoryIconPrefab);
		RectTransform obj = (RectTransform)poolableInventoryIcon.transform;
		obj.SetParent(spanFinder.TargetRectTransform, worldPositionStays: false);
		obj.localScale = Vector3.one;
		Vector2 anchorMax = (obj.anchorMin = spanFinder.TargetRectTransform.pivot);
		obj.anchorMax = anchorMax;
		obj.pivot = new Vector2(0.5f, 0.5f);
		poolableInventoryIcon.PlayDisplayTweenCollection(0.25f + (float)displayIndex * 0.1f);
		return poolableInventoryIcon;
	}

	protected override void UpdateIcon(PoolableInventoryIcon icon, InlineSpan span, TMP_Text source, int displayIndex)
	{
		ResolveSpriteAndQuantity(span, out var sprite, out var quantityText);
		Image image = icon.Image;
		image.sprite = sprite;
		image.preserveAspect = true;
		icon.SetQuantity(quantityText);
		RectTransform obj = (RectTransform)icon.transform;
		obj.anchoredPosition = span.center;
		obj.sizeDelta = new Vector2(span.sideLength, span.sideLength) * sizeMultiplier;
	}

	protected override void DespawnIcon(PoolableInventoryIcon icon)
	{
		MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn(icon);
	}
}
