using System;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Endless.Shared.UI
{
	// Token: 0x02000273 RID: 627
	public class UITooltip : UIGameObject, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
	{
		// Token: 0x170002FA RID: 762
		// (get) Token: 0x06000FC1 RID: 4033 RVA: 0x000438C6 File Offset: 0x00041AC6
		// (set) Token: 0x06000FC2 RID: 4034 RVA: 0x000438CE File Offset: 0x00041ACE
		public bool ShouldShow { get; set; } = true;

		// Token: 0x06000FC3 RID: 4035 RVA: 0x000438D7 File Offset: 0x00041AD7
		public void OnPointerEnter(PointerEventData eventData)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnPointerEnter", new object[] { eventData });
			}
			if (this.ShouldShow)
			{
				MonoBehaviourSingleton<UITooltipManager>.Instance.Display(this.tooltip, base.RectTransform, this);
			}
		}

		// Token: 0x06000FC4 RID: 4036 RVA: 0x00043915 File Offset: 0x00041B15
		public void OnPointerExit(PointerEventData eventData)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnPointerExit", new object[] { eventData });
			}
			MonoBehaviourSingleton<UITooltipManager>.Instance.Hide();
		}

		// Token: 0x06000FC5 RID: 4037 RVA: 0x0004393E File Offset: 0x00041B3E
		public void SetTooltip(string tooltip)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetTooltip", new object[] { tooltip });
			}
			this.tooltip = tooltip;
			MonoBehaviourSingleton<UITooltipManager>.Instance.UpdateText(tooltip, this);
		}

		// Token: 0x04000A0E RID: 2574
		[TextArea]
		[SerializeField]
		private string tooltip = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vestibulum facilisis luctus mi, sit amet elementum leo rutrum sed. Mauris tempus at metus eu cursus. Fusce non lacus nec urna pretium tempus a ut elit.";

		// Token: 0x04000A0F RID: 2575
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
