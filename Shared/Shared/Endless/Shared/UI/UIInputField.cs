using System;
using System.Collections;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Endless.Shared.UI
{
	// Token: 0x02000118 RID: 280
	[RequireComponent(typeof(LayoutElement))]
	public class UIInputField : TMP_InputField
	{
		// Token: 0x17000117 RID: 279
		// (get) Token: 0x060006BC RID: 1724 RVA: 0x0001CBCE File Offset: 0x0001ADCE
		// (set) Token: 0x060006BD RID: 1725 RVA: 0x0001CBD6 File Offset: 0x0001ADD6
		protected bool VerboseLogging { get; set; }

		// Token: 0x17000118 RID: 280
		// (get) Token: 0x060006BE RID: 1726 RVA: 0x0001CBDF File Offset: 0x0001ADDF
		public override float minWidth
		{
			get
			{
				if (!this.LayoutElement.enabled)
				{
					return base.minWidth;
				}
				return this.LayoutElement.minWidth;
			}
		}

		// Token: 0x17000119 RID: 281
		// (get) Token: 0x060006BF RID: 1727 RVA: 0x0001CC00 File Offset: 0x0001AE00
		public override float preferredWidth
		{
			get
			{
				if (!this.LayoutElement.enabled)
				{
					return base.preferredWidth;
				}
				return this.LayoutElement.preferredWidth;
			}
		}

		// Token: 0x1700011A RID: 282
		// (get) Token: 0x060006C0 RID: 1728 RVA: 0x0001CC21 File Offset: 0x0001AE21
		public override float flexibleWidth
		{
			get
			{
				if (!this.LayoutElement.enabled)
				{
					return base.flexibleWidth;
				}
				return this.LayoutElement.flexibleWidth;
			}
		}

		// Token: 0x1700011B RID: 283
		// (get) Token: 0x060006C1 RID: 1729 RVA: 0x0001CC42 File Offset: 0x0001AE42
		public override float minHeight
		{
			get
			{
				if (!this.LayoutElement.enabled)
				{
					return base.minHeight;
				}
				return this.LayoutElement.minHeight;
			}
		}

		// Token: 0x1700011C RID: 284
		// (get) Token: 0x060006C2 RID: 1730 RVA: 0x0001CC63 File Offset: 0x0001AE63
		public override float preferredHeight
		{
			get
			{
				if (!this.LayoutElement.enabled)
				{
					return base.preferredHeight;
				}
				return this.LayoutElement.preferredHeight;
			}
		}

		// Token: 0x1700011D RID: 285
		// (get) Token: 0x060006C3 RID: 1731 RVA: 0x0001CC84 File Offset: 0x0001AE84
		public override float flexibleHeight
		{
			get
			{
				if (!this.LayoutElement.enabled)
				{
					return base.flexibleHeight;
				}
				return this.LayoutElement.flexibleHeight;
			}
		}

		// Token: 0x1700011E RID: 286
		// (get) Token: 0x060006C4 RID: 1732 RVA: 0x0001CCA5 File Offset: 0x0001AEA5
		public override int layoutPriority
		{
			get
			{
				if (!this.LayoutElement.enabled)
				{
					return base.layoutPriority;
				}
				return this.LayoutElement.layoutPriority;
			}
		}

		// Token: 0x1700011F RID: 287
		// (get) Token: 0x060006C5 RID: 1733 RVA: 0x000050D2 File Offset: 0x000032D2
		public virtual bool CanSelectNextUiOnTab
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000120 RID: 288
		// (get) Token: 0x060006C6 RID: 1734 RVA: 0x0001CCC6 File Offset: 0x0001AEC6
		public UnityEvent<string> DeselectAndValueChangedUnityEvent { get; } = new UnityEvent<string>();

		// Token: 0x17000121 RID: 289
		// (get) Token: 0x060006C7 RID: 1735 RVA: 0x0001CCCE File Offset: 0x0001AECE
		public UnityEvent<int> CaretPositionChangedUnityEvent { get; } = new UnityEvent<int>();

		// Token: 0x17000122 RID: 290
		// (get) Token: 0x060006C8 RID: 1736 RVA: 0x0001CCD6 File Offset: 0x0001AED6
		public RectTransform RectTransform
		{
			get
			{
				if (!this.rectTransform)
				{
					base.TryGetComponent<RectTransform>(out this.rectTransform);
				}
				return this.rectTransform;
			}
		}

		// Token: 0x17000123 RID: 291
		// (get) Token: 0x060006C9 RID: 1737 RVA: 0x0001CCF8 File Offset: 0x0001AEF8
		private LayoutElement LayoutElement
		{
			get
			{
				if (!this.layoutElement)
				{
					base.TryGetComponent<LayoutElement>(out this.layoutElement);
				}
				return this.layoutElement;
			}
		}

		// Token: 0x060006CA RID: 1738 RVA: 0x0001CD1C File Offset: 0x0001AF1C
		protected override void Start()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			base.Start();
			this.UpdateCharacterLimitText();
			base.onSelect.AddListener(new UnityAction<string>(this.OnSelected));
			base.onDeselect.AddListener(new UnityAction<string>(this.OnDeselect));
			base.onValueChanged.AddListener(new UnityAction<string>(this.OnValueChanged));
			base.onEndEdit.AddListener(new UnityAction<string>(this.OnEndEdit));
			if (this.updateValueAtSelectionOnSubmit)
			{
				base.onSubmit.AddListener(new UnityAction<string>(this.SetValueAtSelection));
			}
		}

		// Token: 0x060006CB RID: 1739 RVA: 0x0001CDC8 File Offset: 0x0001AFC8
		protected override void OnDisable()
		{
			base.OnDisable();
			if (ExitManager.IsQuitting || !Application.isPlaying)
			{
				return;
			}
			if (this.clearOnDisable)
			{
				this.Clear(true);
			}
			MonoBehaviourSingleton<InputManager>.Instance.ReleaseInputField(this);
			this.parentScrollRect = null;
			this.parentScrollRectAttempted = false;
		}

		// Token: 0x060006CC RID: 1740 RVA: 0x0001CE07 File Offset: 0x0001B007
		protected override void LateUpdate()
		{
			base.LateUpdate();
			if (this.lastCaretPosition == base.caretPosition)
			{
				return;
			}
			this.lastCaretPosition = base.caretPosition;
			this.CaretPositionChangedUnityEvent.Invoke(this.lastCaretPosition);
		}

		// Token: 0x060006CD RID: 1741 RVA: 0x0001CE3B File Offset: 0x0001B03B
		public void Clear(bool triggerEvent = true)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Clear", new object[] { triggerEvent });
			}
			if (triggerEvent)
			{
				base.text = string.Empty;
				return;
			}
			base.SetTextWithoutNotify(string.Empty);
		}

		// Token: 0x060006CE RID: 1742 RVA: 0x0001CE79 File Offset: 0x0001B079
		public bool IsNullOrEmptyOrWhiteSpace(bool playInvalidInputTweensIfSo = true)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "IsNullOrEmptyOrWhiteSpace", new object[] { playInvalidInputTweensIfSo });
			}
			bool flag = base.text.IsNullOrEmptyOrWhiteSpace();
			if (flag && playInvalidInputTweensIfSo)
			{
				this.PlayInvalidInputTweens();
			}
			return flag;
		}

		// Token: 0x060006CF RID: 1743 RVA: 0x0001CEB3 File Offset: 0x0001B0B3
		public void PlayInvalidInputTweens()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "PlayInvalidInputTweens", Array.Empty<object>());
			}
			this.invalidInputTweens.Tween();
		}

		// Token: 0x060006D0 RID: 1744 RVA: 0x0001CED8 File Offset: 0x0001B0D8
		public void SetCaretPosition(int newCaretPosition)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetCaretPosition", "newCaretPosition", newCaretPosition), this);
			}
			base.caretPosition = newCaretPosition;
		}

		// Token: 0x060006D1 RID: 1745 RVA: 0x0001CF09 File Offset: 0x0001B109
		public void SetCaretPositionWithoutNotify(int newCaretPosition)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetCaretPositionWithoutNotify", "newCaretPosition", newCaretPosition), this);
			}
			this.lastCaretPosition = newCaretPosition;
			this.SetCaretPosition(newCaretPosition);
		}

		// Token: 0x060006D2 RID: 1746 RVA: 0x0001CF44 File Offset: 0x0001B144
		public override void OnDrag(PointerEventData eventData)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDrag", new object[] { eventData });
			}
			base.OnDrag(eventData);
			if (this.parentScrollRect)
			{
				this.parentScrollRect.OnDrag(eventData);
				return;
			}
			if (this.parentScrollRectAttempted)
			{
				return;
			}
			this.parentScrollRect = base.GetComponentInParent<UIScrollRect>();
			this.parentScrollRectAttempted = true;
		}

		// Token: 0x060006D3 RID: 1747 RVA: 0x0001CFAC File Offset: 0x0001B1AC
		public override void OnScroll(PointerEventData eventData)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnScroll", new object[] { eventData });
			}
			float globalScrollSensitivity = UIScrollRect.GlobalScrollSensitivity;
			if (!Mathf.Approximately(globalScrollSensitivity, 1f))
			{
				PointerEventData pointerEventData = new PointerEventData(EventSystem.current)
				{
					scrollDelta = eventData.scrollDelta * globalScrollSensitivity
				};
				base.OnScroll(pointerEventData);
			}
			else
			{
				base.OnScroll(eventData);
			}
			if (this.parentScrollRect)
			{
				this.parentScrollRect.OnScroll(eventData);
				return;
			}
			if (this.parentScrollRectAttempted)
			{
				return;
			}
			this.parentScrollRect = base.GetComponentInParent<UIScrollRect>();
			this.parentScrollRectAttempted = true;
		}

		// Token: 0x060006D4 RID: 1748 RVA: 0x0001D04C File Offset: 0x0001B24C
		private void OnValueChanged(string newValue)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnValueChanged", new object[] { newValue });
			}
			if (!base.multiLine && (newValue.Contains("\n") || newValue.Contains("\r")))
			{
				newValue = newValue.Replace("\n", "").Replace("\r", "");
				base.SetTextWithoutNotify(newValue);
			}
			if (base.characterLimit > 0)
			{
				this.UpdateCharacterLimitText();
			}
		}

		// Token: 0x060006D5 RID: 1749 RVA: 0x0001D0D4 File Offset: 0x0001B2D4
		private void OnEndEdit(string value)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEndEdit", new object[] { value });
			}
			if (ExitManager.IsQuitting)
			{
				return;
			}
			MonoBehaviourSingleton<InputManager>.Instance.ReleaseInputField(this);
			if (!base.gameObject.activeInHierarchy)
			{
				return;
			}
			base.StartCoroutine(this.WaitForEndOfFrameAndClearSelectedGameObject());
		}

		// Token: 0x060006D6 RID: 1750 RVA: 0x0001D12C File Offset: 0x0001B32C
		private IEnumerator WaitForEndOfFrameAndClearSelectedGameObject()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "WaitForEndOfFrameAndClearSelectedGameObject", Array.Empty<object>());
			}
			yield return new WaitForEndOfFrame();
			if (EventSystem.current.currentSelectedGameObject == base.gameObject)
			{
				EventSystem.current.SetSelectedGameObject(null);
			}
			yield break;
		}

		// Token: 0x060006D7 RID: 1751 RVA: 0x0001D13B File Offset: 0x0001B33B
		private void OnSelected(string valueAtSelection)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnSelected", new object[] { valueAtSelection });
			}
			MonoBehaviourSingleton<InputManager>.Instance.SetInputField(this);
			this.SetValueAtSelection(valueAtSelection);
		}

		// Token: 0x060006D8 RID: 1752 RVA: 0x0001D16C File Offset: 0x0001B36C
		private void SetValueAtSelection(string valueAtSelection)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetValueAtSelection", new object[] { valueAtSelection });
			}
			this.valueAtSelection = valueAtSelection;
		}

		// Token: 0x060006D9 RID: 1753 RVA: 0x0001D194 File Offset: 0x0001B394
		private void OnDeselect(string valueAtDeselection)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDeselect", new object[] { valueAtDeselection });
			}
			if (this.valueAtSelection == valueAtDeselection)
			{
				return;
			}
			this.valueAtSelection = valueAtDeselection;
			this.DeselectAndValueChangedUnityEvent.Invoke(valueAtDeselection);
		}

		// Token: 0x060006DA RID: 1754 RVA: 0x0001D1E0 File Offset: 0x0001B3E0
		private void UpdateCharacterLimitText()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "UpdateCharacterLimitText", Array.Empty<object>());
			}
			if (base.characterLimit <= 0)
			{
				this.characterLimitBadge.gameObject.SetActive(false);
				return;
			}
			this.characterLimitBadge.Display(string.Format("{0}/{1}", base.text.Length, base.characterLimit));
			bool flag = base.characterLimit - base.text.Length <= this.characterShowThreshold;
			this.characterLimitBadge.gameObject.SetActive(flag);
		}

		// Token: 0x060006DB RID: 1755 RVA: 0x0001D280 File Offset: 0x0001B480
		private bool ShouldIgnore()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ShouldIgnore", Array.Empty<object>());
			}
			if (this.keyCodesToIgnore.Length == 0)
			{
				return false;
			}
			Key[] array = this.keyCodesToIgnore;
			for (int i = 0; i < array.Length; i++)
			{
				if (EndlessInput.GetKeyDown(array[i]))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x040003F0 RID: 1008
		[Header("UIInputField")]
		[SerializeField]
		private bool clearOnDisable;

		// Token: 0x040003F1 RID: 1009
		[Header("Character Limit")]
		[SerializeField]
		[Tooltip("Will display badge when this many characters is left")]
		private int characterShowThreshold = 50;

		// Token: 0x040003F2 RID: 1010
		[SerializeField]
		private UIBadge characterLimitBadge;

		// Token: 0x040003F3 RID: 1011
		[SerializeField]
		[Tooltip("This is to for how OnDeselectAndValueChanged is triggered.")]
		private bool updateValueAtSelectionOnSubmit;

		// Token: 0x040003F4 RID: 1012
		[SerializeField]
		private TweenCollection invalidInputTweens;

		// Token: 0x040003F5 RID: 1013
		[SerializeField]
		private Key[] keyCodesToIgnore = Array.Empty<Key>();

		// Token: 0x040003F7 RID: 1015
		private LayoutElement layoutElement;

		// Token: 0x040003F8 RID: 1016
		private RectTransform rectTransform;

		// Token: 0x040003F9 RID: 1017
		private string valueAtSelection = string.Empty;

		// Token: 0x040003FA RID: 1018
		private int lastCaretPosition;

		// Token: 0x040003FB RID: 1019
		private UIScrollRect parentScrollRect;

		// Token: 0x040003FC RID: 1020
		private bool parentScrollRectAttempted;
	}
}
