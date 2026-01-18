using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Shared.Debugging;
using Endless.Shared.Validation;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Endless.Shared.UI
{
	// Token: 0x02000133 RID: 307
	public abstract class UIBaseDropdown<T> : UIGameObject, IUIDropdownable, IValidatable
	{
		// Token: 0x1700014A RID: 330
		// (get) Token: 0x06000788 RID: 1928 RVA: 0x0001FA30 File Offset: 0x0001DC30
		// (set) Token: 0x06000789 RID: 1929 RVA: 0x0001FA38 File Offset: 0x0001DC38
		public UnityEvent OnOptionsDisplayed { get; private set; } = new UnityEvent();

		// Token: 0x1700014B RID: 331
		// (get) Token: 0x0600078A RID: 1930 RVA: 0x0001FA41 File Offset: 0x0001DC41
		// (set) Token: 0x0600078B RID: 1931 RVA: 0x0001FA49 File Offset: 0x0001DC49
		public UnityEvent OnOptionsHidden { get; private set; } = new UnityEvent();

		// Token: 0x1700014C RID: 332
		// (get) Token: 0x0600078C RID: 1932 RVA: 0x0001FA52 File Offset: 0x0001DC52
		// (set) Token: 0x0600078D RID: 1933 RVA: 0x0001FA5A File Offset: 0x0001DC5A
		protected bool VerboseLogging { get; set; }

		// Token: 0x1700014D RID: 333
		// (get) Token: 0x0600078E RID: 1934 RVA: 0x0001FA63 File Offset: 0x0001DC63
		// (set) Token: 0x0600078F RID: 1935 RVA: 0x0001FA6B File Offset: 0x0001DC6B
		protected bool SuperVerboseLogging { get; set; }

		// Token: 0x1700014E RID: 334
		// (get) Token: 0x06000790 RID: 1936 RVA: 0x0001FA74 File Offset: 0x0001DC74
		public IReadOnlyList<T> Options
		{
			get
			{
				return this.options;
			}
		}

		// Token: 0x1700014F RID: 335
		// (get) Token: 0x06000791 RID: 1937 RVA: 0x0001FA7C File Offset: 0x0001DC7C
		public IReadOnlyList<T> Value
		{
			get
			{
				return this.value;
			}
		}

		// Token: 0x17000150 RID: 336
		// (get) Token: 0x06000792 RID: 1938 RVA: 0x0001FA84 File Offset: 0x0001DC84
		public UnityEvent<int> OnIndexChanged { get; } = new UnityEvent<int>();

		// Token: 0x17000151 RID: 337
		// (get) Token: 0x06000793 RID: 1939 RVA: 0x0001FA8C File Offset: 0x0001DC8C
		public int IndexOfFirstValue
		{
			get
			{
				if (this.value.Length == 0)
				{
					return -1;
				}
				return Array.IndexOf<T>(this.options, this.value[0]);
			}
		}

		// Token: 0x17000152 RID: 338
		// (get) Token: 0x06000794 RID: 1940 RVA: 0x0001FAB0 File Offset: 0x0001DCB0
		public int Count
		{
			get
			{
				return this.options.Length;
			}
		}

		// Token: 0x17000153 RID: 339
		// (get) Token: 0x06000795 RID: 1941 RVA: 0x0001FABA File Offset: 0x0001DCBA
		public string DisplayedValueText
		{
			get
			{
				return this.valueText.Value;
			}
		}

		// Token: 0x17000154 RID: 340
		// (get) Token: 0x06000796 RID: 1942 RVA: 0x0001FAC7 File Offset: 0x0001DCC7
		protected bool CanHaveMultipleValues
		{
			get
			{
				return this.canHaveMultipleValues;
			}
		}

		// Token: 0x17000155 RID: 341
		// (get) Token: 0x06000797 RID: 1943 RVA: 0x0001FACF File Offset: 0x0001DCCF
		private GameObject Blocker
		{
			get
			{
				if (this.blocker != null)
				{
					return this.blocker;
				}
				this.blocker = this.CreateBlocker();
				return this.blocker;
			}
		}

		// Token: 0x06000798 RID: 1944 RVA: 0x0001FAF8 File Offset: 0x0001DCF8
		protected virtual void Start()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("Start", this);
			}
			this.dropdownButton.onClick.AddListener(new UnityAction(this.ToggleDropdownCanvas));
			this.closeDropdownButton.onClick.AddListener(new UnityAction(this.HideOptions));
			this.SetIsInteractable(this.isInteractable);
			this.dropdownCanvas.Disable();
			this.View();
			if (this.handleLabelVisibilityOnStart)
			{
				this.HandleLabelVisibility();
			}
		}

		// Token: 0x06000799 RID: 1945 RVA: 0x0001FB7B File Offset: 0x0001DD7B
		private void OnEnable()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("OnEnable", this);
			}
			if (this.setBlockerParentToTransformOnEnable)
			{
				this.blocker.transform.SetParent(base.transform);
				this.setBlockerParentToTransformOnEnable = false;
			}
		}

		// Token: 0x0600079A RID: 1946 RVA: 0x0001FBB5 File Offset: 0x0001DDB5
		private void OnDisable()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("OnDisable", this);
			}
			this.HideOptions();
			this.setBlockerParentToTransformOnEnable = true;
		}

		// Token: 0x17000156 RID: 342
		// (get) Token: 0x0600079B RID: 1947 RVA: 0x0001FBD7 File Offset: 0x0001DDD7
		public UnityEvent OnValueChanged { get; } = new UnityEvent();

		// Token: 0x17000157 RID: 343
		// (get) Token: 0x0600079C RID: 1948 RVA: 0x0001FBE0 File Offset: 0x0001DDE0
		public HashSet<int> ValueIndices
		{
			get
			{
				HashSet<int> hashSet = new HashSet<int>();
				for (int i = 0; i < this.value.Length; i++)
				{
					T t = this.value[i];
					int num = Array.IndexOf<T>(this.options, t);
					if (num >= 0)
					{
						hashSet.Add(num);
					}
				}
				return hashSet;
			}
		}

		// Token: 0x17000158 RID: 344
		// (get) Token: 0x0600079D RID: 1949 RVA: 0x0001FAB0 File Offset: 0x0001DCB0
		public int OptionsCount
		{
			get
			{
				return this.options.Length;
			}
		}

		// Token: 0x0600079E RID: 1950 RVA: 0x0001FC30 File Offset: 0x0001DE30
		public virtual void ToggleValueIndex(int valueIndex, bool triggerValueChanged)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", new object[] { "ToggleValueIndex", "valueIndex", valueIndex, "triggerValueChanged", triggerValueChanged }), this);
			}
			this.ValidateIndex(valueIndex, this.options.Length);
			T t = this.options[valueIndex];
			T[] array;
			if (!this.canHaveMultipleValues)
			{
				(array = new T[1])[0] = t;
			}
			else
			{
				array = this.ToggleOptionInValue(t);
			}
			T[] array2 = array;
			this.SetValue(array2, triggerValueChanged);
			if (!this.canHaveMultipleValues)
			{
				this.HideOptions();
			}
			if (triggerValueChanged)
			{
				this.OnIndexChanged.Invoke(valueIndex);
			}
		}

		// Token: 0x0600079F RID: 1951 RVA: 0x0001FCE8 File Offset: 0x0001DEE8
		public virtual bool GetIsSelected(int index)
		{
			if (this.SuperVerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "GetIsSelected", "index", index), this);
			}
			if (index < 0 || index >= this.options.Length)
			{
				return false;
			}
			T t = this.options[index];
			return this.value.Contains(t);
		}

		// Token: 0x060007A0 RID: 1952 RVA: 0x0001FD48 File Offset: 0x0001DF48
		[ContextMenu("Validate")]
		public void Validate()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("Validate", this);
			}
			if (this.options.Length == 0)
			{
				DebugUtility.LogError("options must have more than 0 items!", this);
				return;
			}
			foreach (T t in this.value)
			{
				int num = Array.IndexOf<T>(this.options, t);
				this.ValidateIndex(num, this.options.Length);
			}
		}

		// Token: 0x060007A1 RID: 1953 RVA: 0x0001FDB6 File Offset: 0x0001DFB6
		public void SetLabel(string label)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("SetLabel ( label: " + label + " )", this);
			}
			this.labelText.text = label;
			this.HandleLabelVisibility();
		}

		// Token: 0x060007A2 RID: 1954 RVA: 0x0001FDE8 File Offset: 0x0001DFE8
		public void SetOptionsAndValue(IEnumerable<T> newOptions, IEnumerable<T> newValue, bool triggerValueChanged)
		{
			T[] array = ((newOptions != null) ? newOptions.ToArray<T>() : null) ?? Array.Empty<T>();
			T[] array2 = ((newValue != null) ? newValue.ToArray<T>() : null) ?? Array.Empty<T>();
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4} {5}: {6} )", new object[] { "SetOptionsAndValue", "newOptionsArray", array.Length, "newValueArray", array2.Length, "triggerValueChanged", triggerValueChanged }), this);
			}
			if (array2.Length > 1 && !this.canHaveMultipleValues)
			{
				DebugUtility.LogError("Multiple values are not allowed when CanHaveMultipleValues is false!", this);
				array2 = new T[] { array2[0] };
			}
			this.options = array;
			this.SetValue(array2, triggerValueChanged);
		}

		// Token: 0x060007A3 RID: 1955 RVA: 0x0001FEBC File Offset: 0x0001E0BC
		public void SetValue(IEnumerable<T> newValue, bool triggerValueChanged)
		{
			T[] array = ((newValue != null) ? newValue.ToArray<T>() : null) ?? Array.Empty<T>();
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", new object[] { "SetValue", "newValueArray", array.Length, "triggerValueChanged", triggerValueChanged }), this);
			}
			if (this.SuperVerboseLogging)
			{
				DebugUtility.DebugCollection("newValueArray", array, this);
			}
			if (array.Length == 0 && !this.canHaveNoValue)
			{
				array = new T[] { this.options[0] };
			}
			this.value = array.Distinct<T>().ToArray<T>();
			this.View();
			if (triggerValueChanged)
			{
				this.OnValueChanged.Invoke();
			}
		}

		// Token: 0x060007A4 RID: 1956 RVA: 0x0001FF88 File Offset: 0x0001E188
		public void SetIsInteractable(bool interactable)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetIsInteractable", "interactable", interactable), this);
			}
			this.isInteractable = interactable;
			this.dropdownButton.interactable = interactable;
		}

		// Token: 0x060007A5 RID: 1957 RVA: 0x0001FFC8 File Offset: 0x0001E1C8
		public void SetOptionIndexesToHide(IReadOnlyList<int> optionIndexesToHide)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetOptionIndexesToHide", "optionIndexesToHide", optionIndexesToHide.Count), this);
			}
			this.optionIndexesToHide = new HashSet<int>(optionIndexesToHide);
			foreach (UIDropdownOptionButton uidropdownOptionButton in this.dropdownOptionButtons)
			{
				uidropdownOptionButton.HandleHiddenVisibility();
			}
		}

		// Token: 0x060007A6 RID: 1958
		protected abstract string GetLabelFromOption(int optionIndex);

		// Token: 0x060007A7 RID: 1959
		protected abstract Sprite GetIconFromOption(int optionIndex);

		// Token: 0x060007A8 RID: 1960 RVA: 0x00020054 File Offset: 0x0001E254
		protected void ValidateIndex(int index, int length)
		{
			if (this.SuperVerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", new object[] { "ValidateIndex", "index", index, "length", length }), this);
			}
			if (index < 0 || index >= length)
			{
				throw new ArgumentOutOfRangeException("index", "Index out of range.");
			}
		}

		// Token: 0x060007A9 RID: 1961 RVA: 0x000200C1 File Offset: 0x0001E2C1
		protected void SetCanHaveMultipleValues(bool canHaveMultipleValues)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetCanHaveMultipleValues", "canHaveMultipleValues", canHaveMultipleValues), this);
			}
			this.canHaveMultipleValues = canHaveMultipleValues;
		}

		// Token: 0x060007AA RID: 1962 RVA: 0x000200F2 File Offset: 0x0001E2F2
		protected void HideOptions()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("HideOptions", this);
			}
			this.Blocker.SetActive(false);
			this.dropdownCanvas.Disable();
			UnityEvent onOptionsHidden = this.OnOptionsHidden;
			if (onOptionsHidden == null)
			{
				return;
			}
			onOptionsHidden.Invoke();
		}

		// Token: 0x060007AB RID: 1963 RVA: 0x00020130 File Offset: 0x0001E330
		protected virtual string GetValueText()
		{
			if (this.SuperVerboseLogging)
			{
				DebugUtility.Log("GetValueText", this);
			}
			if (this.value.Length == 0)
			{
				return this.valueTextIfNoValue;
			}
			if (this.canHaveMultipleValues)
			{
				if (this.value.Length == this.options.Length)
				{
					return "All";
				}
				List<string> list = new List<string>(this.value.Length);
				foreach (T t in this.value)
				{
					if (t == null)
					{
						list.Add("None");
					}
					else
					{
						int num = this.options.IndexOf(t);
						list.Add(this.GetLabelFromOption(num));
					}
				}
				string text = string.Join(", ", list);
				if (list.Count > 1 && this.valueText.WouldOverflow(text))
				{
					text = "Multiple";
				}
				return text;
			}
			else
			{
				T t2 = this.value[0];
				if (t2 == null)
				{
					return this.valueTextIfNoValue;
				}
				int num2 = this.options.IndexOf(t2);
				return this.GetLabelFromOption(num2);
			}
		}

		// Token: 0x060007AC RID: 1964 RVA: 0x00020248 File Offset: 0x0001E448
		protected virtual void View()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("View", this);
			}
			Sprite sprite = null;
			if (this.value.Length != 0)
			{
				T t = this.value[0];
				int num = Array.IndexOf<T>(this.options, t);
				sprite = this.GetIconFromOption(num);
			}
			this.valueIcon.sprite = sprite;
			this.valueIcon.gameObject.SetActive(sprite != null);
			UIBaseDropdown<T>.Values values = this.values;
			this.values = UIBaseDropdown<T>.Values.None;
			if (sprite)
			{
				this.values = UIBaseDropdown<T>.Values.Icon;
			}
			this.SetValueText(this.GetValueText());
			if (!this.valueText.IsNullOrEmptyOrWhiteSpace)
			{
				this.values |= UIBaseDropdown<T>.Values.Text;
			}
			bool flag = false;
			if (this.dropdownOptionButtons.Count != this.Options.Count)
			{
				flag = true;
			}
			else
			{
				foreach (T t2 in this.options)
				{
					if (!this.dropdownOptionsCreated.Contains(t2))
					{
						flag = true;
						break;
					}
				}
			}
			if (flag)
			{
				this.CreateDropdownOptionButtons();
			}
			this.UpdateOptionButtonVisibility();
			this.UpdateOptionButtonSelectedStatus();
		}

		// Token: 0x060007AD RID: 1965 RVA: 0x00020365 File Offset: 0x0001E565
		public void SetValueText(string target)
		{
			if (this.SuperVerboseLogging)
			{
				DebugUtility.Log("SetValueText ( target: " + target + " )", this);
			}
			this.valueText.Value = target;
		}

		// Token: 0x060007AE RID: 1966 RVA: 0x00020391 File Offset: 0x0001E591
		public virtual bool OptionShouldBeHidden(int index)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "OptionShouldBeHidden", "index", index), this);
			}
			return this.optionIndexesToHide.Contains(index);
		}

		// Token: 0x060007AF RID: 1967 RVA: 0x000203C8 File Offset: 0x0001E5C8
		private void HandleLabelVisibility()
		{
			if (this.SuperVerboseLogging)
			{
				DebugUtility.Log("HandleLabelVisibility", this);
			}
			bool flag = !string.IsNullOrEmpty(this.labelText.text) && !this.hideLabel;
			this.backgroundImage.enabled = flag;
			this.labelText.gameObject.SetActive(flag);
			UIRectTransformDictionary[] array = this.labelRelatedRectTransformDictionaries;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Apply(flag ? "Standard" : "No Label");
			}
		}

		// Token: 0x060007B0 RID: 1968 RVA: 0x00020453 File Offset: 0x0001E653
		private void ToggleDropdownCanvas()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("ToggleDropdownCanvas", this);
			}
			if (this.dropdownCanvas.Enabled)
			{
				this.HideOptions();
				return;
			}
			this.ShowOptions();
			UnityEvent onOptionsDisplayed = this.OnOptionsDisplayed;
			if (onOptionsDisplayed == null)
			{
				return;
			}
			onOptionsDisplayed.Invoke();
		}

		// Token: 0x060007B1 RID: 1969 RVA: 0x00020494 File Offset: 0x0001E694
		private void ShowOptions()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("ShowOptions", this);
			}
			this.Blocker.SetActive(true);
			Canvas componentInParent = base.transform.parent.GetComponentInParent<Canvas>();
			this.blockerRectTransform.SetParent(componentInParent.transform, AnchorPresets.StretchAll, true);
			int sortingOrder = componentInParent.sortingOrder;
			this.blockerCanvas.overrideSorting = true;
			this.blockerCanvas.sortingOrder = sortingOrder + 1;
			this.dropdownCanvas.Enable();
			this.dropdownCanvas.SetCanvasSortingOrder(sortingOrder + 2);
			this.HandleScrollRectTransformOrientation();
		}

		// Token: 0x060007B2 RID: 1970 RVA: 0x00020528 File Offset: 0x0001E728
		private T[] ToggleOptionInValue(T option)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "ToggleOptionInValue", "option", option), this);
			}
			List<T> list = new List<T>(this.value);
			if (list.Contains(option))
			{
				list.Remove(option);
			}
			else
			{
				list.Add(option);
			}
			return list.ToArray();
		}

		// Token: 0x060007B3 RID: 1971 RVA: 0x0002058C File Offset: 0x0001E78C
		private void CreateDropdownOptionButtons()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("CreateDropdownOptionButtons", this);
			}
			foreach (UIDropdownOptionButton uidropdownOptionButton in this.dropdownOptionButtons)
			{
				this.localPool.Push(uidropdownOptionButton);
				uidropdownOptionButton.OnDespawn();
				uidropdownOptionButton.gameObject.SetActive(false);
			}
			this.dropdownOptionButtons.Clear();
			this.dropdownOptionsCreated.Clear();
			int num = this.options.Length;
			for (int i = 0; i < num; i++)
			{
				UIDropdownOptionButton uidropdownOptionButton3;
				if (this.localPool.Count <= 0)
				{
					PoolManagerT instance = MonoBehaviourSingleton<PoolManagerT>.Instance;
					UIDropdownOptionButton uidropdownOptionButton2 = this.dropdownOptionButtonSource;
					Transform transform = this.optionContainer;
					uidropdownOptionButton3 = instance.Spawn<UIDropdownOptionButton>(uidropdownOptionButton2, default(Vector3), default(Quaternion), transform);
				}
				else
				{
					uidropdownOptionButton3 = this.localPool.Pop();
				}
				UIDropdownOptionButton uidropdownOptionButton4 = uidropdownOptionButton3;
				this.dropdownOptionsCreated.Add(this.options[i]);
				string labelFromOption = this.GetLabelFromOption(i);
				Sprite iconFromOption = this.GetIconFromOption(i);
				uidropdownOptionButton4.Initialize(this, base.gameObject, i, labelFromOption, iconFromOption);
				this.dropdownOptionButtons.Add(uidropdownOptionButton4);
				uidropdownOptionButton4.transform.SetAsLastSibling();
			}
		}

		// Token: 0x060007B4 RID: 1972 RVA: 0x000206DC File Offset: 0x0001E8DC
		private GameObject CreateBlocker()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("CreateBlocker", this);
			}
			GameObject gameObject = new GameObject("Blocker", new Type[]
			{
				typeof(RectTransform),
				typeof(Canvas),
				typeof(GraphicRaycaster),
				typeof(Image),
				typeof(UIButton)
			})
			{
				layer = LayerMask.NameToLayer("UI")
			};
			this.blockerCanvas = gameObject.GetComponent<Canvas>();
			this.blockerRectTransform = gameObject.GetComponent<RectTransform>();
			gameObject.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.8f);
			gameObject.GetComponent<UIButton>().onClick.AddListener(new UnityAction(this.ToggleDropdownCanvas));
			return gameObject;
		}

		// Token: 0x060007B5 RID: 1973 RVA: 0x000207B7 File Offset: 0x0001E9B7
		protected void SetValueIconColor(Color color)
		{
			if (this.SuperVerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetValueIconColor", "color", color), this);
			}
			this.valueIcon.color = color;
		}

		// Token: 0x060007B6 RID: 1974 RVA: 0x000207F0 File Offset: 0x0001E9F0
		private void UpdateOptionButtonVisibility()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("UpdateOptionButtonVisibility", this);
			}
			for (int i = 0; i < this.dropdownOptionButtons.Count; i++)
			{
				bool flag = this.OptionShouldBeHidden(i);
				this.dropdownOptionButtons[i].gameObject.SetActive(!flag);
			}
		}

		// Token: 0x060007B7 RID: 1975 RVA: 0x00020848 File Offset: 0x0001EA48
		private void UpdateOptionButtonSelectedStatus()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("UpdateOptionButtonSelectedStatus", this);
			}
			foreach (UIDropdownOptionButton uidropdownOptionButton in this.dropdownOptionButtons)
			{
				uidropdownOptionButton.ViewSelectedStatus();
			}
		}

		// Token: 0x060007B8 RID: 1976 RVA: 0x000208AC File Offset: 0x0001EAAC
		private void HandleScrollRectTransformOrientation()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("HandleScrollRectTransformOrientation", this);
			}
			this.scrollRectTransformDictionary.Apply("Bottom");
			string text = "Bottom";
			Canvas componentInParent = base.GetComponentInParent<Canvas>();
			Camera camera = null;
			if (componentInParent)
			{
				switch (componentInParent.renderMode)
				{
				case RenderMode.ScreenSpaceCamera:
					camera = componentInParent.worldCamera;
					goto IL_006A;
				case RenderMode.WorldSpace:
					camera = Camera.main;
					goto IL_006A;
				}
				camera = null;
			}
			IL_006A:
			bool flag = UIScreenBoundsUtility.IsRectTransformCompletelyOnScreen(this.scrollRect.RectTransform, camera, this.SuperVerboseLogging);
			if (this.SuperVerboseLogging)
			{
				DebugUtility.Log(string.Format("{0}: {1}", "scrollRectIsCompletelyOnScreen", flag), this);
			}
			if (!flag)
			{
				UIScreenBoundsUtility.ExceededScreenEdges screenEdgeExceeded = UIScreenBoundsUtility.GetScreenEdgeExceeded(this.scrollRect.RectTransform, camera, this.SuperVerboseLogging);
				if (this.SuperVerboseLogging)
				{
					DebugUtility.Log(string.Format("{0}: {1}", "exceededScreenEdges", screenEdgeExceeded), this);
				}
				if (screenEdgeExceeded.HasFlag(UIScreenBoundsUtility.ExceededScreenEdges.Bottom))
				{
					text = "Top";
				}
			}
			this.scrollRectTransformDictionary.Apply(text);
		}

		// Token: 0x04000471 RID: 1137
		private const string scrollRectOrientationKeyBottom = "Bottom";

		// Token: 0x04000472 RID: 1138
		private const string scrollRectOrientationKeyTop = "Top";

		// Token: 0x04000473 RID: 1139
		[Header("UIBaseDropdown")]
		[SerializeField]
		private T[] options = Array.Empty<T>();

		// Token: 0x04000474 RID: 1140
		[SerializeField]
		private T[] value = Array.Empty<T>();

		// Token: 0x04000475 RID: 1141
		[Header("Settings")]
		[SerializeField]
		private bool canHaveMultipleValues;

		// Token: 0x04000476 RID: 1142
		[SerializeField]
		private bool canHaveNoValue;

		// Token: 0x04000477 RID: 1143
		[SerializeField]
		private string valueTextIfNoValue = "None";

		// Token: 0x04000478 RID: 1144
		[SerializeField]
		private bool isInteractable = true;

		// Token: 0x04000479 RID: 1145
		[Header("References")]
		[SerializeField]
		private Image backgroundImage;

		// Token: 0x0400047A RID: 1146
		[SerializeField]
		private TextMeshProUGUI labelText;

		// Token: 0x0400047B RID: 1147
		[SerializeField]
		private bool hideLabel;

		// Token: 0x0400047C RID: 1148
		[SerializeField]
		private Image valueIcon;

		// Token: 0x0400047D RID: 1149
		[SerializeField]
		private UIText valueText;

		// Token: 0x0400047E RID: 1150
		[SerializeField]
		private UIButton dropdownButton;

		// Token: 0x0400047F RID: 1151
		[SerializeField]
		private UIButton closeDropdownButton;

		// Token: 0x04000480 RID: 1152
		[SerializeField]
		private UICanvasHandler dropdownCanvas;

		// Token: 0x04000481 RID: 1153
		[SerializeField]
		private UIScrollRect scrollRect;

		// Token: 0x04000482 RID: 1154
		[SerializeField]
		private UIRectTransformDictionary scrollRectTransformDictionary;

		// Token: 0x04000483 RID: 1155
		[SerializeField]
		private RectTransform optionContainer;

		// Token: 0x04000484 RID: 1156
		[SerializeField]
		private UIDropdownOptionButton dropdownOptionButtonSource;

		// Token: 0x04000485 RID: 1157
		[SerializeField]
		private bool handleLabelVisibilityOnStart = true;

		// Token: 0x04000486 RID: 1158
		[SerializeField]
		private UIRectTransformDictionary[] labelRelatedRectTransformDictionaries = Array.Empty<UIRectTransformDictionary>();

		// Token: 0x04000487 RID: 1159
		[SerializeField]
		private List<UIDropdownOptionButton> dropdownOptionButtons = new List<UIDropdownOptionButton>();

		// Token: 0x04000488 RID: 1160
		private readonly HashSet<T> dropdownOptionsCreated = new HashSet<T>();

		// Token: 0x04000489 RID: 1161
		private readonly Stack<UIDropdownOptionButton> localPool = new Stack<UIDropdownOptionButton>();

		// Token: 0x0400048A RID: 1162
		private UIBaseDropdown<T>.Values values;

		// Token: 0x0400048B RID: 1163
		private GameObject blocker;

		// Token: 0x0400048C RID: 1164
		private RectTransform blockerRectTransform;

		// Token: 0x0400048D RID: 1165
		private Canvas blockerCanvas;

		// Token: 0x0400048E RID: 1166
		private bool setBlockerParentToTransformOnEnable;

		// Token: 0x0400048F RID: 1167
		private HashSet<int> optionIndexesToHide = new HashSet<int>();

		// Token: 0x02000134 RID: 308
		[Flags]
		private enum Values
		{
			// Token: 0x04000497 RID: 1175
			None = 0,
			// Token: 0x04000498 RID: 1176
			Icon = 1,
			// Token: 0x04000499 RID: 1177
			Text = 2
		}
	}
}
