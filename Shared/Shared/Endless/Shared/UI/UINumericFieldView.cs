using System;
using Endless.Shared.Debugging;
using Runtime.Shared;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Endless.Shared.UI
{
	// Token: 0x0200021B RID: 539
	public class UINumericFieldView : UIGameObject, IPoolableT, IUIInteractable
	{
		// Token: 0x1700028B RID: 651
		// (get) Token: 0x06000DD7 RID: 3543 RVA: 0x0003C58D File Offset: 0x0003A78D
		// (set) Token: 0x06000DD8 RID: 3544 RVA: 0x0003C595 File Offset: 0x0003A795
		public MaskableGraphic[] MaskableGraphics { get; private set; } = Array.Empty<MaskableGraphic>();

		// Token: 0x1700028C RID: 652
		// (get) Token: 0x06000DD9 RID: 3545 RVA: 0x0003C59E File Offset: 0x0003A79E
		// (set) Token: 0x06000DDA RID: 3546 RVA: 0x0003C5A6 File Offset: 0x0003A7A6
		public float Value { get; private set; }

		// Token: 0x1700028D RID: 653
		// (get) Token: 0x06000DDB RID: 3547 RVA: 0x0003C5AF File Offset: 0x0003A7AF
		// (set) Token: 0x06000DDC RID: 3548 RVA: 0x0003C5B7 File Offset: 0x0003A7B7
		public MonoBehaviour Prefab { get; set; }

		// Token: 0x1700028E RID: 654
		// (get) Token: 0x06000DDD RID: 3549 RVA: 0x000050D2 File Offset: 0x000032D2
		public bool IsUi
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06000DDE RID: 3550 RVA: 0x0003C5C0 File Offset: 0x0003A7C0
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log("Start", this);
			}
			this.SetType(this.type);
			this.slider.onValueChanged.AddListener(new UnityAction<float>(this.ViewValueFromSlider));
			this.slider.OnRelease.AddListener(new UnityAction<float>(this.ViewAndSetValueFromSlider));
			this.inputField.onSubmit.AddListener(new UnityAction<string>(this.SetValueFromInputField));
			this.inputField.onEndEdit.AddListener(new UnityAction<string>(this.SetValueFromInputField));
			this.decrementButton.onClick.AddListener(new UnityAction(this.Decrement));
			this.incrementButton.onClick.AddListener(new UnityAction(this.Increment));
		}

		// Token: 0x1400004B RID: 75
		// (add) Token: 0x06000DDF RID: 3551 RVA: 0x0003C694 File Offset: 0x0003A894
		// (remove) Token: 0x06000DE0 RID: 3552 RVA: 0x0003C6CC File Offset: 0x0003A8CC
		public event Action<float> OnValueChanged;

		// Token: 0x06000DE1 RID: 3553 RVA: 0x0003C701 File Offset: 0x0003A901
		public void OnSpawn()
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log("OnSpawn", this);
			}
		}

		// Token: 0x06000DE2 RID: 3554 RVA: 0x0003C716 File Offset: 0x0003A916
		public void OnDespawn()
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log("OnDespawn", this);
			}
		}

		// Token: 0x06000DE3 RID: 3555 RVA: 0x0003C72C File Offset: 0x0003A92C
		public void SetInteractable(bool interactable)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetInteractable", new object[] { interactable });
			}
			this.slider.interactable = interactable;
			this.inputField.interactable = interactable;
			this.decrementButton.interactable = interactable;
			this.incrementButton.interactable = interactable;
		}

		// Token: 0x06000DE4 RID: 3556 RVA: 0x0003C78C File Offset: 0x0003A98C
		public void SetType(UINumericFieldView.Types newType)
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetType", "newType", newType), this);
			}
			this.type = newType;
			this.slider.wholeNumbers = this.type == UINumericFieldView.Types.Int;
			UINumericFieldView.Types types = this.type;
			if (types == UINumericFieldView.Types.Int)
			{
				this.inputField.contentType = TMP_InputField.ContentType.IntegerNumber;
				this.inputField.keyboardType = TouchScreenKeyboardType.NumberPad;
				this.inputField.characterValidation = TMP_InputField.CharacterValidation.Integer;
				return;
			}
			if (types != UINumericFieldView.Types.Float)
			{
				DebugUtility.LogNoEnumSupportError<UINumericFieldView.Types>(this, "SetType", this.type, new object[] { newType });
				return;
			}
			this.inputField.contentType = TMP_InputField.ContentType.DecimalNumber;
			this.inputField.keyboardType = TouchScreenKeyboardType.DecimalPad;
			this.inputField.characterValidation = TMP_InputField.CharacterValidation.Decimal;
		}

		// Token: 0x06000DE5 RID: 3557 RVA: 0x0003C858 File Offset: 0x0003AA58
		public void SetValue(float newValue, bool triggerOnValueChanged)
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", new object[] { "SetValue", "newValue", newValue, "triggerOnValueChanged", triggerOnValueChanged }), this);
			}
			this.Value = newValue;
			this.slider.SetValue(this.Value, true);
			string text;
			if (newValue % 1f == 0f)
			{
				text = newValue.ToString("0");
			}
			else
			{
				text = newValue.ToString("0.##");
			}
			this.inputField.SetTextWithoutNotify(text);
			if (triggerOnValueChanged)
			{
				Action<float> onValueChanged = this.OnValueChanged;
				if (onValueChanged == null)
				{
					return;
				}
				onValueChanged(this.Value);
			}
		}

		// Token: 0x06000DE6 RID: 3558 RVA: 0x0003C918 File Offset: 0x0003AB18
		public void SetMinAndMax(float min, float max)
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", new object[] { "SetMinAndMax", "min", min, "max", max }), this);
			}
			this.slider.minValue = min;
			this.slider.maxValue = max;
			bool flag = min > -2E+09f || max < 2E+09f;
			if (this.verboseLogging)
			{
				DebugUtility.Log(string.Format("{0}: {1}", "isClamped", flag), this);
			}
			if (this.sliderVisibilityIsControlledViaClamped)
			{
				this.slider.gameObject.SetActive(flag);
				this.sliderSpace.SetActive(flag);
				this.inputFieldLayoutElement.flexibleWidth = (float)(flag ? (-1) : 1);
			}
			if (this.incrementVisibilityIsControlledViaClamped)
			{
				this.incrementContainer.SetActive(flag);
			}
		}

		// Token: 0x06000DE7 RID: 3559 RVA: 0x0003CA08 File Offset: 0x0003AC08
		public void SetRaycastTargetGraphics(bool state)
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetRaycastTargetGraphics", "state", state), this);
			}
			Graphic[] array = this.raycastTargetGraphics;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].raycastTarget = state;
			}
		}

		// Token: 0x06000DE8 RID: 3560 RVA: 0x0003CA5C File Offset: 0x0003AC5C
		public void SetFieldName(string fieldName)
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log("SetFieldName ( fieldName: " + fieldName + " )", this);
			}
			this.fieldNameText.text = fieldName;
			this.fieldNameText.gameObject.SetActive(!fieldName.IsNullOrEmptyOrWhiteSpace());
		}

		// Token: 0x06000DE9 RID: 3561 RVA: 0x0003CAAC File Offset: 0x0003ACAC
		public void OverrideInputField(string input)
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log("OverrideInputField ( input: " + input + " )", this);
			}
			this.inputField.SetTextWithoutNotify(input);
		}

		// Token: 0x06000DEA RID: 3562 RVA: 0x0003CAD8 File Offset: 0x0003ACD8
		private void ViewValueFromSlider(float valueFromSlider)
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "ViewValueFromSlider", "valueFromSlider", valueFromSlider), this);
			}
			this.SetValue(valueFromSlider, false);
		}

		// Token: 0x06000DEB RID: 3563 RVA: 0x0003CB0A File Offset: 0x0003AD0A
		private void ViewAndSetValueFromSlider(float valueFromSlider)
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "ViewAndSetValueFromSlider", "valueFromSlider", valueFromSlider), this);
			}
			this.SetValue(valueFromSlider, true);
		}

		// Token: 0x06000DEC RID: 3564 RVA: 0x0003CB3C File Offset: 0x0003AD3C
		private void SetValueFromInputField(string valueFromInputField)
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log("SetValueFromInputField ( valueFromInputField: " + valueFromInputField + " )", this);
			}
			float num;
			if (!float.TryParse(valueFromInputField, out num))
			{
				return;
			}
			if (Mathf.Approximately(this.Value, num))
			{
				return;
			}
			this.SetValue(num, true);
		}

		// Token: 0x06000DED RID: 3565 RVA: 0x0003CB89 File Offset: 0x0003AD89
		private void Decrement()
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log("Decrement", this);
			}
			this.SetValue(this.Value - 1f, true);
		}

		// Token: 0x06000DEE RID: 3566 RVA: 0x0003CBB1 File Offset: 0x0003ADB1
		private void Increment()
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log("Increment", this);
			}
			this.SetValue(this.Value + 1f, true);
		}

		// Token: 0x040008DA RID: 2266
		[SerializeField]
		private UINumericFieldView.Types type;

		// Token: 0x040008DB RID: 2267
		[SerializeField]
		private bool sliderVisibilityIsControlledViaClamped = true;

		// Token: 0x040008DC RID: 2268
		[SerializeField]
		private bool incrementVisibilityIsControlledViaClamped = true;

		// Token: 0x040008DD RID: 2269
		[Header("References")]
		[SerializeField]
		private TextMeshProUGUI fieldNameText;

		// Token: 0x040008DE RID: 2270
		[SerializeField]
		private UISlider slider;

		// Token: 0x040008DF RID: 2271
		[SerializeField]
		private GameObject sliderSpace;

		// Token: 0x040008E0 RID: 2272
		[SerializeField]
		private UIInputField inputField;

		// Token: 0x040008E1 RID: 2273
		[SerializeField]
		private LayoutElement inputFieldLayoutElement;

		// Token: 0x040008E2 RID: 2274
		[SerializeField]
		private GameObject incrementContainer;

		// Token: 0x040008E3 RID: 2275
		[SerializeField]
		private UIButton decrementButton;

		// Token: 0x040008E4 RID: 2276
		[SerializeField]
		private UIButton incrementButton;

		// Token: 0x040008E5 RID: 2277
		[SerializeField]
		private Graphic[] raycastTargetGraphics = Array.Empty<Graphic>();

		// Token: 0x040008E6 RID: 2278
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x0200021C RID: 540
		public enum Types
		{
			// Token: 0x040008EB RID: 2283
			Int,
			// Token: 0x040008EC RID: 2284
			Float
		}
	}
}
