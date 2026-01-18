using System;
using Endless.Shared.Debugging;
using Endless.Shared.Validation;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Endless.Shared.UI
{
	// Token: 0x020001A3 RID: 419
	public class UIListCellSwitch : UIGameObject, IValidatable
	{
		// Token: 0x06000ADC RID: 2780 RVA: 0x0002FB10 File Offset: 0x0002DD10
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.button.onClick.AddListener(new UnityAction(this.Switch));
			if (this.targetIListView)
			{
				this.targetIListView.TryGetComponent<IListView>(out this.iListView);
			}
			else
			{
				Transform parent = base.transform.parent.parent.parent;
				this.iListView = parent.gameObject.GetComponentInChildren<IListView>();
				DebugUtility.LogWarning("Set the reference for targetIListView in edit mode!", this);
			}
			this.iListView.ListCellSizeTypeChangedUnityEvent.AddListener(new UnityAction<ListCellSizeTypes>(this.View));
			this.View(this.iListView.ListCellSizeType);
		}

		// Token: 0x14000032 RID: 50
		// (add) Token: 0x06000ADD RID: 2781 RVA: 0x0002FBD4 File Offset: 0x0002DDD4
		// (remove) Token: 0x06000ADE RID: 2782 RVA: 0x0002FC0C File Offset: 0x0002DE0C
		public event Action OnButtonPressed;

		// Token: 0x06000ADF RID: 2783 RVA: 0x0002FC44 File Offset: 0x0002DE44
		[ContextMenu("Validate")]
		public void Validate()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Validate", Array.Empty<object>());
			}
			if (!this.targetIListView)
			{
				return;
			}
			IListView listView;
			if (!this.targetIListView.TryGetComponent<IListView>(out listView))
			{
				DebugUtility.LogError("targetIListView must implement a IListView interface!", this);
			}
		}

		// Token: 0x06000AE0 RID: 2784 RVA: 0x0002FC94 File Offset: 0x0002DE94
		public void Switch()
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log("Switch", this);
			}
			ListCellSizeTypes listCellSizeTypes = ((this.iListView.ListCellSizeType == ListCellSizeTypes.Compact) ? ListCellSizeTypes.Cozy : ListCellSizeTypes.Compact);
			this.iListView.SetListCellSizeType(listCellSizeTypes);
			Action onButtonPressed = this.OnButtonPressed;
			if (onButtonPressed == null)
			{
				return;
			}
			onButtonPressed();
		}

		// Token: 0x06000AE1 RID: 2785 RVA: 0x0002FCE4 File Offset: 0x0002DEE4
		public void View(ListCellSizeTypes listCellSize)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "View", new object[] { listCellSize });
			}
			Image[] array = this.colorIfCompact;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].color = ((listCellSize == ListCellSizeTypes.Compact) ? this.onColor : this.offColor);
			}
		}

		// Token: 0x040006F4 RID: 1780
		[SerializeField]
		private GameObject targetIListView;

		// Token: 0x040006F5 RID: 1781
		[SerializeField]
		private Image[] colorIfCompact = Array.Empty<Image>();

		// Token: 0x040006F6 RID: 1782
		[SerializeField]
		private Color onColor = Color.green;

		// Token: 0x040006F7 RID: 1783
		[SerializeField]
		private Color offColor = Color.red;

		// Token: 0x040006F8 RID: 1784
		[SerializeField]
		private UIButton button;

		// Token: 0x040006F9 RID: 1785
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x040006FA RID: 1786
		private IListView iListView;
	}
}
