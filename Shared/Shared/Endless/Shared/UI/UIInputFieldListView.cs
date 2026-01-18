using System;
using Endless.Shared.Debugging;
using TMPro;

namespace Endless.Shared.UI
{
	// Token: 0x020001AA RID: 426
	public class UIInputFieldListView : UIBaseListView<string>
	{
		// Token: 0x17000214 RID: 532
		// (get) Token: 0x06000AFC RID: 2812 RVA: 0x000304AF File Offset: 0x0002E6AF
		// (set) Token: 0x06000AFD RID: 2813 RVA: 0x000304B7 File Offset: 0x0002E6B7
		public TMP_InputField.ContentType ContentType { get; private set; }

		// Token: 0x17000215 RID: 533
		// (get) Token: 0x06000AFE RID: 2814 RVA: 0x000304C0 File Offset: 0x0002E6C0
		// (set) Token: 0x06000AFF RID: 2815 RVA: 0x000304C8 File Offset: 0x0002E6C8
		public TMP_InputField.LineType LineType { get; private set; }

		// Token: 0x17000216 RID: 534
		// (get) Token: 0x06000B00 RID: 2816 RVA: 0x000304D1 File Offset: 0x0002E6D1
		// (set) Token: 0x06000B01 RID: 2817 RVA: 0x000304D9 File Offset: 0x0002E6D9
		public TMP_InputField.CharacterValidation CharacterValidation { get; private set; }

		// Token: 0x06000B02 RID: 2818 RVA: 0x000304E2 File Offset: 0x0002E6E2
		public void SetContentType(TMP_InputField.ContentType newValue)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetContentType", new object[] { newValue });
			}
			this.ContentType = newValue;
			base.SetDataToAllVisibleCells();
		}

		// Token: 0x06000B03 RID: 2819 RVA: 0x00030513 File Offset: 0x0002E713
		public void SetLineType(TMP_InputField.LineType newValue)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetLineType", new object[] { newValue });
			}
			this.LineType = newValue;
			base.SetDataToAllVisibleCells();
		}

		// Token: 0x06000B04 RID: 2820 RVA: 0x00030544 File Offset: 0x0002E744
		public void SetCharacterValidation(TMP_InputField.CharacterValidation newValue)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetCharacterValidation", new object[] { newValue });
			}
			this.CharacterValidation = newValue;
			base.SetDataToAllVisibleCells();
		}

		// Token: 0x06000B05 RID: 2821 RVA: 0x00030578 File Offset: 0x0002E778
		public void PlayInvalidInputTween(int dataIndex)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "PlayInvalidInputTween", new object[] { dataIndex });
			}
			foreach (UIBaseListItemView<string> uibaseListItemView in base.GetCellViews(true))
			{
				if (uibaseListItemView.DataIndex == dataIndex)
				{
					((UIInputFieldListCellView)uibaseListItemView).PlayInvalidInputTween();
				}
			}
		}
	}
}
