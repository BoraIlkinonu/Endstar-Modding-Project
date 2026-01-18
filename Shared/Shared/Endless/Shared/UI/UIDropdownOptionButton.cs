using System;
using Endless.Shared.Debugging;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Endless.Shared.UI
{
	// Token: 0x02000138 RID: 312
	public class UIDropdownOptionButton : UIGameObject, IPoolableT
	{
		// Token: 0x060007D9 RID: 2009 RVA: 0x000212DA File Offset: 0x0001F4DA
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.selectButton.onClick.AddListener(new UnityAction(this.Select));
		}

		// Token: 0x1700015C RID: 348
		// (get) Token: 0x060007DA RID: 2010 RVA: 0x00021310 File Offset: 0x0001F510
		// (set) Token: 0x060007DB RID: 2011 RVA: 0x00021318 File Offset: 0x0001F518
		public MonoBehaviour Prefab { get; set; }

		// Token: 0x1700015D RID: 349
		// (get) Token: 0x060007DC RID: 2012 RVA: 0x000050D2 File Offset: 0x000032D2
		public bool IsUi
		{
			get
			{
				return true;
			}
		}

		// Token: 0x060007DD RID: 2013 RVA: 0x00021321 File Offset: 0x0001F521
		public void OnSpawn()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnSpawn", Array.Empty<object>());
			}
		}

		// Token: 0x060007DE RID: 2014 RVA: 0x0002133B File Offset: 0x0001F53B
		public void OnDespawn()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDespawn", Array.Empty<object>());
			}
			IUIDropdownable iuidropdownable = this.dropdown;
			if (iuidropdownable == null)
			{
				return;
			}
			iuidropdownable.OnValueChanged.RemoveListener(new UnityAction(this.ViewSelectedStatus));
		}

		// Token: 0x060007DF RID: 2015 RVA: 0x00021378 File Offset: 0x0001F578
		public void Initialize(IUIDropdownable dropdown, GameObject dropdownRef, int index, string label, Sprite icon)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Initialize", new object[]
				{
					(dropdown == null) ? "null" : "populated",
					index,
					label,
					icon
				});
			}
			this.dropdown = dropdown;
			this.dropdownRef = dropdownRef;
			if (dropdown == null)
			{
				DebugUtility.LogException(new NullReferenceException("Provided dropdown is null"), this);
			}
			if (dropdownRef == null)
			{
				DebugUtility.LogException(new NullReferenceException("Provided dropdownRef is null"), this);
			}
			this.index = index;
			dropdown.OnValueChanged.AddListener(new UnityAction(this.ViewSelectedStatus));
			this.HandleHiddenVisibility();
			this.SetLabel(label);
			this.SetIcon(icon);
			this.ViewSelectedStatus();
		}

		// Token: 0x060007E0 RID: 2016 RVA: 0x00021438 File Offset: 0x0001F638
		public void ViewSelectedStatus()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ViewSelectedStatus", Array.Empty<object>());
			}
			bool isSelected = this.dropdown.GetIsSelected(this.index);
			this.selectedIcon.enabled = isSelected;
		}

		// Token: 0x060007E1 RID: 2017 RVA: 0x0002147C File Offset: 0x0001F67C
		public void HandleHiddenVisibility()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "HandleHiddenVisibility", Array.Empty<object>());
			}
			bool flag = this.dropdown.OptionShouldBeHidden(this.index);
			base.gameObject.SetActive(!flag);
		}

		// Token: 0x060007E2 RID: 2018 RVA: 0x000214C4 File Offset: 0x0001F6C4
		private void Select()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Select", Array.Empty<object>());
			}
			if (this.dropdown == null)
			{
				DebugUtility.LogException(new NullReferenceException("Provided dropdown is null"), this);
				return;
			}
			this.dropdown.ToggleValueIndex(this.index, true);
		}

		// Token: 0x060007E3 RID: 2019 RVA: 0x00021514 File Offset: 0x0001F714
		private void SetLabel(string label)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetLabel", new object[] { label });
			}
			this.labelText.enabled = !string.IsNullOrWhiteSpace(label);
			this.labelText.text = label ?? string.Empty;
		}

		// Token: 0x060007E4 RID: 2020 RVA: 0x00021568 File Offset: 0x0001F768
		private void SetIcon(Sprite icon)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetIcon", new object[] { icon.DebugSafeName(true) });
			}
			this.iconImage.enabled = icon;
			this.iconImage.sprite = icon;
		}

		// Token: 0x040004A9 RID: 1193
		[SerializeField]
		private UIButton selectButton;

		// Token: 0x040004AA RID: 1194
		[SerializeField]
		private Image selectButtonBackgroundImage;

		// Token: 0x040004AB RID: 1195
		[SerializeField]
		private Image iconImage;

		// Token: 0x040004AC RID: 1196
		[SerializeField]
		private TextMeshProUGUI labelText;

		// Token: 0x040004AD RID: 1197
		[SerializeField]
		private Image selectedIcon;

		// Token: 0x040004AE RID: 1198
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x040004AF RID: 1199
		private IUIDropdownable dropdown;

		// Token: 0x040004B0 RID: 1200
		private GameObject dropdownRef;

		// Token: 0x040004B1 RID: 1201
		private int index;
	}
}
