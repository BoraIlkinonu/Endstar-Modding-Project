using UnityEngine.Events;

namespace Endless.Creator.UI;

public interface IUIDetailControllable
{
	UnityEvent OnHide { get; }
}
