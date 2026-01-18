using System;
using Endless.Shared;
using Endless.Shared.Tweens;
using Endless.Shared.UI;
using Endless.Shared.Validation;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Gameplay;

public class PoolableInventoryIcon : MonoBehaviour, IPoolableT, IValidatable
{
	[SerializeField]
	private UIText[] quantityTexts = Array.Empty<UIText>();

	[SerializeField]
	private TweenCollection displayTweenCollection;

	[SerializeField]
	private TweenCollection quantityTextsDisplayTweenCollection;

	[SerializeField]
	private TweenCollection quantityTextsDisplayCompleteTweenCollection;

	public bool IsUi => true;

	[field: SerializeField]
	public MonoBehaviour Prefab { get; set; }

	[field: SerializeField]
	public Image Image { get; private set; }

	private void Start()
	{
		quantityTextsDisplayTweenCollection.OnAllTweenCompleted.AddListener(quantityTextsDisplayCompleteTweenCollection.Tween);
	}

	public void Validate()
	{
		quantityTextsDisplayTweenCollection.Validate();
	}

	public void OnDespawn()
	{
		displayTweenCollection.Cancel();
		quantityTextsDisplayTweenCollection.Cancel();
		quantityTextsDisplayCompleteTweenCollection.Cancel();
		displayTweenCollection.SetToEnd();
		quantityTextsDisplayTweenCollection.SetToEnd();
		quantityTextsDisplayCompleteTweenCollection.SetToEnd();
	}

	public void SetQuantity(string quantity)
	{
		UIText[] array = quantityTexts;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Value = quantity;
		}
	}

	public void PlayDisplayTweenCollection(float delay = 0f)
	{
		displayTweenCollection.SetToStart();
		quantityTextsDisplayTweenCollection.SetToStart();
		quantityTextsDisplayCompleteTweenCollection.SetToStart();
		displayTweenCollection.SetDelay(delay);
		displayTweenCollection.Tween(quantityTextsDisplayTweenCollection.Tween);
	}
}
