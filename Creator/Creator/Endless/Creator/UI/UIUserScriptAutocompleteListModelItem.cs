using System;
using Endless.Creator.Test.LuaParsing;

namespace Endless.Creator.UI
{
	// Token: 0x02000177 RID: 375
	[Serializable]
	public struct UIUserScriptAutocompleteListModelItem
	{
		// Token: 0x0600058F RID: 1423 RVA: 0x0001D791 File Offset: 0x0001B991
		public UIUserScriptAutocompleteListModelItem(TokenGroupTypes type, string value)
		{
			this.Type = type;
			this.Value = value;
		}

		// Token: 0x06000590 RID: 1424 RVA: 0x0001D7A1 File Offset: 0x0001B9A1
		public override string ToString()
		{
			return string.Format("| {0}: {1}, {2}: {3} |", new object[] { "Type", this.Type, "Value", this.Value });
		}

		// Token: 0x040004EC RID: 1260
		public TokenGroupTypes Type;

		// Token: 0x040004ED RID: 1261
		public string Value;
	}
}
