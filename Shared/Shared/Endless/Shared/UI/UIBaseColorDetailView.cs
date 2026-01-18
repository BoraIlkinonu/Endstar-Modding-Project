using System;
using System.Runtime.CompilerServices;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Endless.Shared.UI
{
	// Token: 0x020001E2 RID: 482
	public abstract class UIBaseColorDetailView : UIBaseColorView
	{
		// Token: 0x1700022E RID: 558
		// (get) Token: 0x06000BD2 RID: 3026
		protected abstract int ColorMax { get; }

		// Token: 0x1700022F RID: 559
		// (get) Token: 0x06000BD3 RID: 3027 RVA: 0x000330B3 File Offset: 0x000312B3
		protected int RedPresenterModel
		{
			get
			{
				return this.red.Model;
			}
		}

		// Token: 0x17000230 RID: 560
		// (get) Token: 0x06000BD4 RID: 3028 RVA: 0x000330C0 File Offset: 0x000312C0
		protected int GreenPresenterModel
		{
			get
			{
				return this.green.Model;
			}
		}

		// Token: 0x17000231 RID: 561
		// (get) Token: 0x06000BD5 RID: 3029 RVA: 0x000330CD File Offset: 0x000312CD
		protected int BluePresenterModel
		{
			get
			{
				return this.blue.Model;
			}
		}

		// Token: 0x17000232 RID: 562
		// (get) Token: 0x06000BD6 RID: 3030 RVA: 0x000330DA File Offset: 0x000312DA
		protected int AlphaPresenterModel
		{
			get
			{
				return this.alpha.Model;
			}
		}

		// Token: 0x06000BD7 RID: 3031 RVA: 0x000330E7 File Offset: 0x000312E7
		protected virtual void Start()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("Start", this);
			}
			this.SetUpIntPresenters();
			this.SetUpHexadecimalInputField();
			this.CreateHueStripTexture();
			this.CreateSaturationValueTexture();
			this.SetUpEventTriggers();
		}

		// Token: 0x06000BD8 RID: 3032 RVA: 0x0003311A File Offset: 0x0003131A
		protected virtual void OnDestroy()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("OnDestroy", this);
			}
			UIColorHdrUtility.SafeDestroyTexture(ref this.hueStripTexture);
			UIColorHdrUtility.SafeDestroyTexture(ref this.saturationValueTexture);
		}

		// Token: 0x06000BD9 RID: 3033 RVA: 0x00033148 File Offset: 0x00031348
		public override void View(Color model)
		{
			base.View(model);
			this.red.SetModel(Mathf.RoundToInt(Mathf.Clamp01(model.r) * (float)this.ColorMax), false);
			this.green.SetModel(Mathf.RoundToInt(Mathf.Clamp01(model.g) * (float)this.ColorMax), false);
			this.blue.SetModel(Mathf.RoundToInt(Mathf.Clamp01(model.b) * (float)this.ColorMax), false);
			this.alpha.SetModel(Mathf.RoundToInt(Mathf.Clamp01(model.a) * (float)this.ColorMax), false);
			this.UpdateHexFieldIfNotFocused(model);
			this.UpdateHsvFromColor(model);
			this.UpdateCursorPositions();
		}

		// Token: 0x06000BDA RID: 3034 RVA: 0x00033200 File Offset: 0x00031400
		private void SetUpHexadecimalInputField()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("SetUpHexadecimalInputField", this);
			}
			this.hexadecimalInputField.onValueChanged.AddListener(new UnityAction<string>(this.SetColorFromHexadecimalString));
			this.hexadecimalInputField.onEndEdit.AddListener(new UnityAction<string>(this.SetColorFromHexadecimalString));
			this.hexadecimalInputField.onSubmit.AddListener(new UnityAction<string>(this.SetColorFromHexadecimalString));
		}

		// Token: 0x06000BDB RID: 3035 RVA: 0x00033274 File Offset: 0x00031474
		private void CreateHueStripTexture()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("CreateHueStripTexture", this);
			}
			this.hueStripTexture = new Texture2D(1, 360);
			for (int i = 0; i < 360; i++)
			{
				Color color = Color.HSVToRGB((float)i / 360f, 1f, 1f);
				this.hueStripTexture.SetPixel(0, i, color);
			}
			this.hueStripTexture.Apply();
			this.hueStripRawImage.texture = this.hueStripTexture;
		}

		// Token: 0x06000BDC RID: 3036 RVA: 0x000332F7 File Offset: 0x000314F7
		private void SetUpEventTriggers()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("SetUpEventTriggers", this);
			}
			this.SetUpHueEventTrigger();
			this.SetUpSaturationValueEventTrigger();
		}

		// Token: 0x06000BDD RID: 3037 RVA: 0x00033318 File Offset: 0x00031518
		private void SetUpHueEventTrigger()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("SetUpHueEventTrigger", this);
			}
			EventTrigger.Entry entry = new EventTrigger.Entry
			{
				eventID = EventTriggerType.PointerDown
			};
			entry.callback.AddListener(delegate(BaseEventData data)
			{
				this.OnHueAreaClicked((PointerEventData)data);
			});
			this.hueEventTrigger.triggers.Add(entry);
			EventTrigger.Entry entry2 = new EventTrigger.Entry
			{
				eventID = EventTriggerType.Drag
			};
			entry2.callback.AddListener(delegate(BaseEventData data)
			{
				this.OnHueAreaClicked((PointerEventData)data);
			});
			this.hueEventTrigger.triggers.Add(entry2);
		}

		// Token: 0x06000BDE RID: 3038 RVA: 0x000333A4 File Offset: 0x000315A4
		private void SetUpSaturationValueEventTrigger()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("SetUpSaturationValueEventTrigger", this);
			}
			EventTrigger.Entry entry = new EventTrigger.Entry
			{
				eventID = EventTriggerType.PointerDown
			};
			entry.callback.AddListener(delegate(BaseEventData data)
			{
				this.OnSaturationValueAreaClicked((PointerEventData)data);
			});
			this.satValEventTrigger.triggers.Add(entry);
			EventTrigger.Entry entry2 = new EventTrigger.Entry
			{
				eventID = EventTriggerType.Drag
			};
			entry2.callback.AddListener(delegate(BaseEventData data)
			{
				this.OnSaturationValueAreaClicked((PointerEventData)data);
			});
			this.satValEventTrigger.triggers.Add(entry2);
		}

		// Token: 0x06000BDF RID: 3039 RVA: 0x00033430 File Offset: 0x00031630
		private void CreateSaturationValueTexture()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("CreateSaturationValueTexture", this);
			}
			if (!this.saturationValueTexture)
			{
				this.saturationValueTexture = new Texture2D(100, 100);
				this.saturationValueAreaRawImage.texture = this.saturationValueTexture;
			}
			this.UpdateSaturationValueTexture();
		}

		// Token: 0x06000BE0 RID: 3040 RVA: 0x00033484 File Offset: 0x00031684
		protected virtual void SetUpIntPresenters()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("SetUpIntPresenters", this);
			}
			this.SetUpIntPresenter(this.red, 0, this.ColorMax, new Action<object>(this.SetRed));
			this.SetUpIntPresenter(this.green, 0, this.ColorMax, new Action<object>(this.SetGreen));
			this.SetUpIntPresenter(this.blue, 0, this.ColorMax, new Action<object>(this.SetBlue));
			this.SetUpIntPresenter(this.alpha, 0, this.ColorMax, new Action<object>(this.SetAlpha));
		}

		// Token: 0x06000BE1 RID: 3041 RVA: 0x00033524 File Offset: 0x00031724
		protected void SetUpIntPresenter(UIIntPresenter intPresenter, int min, int max, Action<object> invokeOnValueChanged)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Concat(new string[]
				{
					"SetUpIntPresenter ( intPresenter: ",
					intPresenter.gameObject.name,
					", intPresenter: ",
					intPresenter.gameObject.name,
					" )"
				}), this);
			}
			intPresenter.SetMinMax(0, min, max);
			intPresenter.OnModelChanged += invokeOnValueChanged;
		}

		// Token: 0x06000BE2 RID: 3042 RVA: 0x00033590 File Offset: 0x00031790
		public override void SetInteractable(bool interactable)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetInteractable", "interactable", interactable), this);
			}
			this.red.SetInteractable(interactable);
			this.green.SetInteractable(interactable);
			this.blue.SetInteractable(interactable);
			this.alpha.SetInteractable(interactable);
			this.hexadecimalInputField.interactable = interactable;
		}

		// Token: 0x06000BE3 RID: 3043 RVA: 0x00033604 File Offset: 0x00031804
		protected void UpdateHsvFromColor(Color color)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "UpdateHsvFromColor", "color", color), this);
			}
			float num;
			float num2;
			float num3;
			Color.RGBToHSV(new Color(Mathf.Clamp01(color.r), Mathf.Clamp01(color.g), Mathf.Clamp01(color.b), color.a), out num, out num2, out num3);
			if (num2 > 0.001f || num3 > 0.001f)
			{
				if (num2 > 0.001f)
				{
					this.preservedHue = num;
				}
				this.currentHue = num;
			}
			else
			{
				this.currentHue = this.preservedHue;
			}
			this.currentSaturation = num2;
			this.currentValue = num3;
		}

		// Token: 0x06000BE4 RID: 3044 RVA: 0x000336B8 File Offset: 0x000318B8
		protected void UpdateCursorPositions()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("UpdateCursorPositions", this);
			}
			RectTransform rectTransform = this.hueCursorImage.rectTransform;
			RectTransform rectTransform2 = this.hueStripRawImage.rectTransform;
			float num = Mathf.Clamp01(this.currentHue) * rectTransform2.rect.height - rectTransform2.rect.height * 0.5f;
			rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, num);
			RectTransform rectTransform3 = this.colorCursorImage.rectTransform;
			RectTransform rectTransform4 = this.saturationValueAreaRawImage.rectTransform;
			float num2 = Mathf.Clamp01(this.currentSaturation);
			float num3 = Mathf.Clamp01(this.currentValue);
			float num4 = num2 * rectTransform4.rect.width - rectTransform4.rect.width * 0.5f;
			float num5 = num3 * rectTransform4.rect.height - rectTransform4.rect.height * 0.5f;
			rectTransform3.anchoredPosition = new Vector2(num4, num5);
		}

		// Token: 0x06000BE5 RID: 3045 RVA: 0x000337C4 File Offset: 0x000319C4
		protected virtual void UpdateFromHSVChange(Color newColor)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "UpdateFromHSVChange", "newColor", newColor), this);
			}
			Action<Color> onColorChanged = this.OnColorChanged;
			if (onColorChanged == null)
			{
				return;
			}
			onColorChanged(newColor);
		}

		// Token: 0x06000BE6 RID: 3046 RVA: 0x00033800 File Offset: 0x00031A00
		protected virtual void UpdateHexFieldIfNotFocused(Color color)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "UpdateHexFieldIfNotFocused", "color", color), this);
			}
			if (this.hexadecimalInputField.isFocused)
			{
				return;
			}
			string text = UIColorUtility.ToHexString(color, true);
			this.hexadecimalInputField.SetTextWithoutNotify(text);
		}

		// Token: 0x06000BE7 RID: 3047 RVA: 0x00033858 File Offset: 0x00031A58
		protected void UpdateSaturationValueTexture()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("UpdateSaturationValueTexture", this);
			}
			Color[] array = this.saturationValPixelCache;
			if (array == null || array.Length != 10000)
			{
				this.saturationValPixelCache = new Color[10000];
			}
			if (!this.saturationValueTexture)
			{
				this.CreateSaturationValueTexture();
			}
			for (int i = 0; i < this.saturationValPixelCache.Length; i++)
			{
				float num = (float)(i % 100);
				int num2 = i / 100;
				float num3 = num / 100f;
				float num4 = (float)num2 / 100f;
				this.saturationValPixelCache[i] = Color.HSVToRGB(this.currentHue, num3, num4);
			}
			this.saturationValueTexture.SetPixels(this.saturationValPixelCache);
			this.saturationValueTexture.Apply();
		}

		// Token: 0x06000BE8 RID: 3048 RVA: 0x00033914 File Offset: 0x00031B14
		private void OnHueAreaClicked(PointerEventData eventData)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "OnHueAreaClicked", "eventData", eventData), this);
			}
			this.currentHue = this.CalculateHueFromClick(eventData);
			this.UpdateSaturationValueTexture();
			Color color = Color.HSVToRGB(this.currentHue, this.currentSaturation, this.currentValue);
			color = new Color(color.r, color.g, color.b, (float)this.AlphaPresenterModel / (float)this.ColorMax);
			Action<Color> onColorChanged = this.OnColorChanged;
			if (onColorChanged == null)
			{
				return;
			}
			onColorChanged(color);
		}

		// Token: 0x06000BE9 RID: 3049 RVA: 0x000339A8 File Offset: 0x00031BA8
		private float CalculateHueFromClick(PointerEventData eventData)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "CalculateHueFromClick", "eventData", eventData), this);
			}
			RectTransform rectTransform = this.hueStripRawImage.rectTransform;
			Vector2 vector;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out vector);
			return Mathf.Clamp01((vector.y + rectTransform.rect.height * 0.5f) / rectTransform.rect.height);
		}

		// Token: 0x06000BEA RID: 3050 RVA: 0x00033A28 File Offset: 0x00031C28
		private void OnSaturationValueAreaClicked(PointerEventData eventData)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "OnSaturationValueAreaClicked", "eventData", eventData), this);
			}
			ValueTuple<float, float> valueTuple = this.CalculateSaturationValueFromClick(eventData);
			this.currentSaturation = valueTuple.Item1;
			this.currentValue = valueTuple.Item2;
			Color color = Color.HSVToRGB(this.currentHue, this.currentSaturation, this.currentValue);
			color = new Color(color.r, color.g, color.b, (float)this.AlphaPresenterModel / (float)this.ColorMax);
			this.UpdateFromHSVChange(color);
		}

		// Token: 0x06000BEB RID: 3051 RVA: 0x00033AC0 File Offset: 0x00031CC0
		[return: TupleElementNames(new string[] { "saturation", "value" })]
		private ValueTuple<float, float> CalculateSaturationValueFromClick(PointerEventData eventData)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "CalculateSaturationValueFromClick", "eventData", eventData), this);
			}
			RectTransform rectTransform = this.saturationValueAreaRawImage.rectTransform;
			Vector2 vector;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out vector);
			float num = (vector.x + rectTransform.rect.width * 0.5f) / rectTransform.rect.width;
			float num2 = (vector.y + rectTransform.rect.height * 0.5f) / rectTransform.rect.height;
			float num3 = Mathf.Clamp(num, 0.001f, 1f);
			float num4 = Mathf.Clamp(num2, 0.001f, 1f);
			return new ValueTuple<float, float>(num3, num4);
		}

		// Token: 0x06000BEC RID: 3052 RVA: 0x00033B90 File Offset: 0x00031D90
		private void SetColorFromHexadecimalString(string hexadecimalString)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("SetColorFromHexadecimalString ( hexadecimalString: " + hexadecimalString + " )", this);
			}
			Color color;
			if (!UIColorUtility.TryParseHexColor(hexadecimalString, out color))
			{
				this.UpdateHexFieldIfNotFocused(base.CachedModel);
				return;
			}
			Action<Color> onColorChanged = this.OnColorChanged;
			if (onColorChanged == null)
			{
				return;
			}
			onColorChanged(color);
		}

		// Token: 0x06000BED RID: 3053 RVA: 0x00033BE4 File Offset: 0x00031DE4
		protected virtual void SetRed(object newValueAsObject)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetRed", "newValueAsObject", newValueAsObject), this);
			}
			int num = (int)newValueAsObject;
			Color color = new Color((float)num / (float)this.ColorMax, base.CachedModel.g, base.CachedModel.b, base.CachedModel.a);
			Action<Color> onColorChanged = this.OnColorChanged;
			if (onColorChanged != null)
			{
				onColorChanged(color);
			}
			this.UpdateSaturationValueTexture();
		}

		// Token: 0x06000BEE RID: 3054 RVA: 0x00033C68 File Offset: 0x00031E68
		protected virtual void SetGreen(object newValueAsObject)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetGreen", "newValueAsObject", newValueAsObject), this);
			}
			int num = (int)newValueAsObject;
			Color color = new Color(base.CachedModel.r, (float)num / (float)this.ColorMax, base.CachedModel.b, base.CachedModel.a);
			Action<Color> onColorChanged = this.OnColorChanged;
			if (onColorChanged != null)
			{
				onColorChanged(color);
			}
			this.UpdateSaturationValueTexture();
		}

		// Token: 0x06000BEF RID: 3055 RVA: 0x00033CEC File Offset: 0x00031EEC
		protected virtual void SetBlue(object newValueAsObject)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetBlue", "newValueAsObject", newValueAsObject), this);
			}
			int num = (int)newValueAsObject;
			Color color = new Color(base.CachedModel.r, base.CachedModel.g, (float)num / (float)this.ColorMax, base.CachedModel.a);
			Action<Color> onColorChanged = this.OnColorChanged;
			if (onColorChanged != null)
			{
				onColorChanged(color);
			}
			this.UpdateSaturationValueTexture();
		}

		// Token: 0x06000BF0 RID: 3056 RVA: 0x00033D70 File Offset: 0x00031F70
		protected virtual void SetAlpha(object newValueAsObject)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetAlpha", "newValueAsObject", newValueAsObject), this);
			}
			int num = (int)newValueAsObject;
			Color color = new Color(base.CachedModel.r, base.CachedModel.g, base.CachedModel.b, (float)num / (float)this.ColorMax);
			Action<Color> onColorChanged = this.OnColorChanged;
			if (onColorChanged != null)
			{
				onColorChanged(color);
			}
			this.UpdateSaturationValueTexture();
		}

		// Token: 0x040007A4 RID: 1956
		private const float VALUE_THRESHOLD = 0.001f;

		// Token: 0x040007A5 RID: 1957
		private const int HUE_TEXTURE_HEIGHT = 360;

		// Token: 0x040007A6 RID: 1958
		private const float SATURATION_THRESHOLD = 0.001f;

		// Token: 0x040007A7 RID: 1959
		private const int SATURATION_VALUE_TEXTURE_SIZE = 100;

		// Token: 0x040007A8 RID: 1960
		private const float MIN_SATURATION_VALUE_CLAMP = 0.001f;

		// Token: 0x040007A9 RID: 1961
		private const float MAX_SATURATION_VALUE_CLAMP = 1f;

		// Token: 0x040007AA RID: 1962
		[Header("UIBaseColorDetailView")]
		[SerializeField]
		protected UIInputField hexadecimalInputField;

		// Token: 0x040007AB RID: 1963
		[SerializeField]
		private RawImage hueStripRawImage;

		// Token: 0x040007AC RID: 1964
		[SerializeField]
		private RawImage saturationValueAreaRawImage;

		// Token: 0x040007AD RID: 1965
		[SerializeField]
		private EventTrigger hueEventTrigger;

		// Token: 0x040007AE RID: 1966
		[SerializeField]
		private EventTrigger satValEventTrigger;

		// Token: 0x040007AF RID: 1967
		[SerializeField]
		private Image colorCursorImage;

		// Token: 0x040007B0 RID: 1968
		[SerializeField]
		private Image hueCursorImage;

		// Token: 0x040007B1 RID: 1969
		[SerializeField]
		private UIIntPresenter red;

		// Token: 0x040007B2 RID: 1970
		[SerializeField]
		private UIIntPresenter green;

		// Token: 0x040007B3 RID: 1971
		[SerializeField]
		private UIIntPresenter blue;

		// Token: 0x040007B4 RID: 1972
		[SerializeField]
		private UIIntPresenter alpha;

		// Token: 0x040007B5 RID: 1973
		private float currentHue;

		// Token: 0x040007B6 RID: 1974
		private float preservedHue;

		// Token: 0x040007B7 RID: 1975
		private float currentSaturation = 1f;

		// Token: 0x040007B8 RID: 1976
		private float currentValue = 1f;

		// Token: 0x040007B9 RID: 1977
		private Texture2D hueStripTexture;

		// Token: 0x040007BA RID: 1978
		private Texture2D saturationValueTexture;

		// Token: 0x040007BB RID: 1979
		private Color[] saturationValPixelCache;
	}
}
