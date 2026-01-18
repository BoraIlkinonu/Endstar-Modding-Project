using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x02000286 RID: 646
	[Obsolete]
	[DefaultExecutionOrder(-2147483648)]
	[RequireComponent(typeof(RectTransform))]
	public class UIWindowManager : MonoBehaviourSingleton<UIWindowManager>
	{
		// Token: 0x1700031A RID: 794
		// (get) Token: 0x0600103E RID: 4158 RVA: 0x00045260 File Offset: 0x00043460
		public UnityEvent<UIBaseWindowView> DisplayUnityEvent { get; } = new UnityEvent<UIBaseWindowView>();

		// Token: 0x1700031B RID: 795
		// (get) Token: 0x0600103F RID: 4159 RVA: 0x00045268 File Offset: 0x00043468
		public UIBaseWindowView Displayed
		{
			get
			{
				if (!this.IsDisplaying)
				{
					return null;
				}
				return this.displayed[0];
			}
		}

		// Token: 0x1700031C RID: 796
		// (get) Token: 0x06001040 RID: 4160 RVA: 0x00045280 File Offset: 0x00043480
		public bool IsDisplaying
		{
			get
			{
				return this.displayed.Count > 0;
			}
		}

		// Token: 0x06001041 RID: 4161 RVA: 0x00045290 File Offset: 0x00043490
		public bool IsDisplayingType<T>() where T : UIBaseWindowView
		{
			return this.IsDisplaying && this.displayed[0] is T;
		}

		// Token: 0x06001042 RID: 4162 RVA: 0x000452B0 File Offset: 0x000434B0
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			foreach (object obj in this.sources)
			{
				UIBaseWindowView uibaseWindowView = (UIBaseWindowView)obj;
				Type type = uibaseWindowView.GetType();
				if (this.sourceDictionary.ContainsKey(type))
				{
					Debug.LogException(new Exception(string.Format("There is a duplicate entry of {0}!", uibaseWindowView.GetType())), this);
				}
				else
				{
					this.sourceDictionary.Add(uibaseWindowView.GetType(), uibaseWindowView);
				}
			}
			UIBaseWindowView.CloseAction = (Action<UIBaseWindowView>)Delegate.Combine(UIBaseWindowView.CloseAction, new Action<UIBaseWindowView>(this.RemoveFromDisplayed));
		}

		// Token: 0x06001043 RID: 4163 RVA: 0x00045380 File Offset: 0x00043580
		[Obsolete]
		public UIBaseWindowView Display<T>(Transform parent = null, Dictionary<string, object> supplementalData = null) where T : UIBaseWindowView
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Display", new object[]
				{
					parent.DebugSafeName(true),
					(supplementalData == null) ? "null" : supplementalData.Count
				});
			}
			Type typeFromHandle = typeof(T);
			if (!this.sourceDictionary.ContainsKey(typeFromHandle))
			{
				Debug.LogException(new Exception(string.Format("No source with a type of {0} is in {1}!", typeFromHandle, "sourceDictionary")), this);
			}
			if (this.displayed.Count((UIBaseWindowView entry) => entry is T) >= this.sourceDictionary[typeFromHandle].MaxInstances)
			{
				return null;
			}
			UIBaseWindowView uibaseWindowView = this.sourceDictionary[typeFromHandle];
			PoolManagerT instance = MonoBehaviourSingleton<PoolManagerT>.Instance;
			UIBaseWindowView uibaseWindowView2 = uibaseWindowView;
			Transform transform = ((parent == null) ? base.transform : parent);
			UIBaseWindowView uibaseWindowView3 = instance.Spawn<UIBaseWindowView>(uibaseWindowView2, default(Vector3), default(Quaternion), transform);
			this.displayed.Add(uibaseWindowView3);
			uibaseWindowView3.Initialize(supplementalData);
			RectTransformValue rectTransformValue = new RectTransformValue(uibaseWindowView.RectTransform);
			rectTransformValue.ApplyTo(uibaseWindowView3.RectTransform);
			this.DisplayUnityEvent.Invoke(uibaseWindowView3);
			return uibaseWindowView3;
		}

		// Token: 0x06001044 RID: 4164 RVA: 0x000454B8 File Offset: 0x000436B8
		[Obsolete]
		public void CloseAllInstancesOf<T>() where T : UIBaseWindowView
		{
			Type typeFromHandle = typeof(T);
			if (this.verboseLogging)
			{
				DebugUtility.LogMethodWithAppension(this, "CloseAllInstancesOf", typeFromHandle.Name, Array.Empty<object>());
			}
			for (int i = this.displayed.Count - 1; i >= 0; i--)
			{
				if (this.displayed[i].GetType() == typeFromHandle)
				{
					this.displayed[i].Close();
				}
			}
		}

		// Token: 0x06001045 RID: 4165 RVA: 0x00045530 File Offset: 0x00043730
		[Obsolete]
		public void CloseAll()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "CloseAll", Array.Empty<object>());
			}
			for (int i = this.displayed.Count - 1; i >= 0; i--)
			{
				this.displayed[i].Close();
			}
		}

		// Token: 0x06001046 RID: 4166 RVA: 0x00045580 File Offset: 0x00043780
		[Obsolete]
		private void RemoveFromDisplayed(UIBaseWindowView target)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "RemoveFromDisplayed", new object[] { target.GetType().Name });
			}
			if (!this.displayed.Contains(target))
			{
				DebugUtility.LogWarning(this, "RemoveFromDisplayed", "UIWindowManager was not aware of this!", new object[] { target.GetType().Name });
				return;
			}
			this.displayed.Remove(target);
		}

		// Token: 0x04000A54 RID: 2644
		[SerializeField]
		private UIWindowScriptableObjectArray sources;

		// Token: 0x04000A55 RID: 2645
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x04000A56 RID: 2646
		private readonly List<UIBaseWindowView> displayed = new List<UIBaseWindowView>();

		// Token: 0x04000A57 RID: 2647
		private Dictionary<Type, UIBaseWindowView> sourceDictionary = new Dictionary<Type, UIBaseWindowView>();
	}
}
