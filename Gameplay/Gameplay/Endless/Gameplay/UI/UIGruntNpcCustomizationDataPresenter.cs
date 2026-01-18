using System;
using Endless.Gameplay.LuaEnums;

namespace Endless.Gameplay.UI
{
	// Token: 0x020003E5 RID: 997
	public class UIGruntNpcCustomizationDataPresenter : UINpcClassCustomizationDataPresenter<GruntNpcCustomizationData>
	{
		// Token: 0x1700051E RID: 1310
		// (get) Token: 0x0600190E RID: 6414 RVA: 0x00017586 File Offset: 0x00015786
		public override NpcClass NpcClass
		{
			get
			{
				return NpcClass.Grunt;
			}
		}
	}
}
