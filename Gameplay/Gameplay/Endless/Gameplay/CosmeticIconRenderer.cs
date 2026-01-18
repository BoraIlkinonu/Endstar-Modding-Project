using System;
using Endless.Shared;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Gameplay
{
	// Token: 0x02000368 RID: 872
	public class CosmeticIconRenderer : CosmeticIconRendererBase<PoolableInventoryIcon>
	{
		// Token: 0x06001667 RID: 5735 RVA: 0x00069426 File Offset: 0x00067626
		private new void Start()
		{
			MonoBehaviourSingleton<PoolManagerT>.Instance.PrewarmPool<PoolableInventoryIcon>(this.poolableInventoryIconPrefab, 4);
		}

		// Token: 0x06001668 RID: 5736 RVA: 0x0006943C File Offset: 0x0006763C
		protected override PoolableInventoryIcon SpawnIcon(InlineSpan span, TMP_Text source, int displayIndex)
		{
			PoolableInventoryIcon poolableInventoryIcon = MonoBehaviourSingleton<PoolManagerT>.Instance.Spawn<PoolableInventoryIcon>(this.poolableInventoryIconPrefab, default(Vector3), default(Quaternion), null);
			RectTransform rectTransform = (RectTransform)poolableInventoryIcon.transform;
			rectTransform.SetParent(this.spanFinder.TargetRectTransform, false);
			rectTransform.localScale = Vector3.one;
			Vector2 pivot = this.spanFinder.TargetRectTransform.pivot;
			rectTransform.anchorMin = pivot;
			rectTransform.anchorMax = pivot;
			rectTransform.pivot = new Vector2(0.5f, 0.5f);
			poolableInventoryIcon.PlayDisplayTweenCollection(0.25f + (float)displayIndex * 0.1f);
			return poolableInventoryIcon;
		}

		// Token: 0x06001669 RID: 5737 RVA: 0x000694DC File Offset: 0x000676DC
		protected override void UpdateIcon(PoolableInventoryIcon icon, InlineSpan span, TMP_Text source, int displayIndex)
		{
			Sprite sprite;
			string text;
			base.ResolveSpriteAndQuantity(span, out sprite, out text);
			Image image = icon.Image;
			image.sprite = sprite;
			image.preserveAspect = true;
			icon.SetQuantity(text);
			RectTransform rectTransform = (RectTransform)icon.transform;
			rectTransform.anchoredPosition = span.center;
			rectTransform.sizeDelta = new Vector2(span.sideLength, span.sideLength) * this.sizeMultiplier;
		}

		// Token: 0x0600166A RID: 5738 RVA: 0x00069546 File Offset: 0x00067746
		protected override void DespawnIcon(PoolableInventoryIcon icon)
		{
			MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn<PoolableInventoryIcon>(icon);
		}

		// Token: 0x04001215 RID: 4629
		[Header("UI Prefab")]
		[SerializeField]
		private PoolableInventoryIcon poolableInventoryIconPrefab;
	}
}
