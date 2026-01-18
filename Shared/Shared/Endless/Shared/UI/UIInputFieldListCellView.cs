using System;
using Endless.Shared.Debugging;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x020001A7 RID: 423
	public class UIInputFieldListCellView : UIBaseListCellView<string>
	{
		// Token: 0x06000AF3 RID: 2803 RVA: 0x0003030C File Offset: 0x0002E50C
		public override void View(UIBaseListView<string> listView, int dataIndex)
		{
			base.View(listView, dataIndex);
			UIInputFieldListView uiinputFieldListView = (UIInputFieldListView)base.ListView;
			this.inputField.contentType = uiinputFieldListView.ContentType;
			this.inputField.lineType = uiinputFieldListView.LineType;
			this.inputField.characterValidation = uiinputFieldListView.CharacterValidation;
			this.inputField.text = base.Model;
		}

		// Token: 0x06000AF4 RID: 2804 RVA: 0x00030371 File Offset: 0x0002E571
		public void PlayInvalidInputTween()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "PlayInvalidInputTween", Array.Empty<object>());
			}
			this.inputField.PlayInvalidInputTweens();
		}

		// Token: 0x0400070F RID: 1807
		[Header("UIInputFieldListCellView")]
		[SerializeField]
		private UIInputField inputField;
	}
}
