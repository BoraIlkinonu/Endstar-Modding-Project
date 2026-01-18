using System.Collections.Generic;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Gameplay.UI;

public class UIGameplayPlayerListCellView : UIBasePlayerListCellView
{
	[Header("UIGameplayPlayerListCellView")]
	[SerializeField]
	private UIRectTransformDictionary rectTransformDictionary;

	[Header("Health")]
	[SerializeField]
	private RectTransform heartsContainer;

	[SerializeField]
	private UIHealthViewHeart leftHeartSource;

	[SerializeField]
	private UIHealthViewHeart rightHeartSource;

	private readonly string theFirstEntryKey = "The First Entry";

	private readonly string notTheFirstEntryKey = "Not The First Entry";

	private HealthComponent healthComponent;

	private readonly List<UIHealthViewHeart> hearts = new List<UIHealthViewHeart>();

	private int maxHealth;

	private int health;

	public override void OnDespawn()
	{
		base.OnDespawn();
		foreach (UIHealthViewHeart heart in hearts)
		{
			MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn(heart);
		}
		hearts.Clear();
		health = 0;
		maxHealth = 0;
		healthComponent.OnHealthChanged.RemoveListener(HealthChanged);
		healthComponent.OnMaxHealthChanged.RemoveListener(MaxHealthChanged);
	}

	public override void View(UIBaseListView<PlayerReferenceManager> listView, int dataIndex)
	{
		base.View(listView, dataIndex);
		SetSpacingForSocialButton();
		healthComponent = base.PlayerReferenceManager.HealthComponent;
		healthComponent.OnMaxHealthChanged.AddListener(MaxHealthChanged);
		healthComponent.OnHealthChanged.AddListener(HealthChanged);
		MaxHealthChanged(0, healthComponent.MaxHealth);
		HealthChanged(0, healthComponent.CurrentHealth);
	}

	public void SetSpacingForSocialButton()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethodWithAppension(this, "SetSpacingForSocialButton", base.ViewIndex.ToString());
		}
		rectTransformDictionary.Apply((base.ListView.IndexOf(this) == 0) ? theFirstEntryKey : notTheFirstEntryKey);
	}

	private void MaxHealthChanged(int previousValue, int newValue)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "MaxHealthChanged", previousValue, newValue);
		}
		if (previousValue == newValue || maxHealth == newValue)
		{
			if (base.VerboseLogging)
			{
				if (previousValue == newValue)
				{
					DebugUtility.Log("previousValue == newValue", this);
				}
				if (maxHealth == newValue)
				{
					DebugUtility.Log("maxHealth == newValue", this);
				}
			}
			return;
		}
		maxHealth = newValue;
		if (previousValue < 0)
		{
			DebugUtility.LogWarning(this, "MaxHealthChanged", "UIGameplayPlayerListCellView doesn't support a previousValue below zero", previousValue, newValue);
			previousValue = 0;
		}
		if (newValue < 0)
		{
			DebugUtility.LogWarning(this, "MaxHealthChanged", "UIGameplayPlayerListCellView doesn't support a newValue below zero", previousValue, newValue);
			newValue = 0;
		}
		bool num = newValue > previousValue;
		int num2 = 1;
		if (num)
		{
			for (int i = previousValue; i < newValue; i++)
			{
				UIHealthViewHeart prefab = ((i % 2 == 0) ? leftHeartSource : rightHeartSource);
				PoolManagerT instance = MonoBehaviourSingleton<PoolManagerT>.Instance;
				Transform parent = heartsContainer;
				UIHealthViewHeart uIHealthViewHeart = instance.Spawn(prefab, default(Vector3), default(Quaternion), parent);
				hearts.Add(uIHealthViewHeart);
				uIHealthViewHeart.Initialize(i, num2);
				num2++;
			}
		}
		else
		{
			int index = previousValue;
			while (index-- > newValue)
			{
				UIHealthViewHeart uIHealthViewHeart2 = hearts[index];
				hearts.RemoveAt(index);
				uIHealthViewHeart2.TweenAwayAndDespawn(num2);
				num2++;
			}
		}
	}

	private void HealthChanged(int previousValue, int newValue)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "HealthChanged", previousValue, newValue);
		}
		if (maxHealth != healthComponent.MaxHealth)
		{
			MaxHealthChanged(maxHealth, healthComponent.MaxHealth);
		}
		if (newValue > maxHealth)
		{
			MaxHealthChanged(maxHealth, newValue);
		}
		if (previousValue == newValue || newValue == health)
		{
			return;
		}
		health = newValue;
		bool num = newValue > previousValue;
		int num2 = 1;
		if (num)
		{
			for (int i = previousValue; i < newValue; i++)
			{
				hearts[i].Toggle(state: true, num2);
				num2++;
			}
			return;
		}
		if (previousValue > healthComponent.MaxHealth)
		{
			previousValue = healthComponent.MaxHealth;
		}
		int index = previousValue;
		while (index-- > newValue)
		{
			hearts[index].Toggle(state: false, num2);
			num2++;
		}
	}
}
