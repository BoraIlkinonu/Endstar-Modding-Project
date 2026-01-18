using System;
using Endless.Gameplay.LuaEnums;

namespace Endless.Gameplay.UI
{
	// Token: 0x020003E3 RID: 995
	public class UIBlankNpcCustomizationDataPresenter : UINpcClassCustomizationDataPresenter<BlankNpcCustomizationData>
	{
		// Token: 0x1700051D RID: 1309
		// (get) Token: 0x0600190A RID: 6410 RVA: 0x0001965C File Offset: 0x0001785C
		public override NpcClass NpcClass
		{
			get
			{
				return NpcClass.Blank;
			}
		}
	}
}
