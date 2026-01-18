using System;
using Endless.Shared.Debugging;
using UnityEngine;

namespace Endless.Shared.UI.Test
{
	// Token: 0x0200029B RID: 667
	public class UITestLocalFilterableListController : UIBaseLocalFilterableListController<int>
	{
		// Token: 0x06001094 RID: 4244 RVA: 0x00046C0D File Offset: 0x00044E0D
		protected override void Synchronize()
		{
			base.Synchronize();
			this.testListModelHandler.Synchronize();
		}

		// Token: 0x06001095 RID: 4245 RVA: 0x00046C20 File Offset: 0x00044E20
		protected override bool IncludeInFilteredResults(int item)
		{
			if (base.SuperVerboseLogging)
			{
				DebugUtility.LogMethod(this, "IncludeInFilteredResults", new object[] { item });
			}
			string text = item.ToString();
			if (!base.CaseSensitive)
			{
				text = text.ToLower();
			}
			string text2 = base.StringFilter;
			if (!base.CaseSensitive)
			{
				text2 = text2.ToLower();
			}
			return text.Contains(text2);
		}

		// Token: 0x04000A79 RID: 2681
		[Header("UITestLocalFilterableListController")]
		[SerializeField]
		private UITestListModelHandler testListModelHandler;
	}
}
