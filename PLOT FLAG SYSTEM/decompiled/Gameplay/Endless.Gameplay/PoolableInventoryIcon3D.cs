using Endless.Shared;
using TMPro;
using UnityEngine;

namespace Endless.Gameplay;

public class PoolableInventoryIcon3D : MonoBehaviour, IPoolableT
{
	[Header("Rendering")]
	[SerializeField]
	private Transform iconRoot;

	[SerializeField]
	private SpriteRenderer spriteRenderer;

	[Header("Quantity")]
	[SerializeField]
	private TMP_Text quantityText;

	[field: SerializeField]
	public MonoBehaviour Prefab { get; set; }

	public void SetSprite(Sprite sprite)
	{
		spriteRenderer.sprite = sprite;
		if (sprite != null)
		{
			spriteRenderer.transform.localPosition = -sprite.bounds.center;
		}
	}

	public void SetQuantity(string newText)
	{
		if (quantityText != null)
		{
			quantityText.text = newText;
		}
	}

	public void SetSize(float side)
	{
		if (!(spriteRenderer.sprite == null) && !(iconRoot == null))
		{
			Vector2 vector = spriteRenderer.sprite.bounds.size;
			float num = Mathf.Max(vector.x, vector.y);
			if (!(num <= 0.0001f))
			{
				float num2 = side / num;
				iconRoot.localScale = Vector3.one * num2;
			}
		}
	}
}
