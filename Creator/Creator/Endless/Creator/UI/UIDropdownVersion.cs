using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020002C6 RID: 710
	public class UIDropdownVersion : UIGameObject
	{
		// Token: 0x17000188 RID: 392
		// (get) Token: 0x06000BF2 RID: 3058 RVA: 0x00039446 File Offset: 0x00037646
		// (set) Token: 0x06000BF3 RID: 3059 RVA: 0x0003944E File Offset: 0x0003764E
		public UnityEvent OnOptionsDisplayed { get; set; } = new UnityEvent();

		// Token: 0x17000189 RID: 393
		// (get) Token: 0x06000BF4 RID: 3060 RVA: 0x00039457 File Offset: 0x00037657
		// (set) Token: 0x06000BF5 RID: 3061 RVA: 0x0003945F File Offset: 0x0003765F
		public UnityEvent OnOptionsHidden { get; set; } = new UnityEvent();

		// Token: 0x1700018A RID: 394
		// (get) Token: 0x06000BF6 RID: 3062 RVA: 0x00039468 File Offset: 0x00037668
		// (set) Token: 0x06000BF7 RID: 3063 RVA: 0x00039470 File Offset: 0x00037670
		public UnityEvent OnValueChanged { get; set; } = new UnityEvent();

		// Token: 0x1700018B RID: 395
		// (get) Token: 0x06000BF8 RID: 3064 RVA: 0x00039479 File Offset: 0x00037679
		public string Value
		{
			get
			{
				if (this.iEnumerablePresenter.SelectedItemsList.Count != 1)
				{
					return string.Empty;
				}
				return ((UIVersion)this.iEnumerablePresenter.SelectedItemsList[0]).Version;
			}
		}

		// Token: 0x1700018C RID: 396
		// (get) Token: 0x06000BF9 RID: 3065 RVA: 0x000394B0 File Offset: 0x000376B0
		public int IndexOfValue
		{
			get
			{
				if (this.iEnumerablePresenter.SelectedItemsList.Count != 1)
				{
					Debug.LogError(string.Format("{0}.{1} is expected to have a Count of 1! Instead it has a Count of {2}!", "iEnumerablePresenter", "SelectedItemsList", this.iEnumerablePresenter.SelectedItemsList.Count), this);
					return -1;
				}
				object obj = this.iEnumerablePresenter.SelectedItemsList[0];
				return this.iEnumerablePresenter.ModelList.IndexOf(obj);
			}
		}

		// Token: 0x1700018D RID: 397
		// (get) Token: 0x06000BFA RID: 3066 RVA: 0x00039524 File Offset: 0x00037724
		public string UserFacingVersion
		{
			get
			{
				if (this.iEnumerablePresenter.SelectedItemsList.Count != 1)
				{
					return this.noValueText;
				}
				return ((UIVersion)this.iEnumerablePresenter.SelectedItemsList[0]).UserFacingVersion;
			}
		}

		// Token: 0x06000BFB RID: 3067 RVA: 0x0003956C File Offset: 0x0003776C
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.toggleOptionsButton.onClick.AddListener(new UnityAction(this.ToggleOptionsVisibility));
			this.iEnumerablePresenter.OnSelectionChanged += new Action<IReadOnlyList<object>>(this.OnItemSelected);
			this.HideDropdownOptions();
		}

		// Token: 0x06000BFC RID: 3068 RVA: 0x000395CA File Offset: 0x000377CA
		private void OnDisable()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDisable", Array.Empty<object>());
			}
			this.HideDropdownOptions();
		}

		// Token: 0x06000BFD RID: 3069 RVA: 0x000395EC File Offset: 0x000377EC
		private void OnDestroy()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDestroy", Array.Empty<object>());
			}
			this.toggleOptionsButton.onClick.RemoveListener(new UnityAction(this.ToggleOptionsVisibility));
			this.iEnumerablePresenter.OnSelectionChanged -= new Action<IReadOnlyList<object>>(this.OnItemSelected);
		}

		// Token: 0x06000BFE RID: 3070 RVA: 0x00039644 File Offset: 0x00037844
		public void SetOptionsAndValue(IEnumerable<string> newOptions, string newValue, bool triggerOnValueChanged)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetOptionsAndValue", new object[] { newOptions, newValue, triggerOnValueChanged });
			}
			string[] array = ((newOptions == null) ? Array.Empty<string>() : newOptions.ToArray<string>());
			if (this.verboseLogging)
			{
				DebugUtility.DebugEnumerable<string>("newOptionsArray", array, this);
			}
			int num = -1;
			UIVersion[] array2;
			if (array.Length == 0)
			{
				array2 = Array.Empty<UIVersion>();
			}
			else
			{
				array2 = new UIVersion[array.Length];
				for (int i = 0; i < array.Length; i++)
				{
					if (array[i] == newValue)
					{
						num = i;
					}
					array2[i] = new UIVersion(array[i]);
				}
			}
			this.versions = array2;
			if (this.typeStyleOverrideDictionary.Count == 0)
			{
				this.typeStyleOverrideDictionary = new Dictionary<Type, Enum> { 
				{
					typeof(UIVersion),
					this.style
				} };
			}
			this.iEnumerableView.SetTypeStyleOverrideDictionary(this.typeStyleOverrideDictionary);
			this.iEnumerablePresenter.SetModel(this.versions, false);
			if (this.verboseLogging)
			{
				DebugUtility.Log(string.Format("{0}: {1}", "indexOfValue", num), this);
			}
			if (num >= 0)
			{
				this.iEnumerablePresenter.SetSelected(new List<object> { this.iEnumerablePresenter.ModelList[num] }, true, false);
				this.UpdateValueText();
			}
			else if (this.versions.Length != 0)
			{
				num = 0;
				this.iEnumerablePresenter.SetSelected(new List<object> { this.iEnumerablePresenter.ModelList[num] }, true, false);
				this.UpdateValueText();
			}
			else
			{
				this.valueText.Value = this.noValueText;
			}
			if (triggerOnValueChanged)
			{
				UnityEvent onValueChanged = this.OnValueChanged;
				if (onValueChanged == null)
				{
					return;
				}
				onValueChanged.Invoke();
			}
		}

		// Token: 0x06000BFF RID: 3071 RVA: 0x000397F8 File Offset: 0x000379F8
		public void SetValue(string newValue, bool triggerOnValueChanged)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetValue", new object[] { newValue, triggerOnValueChanged });
			}
			if (this.versions == null)
			{
				Debug.LogError("Cannot set value when versions is null", this);
				return;
			}
			int num = -1;
			for (int i = 0; i < this.versions.Length; i++)
			{
				if (!(this.versions[i].Version != newValue))
				{
					num = i;
					break;
				}
			}
			if (num == -1)
			{
				Debug.LogError("newValue '" + newValue + "' not found in versions array", this);
				return;
			}
			this.iEnumerablePresenter.SetSelected(new List<object> { this.iEnumerablePresenter.ModelList[num] }, true, false);
			this.UpdateValueText();
			if (triggerOnValueChanged)
			{
				UnityEvent onValueChanged = this.OnValueChanged;
				if (onValueChanged == null)
				{
					return;
				}
				onValueChanged.Invoke();
			}
		}

		// Token: 0x06000C00 RID: 3072 RVA: 0x000398CE File Offset: 0x00037ACE
		public void SetValueText(string newValueText)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetValueText", new object[] { newValueText });
			}
			this.valueText.Value = newValueText;
		}

		// Token: 0x06000C01 RID: 3073 RVA: 0x000398F9 File Offset: 0x00037AF9
		public void SetIsInteractable(bool interactable)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetIsInteractable", new object[] { interactable });
			}
			this.toggleOptionsButton.interactable = interactable;
		}

		// Token: 0x06000C02 RID: 3074 RVA: 0x00039929 File Offset: 0x00037B29
		private void UpdateValueText()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "UpdateValueText", Array.Empty<object>());
			}
			this.valueText.Value = this.UserFacingVersion;
		}

		// Token: 0x06000C03 RID: 3075 RVA: 0x00039954 File Offset: 0x00037B54
		private void ToggleOptionsVisibility()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ToggleOptionsVisibility", Array.Empty<object>());
			}
			bool activeSelf = this.optionsContainer.gameObject.activeSelf;
			this.optionsContainer.gameObject.SetActive(!activeSelf);
			if (this.optionsContainer.gameObject.activeSelf)
			{
				MonoBehaviourSingleton<UICoroutineManager>.Instance.WaitFramesAndInvoke(new Action(this.iEnumerableView.ReloadDataAndKeepPosition), 1);
				Canvas componentInParent = base.gameObject.GetComponentInParent<Canvas>();
				this.optionsContainerCanvas.sortingOrder = componentInParent.sortingOrder + 1;
				this.OnOptionsDisplayed.Invoke();
				return;
			}
			this.OnOptionsHidden.Invoke();
		}

		// Token: 0x06000C04 RID: 3076 RVA: 0x00039A02 File Offset: 0x00037C02
		private void OnItemSelected(object item)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnItemSelected", new object[] { item });
			}
			this.UpdateValueText();
			this.HideDropdownOptions();
			UnityEvent onValueChanged = this.OnValueChanged;
			if (onValueChanged == null)
			{
				return;
			}
			onValueChanged.Invoke();
		}

		// Token: 0x06000C05 RID: 3077 RVA: 0x00039A3D File Offset: 0x00037C3D
		private void HideDropdownOptions()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "HideDropdownOptions", Array.Empty<object>());
			}
			this.optionsContainer.gameObject.SetActive(false);
			this.OnOptionsHidden.Invoke();
		}

		// Token: 0x04000A55 RID: 2645
		[SerializeField]
		private UIText valueText;

		// Token: 0x04000A56 RID: 2646
		[SerializeField]
		private string noValueText = string.Empty;

		// Token: 0x04000A57 RID: 2647
		[SerializeField]
		private UIButton toggleOptionsButton;

		// Token: 0x04000A58 RID: 2648
		[SerializeField]
		private RectTransform optionsContainer;

		// Token: 0x04000A59 RID: 2649
		[SerializeField]
		private Canvas optionsContainerCanvas;

		// Token: 0x04000A5A RID: 2650
		[SerializeField]
		private UIVersionView.Styles style;

		// Token: 0x04000A5B RID: 2651
		[SerializeField]
		private UIIEnumerablePresenter iEnumerablePresenter;

		// Token: 0x04000A5C RID: 2652
		[SerializeField]
		private UIIEnumerableStraightVirtualizedView iEnumerableView;

		// Token: 0x04000A5D RID: 2653
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x04000A5E RID: 2654
		private UIVersion[] versions = Array.Empty<UIVersion>();

		// Token: 0x04000A5F RID: 2655
		private Dictionary<Type, Enum> typeStyleOverrideDictionary = new Dictionary<Type, Enum>();
	}
}
