using System;
using System.Collections.Generic;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay.UI
{
	// Token: 0x020003BF RID: 959
	public class UIGameplayPlayerListCellView : UIBasePlayerListCellView
	{
		// Token: 0x0600186F RID: 6255 RVA: 0x000716E0 File Offset: 0x0006F8E0
		public override void OnDespawn()
		{
			base.OnDespawn();
			foreach (UIHealthViewHeart uihealthViewHeart in this.hearts)
			{
				MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn<UIHealthViewHeart>(uihealthViewHeart);
			}
			this.hearts.Clear();
			this.health = 0;
			this.maxHealth = 0;
			this.healthComponent.OnHealthChanged.RemoveListener(new UnityAction<int, int>(this.HealthChanged));
			this.healthComponent.OnMaxHealthChanged.RemoveListener(new UnityAction<int, int>(this.MaxHealthChanged));
		}

		// Token: 0x06001870 RID: 6256 RVA: 0x00071790 File Offset: 0x0006F990
		public override void View(UIBaseListView<PlayerReferenceManager> listView, int dataIndex)
		{
			base.View(listView, dataIndex);
			this.SetSpacingForSocialButton();
			this.healthComponent = base.PlayerReferenceManager.HealthComponent;
			this.healthComponent.OnMaxHealthChanged.AddListener(new UnityAction<int, int>(this.MaxHealthChanged));
			this.healthComponent.OnHealthChanged.AddListener(new UnityAction<int, int>(this.HealthChanged));
			this.MaxHealthChanged(0, this.healthComponent.MaxHealth);
			this.HealthChanged(0, this.healthComponent.CurrentHealth);
		}

		// Token: 0x06001871 RID: 6257 RVA: 0x00071818 File Offset: 0x0006FA18
		public void SetSpacingForSocialButton()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethodWithAppension(this, "SetSpacingForSocialButton", base.ViewIndex.ToString(), Array.Empty<object>());
			}
			this.rectTransformDictionary.Apply((base.ListView.IndexOf(this) == 0) ? this.theFirstEntryKey : this.notTheFirstEntryKey);
		}

		// Token: 0x06001872 RID: 6258 RVA: 0x00071874 File Offset: 0x0006FA74
		private void MaxHealthChanged(int previousValue, int newValue)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "MaxHealthChanged", new object[] { previousValue, newValue });
			}
			if (previousValue == newValue || this.maxHealth == newValue)
			{
				if (base.VerboseLogging)
				{
					if (previousValue == newValue)
					{
						DebugUtility.Log("previousValue == newValue", this);
					}
					if (this.maxHealth == newValue)
					{
						DebugUtility.Log("maxHealth == newValue", this);
					}
				}
				return;
			}
			this.maxHealth = newValue;
			if (previousValue < 0)
			{
				DebugUtility.LogWarning(this, "MaxHealthChanged", "UIGameplayPlayerListCellView doesn't support a previousValue below zero", new object[] { previousValue, newValue });
				previousValue = 0;
			}
			if (newValue < 0)
			{
				DebugUtility.LogWarning(this, "MaxHealthChanged", "UIGameplayPlayerListCellView doesn't support a newValue below zero", new object[] { previousValue, newValue });
				newValue = 0;
			}
			bool flag = newValue > previousValue;
			int num = 1;
			if (flag)
			{
				for (int i = previousValue; i < newValue; i++)
				{
					UIHealthViewHeart uihealthViewHeart = ((i % 2 == 0) ? this.leftHeartSource : this.rightHeartSource);
					PoolManagerT instance = MonoBehaviourSingleton<PoolManagerT>.Instance;
					UIHealthViewHeart uihealthViewHeart2 = uihealthViewHeart;
					Transform transform = this.heartsContainer;
					UIHealthViewHeart uihealthViewHeart3 = instance.Spawn<UIHealthViewHeart>(uihealthViewHeart2, default(Vector3), default(Quaternion), transform);
					this.hearts.Add(uihealthViewHeart3);
					uihealthViewHeart3.Initialize(i, num);
					num++;
				}
				return;
			}
			int num2 = previousValue;
			while (num2-- > newValue)
			{
				UIHealthViewHeart uihealthViewHeart4 = this.hearts[num2];
				this.hearts.RemoveAt(num2);
				uihealthViewHeart4.TweenAwayAndDespawn(num);
				num++;
			}
		}

		// Token: 0x06001873 RID: 6259 RVA: 0x000719E8 File Offset: 0x0006FBE8
		private void HealthChanged(int previousValue, int newValue)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "HealthChanged", new object[] { previousValue, newValue });
			}
			if (this.maxHealth != this.healthComponent.MaxHealth)
			{
				this.MaxHealthChanged(this.maxHealth, this.healthComponent.MaxHealth);
			}
			if (newValue > this.maxHealth)
			{
				this.MaxHealthChanged(this.maxHealth, newValue);
			}
			if (previousValue == newValue || newValue == this.health)
			{
				return;
			}
			this.health = newValue;
			bool flag = newValue > previousValue;
			int num = 1;
			if (flag)
			{
				for (int i = previousValue; i < newValue; i++)
				{
					this.hearts[i].Toggle(true, num);
					num++;
				}
				return;
			}
			if (previousValue > this.healthComponent.MaxHealth)
			{
				previousValue = this.healthComponent.MaxHealth;
			}
			int num2 = previousValue;
			while (num2-- > newValue)
			{
				this.hearts[num2].Toggle(false, num);
				num++;
			}
		}

		// Token: 0x040013A1 RID: 5025
		[Header("UIGameplayPlayerListCellView")]
		[SerializeField]
		private UIRectTransformDictionary rectTransformDictionary;

		// Token: 0x040013A2 RID: 5026
		[Header("Health")]
		[SerializeField]
		private RectTransform heartsContainer;

		// Token: 0x040013A3 RID: 5027
		[SerializeField]
		private UIHealthViewHeart leftHeartSource;

		// Token: 0x040013A4 RID: 5028
		[SerializeField]
		private UIHealthViewHeart rightHeartSource;

		// Token: 0x040013A5 RID: 5029
		private readonly string theFirstEntryKey = "The First Entry";

		// Token: 0x040013A6 RID: 5030
		private readonly string notTheFirstEntryKey = "Not The First Entry";

		// Token: 0x040013A7 RID: 5031
		private HealthComponent healthComponent;

		// Token: 0x040013A8 RID: 5032
		private readonly List<UIHealthViewHeart> hearts = new List<UIHealthViewHeart>();

		// Token: 0x040013A9 RID: 5033
		private int maxHealth;

		// Token: 0x040013AA RID: 5034
		private int health;
	}
}
