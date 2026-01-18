using System;
using Endless.Gameplay.LuaEnums;
using Endless.Shared;
using Endless.Shared.UI;

namespace Endless.Gameplay.UI;

public interface IUINpcClassCustomizationDataPresentable : IUIPresentable, IPoolableT, IClearable
{
	event Action<NpcClass> OnNpcClassChanged;
}
