using System;
using Endless.Gameplay.LuaEnums;
using Endless.Shared;
using Endless.Shared.UI;

namespace Endless.Gameplay.UI
{
	// Token: 0x020003DE RID: 990
	public interface IUINpcClassCustomizationDataPresentable : IUIPresentable, IPoolableT, IClearable
	{
		// Token: 0x14000031 RID: 49
		// (add) Token: 0x060018F7 RID: 6391
		// (remove) Token: 0x060018F8 RID: 6392
		event Action<NpcClass> OnNpcClassChanged;
	}
}
