using System;
using Endless.Shared;
using TMPro;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000367 RID: 871
	public class Cosmetic3DIconRenderer : CosmeticIconRendererBase<PoolableInventoryIcon3D>
	{
		// Token: 0x06001662 RID: 5730 RVA: 0x00069343 File Offset: 0x00067543
		private new void Start()
		{
			MonoBehaviourSingleton<PoolManagerT>.Instance.PrewarmPool<PoolableInventoryIcon3D>(this.iconPrefab, 4);
		}

		// Token: 0x06001663 RID: 5731 RVA: 0x00069358 File Offset: 0x00067558
		protected override PoolableInventoryIcon3D SpawnIcon(InlineSpan span, TMP_Text source, int displayIndex)
		{
			PoolableInventoryIcon3D poolableInventoryIcon3D = MonoBehaviourSingleton<PoolManagerT>.Instance.Spawn<PoolableInventoryIcon3D>(this.iconPrefab, default(Vector3), default(Quaternion), null);
			poolableInventoryIcon3D.transform.SetParent(source.transform, false);
			return poolableInventoryIcon3D;
		}

		// Token: 0x06001664 RID: 5732 RVA: 0x0006939C File Offset: 0x0006759C
		protected override void UpdateIcon(PoolableInventoryIcon3D icon, InlineSpan span, TMP_Text source, int displayIndex)
		{
			Sprite sprite;
			string text;
			base.ResolveSpriteAndQuantity(span, out sprite, out text);
			icon.SetSprite(sprite);
			icon.SetQuantity(text);
			Vector3 vector = new Vector3(span.center.x, span.center.y, this.depthOffset);
			icon.transform.localPosition = vector;
			float num = span.sideLength * this.sizeMultiplier;
			icon.SetSize(num);
		}

		// Token: 0x06001665 RID: 5733 RVA: 0x00069406 File Offset: 0x00067606
		protected override void DespawnIcon(PoolableInventoryIcon3D icon)
		{
			MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn<PoolableInventoryIcon3D>(icon);
		}

		// Token: 0x04001214 RID: 4628
		[Header("3D Settings")]
		[Tooltip("Depth offset so icons draw slightly in front of the text mesh.")]
		[SerializeField]
		private float depthOffset = -0.01f;
	}
}
