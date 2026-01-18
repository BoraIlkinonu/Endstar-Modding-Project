using System;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x020001B8 RID: 440
	public class UIStringListCellController : UIBaseListCellController<string>
	{
		// Token: 0x06000B24 RID: 2852 RVA: 0x000308FC File Offset: 0x0002EAFC
		protected override void Start()
		{
			base.Start();
			this.inputField.onSubmit.AddListener(new UnityAction<string>(this.UpdateValue));
			this.removeButton.onClick.AddListener(new UnityAction(this.Remove));
		}

		// Token: 0x06000B25 RID: 2853 RVA: 0x00030948 File Offset: 0x0002EB48
		protected override void OnAddButton()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnAddButton", Array.Empty<object>());
			}
			UIStringListModel uistringListModel = (UIStringListModel)base.ListModel;
			base.ListModel.Add(uistringListModel.DefaultValueOnAdd, true);
		}

		// Token: 0x06000B26 RID: 2854 RVA: 0x0003098B File Offset: 0x0002EB8B
		private void UpdateValue(string newValue)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "UpdateValue", new object[] { newValue });
			}
			base.ListModel.SetItem(base.DataIndex, newValue, true);
		}

		// Token: 0x04000726 RID: 1830
		[Header("UIStringListCellController")]
		[SerializeField]
		private UIInputField inputField;

		// Token: 0x04000727 RID: 1831
		[SerializeField]
		private UIButton removeButton;
	}
}
