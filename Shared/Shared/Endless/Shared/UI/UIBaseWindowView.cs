using System;
using System.Collections.Generic;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Endless.Shared.UI
{
	// Token: 0x02000283 RID: 643
	[DisallowMultipleComponent]
	public abstract class UIBaseWindowView : UIGameObject, IPoolableT, IBackable
	{
		// Token: 0x17000313 RID: 787
		// (get) Token: 0x06001021 RID: 4129 RVA: 0x00044DF6 File Offset: 0x00042FF6
		// (set) Token: 0x06001022 RID: 4130 RVA: 0x00044DFE File Offset: 0x00042FFE
		public int MaxInstances { get; private set; } = 1;

		// Token: 0x17000314 RID: 788
		// (get) Token: 0x06001023 RID: 4131 RVA: 0x00044E07 File Offset: 0x00043007
		// (set) Token: 0x06001024 RID: 4132 RVA: 0x00044E0F File Offset: 0x0004300F
		protected bool VerboseLogging { get; set; }

		// Token: 0x17000315 RID: 789
		// (get) Token: 0x06001025 RID: 4133 RVA: 0x00044E18 File Offset: 0x00043018
		public UnityEvent CloseUnityEvent { get; } = new UnityEvent();

		// Token: 0x17000316 RID: 790
		// (get) Token: 0x06001026 RID: 4134 RVA: 0x00044E20 File Offset: 0x00043020
		// (set) Token: 0x06001027 RID: 4135 RVA: 0x00044E28 File Offset: 0x00043028
		public MonoBehaviour Prefab { get; set; }

		// Token: 0x17000317 RID: 791
		// (get) Token: 0x06001028 RID: 4136 RVA: 0x000050D2 File Offset: 0x000032D2
		public bool IsUi
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000318 RID: 792
		// (get) Token: 0x06001029 RID: 4137 RVA: 0x000050D2 File Offset: 0x000032D2
		protected virtual bool Backable
		{
			get
			{
				return true;
			}
		}

		// Token: 0x0600102A RID: 4138 RVA: 0x00044E34 File Offset: 0x00043034
		protected virtual void Start()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("Start", this);
			}
			this.displayAndHideHandler.OnHideComplete.AddListener(new UnityAction(this.Despawn));
			this.displayAndHideHandler.OnDisplayStart.AddListener(new UnityAction(this.EnableGraphicRaycaster));
			this.displayAndHideHandler.OnHideStart.AddListener(new UnityAction(this.DisplayGraphicRaycaster));
		}

		// Token: 0x0600102B RID: 4139 RVA: 0x00044EA8 File Offset: 0x000430A8
		public virtual void OnBack()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("OnBack", this);
			}
			this.Close();
		}

		// Token: 0x0600102C RID: 4140 RVA: 0x00044EC3 File Offset: 0x000430C3
		public virtual void OnSpawn()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("OnSpawn", this);
			}
			if (this.Backable && !MonoBehaviourSingleton<BackManager>.Instance.HasContext(this))
			{
				MonoBehaviourSingleton<BackManager>.Instance.ClaimContext(this);
			}
		}

		// Token: 0x0600102D RID: 4141 RVA: 0x00044EF8 File Offset: 0x000430F8
		public virtual void OnDespawn()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("OnDespawn", this);
			}
		}

		// Token: 0x0600102E RID: 4142 RVA: 0x00044F10 File Offset: 0x00043110
		public virtual void Initialize(Dictionary<string, object> supplementalData)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "Initialize", "supplementalData", (supplementalData == null) ? "null" : supplementalData.Count), this);
				if (supplementalData != null)
				{
					foreach (KeyValuePair<string, object> keyValuePair in supplementalData)
					{
						DebugUtility.Log(string.Format("{0}: {1}", keyValuePair.Key, keyValuePair.Value), this);
					}
				}
			}
			base.RectTransform.anchoredPosition = this.initialAnchoredPosition;
			this.displayAndHideHandler.Display();
		}

		// Token: 0x0600102F RID: 4143 RVA: 0x00044FCC File Offset: 0x000431CC
		public virtual void Close()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("Close", this);
			}
			if (this.Backable && MonoBehaviourSingleton<BackManager>.Instance.HasContext(this))
			{
				MonoBehaviourSingleton<BackManager>.Instance.UnclaimContext(this);
			}
			this.displayAndHideHandler.Hide();
			Action<UIBaseWindowView> closeAction = UIBaseWindowView.CloseAction;
			if (closeAction != null)
			{
				closeAction(this);
			}
			this.CloseUnityEvent.Invoke();
		}

		// Token: 0x06001030 RID: 4144 RVA: 0x00045033 File Offset: 0x00043233
		private void Despawn()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("Despawn", this);
			}
			MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn<UIBaseWindowView>(this);
		}

		// Token: 0x06001031 RID: 4145 RVA: 0x00045053 File Offset: 0x00043253
		private void EnableGraphicRaycaster()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("EnableGraphicRaycaster", this);
			}
			this.graphicRaycaster.enabled = true;
		}

		// Token: 0x06001032 RID: 4146 RVA: 0x00045074 File Offset: 0x00043274
		private void DisplayGraphicRaycaster()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("DisplayGraphicRaycaster", this);
			}
			this.graphicRaycaster.enabled = false;
		}

		// Token: 0x04000A49 RID: 2633
		public static Action<UIBaseWindowView> CloseAction;

		// Token: 0x04000A4A RID: 2634
		[SerializeField]
		private Vector2 initialAnchoredPosition = Vector2.zero;

		// Token: 0x04000A4B RID: 2635
		[SerializeField]
		private UIDisplayAndHideHandler displayAndHideHandler;

		// Token: 0x04000A4C RID: 2636
		[SerializeField]
		private GraphicRaycaster graphicRaycaster;
	}
}
