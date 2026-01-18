using System;
using Endless.Props.Assets;
using Endless.Shared.UI;

namespace Endless.Creator.UI;

public class UIScriptReferenceListModel : UIBaseLocalFilterableListModel<ScriptReference>
{
	protected override Comparison<ScriptReference> DefaultSort => (ScriptReference x, ScriptReference y) => string.Compare(x.NameInCode, y.NameInCode, StringComparison.Ordinal);
}
