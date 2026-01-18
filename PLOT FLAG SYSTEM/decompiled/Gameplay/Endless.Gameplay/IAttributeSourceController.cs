using System;
using System.Collections.Generic;

namespace Endless.Gameplay;

public interface IAttributeSourceController
{
	event Action OnAttributeSourceChanged;

	List<INpcAttributeModifier> GetAttributeModifiers();
}
