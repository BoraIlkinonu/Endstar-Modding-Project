using System;
using Endless.Shared;
using TMPro;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x0200036D RID: 877
	public class PoolableInventoryIcon3D : MonoBehaviour, IPoolableT
	{
		// Token: 0x170004BF RID: 1215
		// (get) Token: 0x0600168B RID: 5771 RVA: 0x00069F11 File Offset: 0x00068111
		// (set) Token: 0x0600168C RID: 5772 RVA: 0x00069F19 File Offset: 0x00068119
		public MonoBehaviour Prefab { get; set; }

		// Token: 0x0600168D RID: 5773 RVA: 0x00069F24 File Offset: 0x00068124
		public void SetSprite(Sprite sprite)
		{
			this.spriteRenderer.sprite = sprite;
			if (sprite != null)
			{
				this.spriteRenderer.transform.localPosition = -sprite.bounds.center;
			}
		}

		// Token: 0x0600168E RID: 5774 RVA: 0x00069F69 File Offset: 0x00068169
		public void SetQuantity(string newText)
		{
			if (this.quantityText != null)
			{
				this.quantityText.text = newText;
			}
		}

		// Token: 0x0600168F RID: 5775 RVA: 0x00069F88 File Offset: 0x00068188
		public void SetSize(float side)
		{
			if (this.spriteRenderer.sprite == null || this.iconRoot == null)
			{
				return;
			}
			Vector2 vector = this.spriteRenderer.sprite.bounds.size;
			float num = Mathf.Max(vector.x, vector.y);
			if (num <= 0.0001f)
			{
				return;
			}
			float num2 = side / num;
			this.iconRoot.localScale = Vector3.one * num2;
		}

		// Token: 0x04001232 RID: 4658
		[Header("Rendering")]
		[SerializeField]
		private Transform iconRoot;

		// Token: 0x04001233 RID: 4659
		[SerializeField]
		private SpriteRenderer spriteRenderer;

		// Token: 0x04001234 RID: 4660
		[Header("Quantity")]
		[SerializeField]
		private TMP_Text quantityText;
	}
}
