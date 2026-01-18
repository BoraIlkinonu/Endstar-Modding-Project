using System;
using Endless.Gameplay.LevelEditing;
using Endless.Shared.Debugging;
using Endless.Shared.UI;

namespace Endless.Creator.UI
{
	// Token: 0x02000165 RID: 357
	public class UIRuntimePropInfoListController : UIBaseLocalFilterableListController<PropLibrary.RuntimePropInfo>
	{
		// Token: 0x06000554 RID: 1364 RVA: 0x0001CD28 File Offset: 0x0001AF28
		protected override bool IncludeInFilteredResults(PropLibrary.RuntimePropInfo item)
		{
			if (base.SuperVerboseLogging)
			{
				DebugUtility.LogMethod(this, "IncludeInFilteredResults", new object[] { item });
			}
			if (item == null)
			{
				DebugUtility.LogError("PropLibrary.RuntimePropInfo was null!", this);
				return false;
			}
			string text = item.PropData.Name;
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
	}
}
