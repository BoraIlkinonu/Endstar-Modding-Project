using System;
using Endless.Creator.Test.LuaParsing;
using Endless.Shared;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x020002DE RID: 734
	[CreateAssetMenu(menuName = "ScriptableObject/UI/Creator/Dictionaries/Token Group Type Color Dictionary", fileName = "Token Group Type Color Dictionary")]
	public class UITokenGroupTypeColorDictionary : BaseEnumKeyScriptableObjectDictionary<TokenGroupTypes, Color>
	{
		// Token: 0x1700019A RID: 410
		// (get) Token: 0x06000C7A RID: 3194 RVA: 0x0003B98C File Offset: 0x00039B8C
		// (set) Token: 0x06000C7B RID: 3195 RVA: 0x0003B994 File Offset: 0x00039B94
		public Color LineHighlightColor { get; private set; } = Color.white;

		// Token: 0x1700019B RID: 411
		// (get) Token: 0x06000C7C RID: 3196 RVA: 0x0003B99D File Offset: 0x00039B9D
		// (set) Token: 0x06000C7D RID: 3197 RVA: 0x0003B9A5 File Offset: 0x00039BA5
		public Color LineErrorColor { get; private set; } = Color.red;
	}
}
