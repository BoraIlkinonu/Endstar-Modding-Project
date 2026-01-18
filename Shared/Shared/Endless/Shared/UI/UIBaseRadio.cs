using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Shared.Debugging;
using Endless.Shared.Validation;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x02000244 RID: 580
	public abstract class UIBaseRadio<T> : UIGameObject, IValidatable
	{
		// Token: 0x170002C5 RID: 709
		// (get) Token: 0x06000EBC RID: 3772 RVA: 0x0003F88B File Offset: 0x0003DA8B
		// (set) Token: 0x06000EBD RID: 3773 RVA: 0x0003F893 File Offset: 0x0003DA93
		protected bool VerboseLogging { get; set; }

		// Token: 0x170002C6 RID: 710
		// (get) Token: 0x06000EBE RID: 3774 RVA: 0x0003F89C File Offset: 0x0003DA9C
		// (set) Token: 0x06000EBF RID: 3775 RVA: 0x0003F8A4 File Offset: 0x0003DAA4
		public T Value { get; private set; }

		// Token: 0x170002C7 RID: 711
		// (get) Token: 0x06000EC0 RID: 3776 RVA: 0x0003F8AD File Offset: 0x0003DAAD
		public UnityEvent<T> OnValueChanged { get; } = new UnityEvent<T>();

		// Token: 0x170002C8 RID: 712
		// (get) Token: 0x06000EC1 RID: 3777 RVA: 0x0003F8B5 File Offset: 0x0003DAB5
		protected IReadOnlyList<UIBaseRadioButton<T>> RadioButtons
		{
			get
			{
				return this.radioButtons;
			}
		}

		// Token: 0x170002C9 RID: 713
		// (get) Token: 0x06000EC2 RID: 3778 RVA: 0x0003F8BD File Offset: 0x0003DABD
		protected virtual T[] Values
		{
			get
			{
				return this.GetValues();
			}
		}

		// Token: 0x06000EC3 RID: 3779 RVA: 0x0003F8C5 File Offset: 0x0003DAC5
		protected virtual void Start()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("Start", this);
			}
			if (!this.initialized)
			{
				this.Initialize();
			}
		}

		// Token: 0x06000EC4 RID: 3780 RVA: 0x0003F8E8 File Offset: 0x0003DAE8
		[ContextMenu("Validate")]
		public virtual void Validate()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("Validate", this);
			}
			DebugUtility.DebugHasDuplicates<T>(this.valuesToHide, "valuesToHide", this);
			if (this.valuesToHide.Contains(this.defaultValue))
			{
				DebugUtility.LogError(string.Format("The {0} of {1} is within {2}! That will not be hidden!", "defaultValue", this.defaultValue, "valuesToHide"), this);
			}
		}

		// Token: 0x06000EC5 RID: 3781 RVA: 0x0003F952 File Offset: 0x0003DB52
		public void SetDefaultValue(T defaultValue)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetDefaultValue", "defaultValue", defaultValue), this);
			}
			this.defaultValue = defaultValue;
		}

		// Token: 0x06000EC6 RID: 3782 RVA: 0x0003F983 File Offset: 0x0003DB83
		public void SetValuesToHide(T[] valuesToHide)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetValuesToHide", "valuesToHide", valuesToHide.Length), this);
			}
			this.valuesToHide = valuesToHide;
		}

		// Token: 0x06000EC7 RID: 3783 RVA: 0x0003F9B8 File Offset: 0x0003DBB8
		public void SetValue(T value, bool triggerOnValueChanged)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", new object[] { "SetValue", "value", value, "triggerOnValueChanged", triggerOnValueChanged }), this);
			}
			if (!this.initialized)
			{
				this.Initialize();
			}
			this.Value = value;
			foreach (UIBaseRadioButton<T> uibaseRadioButton in this.radioButtons)
			{
				if (uibaseRadioButton != null)
				{
					uibaseRadioButton.View(this.Value);
				}
			}
			if (triggerOnValueChanged)
			{
				this.OnValueChanged.Invoke(this.Value);
			}
		}

		// Token: 0x06000EC8 RID: 3784 RVA: 0x0003FA5F File Offset: 0x0003DC5F
		public void SetValueToDefault(bool triggerOnValueChanged)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetValueToDefault", "triggerOnValueChanged", triggerOnValueChanged), this);
			}
			this.SetValue(this.defaultValue, triggerOnValueChanged);
		}

		// Token: 0x06000EC9 RID: 3785 RVA: 0x0003FA98 File Offset: 0x0003DC98
		public void EnableControls()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "EnableControls", Array.Empty<object>());
			}
			if (!this.initialized)
			{
				this.Initialize();
			}
			UIBaseRadioButton<T>[] array = this.radioButtons;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetInteractable(true);
			}
		}

		// Token: 0x06000ECA RID: 3786 RVA: 0x0003FAEC File Offset: 0x0003DCEC
		public void DisableControls()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("DisableControls", this);
			}
			if (!this.initialized)
			{
				this.Initialize();
			}
			UIBaseRadioButton<T>[] array = this.radioButtons;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetInteractable(false);
			}
		}

		// Token: 0x06000ECB RID: 3787 RVA: 0x0003FB38 File Offset: 0x0003DD38
		protected virtual void Initialize()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("Initialize", this);
			}
			if (this.initialized)
			{
				return;
			}
			this.initialized = true;
			this.SetValue(this.defaultValue, false);
			UIBaseRadio<T>.Types types = this.type;
			if (types == UIBaseRadio<T>.Types.SpawnRadioButtonForeachValue)
			{
				this.SpawnRadioButtonForeachValue();
				return;
			}
			if (types != UIBaseRadio<T>.Types.GetComponentsInChildren)
			{
				DebugUtility.LogNoEnumSupportError<UIBaseRadio<T>.Types>(this, this.type);
				return;
			}
			this.radioButtons = base.GetComponentsInChildren<UIBaseRadioButton<T>>();
			for (int i = 0; i < this.Values.Length; i++)
			{
				T t = this.Values[i];
				if (this.VerboseLogging)
				{
					DebugUtility.Log(string.Format("{0}[{1}]: {2}", "Values", i, t), this);
				}
				this.radioButtons[i].Initialize(this, t);
			}
		}

		// Token: 0x06000ECC RID: 3788
		protected abstract T[] GetValues();

		// Token: 0x06000ECD RID: 3789 RVA: 0x0003FC00 File Offset: 0x0003DE00
		private void SpawnRadioButtonForeachValue()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("SpawnRadioButtonForeachValue", this);
			}
			if (this.radioButtons.Length != 0)
			{
				foreach (UIBaseRadioButton<T> uibaseRadioButton in this.radioButtons)
				{
					MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn<UIBaseRadioButton<T>>(uibaseRadioButton);
				}
			}
			this.radioButtons = new UIBaseRadioButton<T>[this.Values.Length];
			HashSet<T> hashSet = new HashSet<T>();
			foreach (T t in this.valuesToHide)
			{
				hashSet.Add(t);
			}
			for (int j = 0; j < this.Values.Length; j++)
			{
				T t2 = this.Values[j];
				if (!hashSet.Contains(t2))
				{
					PoolManagerT instance = MonoBehaviourSingleton<PoolManagerT>.Instance;
					UIBaseRadioButton<T> uibaseRadioButton2 = this.radioButtonSource;
					Transform transform = this.radioButtonsContainer;
					UIBaseRadioButton<T> uibaseRadioButton3 = instance.Spawn<UIBaseRadioButton<T>>(uibaseRadioButton2, default(Vector3), default(Quaternion), transform);
					uibaseRadioButton3.Initialize(this, t2);
					this.radioButtons[j] = uibaseRadioButton3;
				}
			}
		}

		// Token: 0x04000942 RID: 2370
		[SerializeField]
		private UIBaseRadio<T>.Types type;

		// Token: 0x04000943 RID: 2371
		[SerializeField]
		private T defaultValue;

		// Token: 0x04000944 RID: 2372
		[SerializeField]
		private T[] valuesToHide = Array.Empty<T>();

		// Token: 0x04000945 RID: 2373
		[SerializeField]
		private UIBaseRadioButton<T> radioButtonSource;

		// Token: 0x04000946 RID: 2374
		[SerializeField]
		private RectTransform radioButtonsContainer;

		// Token: 0x04000948 RID: 2376
		private UIBaseRadioButton<T>[] radioButtons = Array.Empty<UIBaseRadioButton<T>>();

		// Token: 0x04000949 RID: 2377
		private bool initialized;

		// Token: 0x02000245 RID: 581
		private enum Types
		{
			// Token: 0x0400094D RID: 2381
			SpawnRadioButtonForeachValue,
			// Token: 0x0400094E RID: 2382
			GetComponentsInChildren
		}
	}
}
