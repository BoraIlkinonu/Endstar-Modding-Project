using System;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using Endless.Shared.Validation;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Endless.Shared.UI
{
	// Token: 0x0200012B RID: 299
	public class UIDisplayAndHideHandler : UIGameObject, IValidatable
	{
		// Token: 0x17000139 RID: 313
		// (get) Token: 0x06000744 RID: 1860 RVA: 0x0001E980 File Offset: 0x0001CB80
		// (set) Token: 0x06000745 RID: 1861 RVA: 0x0001E988 File Offset: 0x0001CB88
		public bool IsDisplaying { get; private set; }

		// Token: 0x1700013A RID: 314
		// (get) Token: 0x06000746 RID: 1862 RVA: 0x0001E991 File Offset: 0x0001CB91
		public bool IsTweeningDisplay
		{
			get
			{
				return this.displayTweenCollection.IsAnyTweening();
			}
		}

		// Token: 0x1700013B RID: 315
		// (get) Token: 0x06000747 RID: 1863 RVA: 0x0001E99E File Offset: 0x0001CB9E
		public bool IsTweeningHide
		{
			get
			{
				return this.hideTweenCollection.IsAnyTweening();
			}
		}

		// Token: 0x1700013C RID: 316
		// (get) Token: 0x06000748 RID: 1864 RVA: 0x0001E9AB File Offset: 0x0001CBAB
		public bool IsTweeningDisplayOrHide
		{
			get
			{
				return this.IsTweeningDisplay || this.IsTweeningHide;
			}
		}

		// Token: 0x06000749 RID: 1865 RVA: 0x0001E9BD File Offset: 0x0001CBBD
		[ContextMenu("Validate")]
		public void Validate()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Validate", Array.Empty<object>());
			}
			if (this.ignoreValidation)
			{
				return;
			}
			this.displayTweenCollection.ValidateForNumberOfTweens(1);
			this.hideTweenCollection.ValidateForNumberOfTweens(1);
		}

		// Token: 0x0600074A RID: 1866 RVA: 0x0001E9F8 File Offset: 0x0001CBF8
		public void Display()
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log("Display", this);
			}
			this.Display(null);
		}

		// Token: 0x0600074B RID: 1867 RVA: 0x0001EA14 File Offset: 0x0001CC14
		public void Display(Action onTweenComplete)
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log("Display ( onTweenComplete: " + onTweenComplete.DebugIsNull() + " )", this);
			}
			if (!this.initialized)
			{
				this.Initialize();
			}
			if (this.handleSetActive)
			{
				base.gameObject.SetActive(true);
			}
			if (this.hideTweenCollection.IsAnyTweening())
			{
				this.hideTweenCollection.Cancel();
			}
			this.IsDisplaying = true;
			this.displayTweenCollection.Tween(onTweenComplete);
			this.OnDisplayStart.Invoke();
		}

		// Token: 0x0600074C RID: 1868 RVA: 0x0001EA9C File Offset: 0x0001CC9C
		public void Hide()
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log("Hide", this);
			}
			this.Hide(null);
		}

		// Token: 0x0600074D RID: 1869 RVA: 0x0001EAB8 File Offset: 0x0001CCB8
		public void Hide(Action onTweenComplete)
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log("Hide ( onTweenComplete: " + onTweenComplete.DebugIsNull() + " )", this);
			}
			if (!this.initialized)
			{
				this.Initialize();
			}
			if (this.displayTweenCollection.IsAnyTweening())
			{
				this.displayTweenCollection.Cancel();
			}
			this.IsDisplaying = false;
			this.hideTweenCollection.Tween(onTweenComplete);
			this.OnHideStart.Invoke();
		}

		// Token: 0x0600074E RID: 1870 RVA: 0x0001EB2C File Offset: 0x0001CD2C
		public void SetDisplayDelay(float delay)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetDisplayDelay", new object[] { delay });
			}
			BaseTween[] tweens = this.displayTweenCollection.Tweens;
			for (int i = 0; i < tweens.Length; i++)
			{
				tweens[i].Delay = delay;
			}
		}

		// Token: 0x0600074F RID: 1871 RVA: 0x0001EB80 File Offset: 0x0001CD80
		public void AddDisplayDelay(float delay)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "AddDisplayDelay", new object[] { delay });
			}
			BaseTween[] tweens = this.displayTweenCollection.Tweens;
			for (int i = 0; i < tweens.Length; i++)
			{
				tweens[i].Delay += delay;
			}
		}

		// Token: 0x06000750 RID: 1872 RVA: 0x0001EBDC File Offset: 0x0001CDDC
		public void SetToDisplayStart(bool triggerUnityEvent)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetToDisplayStart", new object[] { triggerUnityEvent });
			}
			this.IsDisplaying = true;
			this.displayTweenCollection.SetToStart();
			if (this.handleSetActive)
			{
				base.gameObject.SetActive(true);
			}
			if (triggerUnityEvent)
			{
				this.OnDisplayStart.Invoke();
			}
		}

		// Token: 0x06000751 RID: 1873 RVA: 0x0001EC40 File Offset: 0x0001CE40
		public void SetToDisplayEnd(bool triggerUnityEvent)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetToDisplayEnd", new object[] { triggerUnityEvent });
			}
			this.IsDisplaying = true;
			this.displayTweenCollection.SetToEnd();
			if (this.handleSetActive)
			{
				base.gameObject.SetActive(true);
			}
			if (triggerUnityEvent)
			{
				this.OnDisplayStart.Invoke();
				this.OnDisplayComplete.Invoke();
			}
		}

		// Token: 0x06000752 RID: 1874 RVA: 0x0001ECB0 File Offset: 0x0001CEB0
		public void SetToHideStart(bool triggerUnityEvent)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetToHideStart", new object[] { triggerUnityEvent });
			}
			this.IsDisplaying = true;
			this.hideTweenCollection.SetToStart();
			if (triggerUnityEvent)
			{
				this.OnHideStart.Invoke();
			}
		}

		// Token: 0x06000753 RID: 1875 RVA: 0x0001ED00 File Offset: 0x0001CF00
		public void SetToHideEnd(bool triggerUnityEvent)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetToHideEnd", new object[] { triggerUnityEvent });
			}
			this.IsDisplaying = false;
			this.hideTweenCollection.SetToEnd();
			if (this.handleSetActive)
			{
				base.gameObject.SetActive(false);
			}
			if (triggerUnityEvent)
			{
				this.OnHideStart.Invoke();
				this.OnHideComplete.Invoke();
			}
		}

		// Token: 0x06000754 RID: 1876 RVA: 0x0001ED6E File Offset: 0x0001CF6E
		public void Set(bool display)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Set", new object[] { display });
			}
			if (display)
			{
				this.Display();
				return;
			}
			this.Hide();
		}

		// Token: 0x06000755 RID: 1877 RVA: 0x0001EDA2 File Offset: 0x0001CFA2
		public void Toggle()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Toggle", Array.Empty<object>());
			}
			if (this.IsDisplaying)
			{
				this.Hide();
				return;
			}
			this.Display();
		}

		// Token: 0x06000756 RID: 1878 RVA: 0x0001EDD1 File Offset: 0x0001CFD1
		public void CancelAnyTweens()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "CancelAnyTweens", Array.Empty<object>());
			}
			this.CancelDisplayTweens();
			this.CancelHideTweens();
		}

		// Token: 0x06000757 RID: 1879 RVA: 0x0001EDF7 File Offset: 0x0001CFF7
		public void CancelDisplayTweens()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "CancelDisplayTweens", Array.Empty<object>());
			}
			if (this.IsTweeningDisplay)
			{
				this.displayTweenCollection.Cancel();
			}
		}

		// Token: 0x06000758 RID: 1880 RVA: 0x0001EE24 File Offset: 0x0001D024
		public void CancelHideTweens()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "CancelAnyTweens", Array.Empty<object>());
			}
			if (this.IsTweeningHide)
			{
				this.hideTweenCollection.Cancel();
			}
		}

		// Token: 0x06000759 RID: 1881 RVA: 0x0001EE51 File Offset: 0x0001D051
		public void SetDisplayDuration(float inSeconds)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetDisplayDuration", new object[] { inSeconds });
			}
			this.displayTweenCollection.SetInSeconds(inSeconds);
		}

		// Token: 0x0600075A RID: 1882 RVA: 0x0001EE84 File Offset: 0x0001D084
		private void Initialize()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Initialize", Array.Empty<object>());
			}
			if (this.initialized)
			{
				return;
			}
			this.displayTweenCollection.OnAllTweenCompleted.AddListener(new UnityAction(this.OnDisplayTweensComplete));
			this.hideTweenCollection.OnAllTweenCompleted.AddListener(new UnityAction(this.OnHideTweensComplete));
			this.initialized = true;
		}

		// Token: 0x0600075B RID: 1883 RVA: 0x0001EEF1 File Offset: 0x0001D0F1
		private void OnDisplayTweensComplete()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDisplayTweensComplete", Array.Empty<object>());
			}
			this.OnDisplayComplete.Invoke();
		}

		// Token: 0x0600075C RID: 1884 RVA: 0x0001EF16 File Offset: 0x0001D116
		private void OnHideTweensComplete()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnHideTweensComplete", Array.Empty<object>());
			}
			this.IsDisplaying = false;
			if (this.handleSetActive)
			{
				base.gameObject.SetActive(false);
			}
			this.OnHideComplete.Invoke();
		}

		// Token: 0x04000441 RID: 1089
		public UnityEvent OnDisplayStart = new UnityEvent();

		// Token: 0x04000442 RID: 1090
		public UnityEvent OnDisplayComplete = new UnityEvent();

		// Token: 0x04000443 RID: 1091
		public UnityEvent OnHideStart = new UnityEvent();

		// Token: 0x04000444 RID: 1092
		public UnityEvent OnHideComplete = new UnityEvent();

		// Token: 0x04000445 RID: 1093
		[SerializeField]
		private bool handleSetActive = true;

		// Token: 0x04000446 RID: 1094
		[FormerlySerializedAs("displayTweens")]
		[SerializeField]
		private TweenCollection displayTweenCollection;

		// Token: 0x04000447 RID: 1095
		[FormerlySerializedAs("hideTweens")]
		[SerializeField]
		private TweenCollection hideTweenCollection;

		// Token: 0x04000448 RID: 1096
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x04000449 RID: 1097
		[Tooltip("Use this only if you know what you are doing :P")]
		[SerializeField]
		private bool ignoreValidation;

		// Token: 0x0400044A RID: 1098
		private bool initialized;
	}
}
