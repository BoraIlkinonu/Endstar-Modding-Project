using System;
using Endless.Shared.Debugging;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Endless.Shared.UI
{
	// Token: 0x0200011F RID: 287
	public class UIToggleGroupToggleHandler : UIGameObject, IPoolableT
	{
		// Token: 0x17000130 RID: 304
		// (get) Token: 0x06000718 RID: 1816 RVA: 0x0001E36D File Offset: 0x0001C56D
		// (set) Token: 0x06000719 RID: 1817 RVA: 0x0001E375 File Offset: 0x0001C575
		public MonoBehaviour Prefab { get; set; }

		// Token: 0x17000131 RID: 305
		// (get) Token: 0x0600071A RID: 1818 RVA: 0x000050D2 File Offset: 0x000032D2
		public bool IsUi
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000132 RID: 306
		// (get) Token: 0x0600071B RID: 1819 RVA: 0x0001E37E File Offset: 0x0001C57E
		public UIToggle Toggle
		{
			get
			{
				return this.toggle;
			}
		}

		// Token: 0x17000133 RID: 307
		// (get) Token: 0x0600071C RID: 1820 RVA: 0x0001E386 File Offset: 0x0001C586
		// (set) Token: 0x0600071D RID: 1821 RVA: 0x0001E38E File Offset: 0x0001C58E
		public UIToggleGroupOption Option { get; private set; }

		// Token: 0x0600071E RID: 1822 RVA: 0x0001E397 File Offset: 0x0001C597
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.toggle.OnChange.AddListener(new UnityAction<bool>(this.TriggerOnSubToggleChange));
		}

		// Token: 0x0600071F RID: 1823 RVA: 0x0001E3CD File Offset: 0x0001C5CD
		public void OnSpawn()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnSpawn", Array.Empty<object>());
			}
		}

		// Token: 0x06000720 RID: 1824 RVA: 0x0001E3E7 File Offset: 0x0001C5E7
		public void OnDespawn()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDespawn", Array.Empty<object>());
			}
			this.OnSubToggleChange.RemoveAllListeners();
		}

		// Token: 0x06000721 RID: 1825 RVA: 0x0001E40C File Offset: 0x0001C60C
		public void DisplayToggleGroupOption(UIToggleGroupOption option)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "DisplayToggleGroupOption", new object[] { option.Key });
			}
			this.Option = option;
			if (this.keyDisplay)
			{
				this.keyDisplay.text = option.Key;
			}
			if (this.icon)
			{
				this.icon.sprite = option.Icon;
			}
		}

		// Token: 0x06000722 RID: 1826 RVA: 0x0001E47E File Offset: 0x0001C67E
		private void TriggerOnSubToggleChange(bool isOn)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "TriggerOnSubToggleChange", new object[] { isOn });
			}
			this.OnSubToggleChange.Invoke(this);
		}

		// Token: 0x04000424 RID: 1060
		public UnityEvent<UIToggleGroupToggleHandler> OnSubToggleChange = new UnityEvent<UIToggleGroupToggleHandler>();

		// Token: 0x04000425 RID: 1061
		[SerializeField]
		private UIToggle toggle;

		// Token: 0x04000426 RID: 1062
		[SerializeField]
		private TextMeshProUGUI keyDisplay;

		// Token: 0x04000427 RID: 1063
		[SerializeField]
		private Image icon;

		// Token: 0x04000428 RID: 1064
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
