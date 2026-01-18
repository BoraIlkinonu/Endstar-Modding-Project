using Endless.Shared;
using TMPro;
using UnityEngine;

namespace Endless.Gameplay;

public class Cosmetic3DIconRenderer : CosmeticIconRendererBase<PoolableInventoryIcon3D>
{
	[Header("3D Settings")]
	[Tooltip("Depth offset so icons draw slightly in front of the text mesh.")]
	[SerializeField]
	private float depthOffset = -0.01f;

	private new void Start()
	{
		MonoBehaviourSingleton<PoolManagerT>.Instance.PrewarmPool(iconPrefab, 4);
	}

	protected override PoolableInventoryIcon3D SpawnIcon(InlineSpan span, TMP_Text source, int displayIndex)
	{
		PoolableInventoryIcon3D poolableInventoryIcon3D = MonoBehaviourSingleton<PoolManagerT>.Instance.Spawn(iconPrefab);
		poolableInventoryIcon3D.transform.SetParent(source.transform, worldPositionStays: false);
		return poolableInventoryIcon3D;
	}

	protected override void UpdateIcon(PoolableInventoryIcon3D icon, InlineSpan span, TMP_Text source, int displayIndex)
	{
		ResolveSpriteAndQuantity(span, out var sprite, out var quantityText);
		icon.SetSprite(sprite);
		icon.SetQuantity(quantityText);
		Vector3 localPosition = new Vector3(span.center.x, span.center.y, depthOffset);
		icon.transform.localPosition = localPosition;
		float size = span.sideLength * sizeMultiplier;
		icon.SetSize(size);
	}

	protected override void DespawnIcon(PoolableInventoryIcon3D icon)
	{
		MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn(icon);
	}
}
