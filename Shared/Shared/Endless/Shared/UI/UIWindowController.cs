using System;
using Endless.Shared.Debugging;
using Endless.Shared.Validation;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Endless.Shared.UI
{
	// Token: 0x02000285 RID: 645
	public class UIWindowController : UIGameObject, ISelectHandler, IEventSystemHandler, IDeselectHandler, IValidatable
	{
		// Token: 0x17000319 RID: 793
		// (get) Token: 0x06001036 RID: 4150 RVA: 0x000450DF File Offset: 0x000432DF
		// (set) Token: 0x06001037 RID: 4151 RVA: 0x000450E7 File Offset: 0x000432E7
		protected bool VerboseLogging { get; set; }

		// Token: 0x06001038 RID: 4152 RVA: 0x000450F0 File Offset: 0x000432F0
		protected virtual void Start()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("Start", this);
			}
			this.closeButton.onClick.AddListener(new UnityAction(this.Close));
			base.TryGetComponent<UIBaseWindowView>(out this.BaseWindowView);
		}

		// Token: 0x06001039 RID: 4153 RVA: 0x0004512F File Offset: 0x0004332F
		[ContextMenu("Validate")]
		public virtual void Validate()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("Validate", this);
			}
			DebugUtility.DebugHasMonoBehaviour<UIBaseWindowView>(base.gameObject);
			DebugUtility.DebugHasMonoBehaviour<Selectable>(base.gameObject);
		}

		// Token: 0x0600103A RID: 4154 RVA: 0x0004515C File Offset: 0x0004335C
		public void OnSelect(BaseEventData eventData)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "OnSelect", "eventData", eventData.used), this);
			}
			if (!MonoBehaviourSingleton<BackManager>.Instance.HasContext(this.BaseWindowView))
			{
				MonoBehaviourSingleton<BackManager>.Instance.ClaimContext(this.BaseWindowView);
			}
			if (base.RectTransform.GetSiblingIndex() != base.RectTransform.parent.childCount - 1)
			{
				base.RectTransform.SetAsLastSibling();
			}
		}

		// Token: 0x0600103B RID: 4155 RVA: 0x000451E4 File Offset: 0x000433E4
		public void OnDeselect(BaseEventData eventData)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "OnDeselect", "eventData", eventData.used), this);
			}
			if (MonoBehaviourSingleton<BackManager>.Instance.HasContext(this.BaseWindowView))
			{
				MonoBehaviourSingleton<BackManager>.Instance.UnclaimContext(this.BaseWindowView);
			}
		}

		// Token: 0x0600103C RID: 4156 RVA: 0x00045240 File Offset: 0x00043440
		public virtual void Close()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("Close", this);
			}
			this.BaseWindowView.Close();
		}

		// Token: 0x04000A51 RID: 2641
		[SerializeField]
		private UIButton closeButton;

		// Token: 0x04000A53 RID: 2643
		protected UIBaseWindowView BaseWindowView;
	}
}
