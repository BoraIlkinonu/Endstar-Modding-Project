using System;

namespace Endless.Creator.UI;

public interface IUIBasePropertyViewable
{
	event Action<object> OnUserChangedModel;
}
