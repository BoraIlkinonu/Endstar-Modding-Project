using System;
using Endless.Gameplay;

namespace Endless.Creator.UI;

public abstract class UIInspectorPropReferenceView<TModel, TViewStyle> : UIInspectorReferenceView<TModel, TViewStyle> where TModel : InspectorPropReference where TViewStyle : Enum
{
	protected abstract ReferenceFilter ReferenceFilter { get; }
}
