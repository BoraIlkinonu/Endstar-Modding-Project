using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x02000261 RID: 609
	public abstract class UIBaseTabGroup<T> : UIGameObject, IUITabGroup
	{
		// Token: 0x170002E7 RID: 743
		// (get) Token: 0x06000F6D RID: 3949 RVA: 0x00042591 File Offset: 0x00040791
		// (set) Token: 0x06000F6E RID: 3950 RVA: 0x00042599 File Offset: 0x00040799
		public UnityEvent<int> OnValueChangedWithIndex { get; private set; } = new UnityEvent<int>();

		// Token: 0x170002E8 RID: 744
		// (get) Token: 0x06000F6F RID: 3951 RVA: 0x000425A2 File Offset: 0x000407A2
		// (set) Token: 0x06000F70 RID: 3952 RVA: 0x000425AA File Offset: 0x000407AA
		public int ValueIndex { get; private set; }

		// Token: 0x170002E9 RID: 745
		// (get) Token: 0x06000F71 RID: 3953 RVA: 0x000425B3 File Offset: 0x000407B3
		// (set) Token: 0x06000F72 RID: 3954 RVA: 0x000425BB File Offset: 0x000407BB
		public Canvas Canvas { get; private set; }

		// Token: 0x170002EA RID: 746
		// (get) Token: 0x06000F73 RID: 3955 RVA: 0x000425C4 File Offset: 0x000407C4
		// (set) Token: 0x06000F74 RID: 3956 RVA: 0x000425CC File Offset: 0x000407CC
		protected bool VerboseLogging { get; set; }

		// Token: 0x170002EB RID: 747
		// (get) Token: 0x06000F75 RID: 3957 RVA: 0x000425D5 File Offset: 0x000407D5
		public T Value
		{
			get
			{
				return this.options[this.ValueIndex];
			}
		}

		// Token: 0x170002EC RID: 748
		// (get) Token: 0x06000F76 RID: 3958 RVA: 0x000425E8 File Offset: 0x000407E8
		public int OptionsLength
		{
			get
			{
				return this.options.Length;
			}
		}

		// Token: 0x170002ED RID: 749
		// (get) Token: 0x06000F77 RID: 3959 RVA: 0x000425F2 File Offset: 0x000407F2
		// (set) Token: 0x06000F78 RID: 3960 RVA: 0x000425FA File Offset: 0x000407FA
		public T PreviousValue { get; private set; }

		// Token: 0x06000F79 RID: 3961 RVA: 0x00042603 File Offset: 0x00040803
		protected virtual void Start()
		{
			if (this.VerboseLogging)
			{
				Debug.Log("Start", this);
			}
			this.SetHiddenOptions(this.hiddenOptionsList);
			if (this.tabs.Length == 0)
			{
				this.SpawnTabs();
			}
			this.ViewAllTabs();
		}

		// Token: 0x06000F7A RID: 3962 RVA: 0x0004263C File Offset: 0x0004083C
		public void SetOptionsAndValue(T[] options, int valueIndex, bool triggerOnValueChanged)
		{
			if (this.VerboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2}, {3}: {4}, {5}: {6} )", new object[] { "SetOptionsAndValue", "options", options.Length, "valueIndex", valueIndex, "triggerOnValueChanged", triggerOnValueChanged }), this);
			}
			if (options.Length == 0)
			{
				Debug.LogError("options must have at least one item!", this);
				return;
			}
			this.options = options;
			if (valueIndex < 0 || valueIndex >= options.Length)
			{
				Debug.LogError(string.Format("{0} '{1}' is out of range! {2} has a size of {3}!", new object[] { "valueIndex", valueIndex, "options", options.Length }), this);
				valueIndex = 0;
			}
			this.optionToIndexMap.Clear();
			for (int i = 0; i < options.Length; i++)
			{
				if (options[i] != null && !this.optionToIndexMap.ContainsKey(options[i]))
				{
					this.optionToIndexMap[options[i]] = i;
				}
			}
			this.PreviousValue = this.Value;
			this.ValueIndex = valueIndex;
			this.SpawnTabs();
			if (triggerOnValueChanged)
			{
				this.OnValueChangedWithIndex.Invoke(this.ValueIndex);
			}
		}

		// Token: 0x06000F7B RID: 3963 RVA: 0x00042780 File Offset: 0x00040980
		public void SetValue(T value, bool triggerOnValueChanged)
		{
			if (this.VerboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", new object[] { "SetValue", "value", value, "triggerOnValueChanged", triggerOnValueChanged }), this);
			}
			int num;
			if (!this.optionToIndexMap.TryGetValue(value, out num))
			{
				Debug.LogError(string.Format("Could not find index of {0} '{1}' in {2}!", "value", value, "options"), this);
				return;
			}
			this.SetValue(num, triggerOnValueChanged);
		}

		// Token: 0x06000F7C RID: 3964 RVA: 0x00042810 File Offset: 0x00040A10
		public virtual void SetValue(int valueIndex, bool triggerOnValueChanged)
		{
			if (this.VerboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", new object[] { "SetValue", "valueIndex", valueIndex, "triggerOnValueChanged", triggerOnValueChanged }), this);
			}
			if (valueIndex < 0 || valueIndex >= this.options.Length)
			{
				Debug.LogError(string.Format("{0} '{1}' is out of range! {2} has a size of {3}!", new object[]
				{
					"valueIndex",
					valueIndex,
					"options",
					this.options.Length
				}), this);
				valueIndex = 0;
			}
			this.PreviousValue = this.Value;
			this.ValueIndex = valueIndex;
			this.ViewAllTabs();
			if (triggerOnValueChanged)
			{
				this.OnValueChangedWithIndex.Invoke(this.ValueIndex);
			}
		}

		// Token: 0x06000F7D RID: 3965 RVA: 0x000428E3 File Offset: 0x00040AE3
		public void Clear()
		{
			if (this.VerboseLogging)
			{
				Debug.Log("Clear", this);
			}
			this.DespawnAllTabs();
			this.hiddenOptionsList.Clear();
			this.hiddenOptionsHashSet.Clear();
			this.optionToIndexMap.Clear();
		}

		// Token: 0x06000F7E RID: 3966 RVA: 0x00042920 File Offset: 0x00040B20
		public void SetHiddenOptions(List<T> newHiddenOptions)
		{
			if (this.VerboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2} )", "SetHiddenOptions", "newHiddenOptions", newHiddenOptions), this);
			}
			this.hiddenOptionsList = newHiddenOptions;
			this.hiddenOptionsHashSet = new HashSet<T>(newHiddenOptions);
			bool flag = false;
			foreach (T t in newHiddenOptions)
			{
				if (this.ValueIndex == Array.IndexOf<T>(this.options, t))
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				this.SetValue(0, true);
				return;
			}
			this.ViewAllTabs();
		}

		// Token: 0x06000F7F RID: 3967 RVA: 0x000429CC File Offset: 0x00040BCC
		public T GetOption(int index)
		{
			if (index < 0 || index >= this.options.Length)
			{
				Debug.LogError(string.Format("{0} '{1}' is out of range! {2} has a size of {3}!", new object[]
				{
					"index",
					index,
					"options",
					this.options.Length
				}), this);
				return default(T);
			}
			return this.options[index];
		}

		// Token: 0x06000F80 RID: 3968 RVA: 0x00042A40 File Offset: 0x00040C40
		public void SetTabBadge(int index, string text)
		{
			if (this.VerboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", new object[] { "SetTabBadge", "index", index, "text", text }), this);
			}
			this.tabs[index].SetBadge(text);
		}

		// Token: 0x06000F81 RID: 3969 RVA: 0x00042AA0 File Offset: 0x00040CA0
		public bool IsSelected(int index)
		{
			if (index < 0 || index >= this.options.Length)
			{
				Debug.LogError(string.Format("{0} '{1}' is out of range! {2} has a size of {3}!", new object[]
				{
					"index",
					index,
					"options",
					this.options.Length
				}), this);
				return false;
			}
			return index == this.ValueIndex;
		}

		// Token: 0x06000F82 RID: 3970 RVA: 0x00042B08 File Offset: 0x00040D08
		public bool IsHidden(T option)
		{
			if (this.VerboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2} )", "IsHidden", "option", option), this);
			}
			bool flag = this.hiddenOptionsHashSet.Contains(option);
			if (this.VerboseLogging)
			{
				Debug.Log(string.Format("{0}: {1}", "isHidden", flag), this);
			}
			return flag;
		}

		// Token: 0x06000F83 RID: 3971 RVA: 0x00042B6E File Offset: 0x00040D6E
		protected virtual UIBaseTab<T> GetTabSource(int index)
		{
			if (this.VerboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2} )", "GetTabSource", "index", index), this);
			}
			return this.tabSource;
		}

		// Token: 0x06000F84 RID: 3972 RVA: 0x00042BA0 File Offset: 0x00040DA0
		private void DespawnAllTabs()
		{
			if (this.VerboseLogging)
			{
				Debug.Log("DespawnAllTabs", this);
			}
			foreach (UIBaseTab<T> uibaseTab in this.tabs)
			{
				MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn<UIBaseTab<T>>(uibaseTab);
			}
			this.tabs = Array.Empty<UIBaseTab<T>>();
		}

		// Token: 0x06000F85 RID: 3973 RVA: 0x00042BF0 File Offset: 0x00040DF0
		private void SpawnTabs()
		{
			if (this.VerboseLogging)
			{
				Debug.Log("SpawnTabs", this);
			}
			if (this.options.Length == 0)
			{
				Debug.LogError("SpawnTabs requires options to have at least one item!", this);
				return;
			}
			if (this.tabs.Length != 0)
			{
				this.DespawnAllTabs();
			}
			this.tabs = new UIBaseTab<T>[this.options.Length];
			for (int i = 0; i < this.options.Length; i++)
			{
				UIBaseTab<T> uibaseTab = this.GetTabSource(i);
				PoolManagerT instance = MonoBehaviourSingleton<PoolManagerT>.Instance;
				UIBaseTab<T> uibaseTab2 = uibaseTab;
				Transform transform = this.container;
				UIBaseTab<T> uibaseTab3 = instance.Spawn<UIBaseTab<T>>(uibaseTab2, default(Vector3), default(Quaternion), transform);
				this.tabs[i] = uibaseTab3;
				uibaseTab3.Initialize(this, i);
			}
		}

		// Token: 0x06000F86 RID: 3974 RVA: 0x00042C9C File Offset: 0x00040E9C
		private void ViewAllTabs()
		{
			if (this.VerboseLogging)
			{
				Debug.Log("ViewAllTabs", this);
			}
			UIBaseTab<T>[] array = this.tabs;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].View();
			}
		}

		// Token: 0x040009DB RID: 2523
		[SerializeField]
		private UIBaseTab<T> tabSource;

		// Token: 0x040009DC RID: 2524
		[SerializeField]
		private RectTransform container;

		// Token: 0x040009DD RID: 2525
		[SerializeField]
		private T[] options = new T[1];

		// Token: 0x040009DE RID: 2526
		[SerializeField]
		private List<T> hiddenOptionsList = new List<T>();

		// Token: 0x040009DF RID: 2527
		private HashSet<T> hiddenOptionsHashSet = new HashSet<T>();

		// Token: 0x040009E0 RID: 2528
		private UIBaseTab<T>[] tabs = Array.Empty<UIBaseTab<T>>();

		// Token: 0x040009E1 RID: 2529
		private readonly Dictionary<T, int> optionToIndexMap = new Dictionary<T, int>();
	}
}
