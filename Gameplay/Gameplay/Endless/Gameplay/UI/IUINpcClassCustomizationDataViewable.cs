using System;
using Endless.Gameplay.LuaEnums;

namespace Endless.Gameplay.UI
{
	// Token: 0x020003DF RID: 991
	public interface IUINpcClassCustomizationDataViewable
	{
		// Token: 0x14000032 RID: 50
		// (add) Token: 0x060018F9 RID: 6393
		// (remove) Token: 0x060018FA RID: 6394
		event Action<NpcClass> OnNpcClassChanged;
	}
}
