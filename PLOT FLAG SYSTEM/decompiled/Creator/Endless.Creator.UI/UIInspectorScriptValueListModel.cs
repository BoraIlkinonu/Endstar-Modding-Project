using System;
using Endless.Props.Scripting;
using Endless.Shared.UI;

namespace Endless.Creator.UI;

public class UIInspectorScriptValueListModel : UIBaseLocalFilterableListModel<InspectorScriptValue>
{
	protected override Comparison<InspectorScriptValue> DefaultSort => (InspectorScriptValue x, InspectorScriptValue y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal);
}
