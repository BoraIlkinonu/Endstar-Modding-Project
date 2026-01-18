using System;
using Endless.Shared;
using Endless.Shared.Tweens;
using Endless.Shared.UI;
using Endless.Shared.Validation;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Endless.Gameplay
{
	// Token: 0x0200036C RID: 876
	public class PoolableInventoryIcon : MonoBehaviour, IPoolableT, IValidatable
	{
		// Token: 0x170004BC RID: 1212
		// (get) Token: 0x06001680 RID: 5760 RVA: 0x00017586 File Offset: 0x00015786
		public bool IsUi
		{
			get
			{
				return true;
			}
		}

		// Token: 0x170004BD RID: 1213
		// (get) Token: 0x06001681 RID: 5761 RVA: 0x00069DD7 File Offset: 0x00067FD7
		// (set) Token: 0x06001682 RID: 5762 RVA: 0x00069DDF File Offset: 0x00067FDF
		public MonoBehaviour Prefab { get; set; }

		// Token: 0x170004BE RID: 1214
		// (get) Token: 0x06001683 RID: 5763 RVA: 0x00069DE8 File Offset: 0x00067FE8
		// (set) Token: 0x06001684 RID: 5764 RVA: 0x00069DF0 File Offset: 0x00067FF0
		public Image Image { get; private set; }

		// Token: 0x06001685 RID: 5765 RVA: 0x00069DF9 File Offset: 0x00067FF9
		private void Start()
		{
			this.quantityTextsDisplayTweenCollection.OnAllTweenCompleted.AddListener(new UnityAction(this.quantityTextsDisplayCompleteTweenCollection.Tween));
		}

		// Token: 0x06001686 RID: 5766 RVA: 0x00069E1C File Offset: 0x0006801C
		public void Validate()
		{
			this.quantityTextsDisplayTweenCollection.Validate();
		}

		// Token: 0x06001687 RID: 5767 RVA: 0x00069E2C File Offset: 0x0006802C
		public void OnDespawn()
		{
			this.displayTweenCollection.Cancel();
			this.quantityTextsDisplayTweenCollection.Cancel();
			this.quantityTextsDisplayCompleteTweenCollection.Cancel();
			this.displayTweenCollection.SetToEnd();
			this.quantityTextsDisplayTweenCollection.SetToEnd();
			this.quantityTextsDisplayCompleteTweenCollection.SetToEnd();
		}

		// Token: 0x06001688 RID: 5768 RVA: 0x00069E7C File Offset: 0x0006807C
		public void SetQuantity(string quantity)
		{
			UIText[] array = this.quantityTexts;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Value = quantity;
			}
		}

		// Token: 0x06001689 RID: 5769 RVA: 0x00069EA8 File Offset: 0x000680A8
		public void PlayDisplayTweenCollection(float delay = 0f)
		{
			this.displayTweenCollection.SetToStart();
			this.quantityTextsDisplayTweenCollection.SetToStart();
			this.quantityTextsDisplayCompleteTweenCollection.SetToStart();
			this.displayTweenCollection.SetDelay(delay);
			this.displayTweenCollection.Tween(new Action(this.quantityTextsDisplayTweenCollection.Tween));
		}

		// Token: 0x0400122C RID: 4652
		[SerializeField]
		private UIText[] quantityTexts = Array.Empty<UIText>();

		// Token: 0x0400122D RID: 4653
		[SerializeField]
		private TweenCollection displayTweenCollection;

		// Token: 0x0400122E RID: 4654
		[SerializeField]
		private TweenCollection quantityTextsDisplayTweenCollection;

		// Token: 0x0400122F RID: 4655
		[SerializeField]
		private TweenCollection quantityTextsDisplayCompleteTweenCollection;
	}
}
