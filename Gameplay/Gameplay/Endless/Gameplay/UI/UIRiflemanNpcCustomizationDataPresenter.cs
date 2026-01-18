using System;
using Endless.Gameplay.LuaEnums;

namespace Endless.Gameplay.UI
{
	// Token: 0x020003EA RID: 1002
	public class UIRiflemanNpcCustomizationDataPresenter : UINpcClassCustomizationDataPresenter<RiflemanNpcCustomizationData>
	{
		// Token: 0x17000520 RID: 1312
		// (get) Token: 0x0600191F RID: 6431 RVA: 0x0001BD04 File Offset: 0x00019F04
		public override NpcClass NpcClass
		{
			get
			{
				return NpcClass.Rifleman;
			}
		}
	}
}
