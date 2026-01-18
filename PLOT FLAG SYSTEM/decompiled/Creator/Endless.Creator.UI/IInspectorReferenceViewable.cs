using System;

namespace Endless.Creator.UI;

public interface IInspectorReferenceViewable
{
	event Action OnClear;

	event Action OnOpenIEnumerableWindow;
}
