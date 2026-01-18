using System;
using Endless.Shared.Debugging;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Endless.Shared.UI
{
	// Token: 0x0200011A RID: 282
	public class UISlider : Slider
	{
		// Token: 0x17000126 RID: 294
		// (get) Token: 0x060006E3 RID: 1763 RVA: 0x0001D3A9 File Offset: 0x0001B5A9
		// (set) Token: 0x060006E4 RID: 1764 RVA: 0x0001D3B1 File Offset: 0x0001B5B1
		protected bool VerboseLogging { get; set; }

		// Token: 0x060006E5 RID: 1765 RVA: 0x0001D3BC File Offset: 0x0001B5BC
		protected override void Start()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("Start", this);
			}
			base.Start();
			if (this.inputField != null)
			{
				base.onValueChanged.AddListener(new UnityAction<float>(this.ApplySliderValueToInputFieldDirectly));
				this.inputField.contentType = (base.wholeNumbers ? TMP_InputField.ContentType.IntegerNumber : TMP_InputField.ContentType.DecimalNumber);
				this.inputField.onEndEdit.AddListener(new UnityAction<string>(this.ApplyInputFieldValueToSlider));
				this.ApplySliderValueToInputField();
			}
			base.onValueChanged.AddListener(new UnityAction<float>(this.OnSliderValueChanged));
		}

		// Token: 0x060006E6 RID: 1766 RVA: 0x0001D457 File Offset: 0x0001B657
		public override void OnPointerUp(PointerEventData eventData)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "OnPointerUp", "eventData", eventData), this);
			}
			base.OnPointerUp(eventData);
			this.OnRelease.Invoke(this.value);
		}

		// Token: 0x060006E7 RID: 1767 RVA: 0x0001D494 File Offset: 0x0001B694
		public void SetValue(float newValue, bool suppressOnChange)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", new object[] { "SetValue", "newValue", newValue, "suppressOnChange", suppressOnChange }), this);
			}
			if (suppressOnChange)
			{
				this.SetValueWithoutNotify(newValue);
				if (this.inputField != null)
				{
					this.ApplySliderValueToInputField();
					return;
				}
			}
			else
			{
				this.value = newValue;
			}
		}

		// Token: 0x060006E8 RID: 1768 RVA: 0x0001D510 File Offset: 0x0001B710
		public void SetReadOnlyMode(bool readOnly)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetReadOnlyMode", "readOnly", readOnly), this);
			}
			base.interactable = !readOnly;
			this.inputField.interactable = !readOnly;
		}

		// Token: 0x060006E9 RID: 1769 RVA: 0x0001D55E File Offset: 0x0001B75E
		private void ApplySliderValueToInputFieldDirectly(float sliderValue)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "ApplySliderValueToInputFieldDirectly", "sliderValue", sliderValue), this);
			}
			this.ApplySliderValueToInputField();
		}

		// Token: 0x060006EA RID: 1770 RVA: 0x0001D590 File Offset: 0x0001B790
		private void ApplySliderValueToInputField()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("ApplySliderValueToInputField", this);
			}
			string text = ((this.value == (float)Mathf.RoundToInt(this.value)) ? this.value.ToString() : StringUtility.FormatToTwoDecimalsOrLess(this.value));
			this.inputField.SetTextWithoutNotify(text);
		}

		// Token: 0x060006EB RID: 1771 RVA: 0x0001D5F0 File Offset: 0x0001B7F0
		private void ApplyInputFieldValueToSlider(string inputFieldValue)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("ApplyInputFieldValueToSlider ( inputFieldValue: " + inputFieldValue + " )", this);
			}
			inputFieldValue = inputFieldValue.Replace(" ", string.Empty);
			if (string.IsNullOrEmpty(inputFieldValue))
			{
				this.ApplySliderValueToInputField();
				return;
			}
			bool flag;
			if (base.wholeNumbers)
			{
				int num = 0;
				flag = int.TryParse(inputFieldValue, out num);
				if (flag)
				{
					int num2 = num;
					num = Mathf.Clamp(num, (int)base.minValue, (int)base.maxValue);
					this.value = (float)num;
					if (num2 != num)
					{
						this.ApplySliderValueToInputField();
					}
				}
			}
			else
			{
				float num3 = 0f;
				flag = float.TryParse(inputFieldValue, out num3);
				if (flag)
				{
					float num4 = num3;
					num3 = Mathf.Clamp(num3, base.minValue, base.maxValue);
					this.value = num3;
					if (num4 != num3)
					{
						this.ApplySliderValueToInputField();
					}
				}
			}
			if (!flag)
			{
				this.ApplySliderValueToInputField();
			}
		}

		// Token: 0x060006EC RID: 1772 RVA: 0x0001D6BC File Offset: 0x0001B8BC
		private void OnSliderValueChanged(float newValue)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "OnSliderValueChanged", "newValue", newValue), this);
			}
			this.OnChange.Invoke(newValue);
		}

		// Token: 0x04000402 RID: 1026
		public UnityEvent<float> OnChange = new UnityEvent<float>();

		// Token: 0x04000403 RID: 1027
		public UnityEvent<float> OnRelease = new UnityEvent<float>();

		// Token: 0x04000404 RID: 1028
		[SerializeField]
		private TMP_InputField inputField;
	}
}
