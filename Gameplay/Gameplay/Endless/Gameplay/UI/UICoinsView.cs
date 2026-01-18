using System;
using System.Collections;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay.UI
{
	// Token: 0x020003D1 RID: 977
	[RequireComponent(typeof(UIDisplayAndHideHandler))]
	public class UICoinsView : UIGameObject
	{
		// Token: 0x060018AD RID: 6317 RVA: 0x000724F0 File Offset: 0x000706F0
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.amountText.text = StringUtility.AbbreviateQuantity(this.amountDisplayed);
			NetworkBehaviourSingleton<ResourceManager>.Instance.OnLocalCoinAmountUpdated.AddListener(new UnityAction<int>(this.CountUpTo));
			MonoBehaviourSingleton<GameplayManager>.Instance.OnGameplayCleanup.AddListener(new UnityAction(this.Hide));
			base.TryGetComponent<UIDisplayAndHideHandler>(out this.displayAndHideHandler);
			this.displayAndHideHandler.OnDisplayComplete.AddListener(new UnityAction(this.OnDisplayComplete));
			this.displayAndHideHandler.OnHideComplete.AddListener(new UnityAction(this.OnHideComplete));
			this.displayAndHideHandler.SetToHideEnd(true);
		}

		// Token: 0x060018AE RID: 6318 RVA: 0x000725B2 File Offset: 0x000707B2
		private void Display()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Display", Array.Empty<object>());
			}
			if (this.amountDisplayed == 0)
			{
				this.isWaitingForInitialDisplay = true;
			}
			this.displayAndHideHandler.Display();
			this.isDisplayed = true;
		}

		// Token: 0x060018AF RID: 6319 RVA: 0x000725F0 File Offset: 0x000707F0
		private void OnDisplayComplete()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDisplayComplete", Array.Empty<object>());
			}
			if (this.isWaitingForInitialDisplay)
			{
				this.isWaitingForInitialDisplay = false;
				if (this.countUpToCoroutine != null)
				{
					MonoBehaviourSingleton<UICoroutineManager>.Instance.StartThisCoroutine(this.StartCountUpAfterDelay());
				}
			}
		}

		// Token: 0x060018B0 RID: 6320 RVA: 0x0007263D File Offset: 0x0007083D
		private IEnumerator StartCountUpAfterDelay()
		{
			yield return new WaitForSeconds(this.incrementDuration);
			if (this == null || this.countUpToCoroutine == null)
			{
				yield break;
			}
			MonoBehaviourSingleton<UICoroutineManager>.Instance.StartThisCoroutine(this.countUpToCoroutine);
			this.countUpToCoroutine = null;
			yield break;
		}

		// Token: 0x060018B1 RID: 6321 RVA: 0x0007264C File Offset: 0x0007084C
		private void CountUpTo(int target)
		{
			if (!this.isDisplayed)
			{
				this.Display();
			}
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "CountUpTo", new object[] { target });
			}
			this.tooltip.SetTooltip(string.Format("{0}: {1:N0}", "Resource", target));
			this.countUpToAmount = target;
			this.countUpToCoroutine = this.CountUpToCoroutine();
			if (!this.isWaitingForInitialDisplay)
			{
				MonoBehaviourSingleton<UICoroutineManager>.Instance.StartThisCoroutine(this.countUpToCoroutine);
			}
		}

		// Token: 0x060018B2 RID: 6322 RVA: 0x000726D5 File Offset: 0x000708D5
		private IEnumerator CountUpToCoroutine()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "CountUpToCoroutine", Array.Empty<object>());
			}
			int num = this.countUpToAmount - this.amountDisplayed;
			float num2 = this.incrementDuration * (float)num;
			if (num2 > this.maxTotalIncrementDuration)
			{
				num2 = Mathf.Clamp(num2, this.incrementDuration, this.maxTotalIncrementDuration);
			}
			int increments = Mathf.RoundToInt(num2 / this.incrementDuration);
			int incrementAmount = Mathf.RoundToInt((float)num / (float)increments);
			while (increments > 0)
			{
				int num3 = increments;
				increments = num3 - 1;
				this.amountDisplayed += incrementAmount;
				this.amountText.text = StringUtility.AbbreviateQuantity(this.amountDisplayed);
				this.updateAmountTweens.Tween();
				yield return new WaitForSeconds(this.incrementDuration);
			}
			if (this.amountDisplayed != this.countUpToAmount)
			{
				this.amountDisplayed = this.countUpToAmount;
				this.amountText.text = StringUtility.AbbreviateQuantity(this.amountDisplayed);
				this.updateAmountTweens.Tween();
			}
			this.countUpToCoroutine = null;
			yield break;
		}

		// Token: 0x060018B3 RID: 6323 RVA: 0x000726E4 File Offset: 0x000708E4
		private void Hide()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Hide", Array.Empty<object>());
			}
			this.isDisplayed = false;
			this.displayAndHideHandler.Hide();
		}

		// Token: 0x060018B4 RID: 6324 RVA: 0x00072710 File Offset: 0x00070910
		private void OnHideComplete()
		{
			if (this.countUpToCoroutine == null)
			{
				return;
			}
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnHideComplete", Array.Empty<object>());
			}
			this.updateAmountTweens.ForceDone(true);
			this.amountDisplayed = 0;
			this.countUpToAmount = 0;
			this.amountText.text = StringUtility.AbbreviateQuantity(this.amountDisplayed);
			if (this.countUpToCoroutine != null)
			{
				MonoBehaviourSingleton<UICoroutineManager>.Instance.StopCoroutine(this.countUpToCoroutine);
				this.countUpToCoroutine = null;
			}
			base.gameObject.SetActive(false);
		}

		// Token: 0x040013C8 RID: 5064
		private const string RESOURCE_NAME = "Resource";

		// Token: 0x040013C9 RID: 5065
		[SerializeField]
		private TextMeshProUGUI amountText;

		// Token: 0x040013CA RID: 5066
		[SerializeField]
		private UITooltip tooltip;

		// Token: 0x040013CB RID: 5067
		[SerializeField]
		private float incrementDuration = 0.1f;

		// Token: 0x040013CC RID: 5068
		[SerializeField]
		private float maxTotalIncrementDuration = 3f;

		// Token: 0x040013CD RID: 5069
		[Header("Tweens")]
		[SerializeField]
		private TweenCollection updateAmountTweens;

		// Token: 0x040013CE RID: 5070
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x040013CF RID: 5071
		private UIDisplayAndHideHandler displayAndHideHandler;

		// Token: 0x040013D0 RID: 5072
		private bool isDisplayed;

		// Token: 0x040013D1 RID: 5073
		private int amountDisplayed;

		// Token: 0x040013D2 RID: 5074
		private int countUpToAmount;

		// Token: 0x040013D3 RID: 5075
		private IEnumerator countUpToCoroutine;

		// Token: 0x040013D4 RID: 5076
		private bool isWaitingForInitialDisplay;
	}
}
