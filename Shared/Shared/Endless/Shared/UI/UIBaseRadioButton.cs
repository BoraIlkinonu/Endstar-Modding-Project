using System;
using Endless.Shared.Debugging;
using Endless.Shared.Validation;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x02000246 RID: 582
	public abstract class UIBaseRadioButton<T> : UIGameObject, IPoolableT, IValidatable
	{
		// Token: 0x170002CA RID: 714
		// (get) Token: 0x06000ECF RID: 3791 RVA: 0x0003FD2E File Offset: 0x0003DF2E
		// (set) Token: 0x06000ED0 RID: 3792 RVA: 0x0003FD36 File Offset: 0x0003DF36
		protected bool VerboseLogging { get; set; }

		// Token: 0x170002CB RID: 715
		// (get) Token: 0x06000ED1 RID: 3793 RVA: 0x0003FD3F File Offset: 0x0003DF3F
		// (set) Token: 0x06000ED2 RID: 3794 RVA: 0x0003FD47 File Offset: 0x0003DF47
		public MonoBehaviour Prefab { get; set; }

		// Token: 0x170002CC RID: 716
		// (get) Token: 0x06000ED3 RID: 3795 RVA: 0x000050D2 File Offset: 0x000032D2
		public bool IsUi
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06000ED4 RID: 3796 RVA: 0x0003FD50 File Offset: 0x0003DF50
		private void Start()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("Start", this);
			}
			this.button.onClick.AddListener(new UnityAction(this.Select));
		}

		// Token: 0x06000ED5 RID: 3797 RVA: 0x0003FD81 File Offset: 0x0003DF81
		[ContextMenu("Validate")]
		public void Validate()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Validate", Array.Empty<object>());
			}
			if (!this.selectedVisual && !this.notSelectedVisual)
			{
				DebugUtility.LogWarning("You probably want either a selectedVisual or notSelectedVisual here", this);
			}
		}

		// Token: 0x06000ED6 RID: 3798 RVA: 0x0003FDC0 File Offset: 0x0003DFC0
		public void OnSpawn()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("OnSpawn", this);
			}
		}

		// Token: 0x06000ED7 RID: 3799 RVA: 0x0003FDD8 File Offset: 0x0003DFD8
		public void OnDespawn()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("OnDespawn", this);
			}
			if (this.radio)
			{
				this.radio.OnValueChanged.RemoveListener(new UnityAction<T>(this.View));
				this.radio = null;
			}
		}

		// Token: 0x06000ED8 RID: 3800 RVA: 0x0003FE28 File Offset: 0x0003E028
		public void Select()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("Select", this);
			}
			this.radio.SetValue(this.value, true);
		}

		// Token: 0x06000ED9 RID: 3801 RVA: 0x0003FE4F File Offset: 0x0003E04F
		public virtual void SetInteractable(bool interactable)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetInteractable", "interactable", interactable), this);
			}
			this.button.interactable = interactable;
		}

		// Token: 0x06000EDA RID: 3802 RVA: 0x0003FE88 File Offset: 0x0003E088
		public virtual void Initialize(UIBaseRadio<T> radio, T value)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", new object[] { "Initialize", "radio", radio, "value", value }), this);
			}
			this.radio = radio;
			radio.OnValueChanged.AddListener(new UnityAction<T>(this.View));
			this.value = value;
			this.View(radio.Value);
		}

		// Token: 0x06000EDB RID: 3803 RVA: 0x0003FF0C File Offset: 0x0003E10C
		public void View(T radioValue)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "View", "radioValue", radioValue), this);
			}
			bool flag = radioValue.Equals(this.value);
			if (this.selectedVisual)
			{
				this.selectedVisual.SetActive(flag);
			}
			if (this.notSelectedVisual)
			{
				GameObject gameObject = this.notSelectedVisual;
				if (gameObject == null)
				{
					return;
				}
				gameObject.SetActive(!flag);
			}
		}

		// Token: 0x0400094F RID: 2383
		[SerializeField]
		private UIButton button;

		// Token: 0x04000950 RID: 2384
		[SerializeField]
		private GameObject selectedVisual;

		// Token: 0x04000951 RID: 2385
		[SerializeField]
		private GameObject notSelectedVisual;

		// Token: 0x04000953 RID: 2387
		private UIBaseRadio<T> radio;

		// Token: 0x04000954 RID: 2388
		private T value;
	}
}
