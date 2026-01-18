using System;
using System.Collections.Generic;
using Endless.Shared.Debugging;
using Endless.Shared.Validation;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x0200011E RID: 286
	public class UIToggleGroup : UIGameObject, IValidatable
	{
		// Token: 0x1700012C RID: 300
		// (get) Token: 0x06000708 RID: 1800 RVA: 0x0001DD06 File Offset: 0x0001BF06
		// (set) Token: 0x06000709 RID: 1801 RVA: 0x0001DD0E File Offset: 0x0001BF0E
		public bool Initialized { get; private set; }

		// Token: 0x1700012D RID: 301
		// (get) Token: 0x0600070A RID: 1802 RVA: 0x0001DD17 File Offset: 0x0001BF17
		// (set) Token: 0x0600070B RID: 1803 RVA: 0x0001DD1F File Offset: 0x0001BF1F
		public List<string> ToggledOnValues { get; private set; } = new List<string>();

		// Token: 0x1700012E RID: 302
		// (get) Token: 0x0600070C RID: 1804 RVA: 0x0001DD28 File Offset: 0x0001BF28
		public IReadOnlyList<int> ToggledOnIndexes
		{
			get
			{
				List<int> list = new List<int>();
				for (int i = 0; i < this.toggles.Length; i++)
				{
					if (this.toggles[i].Toggle.IsOn)
					{
						list.Add(i);
					}
				}
				return list;
			}
		}

		// Token: 0x1700012F RID: 303
		// (get) Token: 0x0600070D RID: 1805 RVA: 0x0001DD6C File Offset: 0x0001BF6C
		public int IndexOfFirstToggledOnValue
		{
			get
			{
				for (int i = 0; i < this.toggles.Length; i++)
				{
					if (this.toggles[i].Toggle.IsOn)
					{
						return i;
					}
				}
				return -1;
			}
		}

		// Token: 0x0600070E RID: 1806 RVA: 0x0001DDA3 File Offset: 0x0001BFA3
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			if (this.initializeOnStart)
			{
				this.Initialize();
			}
		}

		// Token: 0x0600070F RID: 1807 RVA: 0x0001DDCC File Offset: 0x0001BFCC
		[ContextMenu("Validate")]
		public void Validate()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Validate", Array.Empty<object>());
			}
			if (this.options.Length == 0)
			{
				DebugUtility.LogError("There must be at least 1 entry in the options field!", this);
			}
			if (this.options.Length == 1)
			{
				DebugUtility.LogError("Only 1 option? Not much of a choice. You could just use a single toggle.", this);
			}
			HashSet<string> hashSet = new HashSet<string>();
			for (int i = 0; i < this.options.Length; i++)
			{
				if (!hashSet.Add(this.options[i].Key))
				{
					DebugUtility.LogError("You have more than one option of " + this.options[i].Key + "! Each option key must be unique; no duplicates!", this);
				}
			}
		}

		// Token: 0x06000710 RID: 1808 RVA: 0x0001DE74 File Offset: 0x0001C074
		public void Initialize()
		{
			if (this.Initialized)
			{
				DebugUtility.LogWarning(this, "Initialize", "Already initialize!", Array.Empty<object>());
				return;
			}
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Initialize", Array.Empty<object>());
			}
			this.SetOptions(this.options);
			this.Initialized = true;
		}

		// Token: 0x06000711 RID: 1809 RVA: 0x0001DECC File Offset: 0x0001C0CC
		public void SetOptionsAsStrings(string[] newOptions)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetOptionsAsStrings", new object[] { newOptions.Length });
			}
			this.options = new UIToggleGroupOption[newOptions.Length];
			for (int i = 0; i < newOptions.Length; i++)
			{
				this.options[i] = new UIToggleGroupOption(newOptions[i]);
			}
			this.SetOptions(this.options);
		}

		// Token: 0x06000712 RID: 1810 RVA: 0x0001DF3C File Offset: 0x0001C13C
		public void SetOptions(UIToggleGroupOption[] newOptions)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetOptions", new object[] { newOptions.Length });
			}
			for (int i = 0; i < this.toggles.Length; i++)
			{
				MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn<UIToggleGroupToggleHandler>(this.toggles[i]);
			}
			this.ToggledOnValues.Clear();
			this.toggleValues.Clear();
			this.options = newOptions;
			this.toggles = new UIToggleGroupToggleHandler[this.options.Length];
			for (int j = 0; j < this.options.Length; j++)
			{
				PoolManagerT instance = MonoBehaviourSingleton<PoolManagerT>.Instance;
				UIToggleGroupToggleHandler uitoggleGroupToggleHandler = this.toggleSource;
				Transform transform = this.valuesContainer;
				UIToggleGroupToggleHandler uitoggleGroupToggleHandler2 = instance.Spawn<UIToggleGroupToggleHandler>(uitoggleGroupToggleHandler, default(Vector3), default(Quaternion), transform);
				uitoggleGroupToggleHandler2.DisplayToggleGroupOption(this.options[j]);
				bool flag = this.restrictToOneValue && j == 0;
				uitoggleGroupToggleHandler2.Toggle.SetIsOn(flag, true, true);
				if (flag)
				{
					this.lastToggleChanged = uitoggleGroupToggleHandler2;
					this.ToggledOnValues.Add(this.options[j].Key);
				}
				this.toggles[j] = uitoggleGroupToggleHandler2;
				this.toggleValues.Add(uitoggleGroupToggleHandler2, this.options[j].Key);
				uitoggleGroupToggleHandler2.OnSubToggleChange.AddListener(new UnityAction<UIToggleGroupToggleHandler>(this.OnSubToggleChange));
			}
		}

		// Token: 0x06000713 RID: 1811 RVA: 0x0001E0A0 File Offset: 0x0001C2A0
		public void SetToggledOnValue(int index, bool suppressOnChange, bool tweenVisuals)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetToggledOnValue", new object[] { index, suppressOnChange, tweenVisuals });
			}
			for (int i = 0; i < this.toggles.Length; i++)
			{
				bool flag = i == index;
				this.toggles[i].Toggle.SetIsOn(flag, true, tweenVisuals);
				if (flag)
				{
					this.lastToggleChanged = this.toggles[i];
				}
			}
			this.ToggledOnValues.Clear();
			this.ToggledOnValues.Add(this.toggles[index].Option.Key);
			if (!suppressOnChange)
			{
				this.OnChange.Invoke();
			}
		}

		// Token: 0x06000714 RID: 1812 RVA: 0x0001E158 File Offset: 0x0001C358
		public void SetToggledOnValues(List<string> toggledOnValues)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetToggledOnValues", new object[] { toggledOnValues.Count });
			}
			this.ToggledOnValues = new List<string>(toggledOnValues);
			foreach (UIToggleGroupToggleHandler uitoggleGroupToggleHandler in this.toggles)
			{
				uitoggleGroupToggleHandler.Toggle.SetIsOn(toggledOnValues.Contains(uitoggleGroupToggleHandler.Option.Key), true, false);
			}
		}

		// Token: 0x06000715 RID: 1813 RVA: 0x0001E1CF File Offset: 0x0001C3CF
		public UIToggleGroupToggleHandler GetToggle(int index)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "GetToggle", new object[] { index });
			}
			return this.toggles[index];
		}

		// Token: 0x06000716 RID: 1814 RVA: 0x0001E1FC File Offset: 0x0001C3FC
		private void OnSubToggleChange(UIToggleGroupToggleHandler toggle)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnSubToggleChange", new object[] { toggle.Toggle.IsOn });
			}
			if (this.restrictToOneValue && toggle == this.lastToggleChanged)
			{
				toggle.Toggle.SetIsOn(true, true, true);
				return;
			}
			this.lastToggleChanged = toggle;
			string text = this.toggleValues[toggle];
			if (toggle.Toggle.IsOn)
			{
				this.ToggledOnValues.Add(text);
			}
			else
			{
				this.ToggledOnValues.Remove(text);
			}
			if (this.restrictToOneValue)
			{
				for (int i = 0; i < this.toggles.Length; i++)
				{
					if (this.toggles[i].Toggle.IsOn && this.toggles[i] != toggle)
					{
						this.toggles[i].Toggle.SetIsOn(false, true, true);
					}
				}
			}
			this.OnChange.Invoke();
		}

		// Token: 0x04000418 RID: 1048
		public UnityEvent OnChange = new UnityEvent();

		// Token: 0x04000419 RID: 1049
		[SerializeField]
		private UIToggleGroupOption[] options = new UIToggleGroupOption[]
		{
			new UIToggleGroupOption("A"),
			new UIToggleGroupOption("B")
		};

		// Token: 0x0400041A RID: 1050
		[SerializeField]
		private bool initializeOnStart = true;

		// Token: 0x0400041B RID: 1051
		[SerializeField]
		private bool restrictToOneValue;

		// Token: 0x0400041C RID: 1052
		[SerializeField]
		private RectTransform valuesContainer;

		// Token: 0x0400041D RID: 1053
		[SerializeField]
		private UIToggleGroupToggleHandler toggleSource;

		// Token: 0x0400041E RID: 1054
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x0400041F RID: 1055
		private UIToggleGroupToggleHandler[] toggles = new UIToggleGroupToggleHandler[0];

		// Token: 0x04000420 RID: 1056
		private readonly Dictionary<UIToggleGroupToggleHandler, string> toggleValues = new Dictionary<UIToggleGroupToggleHandler, string>();

		// Token: 0x04000421 RID: 1057
		private UIToggleGroupToggleHandler lastToggleChanged;
	}
}
