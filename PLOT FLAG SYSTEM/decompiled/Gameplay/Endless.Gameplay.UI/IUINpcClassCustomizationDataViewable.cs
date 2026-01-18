using System;
using Endless.Gameplay.LuaEnums;

namespace Endless.Gameplay.UI;

public interface IUINpcClassCustomizationDataViewable
{
	event Action<NpcClass> OnNpcClassChanged;
}
