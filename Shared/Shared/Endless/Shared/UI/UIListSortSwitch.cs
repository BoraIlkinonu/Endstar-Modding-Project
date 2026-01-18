using System;
using Endless.Shared.Debugging;
using Endless.Shared.Validation;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Endless.Shared.UI
{
	// Token: 0x020001A4 RID: 420
	public class UIListSortSwitch : UIGameObject, IValidatable
	{
		// Token: 0x06000AE3 RID: 2787 RVA: 0x0002FD6C File Offset: 0x0002DF6C
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.button.onClick.AddListener(new UnityAction(this.Switch));
			if (this.targetIListModel)
			{
				this.targetIListModel.TryGetComponent<IListModel>(out this.iListModel);
			}
			else
			{
				Transform parent = base.transform.parent.parent.parent;
				this.iListModel = parent.gameObject.GetComponentInChildren<IListModel>();
				DebugUtility.LogWarning("Set the reference for targetIListModel in edit mode!", this);
			}
			this.iListModel.SortOrderChangedUnityEvent.AddListener(new UnityAction<SortOrders>(this.View));
			this.View(this.iListModel.SortOrder);
		}

		// Token: 0x06000AE4 RID: 2788 RVA: 0x0002FE30 File Offset: 0x0002E030
		[ContextMenu("Validate")]
		public void Validate()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Validate", Array.Empty<object>());
			}
			if (!this.targetIListModel)
			{
				return;
			}
			IListModel listModel;
			if (!this.targetIListModel.TryGetComponent<IListModel>(out listModel))
			{
				DebugUtility.LogError("targetIListModel must implement a IListModel interface!", this);
			}
		}

		// Token: 0x06000AE5 RID: 2789 RVA: 0x0002FE80 File Offset: 0x0002E080
		public void Switch()
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log("Switch", this);
			}
			SortOrders sortOrders = ((this.iListModel.SortOrder == SortOrders.asc) ? SortOrders.desc : SortOrders.asc);
			this.iListModel.SetSortOrder(sortOrders);
		}

		// Token: 0x06000AE6 RID: 2790 RVA: 0x0002FEC0 File Offset: 0x0002E0C0
		private void View(SortOrders sortOrder)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "View", new object[] { sortOrder });
			}
			bool flag = sortOrder == SortOrders.asc;
			this.valueText.text = (flag ? this.ascendingValueDisplay : this.descendingValueDisplay);
			this.ascendingArrowImage.color = (flag ? this.onColor : this.offColor);
			this.descendingArrowImage.color = (flag ? this.offColor : this.onColor);
		}

		// Token: 0x040006FC RID: 1788
		[SerializeField]
		private GameObject targetIListModel;

		// Token: 0x040006FD RID: 1789
		[SerializeField]
		private TextMeshProUGUI valueText;

		// Token: 0x040006FE RID: 1790
		[SerializeField]
		private string ascendingValueDisplay = "Ascending";

		// Token: 0x040006FF RID: 1791
		[SerializeField]
		private string descendingValueDisplay = "Descending";

		// Token: 0x04000700 RID: 1792
		[SerializeField]
		private Image ascendingArrowImage;

		// Token: 0x04000701 RID: 1793
		[SerializeField]
		private Image descendingArrowImage;

		// Token: 0x04000702 RID: 1794
		[SerializeField]
		private Color onColor = Color.green;

		// Token: 0x04000703 RID: 1795
		[SerializeField]
		private Color offColor = Color.red;

		// Token: 0x04000704 RID: 1796
		[SerializeField]
		private UIButton button;

		// Token: 0x04000705 RID: 1797
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x04000706 RID: 1798
		private IListModel iListModel;
	}
}
